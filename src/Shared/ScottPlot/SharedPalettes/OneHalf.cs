﻿/* Sourced from Son A. Pham's Sublime color scheme by the same name
 * https://github.com/sonph/onehalf
 */

namespace ScottPlot.Palettes;

public class OneHalf : ISharedPalette
{
    public string Name { get; } = "One Half"; 
    
    public string Description { get; } = "A Sublime color scheme " +
        "by Son A. Pham: https://github.com/sonph/onehalf";

    public SharedColor[] Colors { get; } = SharedColor.FromHex(HexColors);

    private static readonly string[] HexColors =
    {
        "#383a42", "#e4564a", "#50a14f", "#c18402", "#0084bc", "#a626a4", "#0897b3"
    };
}
