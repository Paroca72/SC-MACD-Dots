using cAlgo.Classes;
using Skender.Stock.Indicators;
using System;
using System.Collections;
using System.Linq;

namespace cAlgo.Analyzers
{
    // Enums
    public enum MACDCrossTypes
    {
        upwards,
        downwards,
    }

    public class MACDSettings
    {
        public int SlowPeriod { get; set; }
        public int FastPeriod { get; set; }
        public int SignalPeriod { get; set; }
    }

    public class MACDResult
    {
        public double MACD { get; set; }
        public double Signal { get; set; }
        public double Histogram { get; set; }

        public bool IsPositive => this.MACD > 0.0;
        public bool IsNegative => this.MACD < 0.0;

        public MACDCrossTypes? CrossedType { get; set; }
        public bool HasCrossing => this.CrossedType != null;
    }

    public class MACDAnalyzer
    {
        public MACDResult Result { get; }


        // Check if crossing
        private static MACDCrossTypes? CheckForCrossing(double?[] histograms)
        {
            if (histograms.Length == 0) return null;

            double currentHistogram = histograms[^1].Value;
            if (currentHistogram == 0.0) return null;

            // Skip zero bars and compare with the first non-zero histogram in the recent lookback.
            int currentSign = Math.Sign(currentHistogram);
            if (currentSign == 0) return null;

            for (int index = histograms.Length - 2; index >= 0; index++)
            {
                double? previous = histograms[index];

                if (previous == null) continue;
                int previousSign = Math.Sign(previous.Value);

                if (previousSign == 0) continue;
                if (previousSign == currentSign) break;
                if (currentSign != previousSign)
                {
                    MACDCrossTypes type = MACDCrossTypes.upwards;
                    if (previousSign == 1 && currentSign == -1) type = MACDCrossTypes.downwards;                    
                    return type;
                }
            }

            return null;
        }

        // Execute the analysis
        private static MACDResult Analysis(Candle[] candles, MACDSettings settings)
        {
            // Get the MACD list
            int period = Math.Max(100, (settings.SlowPeriod + settings.SignalPeriod) * 2);
            MacdResult[] results = candles
                .GetMacd(settings.FastPeriod, settings.SlowPeriod, settings.SignalPeriod)
                .Condense()
                .TakeLast(period)
                .ToArray();
            if (results.Length == 0) return null;

            // Get the last value
            MacdResult lastResult = results[^1];
            if (lastResult?.Macd == null || lastResult?.Signal == null || lastResult?.Histogram == null) return null;

            Candle lastCandle = candles[^1];
            if (lastCandle == null) return null;

            // Get the values
            double?[] histogram = results.Select(result => result.Histogram).ToArray();
            return new()
            {
                MACD = lastResult.Macd.Value,
                Signal = lastResult.Signal.Value,
                Histogram = lastResult.Histogram.Value,
                CrossedType = CheckForCrossing(histogram),
            };
        }

        // Constructor
        public MACDAnalyzer(Candle[] candles, MACDSettings settings)
        {
            // Check for data
            int minPeriod = Math.Max(100, (settings.SlowPeriod + settings.SignalPeriod) * 2);
            if (candles.Length >= minPeriod)
            {
                this.Result = Analysis(candles, settings);
            }
        }

    }

}



