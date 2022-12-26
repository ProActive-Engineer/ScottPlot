﻿namespace ScottPlot;

/// <summary>
/// This class provides cross-platform tools for working with fonts
/// </summary>
public static class FontService
{
    public static PixelSize MeasureString(Font font, string text)
    {
        float width = font.Size * text.Length;
        float height = font.Size;
        return new PixelSize(width, height);
    }

    public static Font GetSystemDefaultMonospaceFont(float size)
    {
        string name = "Consolas";
        return new Font(name, size);
    }
}
