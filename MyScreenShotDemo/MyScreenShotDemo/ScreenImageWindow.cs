using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace MyScreenShotDemo
{
    using System.Diagnostics;

    public partial class ScreenImageWindow : Form
    {
        #region 属性
        /// <summary>
        /// 桌面背景图片
        /// </summary>
        private Image m_screenImage;
        /// <summary>
        /// 当前矩形的一个副本
        /// </summary>
        private Rectangle m_selectImageBounds;

        private Rectangle selectImageRect;
        //展示在窗体上的矩形
        public Rectangle SelectImageRect
        {
            get
            {
                return selectImageRect;
            }
            set
            {
                selectImageRect = value;
                if (!selectImageRect.IsEmpty)
                {
                    CalCulateSizeGripRect();
                    //使整个窗口客户区无效，此时就需要重绘，这个就会自动调用窗口类的OnPaint函数
                    base.Invalidate();
                }
            }
        }

        private bool selectedImage;
        /// <summary>
        /// 是否已画出矩形
        /// </summary>
        public bool SelectedImage
        {
            get { return selectedImage; }
            set { selectedImage = value; }
        }

        private Dictionary<SizeGrip, Rectangle> sizeGripRectList;

        public Dictionary<SizeGrip, Rectangle> SizeGripRectList
        {
            get
            {
                if (sizeGripRectList == null)
                    sizeGripRectList = new Dictionary<SizeGrip, Rectangle>();
                return sizeGripRectList;
            }
        }

        private SizeGrip sizeGrip;
        public SizeGrip SizeGrip
        {
            get
            {
                return sizeGrip;
            }
            set
            {
                sizeGrip = value;
            }
        }

        private bool mouseDown;
        private Point mouseDownPoint;
        private Point endPoint;

        private static readonly Font TextFont =
           new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
        
        #endregion

        #region
        public ScreenImageWindow()
        {
            InitializeComponent();
            Init();
        }

        #endregion

        #region 初始化
        private void Init()
        {

            SetStyle(
                ControlStyles.UserPaint | 
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer, true);

            TopMost = true;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;

            Bounds = Screen.GetBounds(this);
            //Bounds = new Rectangle(200, 200, 500, 500);
            m_screenImage = this.GetScreenImage();

            Image backScreen = new Bitmap(m_screenImage);

            Graphics g = Graphics.FromImage(backScreen);
            SolidBrush mask = new SolidBrush(Color.FromArgb(100, 0, 0, 0));
            g.FillRectangle(mask, 0, 0, backScreen.Width, backScreen.Height);
            g.Dispose();
            BackgroundImage = backScreen;

        }
        #endregion

        #region Override方法

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            Cursor = Cursors.Default;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                if (SelectedImage)
                {
                    if (SizeGrip != SizeGrip.None)
                    {
                        mouseDown = true;
                        mouseDownPoint = e.Location;
                        
                    }
                    if (selectImageRect.Contains(e.Location))
                    {
                        mouseDown = true;
                        mouseDownPoint = e.Location;
                        ClipCursor(true);
                    }
                }
                else
                {
                    mouseDown = true;
                    mouseDownPoint = e.Location;
                }
            }
            else
            {
                SaveImage();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (mouseDown)
            {
                if (!SelectedImage)
                {
                    SelectImageRect = GetSelectImageRect(e.Location);
                }
                else
                {
                    if (SizeGrip != SizeGrip.None)
                    {
                        ChangeSelctImageRect(e.Location);
                    }
                }
            }
            else
            {
                if (!SelectedImage)
                {
                }
                else
                {
                    SetSizeGrip(e.Location);
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left)
            {
                if (!SelectedImage)
                {
                    SelectImageRect = GetSelectImageRect(e.Location);
                    if (!SelectImageRect.IsEmpty)
                    {
                        SelectedImage = true;
                    }
                }
                else
                {
                    endPoint = e.Location;
                    base.Invalidate();
                    if (SizeGrip != SizeGrip.None)
                    {
                        m_selectImageBounds = SelectImageRect;
                        SizeGrip = SizeGrip.None;
                    }
                }
                mouseDown = false;
                mouseDownPoint = Point.Empty;
            }
            else if (e.Button == MouseButtons.Right)
            {
                
            }

        }

        protected override void OnPaint(PaintEventArgs e)
        {            
            base.OnPaint(e);
            Graphics g = e.Graphics;
            if (!selectImageRect.IsEmpty)
            {
                //显示原色
                g.DrawImage(m_screenImage, selectImageRect, selectImageRect, GraphicsUnit.Pixel);
            }
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (SelectImageRect.Width != 0 && SelectImageRect.Height != 0)
            {
                Rectangle rect = SelectImageRect;
                if (mouseDown)
                {
                    if (!SelectedImage || SizeGrip != SizeGrip.None)
                    {
                        //using (SolidBrush brush = new SolidBrush(Color.FromArgb(95, Color.LightBlue)))
                        //{
                        //    g.FillRectangle(brush, rect);
                        //}
                        DrawImageSizeInfo(g, rect);
                    }
                }

                using (Pen pen = new Pen(Color.Blue))
                {
                    g.DrawRectangle(pen, rect);
                    using (SolidBrush brush = new SolidBrush(Color.Blue))
                    {
                        foreach (var rectangle in SizeGripRectList.Values)
                        {
                            g.FillRectangle(brush, rectangle);
                        }
                    }
                }
            }                        
        }

        #endregion

        #region 得到屏幕图片
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
        #endregion

        #region 矩形边的六个小角
        private void CalCulateSizeGripRect()
        {
            Rectangle rect = SelectImageRect;

            int x = rect.X;
            int y = rect.Y;
            int centerX = x + rect.Width / 2;
            int centerY = y + rect.Height / 2;

            Dictionary<SizeGrip, Rectangle> list = SizeGripRectList;

            list.Clear();

            list.Add(
                SizeGrip.TopLeft,
                new Rectangle(x - 2, y - 2, 5, 5));
            list.Add(
                SizeGrip.TopRight,
                new Rectangle(rect.Right - 2, y - 2, 5, 5));
            list.Add(
                SizeGrip.BottomLeft,
                new Rectangle(x - 2, rect.Bottom - 2, 5, 5));
            list.Add(
                SizeGrip.BottomRight,
                new Rectangle(rect.Right - 2, rect.Bottom - 2, 5, 5));
            list.Add(
                SizeGrip.Top,
                new Rectangle(centerX - 2, y - 2, 5, 5));
            list.Add(
                SizeGrip.Bottom,
                new Rectangle(centerX - 2, rect.Bottom - 2, 5, 5));
            list.Add(
                SizeGrip.Left,
                new Rectangle(x - 2, centerY - 2, 5, 5));
            list.Add(
                SizeGrip.Right,
                new Rectangle(rect.Right - 2, centerY - 2, 5, 5));
        }
        #endregion

        #region 改变已经画好的矩形的大小

        /// <summary>
        /// 改变已经画好的矩形的大小
        /// </summary>
        /// <param name="point"></param>
        private void ChangeSelctImageRect(Point point)
        {
            Rectangle rect = m_selectImageBounds;

            int left = rect.Left;
            int top = rect.Top;
            int right = rect.Right;
            int bottom = rect.Bottom;
            bool sizeGripAll = false;
            switch (SizeGrip)
            {
                case SizeGrip.All:
                    rect.Offset(point.X - mouseDownPoint.X, point.Y - mouseDownPoint.Y);
                    sizeGripAll = true;
                    break;
                case SizeGrip.TopLeft:
                    left = point.X;
                    top = point.Y;
                    break;
                case SizeGrip.TopRight:
                    right = point.X;
                    top = point.Y;
                    break;
                case SizeGrip.BottomLeft:
                    left = point.X;
                    bottom = point.Y;
                    break;
                case SizeGrip.BottomRight:
                    right = point.X;
                    bottom = point.Y;
                    break;
                case SizeGrip.Top:
                    top = point.Y;
                    break;
                case SizeGrip.Bottom:
                    bottom = point.Y;
                    break;
                case SizeGrip.Left:
                    left = point.X;
                    break;
                case SizeGrip.Right:
                    right = point.X;
                    break;
            }
            if (!sizeGripAll)
            {
                rect.X = left;
                rect.Y = top;
                rect.Width = right - left;
                rect.Height = bottom - top;
            }
            #region move scope
            if (rect.X < 0)
            {
                rect.X = 0;
            }
            if (rect.Y < 0)
            {
                rect.Y = 0;
            }
            if (rect.Right > this.Width)
            {
                rect.X = this.Right - rect.Width;
            }
            if (rect.Bottom > this.Height)
            {
                rect.Y = this.Height - rect.Height;
            }
            #endregion
            mouseDownPoint = point;
            //把计算好的range 赋值给selectImageBounds
            m_selectImageBounds = rect;
            
            SelectImageRect = ImageBoundsToRect(rect);        
            Debug.WriteLine("change");
        }
        #endregion

        #region 按键事件
        private void ScreenImageWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode==Keys.Escape)
                this.Close();
        }
        #endregion

        #region 鼠标可移动范围
        /// <summary>
        /// 鼠标可移动范围
        /// </summary>
        /// <param name="reset"></param>
        private void ClipCursor(bool reset)
        {
            Rectangle rect;
            if (reset)
            {
                rect = Screen.GetBounds(this);
            }
            else
            {
                rect = SelectImageRect;
            }

            MouseCanMoveRange.RECT nativeRect = new MouseCanMoveRange.RECT(rect);

            MouseCanMoveRange.ClipCursor(ref nativeRect);
        }
        #endregion

        #region 截图的矩形

        /// <summary>
        /// 根据点画出矩形
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        private Rectangle GetSelectImageRect(Point endPoint)
        {
            m_selectImageBounds = Rectangle.FromLTRB(mouseDownPoint.X,
                mouseDownPoint.Y,
                endPoint.X, endPoint.Y);

            return ImageBoundsToRect(m_selectImageBounds);
        }

        /// <summary>
        /// 把矩形的坐标转换成正确
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns></returns>
        private Rectangle ImageBoundsToRect(Rectangle bounds)
        {
            Rectangle rect = bounds;
            int x = 0, y = 0;
            x = Math.Min(rect.X, rect.Right);
            y = Math.Min(rect.Y, rect.Bottom);

            rect.X = x;
            rect.Y = y;

            rect.Width = Math.Max(1, Math.Abs(rect.Width));
            rect.Height = Math.Max(1, Math.Abs(rect.Height));
            return rect;
        }
        #endregion

        #region 鼠标移动到画好的矩形上的样式
        /// <summary>
        /// 鼠标移动到画好的矩形上的样式
        /// </summary>
        /// <param name="point"></param>
        private void SetSizeGrip(Point point)
        {
            SizeGrip = SizeGrip.None;
            foreach (SizeGrip sizeGrip in SizeGripRectList.Keys)
            {
                if (sizeGripRectList[sizeGrip].Contains(point))
                {
                    SizeGrip = sizeGrip;
                    break;
                }
            }

            if (SizeGrip == SizeGrip.None)
            {
                if (selectImageRect.Contains(point))
                {
                    SizeGrip = SizeGrip.All;
                }
            }

            switch (SizeGrip)
            {
                case SizeGrip.TopLeft:
                case SizeGrip.BottomRight:
                    Cursor = Cursors.SizeNWSE;
                    break;
                case SizeGrip.TopRight:
                case SizeGrip.BottomLeft:
                    Cursor = Cursors.SizeNESW;
                    break;
                case SizeGrip.Top:
                case SizeGrip.Bottom:
                    Cursor = Cursors.SizeNS;
                    break;
                case SizeGrip.Left:
                case SizeGrip.Right:
                    Cursor = Cursors.SizeWE;
                    break;
                case SizeGrip.All:
                    Cursor = Cursors.SizeAll;
                    break;
                default:
                    Cursor = Cursors.Default;
                    break;
            }
        }
        #endregion

        #region 截图矩形的大小
        /// <summary>
        /// 截图矩形的大小信息
        /// </summary>
        /// <param name="g"></param>
        /// <param name="rect"></param>
        private void DrawImageSizeInfo(Graphics g, Rectangle rect)
        {
            string text = string.Format(
                            "{0}x{1}",
                            rect.Width,
                            rect.Height);
            Size textSize = TextRenderer.MeasureText(text, TextFont);
            Rectangle screenBounds = Screen.GetBounds(this);
            int x = 0;
            int y = 0;
            if (rect.X + textSize.Width > screenBounds.Right - 3)
            {
                x = screenBounds.Right - textSize.Width - 3;
            }
            else
            {
                x = rect.X + 2;
            }

            if (rect.Y - textSize.Width < screenBounds.Y + 3)
            {
                y = rect.Y + 2;
            }
            else
            {
                y = rect.Y - textSize.Height - 2;
            }
            Rectangle textrect = new Rectangle(x, y, textSize.Width, textSize.Height);
            g.FillRectangle(Brushes.Black, textrect);
            TextRenderer.DrawText(g, text, TextFont, textrect, Color.White);
        }
        #endregion

        #region 保存
        /// <summary>
        /// 保存
        /// </summary>
        public void SaveImage()
        {
            if (SelectedImage)
            {
                SaveFileDialog saveFile=new SaveFileDialog();
                saveFile.Filter =
                    @"BMP 文件(*.bmp)|*.bmp|JPEG 文件(*.jpg,*.jpeg)|*.jpg,*.jpeg|PNG 文件(*.png)|*.png|GIF 文件(*.gif)|*.gif";
                saveFile.DefaultExt = @"bmp";
                if (saveFile.ShowDialog() == DialogResult.OK)
                {
                    Image img = DrawImage();
                    ImageFormat imageFormat = ImageFormat.Bmp;
                    string fileName = saveFile.FileName;
                    int index = fileName.LastIndexOf('.');
                    string extion = fileName.Substring(index + 1, fileName.Length - index - 1);

                    switch (extion)
                    {
                        case "jpg":
                        case "jpeg":
                            imageFormat = ImageFormat.Jpeg;
                            break;
                        case "png":
                            imageFormat = ImageFormat.Png;
                            break;
                        case "gif":
                            imageFormat = ImageFormat.Gif;
                            break;
                    }
                    img.Save(saveFile.FileName, imageFormat);
                    this.Close();
                }
            }
        }

        private Image DrawImage()
        {
            Image image;
            using (Bitmap allbmp = new Bitmap(Width, Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics allgs = Graphics.FromImage(allbmp))
                {
                    allgs.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    allgs.SmoothingMode = SmoothingMode.AntiAlias;
                    allgs.DrawImage(m_screenImage, Point.Empty);
                    allgs.Flush();

                    Bitmap bmp = new Bitmap(selectImageRect.Width, selectImageRect.Height, PixelFormat.Format32bppArgb);

                    Graphics g = Graphics.FromImage(bmp);
                    g.DrawImage(allbmp, 0, 0, selectImageRect, GraphicsUnit.Pixel);
                    g.Flush();
                    g.Dispose();
                    image = bmp;
                }
            }
            return image;
        }
        #endregion

        #region 清空
        // 清空
        private void ResetSelectImage()
        {
            SelectedImage = false;
            m_selectImageBounds = Rectangle.Empty;
            SelectImageRect = Rectangle.Empty;
            SizeGrip = SizeGrip.None;                        
            base.Invalidate();
        }
        #endregion

    }

    
}
