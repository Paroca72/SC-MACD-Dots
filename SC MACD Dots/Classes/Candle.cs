using cAlgo.API;
using Skender.Stock.Indicators;
using System;

namespace cAlgo.Classes
{
    public class Candle : IQuote
    {
        // required base properties
        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }

        // Point info
        public PointInfo Point { get; set; }

        public static Candle FromBar(Bar bar) {
            return new()
            {
                Date = bar.OpenTime,
                Open = (decimal)bar.Open,
                High = (decimal)bar.High,
                Low = (decimal)bar.Low,
                Close = (decimal)bar.Close,
                Volume = 0,
            };
        }
    }
}
