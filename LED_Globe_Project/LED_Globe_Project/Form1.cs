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
using System.Timers;
using System.Threading;
using System.IO.Ports;
using System.Diagnostics;

namespace LED_Globe_Project
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            getAvailableComPorts();

            foreach (string port in ports)
            {
                comboBox1.Items.Add(port);
                Console.WriteLine(port);
                if (ports[0] != null)
                {
                    comboBox1.SelectedItem = ports[0];
                }
            }
        }

        void getAvailableComPorts()
        {
            ports = SerialPort.GetPortNames();
        }

        private void connectToArduino()
        {
            isConnected = true;
            string selectedPort = comboBox1.GetItemText(comboBox1.SelectedItem);
            port = new SerialPort(selectedPort, 9600, Parity.None, 8, StopBits.One);
            port.Open();
        }

        private int DIRECTION;
        private bool HORIZONTAL;
        private String[] ports;
        private SerialPort port;
        private bool isConnected = false;

        private void Form1_Load(object sender, EventArgs e)
        {
            button5.BackColor = colorDialog1.Color;
            openFileDialog1.Title = "Ikelti Paveiksleli";
            DIRECTION = -1;
            HORIZONTAL = true;
        }
        // Ijungti Varikli
        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.AppendText("[ON] Variklis Ijungtas !\r\n");
            byte[] inline = new byte[3];
            inline[0] = (byte)'O';
            inline[1] = 0;
            inline[2] = (byte)'\n';
            if (isConnected) port.Write(inline, 0, 3);
        }

        // Isjungti Varikli
        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.AppendText("[OFF] Variklis Išjungtas !\r\n");
            byte[] inline = new byte[3];
            inline[0] = (byte)'F';
            inline[1] = 0;
            inline[2] = (byte)'\n';
            if (isConnected) port.Write(inline, 0, 3);
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
            pictureBox2.Image = simulatedProjection(solidFill(button5.BackColor), 3);
            pictureBox1.Image = simulatedProjection(solidFill(button5.BackColor), 1);
            realProjection(solidFill(button5.BackColor), (byte)'S', 0, 0);
        }

        // Atvaizduoti Teksta
        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.AppendText(string.Format("[TEXT] Atvaizduojama tekstas : \"{0}\"  \r\n",
                textBox2.Text));
            pictureBox2.Image = simulatedProjection(resizeImage(360,70,textAsImage(textBox2.Text)), 3);
            pictureBox1.Image = simulatedProjection(resizeImage(360, 70, textAsImage(textBox2.Text)), 1);
            realProjection(resizeImage(360, 70, textAsImage(textBox2.Text)), (byte)'S', 0, 0);

        }

        // Atvaizduoti Paveiksleli
        private void button6_Click(object sender, EventArgs e)
        {
            textBox1.AppendText(string.Format("[IMAGE] Atvaizduojamas paveikslelis \"{0}\"  \r\n",
                openFileDialog1.SafeFileName));
            pictureBox2.Image = simulatedProjection(pictureBox1.Image, 3);
            pictureBox1.Image = simulatedProjection(pictureBox1.Image, 1);
            realProjection(pictureBox1.Image, (byte)'S', 0, 0);

        }

        // Pasirinkti Paveiksleli
        private void button7_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var uploadedImage = Image.FromStream(openFileDialog1.OpenFile());
                var resizedImage = resizeImage(360, 70, uploadedImage);
                pictureBox1.Image = resizedImage;
                pictureBox2.Image = simulatedProjection(pictureBox1.Image, 3);
                //var resizedBMP = (Bitmap)resizedImage;
                //var arrayString = Bmp2String(resizedBMP);
                //textBox1.AppendText(arrayString);
            }

        }

        // Slenkanti Animacija
        private void button8_Click(object sender, EventArgs e)
        {
            realProjection(pictureBox1.Image, (byte)'A', 1, 1);
            // FastScroll (kartai, greitis(delay), pikseliu atskirtis(atvaizdavimui), horizontalus(BOOL), kryptisX(1 arba-1), image'as)
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                FastScroll(1, 3, 1, HORIZONTAL, DIRECTION, 1);

            }).Start();
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                FastScroll(1, 3, 3, HORIZONTAL, DIRECTION, 2);
            }).Start();
        }

        // Prisijungti
        private void button9_Click(object sender, EventArgs e)
        {
            connectToArduino();
        }

        // I virsu
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            HORIZONTAL = false;
            DIRECTION = 1;
        }
        // I apacia
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            HORIZONTAL = false;
            DIRECTION = -1;
        }
        // I kaire
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            HORIZONTAL = true;
            DIRECTION = 1;
        }
        // I desine
        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            HORIZONTAL = true;
            DIRECTION = -1;
        }

        private int[,] Bmp2Array (Bitmap bmp)
        {
            int[,] arr = new int[bmp.Height, bmp.Width];
            for (int x = 0; x < bmp.Height; x++)
            {
                for (int y = 0; y < bmp.Width; y++)
                {
                    arr[x,y] = bmp.GetPixel(y, x).ToArgb();
                }
            }
            return arr;
        }

        private string Bmp2String(Bitmap bmp)
        {
            string arr = "";
            for (int x = 0; x < bmp.Height; x++)
            {
                for (int y = 0; y < bmp.Width; y++)
                {
                    arr += string.Format("[{0}]",bmp.GetPixel(y, x).ToArgb());
                }
                arr += "\r\n";
            }
            return arr;
        }

        private void realProjection(Image original,byte command, byte param1, byte param2)
        {
            int spacing = 1;
            var image = original;
            var bmImage = (Bitmap)image;
            Bitmap bm = new Bitmap(image.Width * spacing, image.Height * spacing);

            int ARRAY_SIZE = (bm.Width * bm.Height * 3) + 1 + 2 + 1;

            byte[] inline = new byte[ARRAY_SIZE];

            inline[0] = command;
            inline[1] = param1;
            inline[2] = param2;

            for (int x = 0; x < bm.Width; x++)
            {
                for (int y = 0; y < bm.Height*3; y+=3)
                {
                    inline[3 + (x * bm.Height * 3) + y] = bmImage.GetPixel(x, y/3).R;
                    inline[3 + (x * bm.Height * 3) + y + 1] = bmImage.GetPixel(x, y/3).G;
                    inline[3 + (x * bm.Height * 3) + y + 2] = bmImage.GetPixel(x, y/3).B;
                }
            }

            inline[ARRAY_SIZE-1] = (byte)'\n';
            if(isConnected) port.Write(inline, 0, ARRAY_SIZE);            
            //string s = BitConverter.ToString(inline);
            //textBox1.AppendText(s);
        }

        private Image simulatedProjection(Image original, int spacing)
        {
            if (original == null) return null;
            var image = original;
            var bmImage = (Bitmap)image;
            Bitmap bm = new Bitmap(image.Width * spacing, image.Height * spacing);
            //int[,] arr = new int[bm.Width, bm.Height];
           // byte[] inline = new byte[];
            for (int x = 0; x < bm.Width; x++)
            {
                for (int y = 0; y < bm.Height; y++)
                {
                    if (x % spacing == 0 && y % spacing == 0)
                    {
                        bm.SetPixel(x, y, bmImage.GetPixel(x / spacing, y / spacing));
                        //if (spacing == 1) arr[x, y] = bm.GetPixel(x, y).ToArgb();
                    }
                }
            }
            
            //if(isConnected) port.Write(string.Format("D{0}\n",inline));
            //port.Write()
            return bm;
        }

        public Image solidFill(Color color)
        {
            Bitmap bm = new Bitmap(360, 70);
            for (int x = 0; x < bm.Width; x++)
            {
                for (int y = 0; y < bm.Height; y++)
                {
                    bm.SetPixel(x, y, color);
                }
            }
            return bm;
        }

        public void FastScroll(int n, int interval, int spacing, bool isHorizontal, int direction, int pictureBox)
        {
            for (int time = 0; time < n; time++)
            {
                var originalMain = pictureBox == 2 ? (Bitmap)pictureBox2.Image : (Bitmap)pictureBox1.Image;
                var extended = isHorizontal ? new Bitmap(originalMain.Width*2, originalMain.Height) : new Bitmap(originalMain.Width, originalMain.Height * 2);
                using (TextureBrush brush = new TextureBrush(originalMain, WrapMode.Tile))
                using (Graphics g = Graphics.FromImage(extended))
                {
                    // Do your painting in here
                    g.FillRectangle(brush, 0, 0, extended.Width, extended.Height);
                }
                var distance = isHorizontal ? originalMain.Width : originalMain.Height;
                for (int x = 0; x < distance/spacing; x++)
                {
                    Bitmap bm = (Bitmap)scrollCanvas(extended, spacing, isHorizontal, direction);
                    var directionXY = direction < 0 ? originalMain.Height : 0;
                    directionXY = isHorizontal ? originalMain.Width : directionXY;
                    directionXY = isHorizontal == true && direction > 0 ? 0 : directionXY;
                    var output = isHorizontal ? bm.Clone(new Rectangle(directionXY, 0, originalMain.Width, originalMain.Height), PixelFormat.Format32bppArgb) :
                        bm.Clone(new Rectangle(0, directionXY, originalMain.Width, originalMain.Height), PixelFormat.Format32bppArgb);
                    if(pictureBox == 2)
                    {
                        ThreadHelperClass.SetImage(this, pictureBox2, output);
                        ThreadHelperClass.Refresh(this, pictureBox2, output);
                    }
                    else
                    {
                        ThreadHelperClass.SetImage(this, pictureBox1, output);
                        ThreadHelperClass.Refresh(this, pictureBox1, output);

                    }
                    
                    System.Threading.Thread.Sleep(interval);
                    extended = bm;
                }
            }
        }



        private Image scrollCanvas(Image _bitmap1,int amountPixels,bool isHorizontal, int direction)
        {
            Rectangle source, dest;
            Bitmap bm = (Bitmap)_bitmap1;
            int w, h, pw, ph;

            w = _bitmap1.Width;
            h = _bitmap1.Height;

            amountPixels *= direction;
            if (isHorizontal)
            {
                if (amountPixels < 0)
                {
                    source = new Rectangle(0, 0, w + amountPixels, h);
                    dest = new Rectangle(-amountPixels, 0, w + amountPixels, h);
                }
                else
                {
                    source = new Rectangle(+amountPixels,0 , w - amountPixels, h );
                    dest = new Rectangle(0, 0, w - amountPixels, h);
                }
            }
            else
            {
                if (amountPixels < 0)
                {
                    source = new Rectangle(0, 0, w, h + amountPixels);
                    dest = new Rectangle(0, -amountPixels, w, h + amountPixels);
                }
                else
                {
                    source = new Rectangle(0, +amountPixels, w, h - amountPixels);
                    dest = new Rectangle(0, 0, w, h - amountPixels);
                }
            }

            using (Graphics g = Graphics.FromImage(bm))
            {
                // Do your painting in here
                g.DrawImage(_bitmap1, dest, source, GraphicsUnit.Pixel);
            }
            return bm;
        }

        public Image textAsImage(String text)
        {
            Font font = new Font("Arial", 16);
            SolidBrush brush = new SolidBrush(Color.Red);
            Point point = new Point(0, 20);
            var bmp = new Bitmap(360, 70);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.DrawString(text,font,brush,point);
            }

            return bmp;
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

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }

    public static class ThreadHelperClass
    {
        delegate void SetImageCallback(Form f, PictureBox ctrl, Image img);
        /// <summary>
        /// Set text property of various controls
        /// </summary>
        /// <param name="form">The calling form</param>
        /// <param name="ctrl"></param>
        /// <param name="text"></param>
        public static void SetImage(Form form, PictureBox ctrl, Image img)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (ctrl.InvokeRequired)
            {
                SetImageCallback d = new SetImageCallback(SetImage);
                form.Invoke(d, new object[] { form, ctrl, img });
            }
            else
            {
                ctrl.Image = img;
            }
        }

        public static void Refresh(Form form, PictureBox ctrl, Image img)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (ctrl.InvokeRequired)
            {
                SetImageCallback d = new SetImageCallback(SetImage);
                form.Invoke(d, new object[] { form, ctrl, img});
            }
            else
            {
                ctrl.Refresh();
            }
        }
    }
}
