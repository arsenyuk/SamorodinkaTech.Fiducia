using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using ZXing;
using ZXing.Common;
using UglyToad.PdfPig;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Сервис чтения QR-кодов из изображений и PDF-файлов.
/// </summary>
public sealed class QrCodeReaderService : IQrCodeReaderService
{
    private readonly ILogger<QrCodeReaderService> _logger;

    public QrCodeReaderService(ILogger<QrCodeReaderService> logger)
    {
        _logger = logger;
    }

    public async Task<string?> ReadFromImageAsync(Stream imageStream, CancellationToken ct = default)
    {
        try
        {
            imageStream.Position = 0;

            using var image = await Image.LoadAsync<Rgba32>(imageStream, ct);

            // Конвертируем ImageSharp → byte[] (RGB) для ZXing
            var width = image.Width;
            var height = image.Height;
            var pixels = new byte[width * height * 3];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pixel = image[x, y];
                    var offset = (y * width + x) * 3;
                    pixels[offset] = pixel.R;
                    pixels[offset + 1] = pixel.G;
                    pixels[offset + 2] = pixel.B;
                }
            }

            var source = new RGBLuminanceSource(pixels, width, height);
            var reader = new BarcodeReaderGeneric
            {
                AutoRotate = true,
                Options = new DecodingOptions
                {
                    TryHarder = true,
                    TryInverted = true,
                    PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE }
                }
            };

            var result = reader.Decode(source);
            if (result is null)
            {
                _logger.LogDebug("QR-код не найден на изображении");
                return null;
            }

            _logger.LogInformation("QR-код декодирован: {Text}", result.Text);
            return result.Text;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка чтения QR-кода из изображения");
            return null;
        }
    }

    public async Task<string?> ReadFromPdfAsync(Stream pdfStream, CancellationToken ct = default)
    {
        try
        {
            pdfStream.Position = 0;

            using var document = PdfDocument.Open(pdfStream);

            foreach (var page in document.GetPages())
            {
                ct.ThrowIfCancellationRequested();

                var images = page.GetImages();
                foreach (var pdfImage in images)
                {
                    try
                    {
                        var imageBytes = pdfImage.RawBytes.ToArray();
                        using var imageStream = new MemoryStream(imageBytes);
                        var qrText = await ReadFromImageAsync(imageStream, ct);
                        if (qrText is not null)
                            return qrText;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Не удалось декодировать QR из изображения на странице {Page}", page.Number);
                    }
                }
            }

            _logger.LogDebug("QR-код не найден ни на одной странице PDF ({Pages} стр.)", document.NumberOfPages);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка чтения QR-кода из PDF");
            return null;
        }
    }
}

/// <summary>
/// LuminanceSource для ZXing на основе RGB byte-массива.
/// </summary>
internal sealed class RGBLuminanceSource : ZXing.LuminanceSource
{
    private readonly byte[] _rgbBytes;
    private readonly int _width;
    private readonly int _height;

    public RGBLuminanceSource(byte[] rgbBytes, int width, int height)
        : base(width, height)
    {
        _rgbBytes = rgbBytes;
        _width = width;
        _height = height;
    }

    public override byte[] Matrix =>
        _rgbBytes;

    public override byte[] getRow(int y, byte[]? row)
    {
        if (row is null || row.Length < _width)
            row = new byte[_width];

        var offset = y * _width * 3;
        for (int x = 0; x < _width; x++)
        {
            row[x] = (byte)((_rgbBytes[offset + x * 3] * 299
                            + _rgbBytes[offset + x * 3 + 1] * 587
                            + _rgbBytes[offset + x * 3 + 2] * 114) / 1000);
        }

        return row;
    }
}
