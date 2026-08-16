using System.Collections.Generic;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services
{
    /// <summary>
    /// Опции конфигурации загрузки файлов (BDR-011).
    /// </summary>
    public class FileUploadOptions
    {
        /// <summary>Значение по умолчанию: максимальный размер файла 50 МБ.</summary>
        public const long DefaultMaxFileSizeBytes = 50_000_000;

        /// <summary>Значение по умолчанию: размер чанка 512 КБ.</summary>
        public const int DefaultChunkSizeBytes = 512 * 1024;

        /// <summary>Значение по умолчанию: максимум 10 одновременных загрузок.</summary>
        public const int DefaultMaxConcurrentUploads = 10;

        /// <summary>Значение по умолчанию: время жизни незавершённой загрузки 24 часа.</summary>
        public const int DefaultUploadExpirationHours = 24;

        /// <summary>Максимальный размер файла в байтах.</summary>
        public long MaxFileSizeBytes { get; set; } = DefaultMaxFileSizeBytes;

        /// <summary>Размер чанка в байтах.</summary>
        public int ChunkSizeBytes { get; set; } = DefaultChunkSizeBytes;

        /// <summary>Максимальное количество одновременных загрузок.</summary>
        public int MaxConcurrentUploads { get; set; } = DefaultMaxConcurrentUploads;

        /// <summary>Время жизни незавершённой загрузки.</summary>
        public int UploadExpirationHours { get; set; } = DefaultUploadExpirationHours;

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
