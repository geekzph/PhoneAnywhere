using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using System.Drawing.Imaging;

namespace WindowsFormsApplication1
{
    public partial class campic : Form
    {
        public campic()
        {
            InitializeComponent();
        }

        private static ImageCodecInfo GetCodecInfo(string mimeType)
        {
            ImageCodecInfo[] CodecInfo = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo ici in CodecInfo)
            {
                if (ici.MimeType == mimeType) return ici;
            }
            return null;
        }
        private void campic_Load(object sender, EventArgs e)
        {
            try
            {
                string campath;
                campath = "c:";
                cVideo video = new cVideo(pictureBox1.Handle, pictureBox1.Width, pictureBox1.Height);
                video.StartWebCam();
                //campath = campath + @"\a.bmp";
                video.GrabImage(pictureBox1.Handle, campath + @"\a.bmp");
                video.CloseWebcam();
                //cameranum = cameranum + 1;
                //=======================================================================================
                Bitmap myBitmap;  //建立位图          
                ImageCodecInfo myImageCodecInfo;
                System.Drawing.Imaging.Encoder myEncoder;//这个是重点类, 
                EncoderParameter myEncoderParameter;
                EncoderParameters myEncoderParameters;
                myBitmap = new Bitmap(campath + @"\a.bmp");
                //请注意这里的myImageCodecInfo声名..可以修改为更通用的.看后面 
                myImageCodecInfo = GetCodecInfo("image/jpeg");
                myEncoder = System.Drawing.Imaging.Encoder.Quality;
                myEncoderParameters = new EncoderParameters(1);
                // 在这里设置图片的质量等级为95L. 
                long level = 95;
                myEncoderParameter = new EncoderParameter(myEncoder, level);
                myEncoderParameters.Param[0] = myEncoderParameter;//将构建出来的EncoderParameter类赋给EncoderParameters数组 
                myBitmap.Save(campath + @"\a.jpg", myImageCodecInfo, myEncoderParameters);//保存图片
                myEncoderParameter.Dispose();
                myEncoderParameters.Dispose();
                myBitmap.Dispose();
                this.Close();
                //Application.Exit();

            }
            catch { this.Close(); }

        }
    }
}
