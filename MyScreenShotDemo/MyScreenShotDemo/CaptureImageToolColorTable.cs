using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MyScreenShotDemo
{
    /// <summary>
    /// 颜色
    /// </summary>
    public class CaptureImageToolColorTable
    {
        private static readonly Color borderColor = Color.FromArgb(65, 173, 236);
        private static readonly Color backColorNormal = Color.FromArgb(229, 243, 251);
        private static readonly Color backColorHover = Color.FromArgb(65, 173, 236);
        private static readonly Color backColorPressed = Color.FromArgb(24, 142, 206);
        private static readonly Color foreColor = Color.FromArgb(12, 83, 124);

        public CaptureImageToolColorTable() { }

        public virtual Color BorderColor
        {
            get { return borderColor; }
        }

        public virtual Color BackColorNormal
        {
            get { return backColorNormal; }
        }

        public virtual Color BackColorHover
        {
            get { return backColorHover; }
        }

        public virtual Color BackColorPressed
        {
            get { return backColorPressed; }
        }

        public virtual Color ForeColor
        {
            get { return foreColor; }
        }
    }
}
