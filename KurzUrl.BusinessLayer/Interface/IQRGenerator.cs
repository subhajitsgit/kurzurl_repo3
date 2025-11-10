using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurzUrl.BusinessLayer.Interface
{
    public interface IQRGenerator
    {
        string GenerateQR(string url, int pixelsPerModule = 15, QRCodeGenerator.ECCLevel eccLevel = QRCodeGenerator.ECCLevel.L);
    }
}
