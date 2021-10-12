using System;
using cAlgo.API;
using cAlgo.API.Internals;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class RobBookerMissedPivotPoints : Indicator
    {
        private Bars _shortTermBars, _mediumTermBars, _longTermBars;

        private Color _shotTermColor, _mediumTermColor, _longTermColor;

        private readonly List<ChartTrendLine> _pivotPointLines = new List<ChartTrendLine>();

        [Parameter("TimeFrame", DefaultValue = "Daily", Group = "Short Term")]
        public TimeFrame ShortTermTimeFrame { get; set; }

        [Parameter("Color", DefaultValue = "Red", Group = "Short Term")]
        public string ShortTermColor { get; set; }

        [Parameter("Thickness", DefaultValue = 1, Group = "Short Term")]
        public int ShortTermThickness { get; set; }

        [Parameter("Style", DefaultValue = LineStyle.Solid, Group = "Short Term")]
        public LineStyle ShortTermStyle { get; set; }

        [Parameter("TimeFrame", DefaultValue = "Weekly", Group = "Medium Term")]
        public TimeFrame MediumTermTimeFrame { get; set; }

        [Parameter("Color", DefaultValue = "Yellow", Group = "Medium Term")]
        public string MediumTermColor { get; set; }

        [Parameter("Thickness", DefaultValue = 1, Group = "Medium Term")]
        public int MediumTermThickness { get; set; }

        [Parameter("Style", DefaultValue = LineStyle.Solid, Group = "Medium Term")]
        public LineStyle MediumTermStyle { get; set; }

        [Parameter("TimeFrame", DefaultValue = "Monthly", Group = "Long Term")]
        public TimeFrame LongTermTimeFrame { get; set; }

        [Parameter("Color", DefaultValue = "Blue", Group = "Long Term")]
        public string LongTermColor { get; set; }

        [Parameter("Thickness", DefaultValue = 1, Group = "Long Term")]
        public int LongTermThickness { get; set; }

        [Parameter("Style", DefaultValue = LineStyle.Solid, Group = "Long Term")]
        public LineStyle LongTermStyle { get; set; }

        protected override void Initialize()
        {
            _shortTermBars = MarketData.GetBars(ShortTermTimeFrame);
            _mediumTermBars = MarketData.GetBars(MediumTermTimeFrame);
            _longTermBars = MarketData.GetBars(LongTermTimeFrame);

            _shotTermColor = GetColor(ShortTermColor);
            _mediumTermColor = GetColor(MediumTermColor);
            _longTermColor = GetColor(LongTermColor);
        }

        public override void Calculate(int index)
        {
            DrawPivotPoint(ShortTermTimeFrame, _shortTermBars, index, _shotTermColor, ShortTermThickness, ShortTermStyle);
            DrawPivotPoint(MediumTermTimeFrame, _mediumTermBars, index, _mediumTermColor, MediumTermThickness, MediumTermStyle);
            DrawPivotPoint(LongTermTimeFrame, _longTermBars, index, _longTermColor, LongTermThickness, LongTermStyle);

            RemoveOrExtendLines(index);
        }

        private void DrawPivotPoint(TimeFrame timeFrame, Bars bars, int index, Color color, int thickness, LineStyle lineStyle)
        {
            var barsIndex = bars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]) - 1;

            var pivotPoints = GetPivotPoint(bars, barsIndex);

            var name = GetLineName(pivotPoints, timeFrame);

            var oldLine = _pivotPointLines.FirstOrDefault(iLine => iLine.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (oldLine != null)
            {
                oldLine.Time2 = Bars.OpenTimes[index];
            }
            else
            {
                var line = Chart.DrawTrendLine(name, Bars.OpenTimes[index], pivotPoints, Bars.OpenTimes[index], pivotPoints, color, thickness, lineStyle);

                _pivotPointLines.Add(line);
            }
        }

        private void RemoveOrExtendLines(int index)
        {
            var linesCopy = _pivotPointLines.ToArray();

            foreach (var line in linesCopy)
            {
                if ((Bars.HighPrices[index] >= line.Y1 && Bars.HighPrices[index - 1] < line.Y1) || (Bars.LowPrices[index] <= line.Y1 && Bars.LowPrices[index - 1] > line.Y1))
                {
                    Chart.RemoveObject(line.Name);

                    _pivotPointLines.Remove(line);
                }
                else
                {
                    line.Time2 = Bars.OpenTimes[index];
                }
            }
        }

        private double GetPivotPoint(Bars bars, int index)
        {
            var barsIndex = bars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);

            return (bars.HighPrices[barsIndex] + bars.LowPrices[barsIndex] + bars.ClosePrices[barsIndex]) / 3;
        }

        private string GetLineName(double pivotPoints, TimeFrame timeFrame)
        {
            return string.Format("RobBookerMissedPivotPoints_{0}_{1}", Math.Round(pivotPoints, Symbol.Digits), timeFrame);
        }

        private Color GetColor(string colorString, int alpha = 255)
        {
            var color = colorString[0] == '#' ? Color.FromHex(colorString) : Color.FromName(colorString);

            return Color.FromArgb(alpha, color);
        }
    }
}