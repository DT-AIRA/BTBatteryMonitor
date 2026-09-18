using System;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace BTBatteryMonitor.Services
{
    public static class ColorPickerHelper
    {
        [DllImport("comdlg32.dll", SetLastError = true)]
        private static extern bool ChooseColor(ref CHOOSECOLOR lpcc);

        [StructLayout(LayoutKind.Sequential)]
        private struct CHOOSECOLOR
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            public int rgbResult;
            public IntPtr lpCustColors;
            public int Flags;
            public IntPtr lCustData;
            public IntPtr lpfnHook;
            public IntPtr lpTemplateName;
        }

        private const int CC_FULLOPEN = 0x00000002;
        private const int CC_RGBINIT = 0x00000001;
        private static readonly int[] CustomColors = new int[16];

        public static Color? ShowColorPicker(IntPtr ownerHwnd, Color currentColor)
        {
            int initColor = (currentColor.R) | (currentColor.G << 8) | (currentColor.B << 16);

            IntPtr pCustColors = Marshal.AllocHGlobal(16 * sizeof(int));
            try
            {
                Marshal.Copy(CustomColors, 0, pCustColors, 16);

                var cc = new CHOOSECOLOR
                {
                    lStructSize = Marshal.SizeOf<CHOOSECOLOR>(),
                    hwndOwner = ownerHwnd,
                    rgbResult = initColor,
                    lpCustColors = pCustColors,
                    Flags = CC_FULLOPEN | CC_RGBINIT
                };

                if (ChooseColor(ref cc))
                {
                    Marshal.Copy(pCustColors, CustomColors, 0, 16);
                    byte r = (byte)(cc.rgbResult & 0xFF);
                    byte g = (byte)((cc.rgbResult >> 8) & 0xFF);
                    byte b = (byte)((cc.rgbResult >> 16) & 0xFF);
                    return Color.FromRgb(r, g, b);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(pCustColors);
            }

            return null;
        }
    }
}
