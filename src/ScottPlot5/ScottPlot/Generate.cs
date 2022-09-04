﻿using System;

namespace ScottPlot;

/// <summary>
/// This class contains methods which generate sample data for testing and demonstration purposes
/// </summary>
public static class Generate
{
    /// <summary>
    /// Return an array of evenly-spaced numbers
    /// </summary>
    public static double[] Consecutive(int count, double delta = 1, double first = 0)
    {
        double[] ys = new double[count];
        for (int i = 0; i < ys.Length; i++)
            ys[i] = i * delta + first;
        return ys;
    }

    public static T[] Consecutive<T>(int count, double delta = 1, double first = 0)
    {
        return Consecutive(count, delta, first).ToGenericArray<T>();
    }

    /// <summary>
    /// Return an array of sine waves between -1 and 1.
    /// Values are multiplied by <paramref name="mult"/> then shifted by <paramref name="offset"/>.
    /// Phase shifts the sine wave horizontally between 0 and 2 Pi.
    /// </summary>
    public static double[] Sin(int count, double mult = 1, double offset = 0, double oscillations = 1, double phase = 0)
    {
        double sinScale = 2 * Math.PI * oscillations / (count - 1);
        double[] ys = new double[count];
        for (int i = 0; i < ys.Length; i++)
            ys[i] = Math.Sin(i * sinScale + phase * Math.PI * 2) * mult + offset;
        return ys;
    }

    public static T[] Sin<T>(int count, double mult = 1, double offset = 0, double oscillations = 1, double phase = 0)
    {
        return Sin(count, mult, offset, oscillations, phase).ToGenericArray<T>();
    }

    /// <summary>
    /// Return an array of cosine waves between -1 and 1.
    /// Values are multiplied by <paramref name="mult"/> then shifted by <paramref name="offset"/>.
    /// Phase shifts the sine wave horizontally between 0 and 2 Pi.
    /// </summary>
    public static double[] Cos(int count, double mult = 1, double offset = 0, double oscillations = 1, double phase = 0)
    {
        double sinScale = 2 * Math.PI * oscillations / (count - 1);
        double[] ys = new double[count];
        for (int i = 0; i < ys.Length; i++)
            ys[i] = Math.Cos(i * sinScale + phase * Math.PI * 2) * mult + offset;
        return ys;
    }

    public static T[] Cos<T>(int count, double mult = 1, double offset = 0, double oscillations = 1, double phase = 0)
    {
        return Cos(count, mult, offset, oscillations, phase).ToGenericArray<T>();
    }

    public static double[] NoisySin(Random rand, int count, double noiseLevel = 1)
    {
        double[] data = Sin(count);
        for (int i = 0; i < data.Length; i++)
        {
            data[i] += rand.NextDouble() * noiseLevel;
        }
        return data;
    }

    public static double[,] Sin2D(int width, int height, double xPeriod = .2, double yPeriod = .2, double multiple = 100)
    {
        double[,] intensities = new double[height, width];

        for (int y = 0; y < height; y++)
        {
            double siny = Math.Cos(y * yPeriod) * multiple;
            for (int x = 0; x < width; x++)
            {
                double sinx = Math.Sin(x * xPeriod) * multiple;
                intensities[y, x] = sinx + siny;
            }
        }

        return intensities;
    }

    public static double[,] Ramp2D(int width, int height, double min = 0, double max = 1)
    {
        double[,] intensities = new double[height, width];

        double span = max - min;

        for (int y = 0; y < height; y++)
        {
            double fracY = (double)y / height;
            double valY = fracY * span + min;

            for (int x = 0; x < width; x++)
            {
                double fracX = (double)x / width;
                double valX = fracX * span + min;

                intensities[y, x] = (valX + valY) / 2;
            }
        }

        return intensities;
    }
}
