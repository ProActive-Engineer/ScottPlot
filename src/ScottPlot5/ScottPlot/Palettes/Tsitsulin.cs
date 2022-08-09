﻿/* A 25-color pelette based on Anton Tsitsulin's 12-color palette
 * Adapted by Arthurits: https://github.com/ScottPlot/ScottPlot/pull/1318
 * http://tsitsul.in/blog/coloropt
 * https://github.com/xgfs/coloropt
 */

namespace ScottPlot.Palettes;

// TODO: add more colormaps from http://tsitsul.in/blog/coloropt/ called XgfsNormal6, XgfsNormal12, etc.

public class Tsitsulin : HexPaletteBase, IPalette
{
    public override string Name => this.GetType().Name;

    public override string Description => string.Empty;

    internal override string[] HexColors => new string[]
    {
        "#ebac23", "#b80058", "#008cf9", "#006e00", "#00bbad",
        "#d163e6", "#b24502", "#ff9287", "#5954d6", "#00c6f8",
        "#878500", "#00a76c",
        "#f6da9c", "#ff5caa", "#8accff", "#4bff4b", "#6efff4",
        "#edc1f5", "#feae7c", "#ffc8c3", "#bdbbef", "#bdf2ff",
        "#fffc43", "#65ffc8",
        "#aaaaaa",
    };
}
