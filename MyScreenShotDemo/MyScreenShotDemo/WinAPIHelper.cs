using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MyScreenShotDemo
{
    public class WinAPIHelper
    {
        /// <summary>
        /// 得到window桌面
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        /// <summary>
        /// 指定窗口的客户区域或整个屏幕的显示设备上下文环境的句柄
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr ptr);
    }
}
