using cAlgo.API;
using cAlgo.Classes;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo.Drawer
{
    public abstract class BaseDrawer
    {
        // Holders
        protected readonly Indicator indicator = null;
        protected readonly Dictionary<int, List<string>> names = new();

        private int firstIndex = int.MaxValue;
        private int lastIndex = int.MinValue;

        public BaseDrawer(Indicator indicator)
        {
            this.indicator = indicator;
        }

        // Manage draw
        public abstract void DrawPoint(int index, PointInfo point);

        // Manage draw
        public abstract void DrawLabel(int index, PointInfo point);

        public virtual void Draw(int first, int last, Dictionary<int, PointInfo> points)
        {
            // Check for empty values
            if (points.Count == 0) return;

            // Holders
            int firstHolder = first;
            int lastHolder = last;

            // Limit by points
            if (firstHolder < points.First().Key) firstHolder = points.First().Key;
            if (lastHolder > points.Last().Key) lastHolder = points.Last().Key;

            // Remove all objects outside the current visible range
            if (firstIndex != int.MaxValue && lastIndex != int.MinValue)
            {
                if (firstHolder > firstIndex) this.Remove(firstIndex, firstHolder - 1);
                if (lastHolder < lastIndex) this.Remove(lastHolder + 1, lastIndex);
            }

            // Update the limits to the requested draw window
            if (firstHolder >= firstIndex && firstHolder <= lastIndex) firstHolder = lastIndex;
            if (lastHolder < lastIndex && lastHolder >= firstIndex) lastHolder = firstHolder;

            firstIndex = firstHolder;
            lastIndex = lastHolder;

            // Cycle all the index
            for (int index = firstHolder; index <= lastHolder; index++)
            {
                PointInfo point = points.ContainsKey(index) ? points[index] : null;
                if (point != null)
                {
                    this.DrawPoint(index, point);
                }
            }

            // Draw label
            if (first != firstHolder)
            {
                int firstVisible = this.indicator.Chart.FirstVisibleBarIndex;
                PointInfo info = points.ContainsKey(firstVisible) ? points[firstVisible] : null;
                if (info != null) this.DrawLabel(firstVisible, info);
            }
        }

        public virtual void Redraw(int first, int last, Dictionary<int, PointInfo> points)
        {
            // Reset index
            this.firstIndex = int.MaxValue;
            this.lastIndex = int.MinValue;

            // Redraw
            this.Draw(first, last, points);
        }

        // Manage names
        public void AddName(string name, int? index = null)
        {
            index ??= this.names.Count == 0 ? 0 : this.names.Keys.Max() + 1;
            if (!this.names.ContainsKey(index.Value)) this.names[index.Value] = new();

            List<string> values = this.names[index.Value];
            if (!values.Contains(name)) values.Add(name);
        }

        // Removing utils
        public void Remove(int index)
        {
            if (names.ContainsKey(index))
            {
                foreach (string name in this.names[index])
                {
                    this.indicator.Chart.RemoveObject(name);
                }
                names.Remove(index);
            }
        }

        public void Remove(int start, int end)
        {
            for (int index = start; index <= end; index++)
            {
                this.Remove(index);
            }
        }

        public void Remove()
        {
            foreach (int index in this.names.Keys)
            {
                this.Remove(index);
            }
        }
    }
}
