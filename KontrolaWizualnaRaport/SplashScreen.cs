using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport
{
    public class SplashScreen
    {
        static Label lbl = new Label();
        public static void SetUpPanel(ref Panel panel)
        {
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Color.FromArgb(255, 66, 73, 76);
            lbl = new Label
            {
                Text = "",
                Dock = DockStyle.Bottom,
                Font = new Font("Arial", 12),
                ForeColor = Color.LightGray,
                AutoSize = false,
                TextAlign = ContentAlignment.BottomLeft
            };
            panel.Controls.Add(lbl);
            panel.Controls.Add(new PictureBox
            {
                Image = KontrolaWizualnaRaport.Properties.Resources.splashScreenLoader,
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.CenterImage
            });
        }

        public static void ReportChange(string message)
        {
            lbl.Text = message;
        }
    }
}
