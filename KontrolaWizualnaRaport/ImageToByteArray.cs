using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualBasic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport
{
    class ImageToByteArray
    {
        public static void ImgToByteArray()
        {
            // MemoryStream

            // Hex function. Also add as a resource to the project.

            //-----------------------------

            StringBuilder sb = new StringBuilder();

            Image image = Image.FromFile(@"C:\image-selectGR.png");
            MemoryStream ms = new MemoryStream();
            image.Save(ms, ImageFormat.Bmp);

            byte[] byteArray = ms.ToArray();

            for (int idx = 0; idx < byteArray.Length; idx++)
            {
                // After writing 16 values, write a newline.
                if (idx % 15 == 0)
                {
                    sb.Append("\n");
                }

                // Prepend a "0x" before each hex value.
                sb.Append("0x");

                // If the hex value is a single digit, prepend a "0"
                if (byteArray[idx] < 16)
                {
                    sb.Append("0");
                }

                // Use the Visual Basic Hex function to convert the byte.
                sb.Append(Conversion.Hex(byteArray[idx]));
                sb.Append(", ");
            }

            Clipboard.SetText(sb.ToString());
        }

    }
}
