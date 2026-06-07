using cAlgo.Analyzers;
using cAlgo.API;
using cAlgo.Classes;

namespace cAlgo.Drawer
{
    public class CrossingDrawer : BaseDrawer
    {
        private const string PrefixLine = "MACD_CROSS_LINE_";
        private const string PrefixIcon = "MACD_CROSS_ICON_";

        public CrossingDrawer(Indicator indicator) : base(indicator)
        {
        }

        public override void DrawPoint(int index, PointInfo point)
        {
            if (point?.CrossedType == null) return;
            ChartIconType icon = point.CrossedType == MACDCrossTypes.upwards ? ChartIconType.UpArrow : ChartIconType.DownArrow;

            string lineName = PrefixLine + index;
            string iconName = PrefixIcon + index;

            this.indicator.Chart.DrawTrendLine(
                lineName,
                index, point.Bottom,
                index, point.Top,
                point.LineColor, 1, LineStyle.DotsVeryRare
            );

            this.indicator.Chart.DrawIcon(
                iconName,
                icon,
                index,
                point.IconY,
                point.LineColor
            );

            this.AddName(lineName, index);
            this.AddName(iconName, index);
        }

        public override void DrawLabel(int index, PointInfo point)
        {
            // NOP
        }
    }

}
