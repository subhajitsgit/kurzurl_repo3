using KurzUrl.BusinessLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QRCoder;
using System.Drawing;

namespace KurzUrl.BusinessLayer.Implementation
{
    public class QRGenerator : IQRGenerator
    {
        public string GenerateQR(string url, int pixelsPerModule = 15, QRCodeGenerator.ECCLevel eccLevel = QRCodeGenerator.ECCLevel.L)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL cannot be empty", nameof(url));
            using var generator = new QRCodeGenerator();
            using QRCodeData data = generator.CreateQrCode(url, eccLevel);
            using var qrcode = new BitmapByteQRCode(data);
            return Convert.ToBase64String(qrcode.GetGraphic(pixelsPerModule));
        }
    }
}
