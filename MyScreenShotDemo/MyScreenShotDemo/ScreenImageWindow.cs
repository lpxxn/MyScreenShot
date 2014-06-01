using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace MyScreenShotDemo
{
    public partial class ScreenImageWindow : Form
    {
        public ScreenImageWindow()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {

            SetStyle(
                ControlStyles.UserPaint | 
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer, true);
            textBox.Visible = false;

            TopMost = true;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;

            Bounds = Screen.GetBounds(this);
            //Bounds = new Rectangle(20, 20, 400, 400);

            BackgroundImage = GetScreenImage();

        }

        /// <summary>
        /// 得到屏幕图片
        /// </summary>
        /// <returns></returns>
        private Image GetScreenImage()
        {
            Rectangle rect = Screen.GetBounds(this);

            Bitmap bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);

            IntPtr gHdc = g.GetHdc();

            IntPtr winScreen = WinAPIHelper.GetDesktopWindow();

            IntPtr dHdc = WinAPIHelper.GetDC(winScreen);

            BitBltHelper.BitBlt(gHdc, 0, 0, Width, Height, dHdc, 0, 0, BitBltHelper.TernaryRasterOperations.SRCCOPY);
            g.ReleaseHdc(gHdc);
            return bmp;

        }

        private void ScreenImageWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode==Keys.Escape)
                this.Close();
        }
    }

    
}
