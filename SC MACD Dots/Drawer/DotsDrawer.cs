using cAlgo.API;
using cAlgo.Classes;
using System.Collections.Generic;
using System.Xml.Linq;

namespace cAlgo.Drawer
{
    public class DotsDrawer : BaseDrawer
    {
        private readonly string prefix = string.Empty;
        private readonly string label = string.Empty;

        // Constructor
        public DotsDrawer(Indicator indicator, string prefix, string title) : base(indicator)
        {
            this.prefix = prefix;
            this.label = title;
        }

        // Draw
        public override void DrawPoint(int index, PointInfo point)
        {
            int zoomLevel = this.indicator.Chart.ZoomLevel;
            double fontSize = zoomLevel == 0 ? 10 : zoomLevel / 3.0;
            string name = prefix + "_" + index;

            ChartText text = indicator.Chart.DrawText(name, "•", index - 1, point.DotY, point.DotColor);
            text.FontSize = fontSize;
            text.VerticalAlignment = VerticalAlignment.Center;
            text.ZIndex = 1000;

            this.AddName(name, index);
        }

        public override void DrawLabel(int index, PointInfo point)
        {
            string name = prefix + "_TEXT";
            this.AddName(name, -1);

            ChartText text = indicator.Chart.DrawText(name, label, index, point.DotY, Color.White);
            text.VerticalAlignment = VerticalAlignment.Center;
            text.ZIndex = 1001;
        }
    }

}
