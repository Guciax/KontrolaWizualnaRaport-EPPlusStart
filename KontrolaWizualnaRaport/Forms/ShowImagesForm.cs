using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport.Forms
{
    public partial class ShowImagesForm : Form
    {
        private readonly List<FileInfo> fileList;
        private readonly string header;
        PictureBox previousPBox;

        public ShowImagesForm(List<FileInfo> fileList, string header)
        {
            InitializeComponent();
            this.fileList = fileList;
            this.header = header;
        }

        private void ShowImagesForm_Load(object sender, EventArgs e)
        {
            foreach (var file in fileList)
            {
                PictureBox picBox = new PictureBox();
                Image img = Bitmap.FromFile(file.FullName);
                picBox.Image = img;
                picBox.SizeMode = PictureBoxSizeMode.Zoom;
                picBox.MouseClick += PicBox_MouseClick;

                flowLayoutPanel1.Controls.Add(picBox);
                this.Text = header;
            }

            if (flowLayoutPanel1.Controls.Count>0)
            {
                PictureBox firstBox = (PictureBox)flowLayoutPanel1.Controls[0];
                pictureBox1.Image = firstBox.Image;
                previousPBox = firstBox;
                firstBox.BorderStyle = BorderStyle.FixedSingle;
            }
        }

        
        private void PicBox_MouseClick(object sender, MouseEventArgs e)
        {
            PictureBox picBox = (PictureBox)sender;
            pictureBox1.Image = picBox.Image;
            picBox.BorderStyle = BorderStyle.FixedSingle;

            if (previousPBox!=null)
            {
                previousPBox.BorderStyle = BorderStyle.None;
            }
            previousPBox = picBox;
        }
    }
}
