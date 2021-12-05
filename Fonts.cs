using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public sealed class Fonts
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont,
            IntPtr pdv, [System.Runtime.InteropServices.In] ref uint pcFonts);

        public static PrivateFontCollection fonts = new PrivateFontCollection();

        public static Font font_1;
    }
}
