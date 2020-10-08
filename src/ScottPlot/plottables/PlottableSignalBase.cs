﻿using ScottPlot.Config;
using ScottPlot.Diagnostic.Attributes;
using ScottPlot.Drawing;
using ScottPlot.MinMaxSearchStrategies;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ScottPlot
{
    public class PlottableSignalBase<T> : Plottable, IExportable where T : struct, IComparable
    {
        private bool MaxRenderIndexLowerYSPromise = false;
        private bool MaxRenderIndexHigherMinRenderIndexPromise = false;
        private bool FillColor1MustBeSetPromise = false;
        private bool FillColor2MustBeSetPromise = false;

        protected IMinMaxSearchStrategy<T> minmaxSearchStrategy;
        [FiniteNumbers, EqualLength]
        private T[] _ys;
        public T[] ys
        {
            get => _ys;
            set
            {
                if (value == null)
                    throw new Exception("Y data cannot be null");

                if (_maxRenderIndex > value.Length - 1)
                    MaxRenderIndexLowerYSPromise = true;
                else
                    MaxRenderIndexLowerYSPromise = false;

                _ys = value;
            }
        }
        [FiniteNumbers]
        private double _sampleRate;
        public double sampleRate
        {
            get => _sampleRate;
            set
            {
                if (value <= 0)
                    throw new Exception("SampleRate must be greater then zero");
                _sampleRate = value;
                _samplePeriod = 1.0 / value;
            }
        }
        [FiniteNumbers]
        private double _samplePeriod;
        public double samplePeriod
        {
            get => _samplePeriod;
            set
            {
                if (_samplePeriod <= 0)
                    throw new Exception("SamplePeriod must be greater then zero");
                _samplePeriod = value;
            }
        }
        public float markerSize { get; set; }
        [FiniteNumbers]
        public double xOffset { get; set; }
        [FiniteNumbers]
        public double yOffset { get; set; }
        public double lineWidth { get; set; }

        protected int _minRenderIndex;
        public int minRenderIndex
        {
            get => _minRenderIndex;
            set
            {
                if (value < 0)
                    throw new ArgumentException("MinRenderIndex must be positive");

                if (value > _maxRenderIndex)
                    MaxRenderIndexHigherMinRenderIndexPromise = true;
                else
                    MaxRenderIndexHigherMinRenderIndexPromise = false;

                _minRenderIndex = value;
            }
        }
        protected int _maxRenderIndex;
        public int maxRenderIndex
        {
            get => _maxRenderIndex;
            set
            {
                if (value < 0)
                    throw new ArgumentException("MaxRenderIndex must be positive");

                if (value < minRenderIndex)
                    MaxRenderIndexHigherMinRenderIndexPromise = true;
                else
                    MaxRenderIndexHigherMinRenderIndexPromise = false;

                if (value > _ys.Length - 1)
                    MaxRenderIndexLowerYSPromise = true;
                else
                    MaxRenderIndexLowerYSPromise = false;

                _maxRenderIndex = value;
            }
        }
        private Pen[] penByDensity;
        private int densityLevelCount = 0;
        public string label { get; set; }
        public Color color { get; set; }
        public LineStyle lineStyle { get; set; }
        public bool useParallel { get; set; } = true;
        private FillType _fillType;
        public FillType fillType
        {
            get => _fillType;
            set
            {
                if (_fillColor1 == null && value != FillType.NoFill)
                    FillColor1MustBeSetPromise = true;
                else
                    FillColor1MustBeSetPromise = false;

                if (_fillColor2 == null && value == FillType.FillAboveAndBelow)
                    FillColor2MustBeSetPromise = true;
                else
                    FillColor2MustBeSetPromise = false;

                _fillType = value;
            }
        }
        private Color? _fillColor1;
        public Color? fillColor1
        {
            get => _fillColor1;
            set
            {
                if (value == null && fillType != FillType.NoFill)
                    FillColor1MustBeSetPromise = true;
                else
                    FillColor1MustBeSetPromise = false;

                _fillColor1 = value;
            }
        }
        public Color? gradientFillColor1 { get; set; }
        private Color? _fillColor2;
        public Color? fillColor2
        {
            get => _fillColor2;
            set
            {
                if (value == null && fillType == FillType.FillAboveAndBelow)
                    FillColor2MustBeSetPromise = true;
                else
                    FillColor2MustBeSetPromise = false;

                _fillColor2 = value;
            }
        }

        public Color? gradientFillColor2 { get; set; }
        public int baseline { get; set; }

        public PlottableSignalBase(T[] ys, double sampleRate, double xOffset, double yOffset, Color color,
            double lineWidth, double markerSize, string label, Color[] colorByDensity,
            int minRenderIndex, int maxRenderIndex, LineStyle lineStyle, bool useParallel,
            IMinMaxSearchStrategy<T> minMaxSearchStrategy = null)
        {
            if (ys == null)
                throw new Exception("Y data cannot be null");

            this._ys = ys;
            this._sampleRate = sampleRate;
            this._samplePeriod = 1.0 / sampleRate;
            this.markerSize = (float)markerSize;
            this.xOffset = xOffset;
            this.label = label;
            this.color = color;
            this.lineWidth = lineWidth;
            this.fillType = FillType.NoFill;
            this.yOffset = yOffset;
            this.baseline = 0;
            if (minRenderIndex < 0 || minRenderIndex > maxRenderIndex)
                throw new ArgumentException("minRenderIndex must be between 0 and maxRenderIndex");
            this.minRenderIndex = minRenderIndex;
            if ((maxRenderIndex > ys.Length - 1) || maxRenderIndex < 0)
                throw new ArgumentException("maxRenderIndex must be a valid index for ys[]");
            this._maxRenderIndex = maxRenderIndex;
            this.lineStyle = lineStyle;
            this.useParallel = useParallel;

            if (colorByDensity != null)
            {
                // turn the ramp into a pen triangle
                densityLevelCount = colorByDensity.Length * 2 - 1;
                penByDensity = new Pen[densityLevelCount];
                for (int i = 0; i < colorByDensity.Length; i++)
                {
                    penByDensity[i] = new Pen(colorByDensity[i]);
                    penByDensity[densityLevelCount - 1 - i] = new Pen(colorByDensity[i]);
                }
            }
            if (minMaxSearchStrategy == null)
                this.minmaxSearchStrategy = new SegmentedTreeMinMaxSearchStrategy<T>();
            else
                this.minmaxSearchStrategy = minMaxSearchStrategy;
            minmaxSearchStrategy.SourceArray = ys;
        }

        public void updateData(int index, T newValue)
        {
            minmaxSearchStrategy.updateElement(index, newValue);
        }

        public void updateData(int from, int to, T[] newData, int fromData = 0) // RangeUpdate
        {
            minmaxSearchStrategy.updateRange(from, to, newData, fromData);
        }

        public void updateData(int from, T[] newData)
        {
            updateData(from, newData.Length, newData);
        }

        public void updateData(T[] newData)
        {
            updateData(0, newData.Length, newData);
        }

        public override Config.AxisLimits2D GetLimits()
        {
            double[] limits = new double[4];
            limits[0] = minRenderIndex + xOffset;
            limits[1] = _samplePeriod * _maxRenderIndex + xOffset;
            minmaxSearchStrategy.MinMaxRangeQuery(minRenderIndex, _maxRenderIndex, out limits[2], out limits[3]);
            limits[2] += yOffset;
            limits[3] += yOffset;

            // TODO: use features of 2d axis
            return new Config.AxisLimits2D(limits);
        }

        private void RenderSingleLine(Settings settings, Pen penHD)
        {
            // this function is for when the graph is zoomed so far out its entire display is a single vertical pixel column
            double yMin, yMax;
            minmaxSearchStrategy.MinMaxRangeQuery(minRenderIndex, _maxRenderIndex, out yMin, out yMax);
            PointF point1 = settings.GetPixel(xOffset, yMin + yOffset);
            PointF point2 = settings.GetPixel(xOffset, yMax + yOffset);
            settings.gfxData.DrawLine(penHD, point1, point2);
        }

        private bool markersAreVisible = false;

        public string ValidationErrorMessage => throw new NotImplementedException();

        private void RenderLowDensity(Settings settings, int visibleIndex1, int visibleIndex2, Brush brush, Pen penLD, Pen penHD)
        {
            // this function is for when the graph is zoomed in so individual data points can be seen

            List<PointF> linePoints = new List<PointF>(visibleIndex2 - visibleIndex1 + 2);
            if (visibleIndex2 > _ys.Length - 2)
                visibleIndex2 = _ys.Length - 2;
            if (visibleIndex2 > _maxRenderIndex)
                visibleIndex2 = _maxRenderIndex - 1;
            if (visibleIndex1 < 0)
                visibleIndex1 = 0;
            if (visibleIndex1 < minRenderIndex)
                visibleIndex1 = minRenderIndex;

            for (int i = visibleIndex1; i <= visibleIndex2 + 1; i++)
                linePoints.Add(settings.GetPixel(_samplePeriod * i + xOffset, minmaxSearchStrategy.SourceElement(i) + yOffset));

            if (linePoints.Count > 1)
            {
                if (penLD.Width > 0)
                    settings.gfxData.DrawLines(penHD, linePoints.ToArray());

                if (fillType == FillType.FillAbove || fillType == FillType.FillBelow)
                {
                    FillAboveOrBelow(settings, linePoints[0].X, linePoints[linePoints.Count - 1].X, linePoints.ToArray(), fillType);
                }
                else if (fillType == FillType.FillAboveAndBelow)
                {
                    FillAboveAndBelow(settings, linePoints[0].X, linePoints[linePoints.Count - 1].X, linePoints.ToArray(), this.baseline);
                }

                if (markerSize > 0)
                {
                    // make markers transition away smoothly by making them smaller as the user zooms out
                    float pixelsBetweenPoints = (float)(_samplePeriod * settings.xAxisScale);
                    float zoomTransitionScale = Math.Min(1, pixelsBetweenPoints / 10);
                    float markerPxDiameter = markerSize * zoomTransitionScale;
                    float markerPxRadius = markerPxDiameter / 2;
                    if (markerPxRadius > .25)
                    {
                        markersAreVisible = true;

                        // adjust marker offset to improve rendering on Linux and MacOS
                        float markerOffsetX = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? 0 : 1;
                        foreach (PointF point in linePoints)
                            settings.gfxData.FillEllipse(brush: brush,
                                x: point.X - markerPxRadius + markerOffsetX, y: point.Y - markerPxRadius,
                                width: markerPxDiameter, height: markerPxDiameter);
                    }
                }
            }
        }

        private class IntervalMinMax
        {
            public float x;
            public float Min;
            public float Max;
            public IntervalMinMax(float x, float Min, float Max)
            {
                this.x = x;
                this.Min = Min;
                this.Max = Max;
            }
            public IEnumerable<PointF> GetPoints()
            {
                yield return new PointF(x, Min);
                yield return new PointF(x, Max);
            }
        }

        private IntervalMinMax CalcInterval(int xPx, double offsetPoints, double columnPointCount, Settings settings)
        {
            int index1 = (int)(offsetPoints + columnPointCount * xPx);
            int index2 = (int)(offsetPoints + columnPointCount * (xPx + 1));

            if (index1 < 0)
                index1 = 0;
            if (index1 < minRenderIndex)
                index1 = minRenderIndex;

            if (index2 > _ys.Length - 1)
                index2 = _ys.Length - 1;
            if (index2 > _maxRenderIndex)
                index2 = _maxRenderIndex;

            // get the min and max value for this column                
            minmaxSearchStrategy.MinMaxRangeQuery(index1, index2, out double lowestValue, out double highestValue);
            float yPxHigh = (float)settings.GetPixelY(lowestValue + yOffset);
            float yPxLow = (float)settings.GetPixelY(highestValue + yOffset);
            return new IntervalMinMax(xPx, yPxLow, yPxHigh);
        }

        private void RenderHighDensity(Settings settings, double offsetPoints, double columnPointCount, Pen penHD)
        {
            int xPxStart = (int)Math.Ceiling((-1 - offsetPoints + minRenderIndex) / columnPointCount - 1);
            int xPxEnd = (int)Math.Ceiling((_maxRenderIndex - offsetPoints) / columnPointCount);
            xPxStart = Math.Max(0, xPxStart);
            xPxEnd = Math.Min(settings.dataSize.Width, xPxEnd);
            if (xPxStart >= xPxEnd)
                return;

            var columns = Enumerable.Range(xPxStart, xPxEnd - xPxStart);

            IEnumerable<IntervalMinMax> intervals;
            if (useParallel)
            {
                intervals = columns
                    .AsParallel()
                    .AsOrdered()
                    .Select(xPx => CalcInterval(xPx, offsetPoints, columnPointCount, settings))
                    .AsSequential();
            }
            else
            {
                intervals = columns
                    .Select(xPx => CalcInterval(xPx, offsetPoints, columnPointCount, settings));
            }

            PointF[] linePoints = intervals
                .SelectMany(c => c.GetPoints())
                .ToArray();

            // adjust order of points to enhance anti-aliasing
            PointF buf;
            for (int i = 1; i < linePoints.Length / 2; i++)
            {
                if (linePoints[i * 2].Y >= linePoints[i * 2 - 1].Y)
                {
                    buf = linePoints[i * 2];
                    linePoints[i * 2] = linePoints[i * 2 + 1];
                    linePoints[i * 2 + 1] = buf;
                }
            }

            if (linePoints.Length > 0)
                settings.gfxData.DrawLines(penHD, linePoints);

            if (fillType == FillType.FillAbove || fillType == FillType.FillBelow)
            {
                FillAboveOrBelow(settings, xPxStart, xPxEnd, linePoints, fillType);
            }
            else if (fillType == FillType.FillAboveAndBelow)
            {
                FillAboveAndBelow(settings, xPxStart, xPxEnd, linePoints, this.baseline);
            }
        }

        private void FillAboveOrBelow(Settings settings, float xPxStart, float xPxEnd, PointF[] linePoints, FillType fillType)
        {
            if (fillType == FillType.FillAbove || fillType == FillType.FillBelow)
            {
                float minVal = 0;
                float maxVal = (settings.dataSize.Height * (fillType == FillType.FillAbove ? -1 : 1));

                PointF first = new PointF(xPxStart, maxVal);
                PointF last = new PointF(xPxEnd, maxVal);

                PointF[] points = new PointF[] { first }
                                .Concat(linePoints)
                                .Concat(new PointF[] { last })
                                .ToArray();

                Rectangle gradientRectangle = new Rectangle(
                        new Point((int)first.X, (int)minVal - (fillType == FillType.FillAbove ? 2 : 0)),
                        new Size(
                            (int)(last.X - first.X),
                            (int)(maxVal - minVal) + 2 * (fillType == FillType.FillAbove ? -1 : 1)));

                LinearGradientBrush linearGradientBrush = new LinearGradientBrush(
                    gradientRectangle,
                    _fillColor1.Value,
                    gradientFillColor1 ?? _fillColor1.Value,
                    LinearGradientMode.Vertical);

                settings.gfxData.FillPolygon(linearGradientBrush, points);
            }
            else
            {
                throw new Exception("Invalid fill type");
            }
        }

        private PointF? GetIntersection(PointF point1, PointF point2, PointF baselineStart, PointF baselineEnd)
        {
            double a1 = point2.Y - point1.Y;
            double b1 = point1.X - point2.X;
            double c1 = a1 * (point1.X) + b1 * (point1.Y);

            double a2 = baselineEnd.Y - baselineStart.Y;
            double b2 = baselineStart.X - baselineEnd.X;
            double c2 = a2 * (baselineStart.X) + b2 * (baselineStart.Y);

            double d = a1 * b2 - a2 * b1;

            if (d == 0)
            {
                // Lines do not intersect. This could also be the case if the plot is zoomed out too much.
                return null;
            }
            else
            {
                double x = (b2 * c1 - b1 * c2) / d;
                double y = (a1 * c2 - a2 * c1) / d;
                return new PointF((float)x, (float)y);
            }
        }

        private void FillAboveAndBelow(Settings settings, float xPxStart, float xPxEnd, PointF[] linePoints, int baseline)
        {
            baseline = (int)settings.GetPixelY(baseline);

            PointF first = new PointF(xPxStart, baseline);
            PointF last = new PointF(xPxEnd, baseline);

            PointF[] points = new PointF[] { first }
                            .Concat(linePoints)
                            .Concat(new PointF[] { last })
                            .ToArray();

            PointF baselinePointStart = new PointF(linePoints[0].X, baseline);
            PointF baselinePointEnd = new PointF(linePoints[linePoints.Length - 1].X, baseline);

            var pointList = points.ToList();
            int newlyAddedItems = 0;
            for (int i = 1; i < points.Length + newlyAddedItems; ++i)
            {
                if ((pointList[i - 1].Y > baseline && pointList[i].Y < baseline) ||
                    (pointList[i - 1].Y < baseline && pointList[i].Y > baseline))
                {
                    var intersection = GetIntersection(pointList[i], pointList[i - 1], baselinePointStart, baselinePointEnd);
                    if (intersection != null)
                    {
                        pointList.Insert(i, intersection.Value);
                        newlyAddedItems++;
                        i++;
                    }
                }
            }

            // Above graph
            var aboveRect = GetFillRectangle(settings, xPxStart, xPxEnd, FillType.FillAbove);
            if (aboveRect.Height != 0 && aboveRect.Width != 0)
            {
                LinearGradientBrush linearGradientBrushAbove = new LinearGradientBrush(
                    aboveRect,
                    _fillColor1.Value,
                    gradientFillColor1 ?? _fillColor1.Value,
                    LinearGradientMode.Vertical);

                settings.gfxData.FillPolygon(linearGradientBrushAbove,
                    new PointF[] { first }
                    .Concat(pointList.Where(p => p.Y <= baseline).ToArray())
                    .Concat(new PointF[] { last })
                    .ToArray());
            }

            // Below graph
            var belowRect = GetFillRectangle(settings, xPxStart, xPxEnd, FillType.FillBelow);
            if (belowRect.Height != 0 && belowRect.Width != 0)
            {
                LinearGradientBrush linearGradientBrushBelow = new LinearGradientBrush(
                    belowRect,
                    _fillColor2.Value,
                    gradientFillColor2 ?? _fillColor2.Value,
                    LinearGradientMode.Vertical);

                settings.gfxData.FillPolygon(linearGradientBrushBelow,
                    new PointF[] { first }
                    .Concat(pointList.Where(p => p.Y >= baseline).ToArray())
                    .Concat(new PointF[] { last })
                    .ToArray());

            }

            // Draw baseline
            settings.gfxData.DrawLine(
                new Pen(Color.Black, 1),
                baselinePointStart,
                baselinePointEnd);
        }

        private Rectangle GetFillRectangle(Settings settings, float startX, float xPxEnd, FillType fillType)
        {
            float maxVal = (settings.dataSize.Height * (fillType == FillType.FillAbove ? -1 : 1));

            Rectangle rectangle = new Rectangle((int)startX, 0, (int)(xPxEnd - startX), (int)maxVal);

            return rectangle;
        }

        private void RenderHighDensityDistributionParallel(Settings settings, double offsetPoints, double columnPointCount)
        {
            int xPxStart = (int)Math.Ceiling((-1 - offsetPoints) / columnPointCount - 1);
            int xPxEnd = (int)Math.Ceiling((_ys.Length - offsetPoints) / columnPointCount);
            xPxStart = Math.Max(0, xPxStart);
            xPxEnd = Math.Min(settings.dataSize.Width, xPxEnd);
            if (xPxStart >= xPxEnd)
                return;
            List<PointF> linePoints = new List<PointF>((xPxEnd - xPxStart) * 2 + 1);

            var levelValues = Enumerable.Range(xPxStart, xPxEnd - xPxStart)
                .AsParallel()
                .AsOrdered()
                .Select(xPx =>
                {
                    // determine data indexes for this pixel column
                    int index1 = (int)(offsetPoints + columnPointCount * xPx);
                    int index2 = (int)(offsetPoints + columnPointCount * (xPx + 1));

                    if (index1 < 0)
                        index1 = 0;
                    if (index1 > _ys.Length - 1)
                        index1 = _ys.Length - 1;
                    if (index2 > _ys.Length - 1)
                        index2 = _ys.Length - 1;

                    var indexes = Enumerable.Range(0, densityLevelCount + 1).Select(x => x * (index2 - index1 - 1) / (densityLevelCount));

                    var levelsValues = new ArraySegment<T>(_ys, index1, index2 - index1)
                        .OrderBy(x => x)
                        .Where((y, i) => indexes.Contains(i)).ToArray();
                    return (xPx, levelsValues);
                })
                .ToArray();

            List<PointF[]> linePointsLevels = levelValues
                .Select(x => x.levelsValues
                                .Select(y => new PointF(x.xPx, (float)settings.GetPixelY(Convert.ToDouble(y) + yOffset)))
                                .ToArray())
                .ToList();

            for (int i = 0; i < densityLevelCount; i++)
            {
                linePoints.Clear();
                for (int j = 0; j < linePointsLevels.Count; j++)
                {
                    if (i + 1 < linePointsLevels[j].Length)
                    {
                        linePoints.Add(linePointsLevels[j][i]);
                        linePoints.Add(linePointsLevels[j][i + 1]);
                    }
                }
                settings.gfxData.DrawLines(penByDensity[i], linePoints.ToArray());


                if (fillType == FillType.FillAbove || fillType == FillType.FillBelow)
                {
                    FillAboveOrBelow(settings, xPxStart, xPxEnd, linePoints.ToArray(), fillType);
                }
                else if (fillType == FillType.FillAboveAndBelow)
                {
                    FillAboveAndBelow(settings, xPxStart, xPxEnd, linePoints.ToArray(), this.baseline);
                }
            }
        }

        public override void Render(Settings settings)
        {
            if (MaxRenderIndexLowerYSPromise)
                throw new ArgumentException("maxRenderIndex must be a valid index for ys[]");
            else if (MaxRenderIndexHigherMinRenderIndexPromise)
                throw new ArgumentException("minRenderIndex must be lower maxRenderIndex");
            else if (FillColor1MustBeSetPromise)
                throw new Exception("A fill color needs to be specified if fill is used");
            else if (FillColor2MustBeSetPromise)
                throw new Exception("Two fill colors needs to be specified if fill above and below is used");


            using (var brush = new SolidBrush(color))
            using (var penLD = GDI.Pen(color, (float)lineWidth, lineStyle, true))
            using (var penHD = GDI.Pen(color, (float)lineWidth, LineStyle.Solid, true))
            {

                double dataSpanUnits = _ys.Length * _samplePeriod;
                double columnSpanUnits = settings.axes.x.span / settings.dataSize.Width;
                double columnPointCount = (columnSpanUnits / dataSpanUnits) * _ys.Length;
                double offsetUnits = settings.axes.x.min - xOffset;
                double offsetPoints = offsetUnits / _samplePeriod;
                int visibleIndex1 = (int)(offsetPoints);
                int visibleIndex2 = (int)(offsetPoints + columnPointCount * (settings.dataSize.Width + 1));
                int visiblePointCount = visibleIndex2 - visibleIndex1;
                double pointsPerPixelColumn = visiblePointCount / settings.dataSize.Width;
                double dataWidthPx2 = visibleIndex2 - visibleIndex1 + 2;

                PointF firstPoint = settings.GetPixel(xOffset, minmaxSearchStrategy.SourceElement(0) + yOffset); ;
                PointF lastPoint = settings.GetPixel(_samplePeriod * (_ys.Length - 1) + xOffset, minmaxSearchStrategy.SourceElement(_ys.Length - 1) + yOffset);
                double dataWidthPx = lastPoint.X - firstPoint.X;

                // set this now, and let the render function change it if it happens
                markersAreVisible = false;

                // use different rendering methods based on how dense the data is on screen
                if ((dataWidthPx <= 1) || (dataWidthPx2 <= 1))
                {
                    RenderSingleLine(settings, penHD);
                }
                else if (pointsPerPixelColumn > 1)
                {
                    if (densityLevelCount > 0 && pointsPerPixelColumn > densityLevelCount)
                        RenderHighDensityDistributionParallel(settings, offsetPoints, columnPointCount);
                    else
                        RenderHighDensity(settings, offsetPoints, columnPointCount, penHD);
                }
                else
                {
                    RenderLowDensity(settings, visibleIndex1, visibleIndex2, brush, penLD, penHD);
                }
            }
        }

        public override string ToString()
        {
            string label = string.IsNullOrWhiteSpace(this.label) ? "" : $" ({this.label})";
            return $"PlottableSignalBase{label} with {GetPointCount()} points ({typeof(T).Name})";
        }

        public void SaveCSV(string filePath, string delimiter = ", ", string separator = "\n")
        {
            System.IO.File.WriteAllText(filePath, GetCSV(delimiter, separator));
        }

        public string GetCSV(string delimiter = ", ", string separator = "\n")
        {
            StringBuilder csv = new StringBuilder();
            for (int i = 0; i < _ys.Length; i++)
                csv.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}{2}{3}", xOffset + i * _samplePeriod, delimiter, minmaxSearchStrategy.SourceElement(i) + yOffset, separator);
            return csv.ToString();
        }

        public override int GetPointCount()
        {
            return _ys.Length;
        }

        public override LegendItem[] GetLegendItems()
        {
            var singleLegendItem = new Config.LegendItem(label, color)
            {
                lineStyle = lineStyle,
                lineWidth = lineWidth,
                markerShape = (markersAreVisible) ? MarkerShape.filledCircle : MarkerShape.none,
                markerSize = (markersAreVisible) ? markerSize : 0
            };
            return new LegendItem[] { singleLegendItem };
        }

        public void Render(PlotDimensions dims, Bitmap bmp)
        {
            throw new NotImplementedException();
        }

        public bool IsValidData(bool deepValidation = false)
        {
            throw new NotImplementedException();
        }
    }
}
