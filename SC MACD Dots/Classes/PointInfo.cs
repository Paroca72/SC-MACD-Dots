using cAlgo.Analyzers;
using cAlgo.API;

namespace cAlgo.Classes
{
    public enum PositionTypes
    {
        Top,
        Bottom
    }

    public class PointInfo
    {
        public double Histogram { set; get; }
        public Color DotColor { set; get; }
        public Color LineColor { set; get; }

        public double DotY { set; get; }
        public double IconY { set; get; }
        public double Top { set; get; }
        public double Bottom { set; get; }

        public MACDCrossTypes? CrossedType {  set; get; }
    }

}
