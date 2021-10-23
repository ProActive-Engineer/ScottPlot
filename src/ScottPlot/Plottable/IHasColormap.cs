﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScottPlot.Plottable
{
    public interface IHasColormap
    {
        Drawing.Colormap Colormap { get; set; }
    }
}
