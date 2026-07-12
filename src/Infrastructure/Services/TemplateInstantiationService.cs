using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;

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
    {
        var template = await ctx.TplOrgIntents
            .Include(t => t.Stages)!.ThenInclude(s => s.Offers)!.ThenInclude(o => o.Tasks)
            .FirstOrDefaultAsync(t => t.Code == "FIRST_BOARD");
        if (template == null)
        {
            _logger.LogWarning("Пропуск инстанцирования: шаблон FIRST_BOARD не найден");
            return 0;
        }

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

        if (template.Stages != null)
        {
            foreach (var ts in template.Stages.OrderBy(s => s.SortOrder))
            {
                var stage = new OrgStage
                {
                    Id = Guid.NewGuid(),
                    IntentId = intent.Id,
                    TemplateStageId = ts.Id,
                    Name = ts.Name,
                    Description = ts.Description,
                    SortOrder = ts.SortOrder,
                    Status = "PLANNED"
                };
                ctx.OrgStages.Add(stage);

                if (ts.Offers != null)
                {
                    foreach (var to in ts.Offers.OrderBy(o => o.SortOrder))
                    {
                        var offer = new OrgOffer
                        {
                            Id = Guid.NewGuid(),
                            StageId = stage.Id,
                            TemplateOfferId = to.Id,
                            Name = to.Name,
                            Description = to.Description,
                            SortOrder = to.SortOrder,
                            Status = "PLANNED"
                        };
                        ctx.OrgOffers.Add(offer);

                        if (to.Tasks != null)
                        {
                            foreach (var tt in to.Tasks.OrderBy(t => t.SortOrder))
                            {
                                // Если задаче назначена должность СД, проверяем, что она есть в составе
                                if (tt.AssignedBoardRoleId.HasValue
                                    && !existingBoardRoleIds.Contains(tt.AssignedBoardRoleId.Value))
                                {
                                    _logger.LogWarning("Задача \"{TaskName}\" пропущена: должность СД не найдена в составе",
                                        tt.Name);
                                    continue;
                                }

                                ctx.OrgTasks.Add(new OrgTask
                                {
                                    Id = Guid.NewGuid(),
                                    OfferId = offer.Id,
                                    TemplateTaskId = tt.Id,
                                    Name = tt.Name,
                                    Description = tt.Description,
                                    SortOrder = tt.SortOrder,
                                    Status = "PLANNED",
                                    AssignedRoleId = tt.AssignedRoleId,
                                    AssignedBoardRoleId = tt.AssignedBoardRoleId
                                });
                                taskCount++;
                            }
                        }
                    }
                }
            }
        }

        _logger.LogDebug("Шаблон FIRST_BOARD инстанцирован для LegalEntity {LegalEntityId}, создано задач: {TaskCount}",
            legalEntityId, taskCount);

        return taskCount;
    }
}
