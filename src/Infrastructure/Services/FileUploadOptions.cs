using System.Collections.Generic;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services
{
    /// <summary>
    /// Опции конфигурации загрузки файлов (BDR-011).
    /// </summary>
    public class FileUploadOptions
    {
        /// <summary>Максимальный размер файла в байтах (по умолчанию 50 МБ).</summary>
        public long MaxFileSizeBytes { get; set; } = 50_000_000;

        /// <summary>Размер чанка в байтах (по умолчанию 512 КБ).</summary>
        public int ChunkSizeBytes { get; set; } = 512 * 1024;

        /// <summary>Максимальное количество одновременных загрузок.</summary>
        public int MaxConcurrentUploads { get; set; } = 10;

        /// <summary>Время жизни незавершённой загрузки.</summary>
        public int UploadExpirationHours { get; set; } = 24;

        /// <summary>Базовый папка для временных чанков (null = подкаталог uploads в BasePath).</summary>
        public string? TempBasePath { get; set; }

        /// <summary>
        /// Запрещённые расширения файлов (без точки, в нижнем регистре).
        /// Файлы с этими расширениями отклоняются при загрузке.
        /// </summary>
        public HashSet<string> BlockedExtensions { get; set; } = new(StringComparer.OrdinalIgnoreCase)
        {
            // Исполняемые файлы
            "exe", "dll", "com", "msi", "bat", "cmd", "scr", "pif",
            "vbs", "vbe", "js", "jse", "ws", "wsh", "wsf",
            "ps1", "psm1", "psd1", "psc1",
            "sh", "bash", "csh", "ksh", "zsh",
            "app", "bin", "command", "cpl", "hta", "inf", "ins", "isp",
            "job", "lnk", "mdb", "msc", "msp",
            "reg", "rgs", "scf", "snap",
            "application", "gadget", "paf",
            "xbap", "xll", "xnk",

            // Архивы
            "zip", "rar", "7z", "tar", "gz", "bz2", "xz", "zst",
            "iso", "img", "vhd", "vhdx", "vmdk", "ova", "ovf",
            "cab", "dmg", "pak", "war", "ear",

            // Другие потенциально опасные
            "cpl", "dll", "sys", "drv", "diagnostics", "msix", "appx",
            "widget", "webpnp"
        };
    }
}
