using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using System.IO;

namespace Web_camera
{
    public partial class MakeScreen : Form
    {
        private Image<Bgr,byte> inputimage= null;
       
        private string fileName = string.Empty;
        public MakeScreen(Image<Bgr,byte> image)
        {
            this.inputimage = image;
            InitializeComponent();
        }

        private void MakeScreen_Load(object sender, EventArgs e)
        {
            fileName = $"Web camera_{DateTime.Now.Day},{DateTime.Now.Month},{DateTime.Now.Year},{DateTime.Now.Minute},{DateTime.Now.Second}.jpeg";
            pictureBox1.Image = inputimage.AsBitmap<Bgr,byte>();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            try
            {
                pictureBox1.Image.Save(fileName, ImageFormat.Jpeg);
                if (File.Exists(fileName))
                    MessageBox.Show("Файл сохранен!");
                else
                    throw new Exception("Не удалось сохранить изображение!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            DialogResult res = openFileDialog1.ShowDialog();
            if (res == DialogResult.OK)
            {
                inputimage = new Image<Bgr, byte>(openFileDialog1.FileName);
                pictureBox1.Image = inputimage.AsBitmap<Bgr, byte>();
            }
            else
                MessageBox.Show("Изображение не выбрано!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            Image<Bgr, byte> new_inputimage = inputimage;
            Image<Gray, byte> grayImage = new_inputimage.SmoothGaussian(5).Convert<Gray,byte>().ThresholdBinaryInv(new Gray(230),new Gray(255));
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hierarchy = new Mat();
            CvInvoke.FindContours(grayImage, contours, hierarchy, Emgu.CV.CvEnum.RetrType.External,Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            for(int i =0;i<contours.Size;i++)
            {
                double perimetr = CvInvoke.ArcLength(contours[i], true);
                VectorOfPoint approximation = new VectorOfPoint();
                CvInvoke.ApproxPolyDP(contours[i], approximation, 0.04 * perimetr, true);
                /*CvInvoke.DrawContours(new_inputimage, contours, i, new MCvScalar(0, 0, 255), 2);*/
                Moments moments = CvInvoke.Moments(contours[i]);
                int x = (int)(moments.M10/moments.M00);
                int y = (int)(moments.M01 / moments.M00);
                if (perimetr > 100)
                {
                    if (radioButton1.Checked)
                    {
                        if (approximation.Size == 3)
                        {
                            CvInvoke.PutText(new_inputimage, "Triangle", new System.Drawing.Point(x, y), Emgu.CV.CvEnum.FontFace.HersheyPlain, 2, new MCvScalar(200, 0, 0), 1);
                        }

                    }
                    if (radioButton2.Checked)
                    {
                        if (approximation.Size == 4)
                        {
                            Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);
                            double aspectRatio = (double)rect.Width / (double)rect.Height;
                            if (aspectRatio >= 0.95 && aspectRatio <= 1.05)
                            {
                                CvInvoke.PutText(new_inputimage, "Square", new System.Drawing.Point(x, y), Emgu.CV.CvEnum.FontFace.HersheyPlain, 2, new MCvScalar(200, 0, 0), 1);
                            }
                            else
                            {
                                CvInvoke.PutText(new_inputimage, "Rectangle", new System.Drawing.Point(x, y), Emgu.CV.CvEnum.FontFace.HersheyPlain, 2, new MCvScalar(200, 0, 0), 1);
                            }
                        }

                    }
                    if (radioButton3.Checked)
                    {
                        if (approximation.Size == 5)
                        {
                            CvInvoke.PutText(new_inputimage, "Pentagon", new System.Drawing.Point(x, y), Emgu.CV.CvEnum.FontFace.HersheyPlain, 2, new MCvScalar(200, 0, 0), 1);
                        }

                    }
                    if (radioButton4.Checked)
                    {
                        if (approximation.Size > 6)
                        {
                            CvInvoke.PutText(new_inputimage, "Circle", new System.Drawing.Point(x, y), Emgu.CV.CvEnum.FontFace.HersheyPlain, 2, new MCvScalar(200, 0, 0), 1);
                        }

                    }
                }
                pictureBox2.Image = new_inputimage.AsBitmap<Bgr, byte>();
                
            }
            
        }
    }
}
