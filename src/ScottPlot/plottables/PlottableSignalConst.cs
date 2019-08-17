﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ScottPlot
{
    // Variation of PlottableSignal that uses a segmented tree for faster min/max range queries
    // - frequent min/max lookups are a bottleneck displaying large signals
    // - limited to 60M points (250M in x64 mode) due to memory (tree uses from 2X to 4X memory)
    // - signlePrecision = true halves memory usage and make x86 limit to 120M points
    // - in x64 mode limit can be up to maximum array size (2G points) with special solution and 64 GB RAM (not tested)
    // - if source array is changed UpdateTrees() must be called
    // - source array can be change by call updateData(), updating by ranges much faster.
    public class PlottableSignalConst<T> : PlottableSignal<T>
    {
        // using 2 x signal memory in best case: ys.Length is Pow2 
        // using 4 x signal memory in worst case: ys.Length is (Pow2 +1);        
        T[] TreeMin;
        T[] TreeMax;
        private int n = 0; // size of each Tree
        public bool TreesReady = false;
        private bool singlePrecision = false; // float type for trees, which uses half memory

        private static Func<T, T, T> MinExp;
        private static Func<T, T, T> MaxExp;
        private static Func<T, T, bool> EqualExp;
        private static Func<T> MaxValue;
        private static Func<T> MinValue;
        public PlottableSignalConst(T[] ys, double sampleRate, double xOffset, double yOffset, Color color, double lineWidth, double markerSize, string label, bool useParallel, bool singlePrecision = false) : base(ys, sampleRate, xOffset, yOffset, color, lineWidth, markerSize, label, useParallel)
        {            
            ParameterExpression paramA = Expression.Parameter(typeof(T), "a");
            ParameterExpression paramB = Expression.Parameter(typeof(T), "b");
            // add the parameters together
            Expression bodyMin = Expression.Condition(Expression.LessThanOrEqual(paramA, paramB), paramA,paramB);
            Expression bodyMax = Expression.Condition(Expression.GreaterThanOrEqual(paramA, paramB), paramA, paramB);
            BinaryExpression bodyEqual = Expression.Equal(paramA, paramB);
            Expression bodyMaxValue = Expression.MakeMemberAccess(null, typeof(T).GetField("MaxValue"));
            Expression bodyMinValue = Expression.MakeMemberAccess(null, typeof(T).GetField("MinValue"));
            // compile it
            MinExp = Expression.Lambda<Func<T, T, T>>(bodyMin, paramA, paramB).Compile();
            MaxExp = Expression.Lambda<Func<T, T, T>>(bodyMax, paramA, paramB).Compile();
            EqualExp = Expression.Lambda<Func<T, T, bool>>(bodyEqual, paramA, paramB).Compile();
            MaxValue = Expression.Lambda<Func<T>>(bodyMaxValue).Compile(); 
            MinValue = Expression.Lambda<Func<T>>(bodyMinValue).Compile();

            this.singlePrecision = singlePrecision;
            if (useParallel)
                UpdateTreesInBackground();
            else
                UpdateTrees();
        }

        public void updateData(int index, T newValue)
        {
            ys[index] = newValue;

            // Update Tree, can be optimized            
            if (index == ys.Length - 1) // last elem haven't pair
            {
                TreeMin[n / 2 + index / 2] = ys[index];
                TreeMax[n / 2 + index / 2] = ys[index];
            }
            else if (index % 2 == 0) // even elem have right pair
            {
                TreeMin[n / 2 + index / 2] = MinExp(ys[index], ys[index + 1]);
                TreeMax[n / 2 + index / 2] = MaxExp(ys[index], ys[index + 1]);
            }
            else // odd elem have left pair
            {
                TreeMin[n / 2 + index / 2] = MinExp(ys[index], ys[index - 1]);
                TreeMax[n / 2 + index / 2] = MaxExp(ys[index], ys[index - 1]);
            }

            T candidate;
            for (int i = (n / 2 + index / 2) / 2; i > 0; i /= 2)
            {
                candidate = MinExp(TreeMin[i * 2], TreeMin[i * 2 + 1]);
                if (EqualExp(TreeMin[i], candidate)) // if node same then new value don't need to recalc all upper
                    break;
                TreeMin[i] = candidate;
            }
            for (int i = (n / 2 + index / 2) / 2; i > 0; i /= 2)
            {
                candidate = MaxExp(TreeMax[i * 2], TreeMax[i * 2 + 1]);
                if (EqualExp(TreeMax[i], candidate)) // if node same then new value don't need to recalc all upper
                    break;
                TreeMax[i] = candidate;
            }
        }

        public void updateData(int from, int to, T[] newData, int fromData = 0) // RangeUpdate
        {
            //update source signal
            for (int i = from; i < to; i++)
            {
                ys[i] = newData[i - from + fromData];
            }

            for (int i = n / 2 + from / 2; i < n / 2 + to / 2; i++)
            {
                TreeMin[i] = MinExp(ys[i * 2 - n], ys[i * 2 + 1 - n]);
                TreeMax[i] = MaxExp(ys[i * 2 - n], ys[i * 2 + 1 - n]);
            }
            if (to == ys.Length) // last elem haven't pair
            {
                TreeMin[n / 2 + to / 2] = ys[to - 1];
                TreeMax[n / 2 + to / 2] = ys[to - 1];
            }
            else if (to % 2 == 1) //last elem even(to-1) and not last
            {
                TreeMin[n / 2 + to / 2] = MinExp(ys[to - 1], ys[to]);
                TreeMax[n / 2 + to / 2] = MaxExp(ys[to - 1], ys[to]);
            }

            from = (n / 2 + from / 2) / 2;
            to = (n / 2 + to / 2) / 2;

            T candidate;
            while (from != 0) // up to root elem, that is [1], [0] - is free elem
            {
                if (from != to)
                {
                    for (int i = from; i <= to; i++) // Recalc all level nodes in range 
                    {
                        TreeMin[i] = MinExp(TreeMin[i * 2], TreeMin[i * 2 + 1]);
                        TreeMax[i] = MaxExp(TreeMax[i * 2], TreeMax[i * 2 + 1]);
                    }
                }
                else
                {
                    // left == rigth, so no need more from to loop
                    for (int i = from; i > 0; i /= 2) // up to root node
                    {
                        candidate = MinExp(TreeMin[i * 2], TreeMin[i * 2 + 1]);
                        if (EqualExp(TreeMin[i], candidate)) // if node same then new value don't need to recalc all upper
                            break;
                        TreeMin[i] = candidate;
                    }

                    for (int i = from; i > 0; i /= 2) // up to root node
                    {
                        candidate = MaxExp(TreeMax[i * 2], TreeMax[i * 2 + 1]);
                        if (EqualExp(TreeMax[i], candidate)) // if node same then new value don't need to recalc all upper
                            break;
                        TreeMax[i] = candidate;
                    }
                    // all work done exit while loop
                    break;
                }
                // level up
                from = from / 2;
                to = to / 2;
            }
        }

        public void updateData(int from, T[] newData)
        {
            updateData(from, newData.Length, newData);
        }

        public void updateData(T[] newData)
        {
            updateData(0, newData.Length, newData);
        }

        public void UpdateTreesInBackground()
        {
            Task.Run(() => { UpdateTrees(); });
        }

        public void UpdateTrees()
        {
            // O(n) to build trees
            TreesReady = false;
            try
            {
                if (ys.Length == 0)
                    throw new ArgumentOutOfRangeException($"Array cant't be empty");
                // Size up to pow2
                if (ys.Length > 0x40_00_00_00) // pow 2 must be more then int.MaxValue
                    throw new ArgumentOutOfRangeException($"Array higher than {0x40_00_00_00} not supported by SignalConst");
                int pow2 = 1;
                while (pow2 < 0x40_00_00_00 && pow2 < ys.Length)
                    pow2 <<= 1;
                n = pow2;
                TreeMin = new T[n];
                TreeMax = new T[n];
                T maxValue = MaxValue();
                T minValue = MinValue();

                // fill bottom layer of tree
                for (int i = 0; i < ys.Length / 2; i++) // with source array pairs min/max
                {
                    TreeMin[n / 2 + i] = MinExp(ys[i * 2], ys[i * 2 + 1]);
                    TreeMax[n / 2 + i] = MaxExp(ys[i * 2], ys[i * 2 + 1]);
                }
                if (ys.Length % 2 == 1) // if array size odd, last element haven't pair to compare
                {
                    TreeMin[n / 2 + ys.Length / 2] = ys[ys.Length - 1];
                    TreeMax[n / 2 + ys.Length / 2] = ys[ys.Length - 1];
                }
                for (int i = n / 2 + (ys.Length + 1) / 2; i < n; i++) // min/max for pairs of nonexistent elements
                {
                    TreeMin[i] = minValue;
                    TreeMax[i] = maxValue;
                }
                // fill other layers
                for (int i = n / 2 - 1; i > 0; i--)
                {
                    TreeMin[i] = MinExp(TreeMin[2 * i], TreeMin[2 * i + 1]);
                    TreeMax[i] = MaxExp(TreeMax[2 * i], TreeMax[2 * i + 1]);
                }
                TreesReady = true;
            }
            catch (OutOfMemoryException)
            {
                TreeMin = null;
                TreeMax = null;
                TreesReady = false;
                return;
            }
        }

        //  O(log(n)) for each range min/max query
        protected override void MinMaxRangeQuery(int l, int r, out double lowestValue, out double highestValue)
        {
            // if the tree calculation isn't finished or if it crashed
            if (!TreesReady)
            {
                // use the original (slower) min/max calculated method
                base.MinMaxRangeQuery(l, r, out lowestValue, out highestValue);
                return;
            }

            T lowestValueT = MaxValue();
            T highestValueT = MinValue();
            if (l == r)
            {
                lowestValue = highestValue = Convert.ToDouble(ys[l]);                
                return;
            }
            // first iteration on source array that virtualy bottom of tree
            if ((l & 1) != 1) // l is left child
            {
                lowestValueT = MinExp(lowestValueT, ys[l]);
                highestValueT = MaxExp(highestValueT, ys[l]);
            }
            if ((r & 1) == 1) // r is right child
            {
                lowestValueT = MinExp(lowestValueT, ys[r]);
                highestValueT = MaxExp(highestValueT, ys[r]);
            }
            // go up from array to bottom of Tree
            l = (l + n) / 2;
            r = (r + n) / 2;
            // next iterations on tree
            while (l <= r)
            {
                if ((l & 1) == 1) // l is right child
                {
                    lowestValueT = MinExp(lowestValueT, TreeMin[l]);
                    highestValueT = MaxExp(highestValueT, TreeMax[l]);
                }
                if ((r & 1) != 1) // r is left child
                {
                    lowestValueT = MinExp(lowestValueT, TreeMin[r]);
                    highestValueT = MaxExp(highestValueT, TreeMax[r]);
                }
                // go up one level
                l = (l + 1) / 2;
                r = (r - 1) / 2;
            }
            lowestValue = Convert.ToDouble(lowestValueT);
            highestValue = Convert.ToDouble(highestValueT);
        }

        public override double[] GetLimits()
        {
            double[] limits = new double[4];
            limits[0] = 0 + xOffset;
            limits[1] = samplePeriod * ys.Length + xOffset;
            MinMaxRangeQuery(0, ys.Length - 1, out limits[2], out limits[3]);
            limits[2] += yOffset;
            limits[3] += yOffset;
            return limits;
        }

        public override string ToString()
        {
            return $"PlottableSignalConst with {pointCount} points, trees {(TreesReady ? "" : "not")} calculated";
        }
    }
}
