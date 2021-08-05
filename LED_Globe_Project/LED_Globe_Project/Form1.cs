using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LED_Globe_Project
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button5.BackColor = colorDialog1.Color;
            openFileDialog1.Title = "Ikelti Paveiksleli";
        }
        // Ijungti Varikli
        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.AppendText("[ON] Variklis Ijungtas !\r\n");
        }

        // Isjungti Varikli
        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.AppendText("[OFF] Variklis Išjungtas !\r\n");
        }

        // Pasirinkti Spalva
        private void button5_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
                button5.BackColor = colorDialog1.Color;
        }

        // Atvaizduoti Spalva
        private void button4_Click(object sender, EventArgs e)
        {
            textBox1.AppendText(string.Format("[COLOR] Atvaizduojama spalva R: {0}  G: {1}  B: {2} \r\n",
                button5.BackColor.R, button5.BackColor.G, button5.BackColor.B));
        }

        // Atvaizduoti Teksta
        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.AppendText(string.Format("[TEXT] Atvaizduojama tekstas : \"{0}\"  \r\n",
                textBox2.Text));
        }

        // Atvaizduoti Paveiksleli
        private void button6_Click(object sender, EventArgs e)
        {
            textBox1.AppendText(string.Format("[IMAGE] Atvaizduojamas paveikslelis \"{0}\"  \r\n",
                openFileDialog1.SafeFileName));
        }

        // Pasirinkti Paveiksleli
        private void button7_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var uploadedImage = Image.FromStream(openFileDialog1.OpenFile());
                var resizedImage = resizeImage(360, 70, uploadedImage);
                pictureBox1.Image = resizedImage;
            }

        }

        public Image resizeImage(int newWidth, int newHeight, Image img)
        {
            Image imgPhoto = img;

            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;

            //Consider vertical pics
            if (sourceWidth < sourceHeight)
            {
                int buff = newWidth;

                newWidth = newHeight;
                newHeight = buff;
            }

            int sourceX = 0, sourceY = 0, destX = 0, destY = 0;
            float nPercent = 0, nPercentW = 0, nPercentH = 0;

            nPercentW = ((float)newWidth / (float)sourceWidth);
            nPercentH = ((float)newHeight / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((newWidth -
                          (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((newHeight -
                          (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);


            Bitmap bmPhoto = new Bitmap(newWidth, newHeight,
                          PixelFormat.Format24bppRgb);

            bmPhoto.SetResolution(newWidth,
                         newHeight);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.Black);
            grPhoto.InterpolationMode =
                System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            imgPhoto.Dispose();
            bmPhoto.Save("LOWER_RES.jpg");
            return bmPhoto;
        }
    }
}
