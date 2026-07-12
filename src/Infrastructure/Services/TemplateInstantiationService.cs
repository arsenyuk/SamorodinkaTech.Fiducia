using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Enums;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using System.Text.Json;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Сервис инстанцирования шаблона организационных задач в реальные задачи.
/// </summary>
public class TemplateInstantiationService : ITemplateInstantiationService
{
    private readonly ILogger<TemplateInstantiationService> _logger;

    public TemplateInstantiationService(ILogger<TemplateInstantiationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<int> InstantiateFirstBoardAsync(
        IApplicationDbContext ctx,
        Guid legalEntityId,
        Guid? boardOfDirectorsId)
        => await InstantiateAsync(ctx, "FIRST_BOARD", legalEntityId, boardOfDirectorsId);

    /// <inheritdoc />
    public async Task<int> InstantiateAsync(
        IApplicationDbContext ctx,
        string code,
        Guid legalEntityId,
        Guid? boardOfDirectorsId)
    {
        var template = await ctx.TplOrgIntents
            .Include(t => t.Stages)!.ThenInclude(s => s.Offers)
            .FirstOrDefaultAsync(t => t.Code == code);
        if (template == null)
        {
            _logger.LogWarning("Пропуск инстанцирования: шаблон {Code} не найден", code);
            return 0;
        }

        // Загружаем контекст ЮЛ для проверки предикатов
        var charter = await ctx.LegalEntityCharters
            .FirstOrDefaultAsync(x => x.LegalEntityId == legalEntityId);
        var boardSettings = await ctx.LegalEntityBoardSettings
            .FirstOrDefaultAsync();
        var votingRules = await ctx.LegalEntityVotingRules
            .FirstOrDefaultAsync(x => x.LegalEntityId == legalEntityId);

        // Собираем ID должностей, которые есть в текущем составе СД
        HashSet<Guid> existingBoardRoleIds = new();
        if (boardOfDirectorsId.HasValue)
        {
            existingBoardRoleIds = (await ctx.BoardMembers
                .Where(bm => bm.BoardOfDirectorsId == boardOfDirectorsId.Value)
                .SelectMany(bm => ctx.BoardMemberAppointments
                    .Where(a => a.BoardMemberId == bm.Id))
                .Select(a => a.RoleId)
                .ToListAsync())
                .ToHashSet();
        }

        var intent = new OrgIntent
        {
            Id = Guid.NewGuid(),
            LegalEntityId = legalEntityId,
            TemplateIntentId = template.Id,
            Name = template.Name,
            Description = template.Description,
            SortOrder = template.SortOrder,
            Status = "PLANNED"
        };
        ctx.OrgIntents.Add(intent);

        var taskCount = 0;
        var triggerDate = DateOnly.FromDateTime(DateTime.Today);
        var holidays = WorkingDayHelper.GetHolidays(triggerDate.Year);
        holidays.UnionWith(WorkingDayHelper.GetHolidays(triggerDate.Year + 1));
        var stageMap = new Dictionary<Guid, Guid>(); // template_stage_id → real_stage_id
        var taskMap = new Dictionary<Guid, Guid>();   // template_offer_id → real_task_id
        var stageEndDates = new Dictionary<Guid, DateOnly>(); // template_stage_id → вычисленный конец
        DateOnly? maxEndSoFar = null;

        if (template.Stages != null)
        {
            foreach (var ts in template.Stages.OrderBy(s => s.SortOrder))
            {
                var rawStart = triggerDate.AddDays(ts.StartOffsetDays ?? 0);

                // Каскад: этап не может начаться раньше следующего дня после конца предыдущего
                if (maxEndSoFar.HasValue && maxEndSoFar.Value > rawStart)
                    rawStart = maxEndSoFar.Value.AddDays(1);

                // Если есть явные предшественники — стартуем не раньше следующего дня после их окончания
                if (!string.IsNullOrEmpty(ts.PredecessorStageIds))
                {
                    try
                    {
                        var predIds = JsonSerializer.Deserialize<Guid[]>(ts.PredecessorStageIds);
                        if (predIds != null)
                        {
                            foreach (var pid in predIds)
                            {
                                if (stageEndDates.TryGetValue(pid, out var pe) && pe > rawStart)
                                    rawStart = pe.AddDays(1);
                            }
                        }
                    }
                    catch { }
                }

                var stageStart = WorkingDayHelper.GetNextWorkingDay(rawStart, holidays);
                var stageEnd = ComputeDeadline(stageStart, ts.DeadlineRule, ts.DeadlineDays, holidays);

                var stage = new OrgStage
                {
                    Id = Guid.NewGuid(),
                    IntentId = intent.Id,
                    TemplateStageId = ts.Id,
                    Name = ts.Name,
                    Description = ts.Description,
                    SortOrder = ts.SortOrder,
                    Status = "PLANNED",
                    PlannedStart = stageStart,
                    PlannedEnd = stageEnd
                };
                ctx.OrgStages.Add(stage);
                stageMap[ts.Id] = stage.Id;

                if (ts.Offers != null)
                {
                    foreach (var to in ts.Offers.OrderBy(o => o.StartOffsetDays ?? 0).ThenBy(o => o.Name))
                    {
                        if (to.AssignedBoardRoleId.HasValue
                            && !existingBoardRoleIds.Contains(to.AssignedBoardRoleId.Value))
                        {
                            _logger.LogWarning("Офер \"{OfferName}\" пропущен: должность СД не найдена в составе",
                                to.Name);
                            continue;
                        }

                        if (!ShouldInclude(to, charter, boardSettings, boardOfDirectorsId, votingRules))
                        {
                            _logger.LogDebug("Офер \"{OfferName}\" пропущен по предикату", to.Name);
                            continue;
                        }

                        var taskStart = WorkingDayHelper.GetNextWorkingDay(
                            stageStart.AddDays(to.StartOffsetDays ?? 0), holidays);
                        var taskEnd = ComputeDeadline(taskStart, to.DeadlineRule, to.DeadlineDays, holidays);

                        var task = new OrgTask
                        {
                            Id = Guid.NewGuid(),
                            StageId = stage.Id,
                            TemplateOfferId = to.Id,
                            Name = to.Name,
                            Description = to.Description,
                            Status = "PLANNED",
                            PlannedStart = taskStart,
                            PlannedEnd = taskEnd,
                            AssignedRoleId = to.AssignedRoleId,
                            AssignedBoardRoleId = to.AssignedBoardRoleId
                        };
                        ctx.OrgTasks.Add(task);
                        taskMap[to.Id] = task.Id;
                        taskCount++;

                        // Окончание этапа расширяется до максимальной даты окончания его задач
                        if (taskEnd.HasValue && (stage.PlannedEnd == null || taskEnd.Value > stage.PlannedEnd.Value))
                            stage.PlannedEnd = taskEnd.Value;
                    }
                }

                stageEndDates[ts.Id] = stage.PlannedEnd ?? stageStart;
                if (stage.PlannedEnd.HasValue && (maxEndSoFar == null || stage.PlannedEnd.Value > maxEndSoFar.Value))
                    maxEndSoFar = stage.PlannedEnd.Value;
                else if (!maxEndSoFar.HasValue)
                    maxEndSoFar = stageStart;
            }

            // Разрешение предшественников этапов
            foreach (var ts in template.Stages)
            {
                if (!string.IsNullOrEmpty(ts.PredecessorStageIds) && stageMap.TryGetValue(ts.Id, out var stageId))
                {
                    var predTemplateIds = JsonSerializer.Deserialize<Guid[]>(ts.PredecessorStageIds);
                    if (predTemplateIds != null)
                    {
                        var realPredIds = predTemplateIds
                            .Where(tid => stageMap.ContainsKey(tid))
                            .Select(tid => stageMap[tid])
                            .ToArray();
                        var stage = ctx.OrgStages.Local.FirstOrDefault(s => s.Id == stageId);
                        if (stage != null)
                            stage.PredecessorStageIds = JsonSerializer.Serialize(realPredIds);
                    }
                }

                if (ts.Offers != null)
                {
                    foreach (var to in ts.Offers)
                    {
                        if (!string.IsNullOrEmpty(to.PredecessorOfferIds) && taskMap.TryGetValue(to.Id, out var taskId))
                        {
                            var predTemplateIds = JsonSerializer.Deserialize<Guid[]>(to.PredecessorOfferIds);
                            if (predTemplateIds != null)
                            {
                                var realPredIds = predTemplateIds
                                    .Where(tid => taskMap.ContainsKey(tid))
                                    .Select(tid => taskMap[tid])
                                    .ToArray();
                                var task = ctx.OrgTasks.Local.FirstOrDefault(t => t.Id == taskId);
                                if (task != null)
                                    task.PredecessorTaskIds = JsonSerializer.Serialize(realPredIds);
                            }
                        }
                    }
                }
            }
        }

        _logger.LogDebug("Шаблон {Code} инстанцирован для LegalEntity {LegalEntityId}, создано задач: {TaskCount}",
            code, legalEntityId, taskCount);

        return taskCount;
    }

    private static DateOnly? ComputeDeadline(DateOnly start, string? rule, int? days, IReadOnlySet<DateOnly> holidays)
    {
        if (string.IsNullOrEmpty(rule) || !days.HasValue) return null;
        var deadline = rule switch
        {
            "FIXED_DAYS" => start.AddDays(Math.Max(0, days.Value - 1)),
            "AFTER_START" => start.AddDays(days.Value),
            _ => (DateOnly?)null
        };
        if (deadline.HasValue && WorkingDayHelper.IsNonWorking(deadline.Value, holidays))
            return WorkingDayHelper.GetNextWorkingDay(deadline.Value, holidays);
        return deadline;
    }

    private static bool ShouldInclude(TplOrgTaskOffer to, LegalEntityCharter? charter, LegalEntityBoardSettings? board, Guid? boardOfDirectorsId, LegalEntityVotingRules? rules)
    {
        if (to.RequireNotaryConfirmation == true && charter?.DecisionConfirmationByAllSign != false)
            return false;
        if (to.RequireNotaryConfirmation == false && charter?.DecisionConfirmationByAllSign != true)
            return false;

        if (to.RequireAllSignConfirmation == true && charter?.DecisionConfirmationByAllSign != true)
            return false;
        if (to.RequireAllSignConfirmation == false && charter?.DecisionConfirmationByAllSign != false)
            return false;

        if (to.RequireCommittees == true && board?.CommitteesMandatory != true)
            return false;
        if (to.RequireCommittees == false && board?.CommitteesMandatory != false)
            return false;

        if (to.RequireBoardRegulation == true && charter?.BoardRegulationDocumentId == null)
            return false;
        if (to.RequireBoardRegulation == false && charter?.BoardRegulationDocumentId != null)
            return false;

        if (to.RequireCustomCharter == true && charter == null)
            return false;
        if (to.RequireCustomCharter == false && charter != null)
            return false;

        if (to.RequireExecutiveBodyA == true && charter?.ExecutiveBody != 'A')
            return false;
        if (to.RequireExecutiveBodyA == false && charter?.ExecutiveBody == 'A')
            return false;

        if (to.RequireBoardOfDirectors == true && boardOfDirectorsId == null)
            return false;
        if (to.RequireBoardOfDirectors == false && boardOfDirectorsId != null)
            return false;

        var isUzedo = rules?.DocumentFlow == DocumentFlowType.Mixed
                   || rules?.DocumentFlow == DocumentFlowType.LegalElectronic;
        if (to.RequireDocumentFlowLegalElectronic == true && isUzedo != true)
            return false;
        if (to.RequireDocumentFlowLegalElectronic == false && isUzedo != false)
            return false;

        // require_mandatory_audit: null = всегда, true = только при обязательном аудите
        if (to.RequireMandatoryAudit == true && charter?.MandatoryAudit != true)
            return false;
        if (to.RequireMandatoryAudit == false && charter?.MandatoryAudit != false)
            return false;

        // require_revision_commission: null = всегда
        // Для ООО: определяется флагом в charter; для АО: ПАО всегда, НАО≥50 всегда
        if (to.RequireRevisionCommission == true && charter?.HasRevisionCommission != true)
            return false;
        if (to.RequireRevisionCommission == false && charter?.HasRevisionCommission != false)
            return false;

        return true;
    }
}
