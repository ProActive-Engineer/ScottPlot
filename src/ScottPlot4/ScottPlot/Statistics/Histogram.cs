﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ScottPlot.Statistics;

public class Histogram
{
    /// <summary>
    /// Number of values counted for each bin.
    /// </summary>
    public readonly double[] Counts;

    /// <summary>
    /// Number of bins.
    /// </summary>
    public readonly int BinCount;

    /// <summary>
    /// Running total of all values counted.
    /// </summary>
    public double Sum { get; private set; }

    /// <summary>
    /// Total number of values accumulated.
    /// </summary>
    public int ValuesCounted { get; private set; }

    /// <summary>
    /// Lower edge for each bin.
    /// </summary>
    public readonly double[] Bins;

    /// <summary>
    /// Default behavior is that outlier values are not counted.
    /// If this is enabled, min/max outliers will be counted in the first/last bin.
    /// </summary>
    public readonly bool AddOutliersToEdgeBins;

    /// <summary>
    /// Lower edge of the first bin (inclusive)
    /// </summary>
    public readonly double Min;

    /// <summary>
    /// Upper edge of the last bin (exclusive)
    /// </summary>
    public readonly double Max;

    /// <summary>
    /// The calculated bin size.
    /// </summary>
    public double BinSize { get; private set; } = 0;

    /// <summary>
    /// Number of values that were smaller than the lower edge of the first bin.
    /// </summary>
    public int MinOutlierCount { get; private set; } = 0;

    /// <summary>
    /// Number of values that were greater than the upper edge of the last bin.
    /// </summary>
    public int MaxOutlierCount { get; private set; } = 0;

    /// <summary>
    /// Create a histogram which will count values supplied by <see cref="Add(double)"/> and <see cref="Add(IEnumerable{double})"/>
    /// </summary>
    /// <param name="min">minimum value to be counted</param>
    /// <param name="max">maximum value to be counted</param>
    /// <param name="binCount">number of bins between <paramref name="min"/> and <paramref name="max"/></param>
    /// <param name="addOutliersToEdgeBins">if false, outliers will not be counted</param>
    /// <param name="addFinalBin">if true, one more bin will be added so values equal to <paramref name="max"/> can be counted too</param>
    public Histogram(double min, double max, int binCount, bool addOutliersToEdgeBins = false, bool addFinalBin = true)
    {
        BinSize = (max - min) / binCount;
        AddOutliersToEdgeBins = addOutliersToEdgeBins;

        if (addFinalBin)
            binCount += 1;

        BinCount = binCount;
        Min = min;
        Max = min + BinSize * binCount;

        Counts = new double[binCount];
        Bins = new double[binCount];
        for (int i = 0; i < binCount; i++)
        {
            Bins[i] = min + BinSize * i;
        }
    }

    /// <summary>
    /// Create a histogram with bins that can count data from <paramref name="min"/> to <paramref name="max"/> (inclusive)
    /// </summary>
    public static Histogram WithFixedBinSize(double min, double max, double binSize, bool addOutliersToEdgeBins = false)
    {
        int binCount = (int)Math.Ceiling((max - min) / binSize) + 1;
        max = binCount * binSize + min;
        return new Histogram(min, max, binCount, addOutliersToEdgeBins, addFinalBin: false);
    }

    /// <summary>
    /// Create a histogram with bins that can count data from <paramref name="min"/> to <paramref name="max"/> (inclusive)
    /// </summary>
    public static Histogram WithFixedBinCount(double min, double max, int binCount, bool addOutliersToEdgeBins = false)
    {
        return new Histogram(min, max, binCount, addOutliersToEdgeBins);
    }

    /// <summary>
    /// Return counts normalized so the sum of all counts equals 1
    /// </summary>
    public double[] GetProbability()
    {
        return Counts.Select(x => x / ValuesCounted).ToArray();
    }

    /// <summary>
    /// Return a function describing the probability function (a Gaussian curve fitted to the histogram probabilities).
    /// </summary>
    public Func<double, double?> GetProbabilityCurve(double[] values, bool scaleToBinnedProbability = false)
    {
        BasicStats stats = new(values);

        Func<double, double?> unscaled = x => Math.Exp(-.5 * Math.Pow((x - stats.Mean) / stats.StDev, 2));
        if (!scaleToBinnedProbability)
            return unscaled;

        double sum = (double)Bins.Select(x => unscaled(x)).Sum();
        Func<double, double?> scaled = x => Math.Exp(-.5 * Math.Pow((x - stats.Mean) / stats.StDev, 2)) / sum;
        return scaled;
    }

    /// <summary>
    /// Return counts normalized so the maximum value equals the given value
    /// </summary>
    public double[] GetNormalized(double maxValue = 1)
    {
        double mult = maxValue / Counts.Max();
        return Counts.Select(x => x * mult).ToArray();
    }

    /// <summary>
    /// Return the cumulative histogram counts.
    /// Each value is the number of counts in that bin and all bins below it.
    /// </summary>
    public double[] GetCumulative()
    {
        double[] cumulative = new double[Counts.Length];
        cumulative[0] = Counts[0];
        for (int i = 1; i < Counts.Length; i++)
        {
            cumulative[i] = cumulative[i - 1] + Counts[i];
        }
        return cumulative;
    }

    /// <summary>
    /// Return the cumulative probability histogram.
    /// Each value is the fraction of counts in that bin and all bins below it.
    /// </summary>
    public double[] GetCumulativeProbability()
    {
        double[] cumulative = GetCumulative();
        double final = cumulative.Last();
        return cumulative.Select(x => x / final).ToArray();
    }

    /// <summary>
    /// Add a single value to the histogram
    /// </summary>
    public void Add(double value)
    {
        if (value < Min)
        {
            MinOutlierCount += 1;
            if (AddOutliersToEdgeBins)
            {
                Counts[0] += 1;
                Sum += value;
                ValuesCounted += 1;
            }
            return;
        }

        if (value >= Max)
        {
            MaxOutlierCount += 1;
            if (AddOutliersToEdgeBins)
            {
                Counts[Counts.Length - 1] += 1;
                Sum += value;
                ValuesCounted += 1;
            }
            return;
        }

        double distanceFromMin = value - Min;
        int binsFromMin = (int)(distanceFromMin / BinSize);
        Counts[binsFromMin] += 1;
        Sum += value;
        ValuesCounted += 1;
    }

    /// <summary>
    /// Add multiple values to the histogram
    /// </summary>
    public void Add(IEnumerable<double> values)
    {
        foreach (double value in values)
            Add(value);
    }

    /// <summary>
    /// Reset the histogram, setting all counts and values to zero
    /// </summary>
    public void Clear()
    {
        MinOutlierCount = 0;
        MaxOutlierCount = 0;
        Sum = 0;
        ValuesCounted = 0;
        for (int i = 0; i < Counts.Length; i++)
        {
            Counts[i] = 0;
        }
    }
}
