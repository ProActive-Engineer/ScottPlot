﻿// This colormap was created by Scott Harden on 2020-06-16 and is released under a MIT license.

namespace ScottPlot.Drawing.Colormaps
{
    class Grayscale : ScottPlot.Drawing.IColormap
    {
        public (byte r, byte g, byte b) GetRGB(byte value)
        {
            return (value, value, value);
        }
    }
}
