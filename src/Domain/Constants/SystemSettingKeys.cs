namespace SamorodinkaTech.Fiducia.Domain.Constants;

/// <summary>
/// Константы ключей системных настроек (system_settings.key).
/// Все обращения к KV-таблице настроек используют только эти константы.
/// </summary>
public static class SystemSettingKeys
{
    // Общие
    public const string GanttEndPaddingWeeks = "gantt_end_padding_weeks";
    public const string BlockedExtensions = "blocked_extensions";
    public const string QrCodeExtensions = "qr_code_extensions";

    // ООО
    public const string OsuProtocolDeadlineDays = "osu_protocol_deadline_days";
    public const string OsuBoardProtocolDeadlineDays = "osu_board_protocol_deadline_days";
    public const string OosuTitleTemplate = "oosu_title_template";
    public const string VosuDefaultThresholdPercent = "vosu_default_threshold_percent";

    // АО
    public const string OsaProtocolDeadlineDays = "osa_protocol_deadline_days";
    public const string OsaBoardProtocolDeadlineDays = "osa_board_protocol_deadline_days";
    public const string GosaTitleTemplate = "gosa_title_template";
    public const string VosaDefaultThresholdPercent = "vosa_default_threshold_percent";
}
