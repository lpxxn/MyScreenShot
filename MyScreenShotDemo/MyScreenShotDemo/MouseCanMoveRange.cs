﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

namespace MyScreenShotDemo
{
    /// <summary>
    /// 限制鼠标的移动范围
    /// </summary>
    public class MouseCanMoveRange
    {
        [DllImport("user32.dll")]
        public static extern bool ClipCursor(ref RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public RECT(Rectangle rect)
            {
                Left = rect.Left;
                Top = rect.Top;
                Right = rect.Right;
                Bottom = rect.Bottom;
            }

            public Rectangle Rect
            {
                get
                {
                    return new Rectangle(Left, Top, Right - Left, Bottom - Top);
                }
            }

            public Size Size
            {
                get
                {
                    return new Size(Right - Left, Bottom - Top);
                }
            }

            public static RECT FromXXWH(int x, int y, int width, int height)
            {
                return new RECT(x, y, x + width, y + height);
            }

            public static RECT FromRectangle(Rectangle rect)
            {
                return new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
            }
        }
    }
}
