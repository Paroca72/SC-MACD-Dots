using cAlgo.Analyzers;
using cAlgo.API;
using cAlgo.Classes;
using cAlgo.Drawer;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo.Indicators;

[Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
public class SCMACDDots : API.Indicator
{
    [Parameter("Slow Period", DefaultValue = 26, Group = "Periods", Description = "Slow EMA period")]
    public int SlowPeriod { get; set; }

    [Parameter("Fast Period", DefaultValue = 12, Group = "Periods", Description = "Fast EMA period")]
    public int FastPeriod { get; set; }

    [Parameter("Signal Period", DefaultValue = 9, Group = "Periods", Description = "Signal line period")]
    public int SignalPeriod { get; set; }


    [Parameter("Show Crossing", DefaultValue = true, Group = "Visual", Description = "Show the crossing line")]
    public bool ShowCrossing { get; set; }

    [Parameter("Position", DefaultValue = PositionTypes.Bottom, Group = "Visual", Description = "Indicator position relative to the chart")]
    public PositionTypes Position { get; set; }

    [Parameter("Offset", DefaultValue = 0, Group = "Visual", Description = "Vertical offset (+/-)")]
    public int Offset { get; set; }


    [Parameter("Positive Histogram", DefaultValue = "Green", Group = "Colors", Description = "Histogram color when the value is positive")]
    public Color HistogramPositiveColor { get; set; }

    [Parameter("Negative Histogram", DefaultValue = "Red", Group = "Colors", Description = "Histogram color when the value is negative")]
    public Color HistogramNegativeColor { get; set; }

    [Parameter("Positive MACD Crossing", DefaultValue = "LimeGreen", Group = "Colors", Description = "Crossing line color when the MACD value is positive")]
    public Color CrossingPositiveColor { get; set; }

    [Parameter("Negative MACD Crossing", DefaultValue = "IndianRed", Group = "Colors", Description = "Crossing line color when the MACD value is negative")]
    public Color CrossingNegativeColor { get; set; }


    // No line drawn
    public IndicatorDataSeries MACD { get; set; }
    public IndicatorDataSeries Signal { get; set; }
    public IndicatorDataSeries Histogram { get; set; }


    private Dictionary<int, PointInfo> points;
    private DotsDrawer dotsDrawer;
    private CrossingDrawer crossingDrawer;

    private bool debug = false;
    private bool ready = false;

    protected override void Initialize()
    {
#if DEBUG
        System.Diagnostics.Debugger.Launch();
        this.debug = true;
        Print("Debugging");
#endif

        // Init indicator
        this.points = new();
        this.dotsDrawer = new(this, "MACD", "MACD (histogram)");
        this.crossingDrawer = new(this);
        this.MACD = this.CreateDataSeries();
        this.Signal = this.CreateDataSeries();
        this.Histogram = this.CreateDataSeries();

        // Events
        Chart.ZoomChanged += (ChartZoomEventArgs obj) =>
        {
            this.UpdatePoints(false);
            this.Draw(true);
        };
        Chart.DragEnd += (ChartDragEventArgs obj) =>
        {
            this.UpdatePoints(false);
            this.Draw(true);
        };
        Chart.ScrollChanged += (ChartScrollEventArgs obj) =>
        {
            this.UpdatePoints(false);
            this.Draw(true);
        };
    }

    private double GetDotY()
    {
        double percentOffset = 0.025 + (this.Offset * 0.01);
        double chartRange = this.Chart.TopY - this.Chart.BottomY;
        double offset = chartRange * percentOffset;

        return this.Position == PositionTypes.Top
            ? this.Chart.TopY - offset
            : this.Chart.BottomY + offset;
    }

    private double GetIconY()
    {
        double top = this.Chart.TopY;
        double bottom = this.Chart.BottomY;
        return this.Position == PositionTypes.Bottom ?
            top - (top - bottom) * 0.05 :
            bottom + (top - bottom) * 0.05;
    }

    public static Color Blend(Color color, double amount)
    {
        Color colorToMix = Color.Black;
        amount = Math.Clamp(amount, 0.0, 1.0);

        byte a = (byte)(color.A + (colorToMix.A - color.A) * amount);
        byte r = (byte)(color.R + (colorToMix.R - color.R) * amount);
        byte g = (byte)(color.G + (colorToMix.G - color.G) * amount);
        byte b = (byte)(color.B + (colorToMix.B - color.B) * amount);

        return Color.FromArgb(a, r, g, b);
    }

    private Color GetColor(double histogram)
    {
        int first = this.Chart.FirstVisibleBarIndex;
        int last = this.Chart.LastVisibleBarIndex - 1;
        IEnumerable<double> filtered = this.Histogram.Skip(first).Take(last - first);

        if (histogram >= 0)
        {
            IEnumerable<double> positives = filtered.Where(value => value > 0);
            double delta = !positives.Any() ? histogram : positives.Max(value => value);

            double amount = delta != 0 ? 0.3 + (histogram / delta) * 0.7 : 1.0;
            return Blend(this.HistogramPositiveColor, 1.0 - amount);
        }
        else
        {
            IEnumerable<double> negatives = filtered.Where(value => value < 0);
            double delta = !negatives.Any() ? histogram : negatives.Min(value => value);

            double amount = delta != 0 ? 0.3 + (histogram / delta) * 0.7 : 1.0;
            return Blend(this.HistogramNegativeColor, 1.0 - amount);
        }
    }

    private MACDResult GetData(int index)
    {
        int warmup = Math.Max(100, (this.SlowPeriod + this.SignalPeriod) * 2);
        if (index - warmup <= 0) return null;
        if (this.Bars.Count < warmup) return null;

        IEnumerable<Bar> bars = this.Bars.Skip(index - warmup).Take(warmup);
        if (bars.Count() < warmup) return null;

        Candle[] candles = bars.Select(bar => Candle.FromBar(bar)).ToArray();
        MACDSettings settings = new()
        {
            SlowPeriod = this.SlowPeriod,
            FastPeriod = this.FastPeriod,
            SignalPeriod = this.SignalPeriod,
        };
        return new MACDAnalyzer(candles, settings).Result;
    }

    private void CreatePoint(int index)
    {
        MACDResult result = this.GetData(index);
        this.MACD[index] = result?.MACD ?? 0.0;
        this.Signal[index] = result?.Signal ?? 0.0;
        double value = this.Histogram[index] = result?.Histogram ?? 0.0;

        this.points[index] = new PointInfo()
        {
            Histogram = value,
            DotY = this.GetDotY(),
            IconY = this.GetIconY(),
            DotColor = this.GetColor(value),
            LineColor = result?.IsPositive ?? false ? this.CrossingPositiveColor : this.CrossingNegativeColor,
            Bottom = this.Chart.BottomY,
            Top = this.Chart.TopY,
            CrossedType = result?.CrossedType,
        };
    }

    private void UpdatePoints(bool all)
    {
        if (this.points.Count == 0) return;

        int first = all ? this.points.First().Key : this.Chart.FirstVisibleBarIndex;
        int last = all ? this.points.Last().Key : this.Chart.LastVisibleBarIndex;

        for (int index = first; index < last; index++)
        {
            this.CreatePoint(index);
        }
    }

    public void Draw(bool force)
    {
        int first = this.Chart.FirstVisibleBarIndex;
        int last = this.Chart.LastVisibleBarIndex - 1;

        if (force)
        {
            this.dotsDrawer.Redraw(first, last, this.points);
            if (this.ShowCrossing) this.crossingDrawer.Redraw(first, last, this.points);
        }
        else
        {
            this.dotsDrawer.Draw(first, last, this.points);
            if (this.ShowCrossing) this.crossingDrawer.Draw(first, last, this.points);
        }

        if (!this.ShowCrossing) this.crossingDrawer.Remove();
    }

    public override void Calculate(int index)
    {
        try
        {
            // Constraints
            if (index >= this.Bars.Count) return;

            // Update the series and create the point
            this.CreatePoint(index);

            // Draw
            if (index >= this.Chart.FirstVisibleBarIndex) ready = true;
            if (this.ready) this.Draw(false);
        }
        catch (Exception ex)
        {
            if (this.debug) this.Print(ex);
        }
    }
}