using QRCoder;

namespace KorID.API.Services;

public class QrCodeService
{
    public byte[] GenerateQrCode(string payload)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        
        return qrCode.GetGraphic(20); 
    }
}