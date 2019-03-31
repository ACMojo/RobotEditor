using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace RobotEditor.Helper
{
    public static class ColorGradient
    {
        #region Fields

        private static readonly byte _alpha = 0xff;

        private static readonly List<Color> _colorsOfMap = new[]
        {
            //Color.FromArgb(Alpha, 0, 0, 0),           //Black
            Color.FromArgb(_alpha, 0, 0, 0xFF), //Blue
            Color.FromArgb(_alpha, 0, 0xFF, 0xFF), //Cyan
            Color.FromArgb(_alpha, 0, 0xFF, 0), //Green
            Color.FromArgb(_alpha, 0xFF, 0xFF, 0), //Yellow
            Color.FromArgb(_alpha, 0xFF, 0, 0), //Red
            //Color.FromArgb(Alpha, 0xFF, 0xFF, 0xFF)   // White
        }.ToList();

        #endregion

        #region Public methods

        public static Color GetColorForValue(double val, double maxVal, double minVal)
        {
            var valPerc = (val - minVal) / (maxVal - minVal);
            var colorPerc = 1d / (_colorsOfMap.Count - 1); // % of each block of color. the last is the "100% Color"
            var blockOfColor = valPerc / colorPerc; // the integer part repersents how many block to skip
            var blockIdx = (int)Math.Truncate(blockOfColor); // Idx of 
            var valPercResidual = valPerc - blockIdx * colorPerc; //remove the part represented of block 
            var percOfColor = valPercResidual / colorPerc; // % of color of this block that will be filled

            var cTarget = _colorsOfMap[blockIdx];
            var cNext = Math.Abs(val - maxVal) < double.Epsilon ? _colorsOfMap[blockIdx] : _colorsOfMap[blockIdx + 1];

            var deltaR = cNext.R - cTarget.R;
            var deltaG = cNext.G - cTarget.G;
            var deltaB = cNext.B - cTarget.B;

            var r = cTarget.R + deltaR * percOfColor;
            var g = cTarget.G + deltaG * percOfColor;
            var b = cTarget.B + deltaB * percOfColor;

            var c = _colorsOfMap[0];
            try
            {
                c = Color.FromArgb(_alpha, (byte)r, (byte)g, (byte)b);
            }
            catch
            {
                // ignored
            }

            return c;
        }

        #endregion
    }
}