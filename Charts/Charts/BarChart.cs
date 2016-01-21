﻿//The MIT License(MIT)

//Copyright(c) 2015 Alberto Rodriguez

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using LiveCharts.Charts;

namespace LiveCharts
{
    public class BarChart : Chart, IBar, ILine
    {
        public BarChart()
        {
            AxisY = new Axis();
            AxisX = new Axis {Separator = new Separator {Step = 1}};
            Hoverable = true;
            ShapeHoverBehavior = ShapeHoverBehavior.Shape;
            LineType = LineChartLineType.Bezier;
            MaxColumnWidth = 30;
            DefaultFillOpacity = 0.75;
        }

        #region Properties

        /// <summary>
        /// Gets or sets maximum column width, default is 60
        /// </summary>
        public double MaxColumnWidth { get; set; }

        /// <summary>
        /// Gets or sets Line Type
        /// </summary>
        public LineChartLineType LineType { get; set; }

        #endregion

        #region Overriden Methods

        protected override Point GetToolTipPosition(HoverableShape sender, List<HoverableShape> sibilings)
        {
            DataToolTip.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var unitW = ToPlotArea(1, AxisTags.X) - PlotArea.X + 5;
            var overflow = unitW - MaxColumnWidth*3 > 0 ? unitW - MaxColumnWidth*3 : 0;
            unitW = unitW > MaxColumnWidth*3 ? MaxColumnWidth*3 : unitW;
            var x = sender.Value.X + 1 > (Min.X + Max.X)/2
                ? ToPlotArea(sender.Value.X, AxisTags.X) + overflow*.5 - DataToolTip.DesiredSize.Width
                : ToPlotArea(sender.Value.X, AxisTags.X) + unitW + overflow*.5;
            var y = ToPlotArea(sibilings.Select(s => s.Value.Y).DefaultIfEmpty(0).Sum()
                               /sibilings.Count, AxisTags.Y);
            y = y + DataToolTip.DesiredSize.Height > ActualHeight
                ? y - (y + DataToolTip.DesiredSize.Height - ActualHeight) - 5
                : y;
            return new Point(x, y);
        }

        protected override void Scale()
        {
            base.Scale();

            Max.X += 1;

            S = new Point(
                 AxisX.Separator.Step ?? CalculateSeparator(Max.X - Min.X, AxisTags.X),
                 AxisY.Separator.Step ?? CalculateSeparator(Max.Y - Min.Y, AxisTags.Y));

            if (AxisY.MaxValue == null) Max.Y = (Math.Truncate(Max.Y/S.Y) + 1)*S.Y;
            if (AxisY.MinValue == null) Min.Y = (Math.Truncate(Min.Y/S.Y) - 1)*S.Y;

            DrawAxes();
        }

        protected override void DrawAxes()
        {
            ConfigureSmartAxis(AxisX);

            Canvas.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            var lastLabelX = Math.Truncate((Max.X - Min.X)/S.X)*S.X;
            var longestYLabelSize = GetLongestLabelSize(AxisY);
            var fistXLabelSize = GetLabelSize(AxisX, Min.X);
            var lastXLabelSize = GetLabelSize(AxisX, lastLabelX);

            const int padding = 5;

            var unitW = ToPlotArea(1, AxisTags.X) - PlotArea.X + 5;
            //unitW = unitW > MaxColumnWidth*3 ? MaxColumnWidth*3 : unitW;
            XOffset = unitW/2;

            PlotArea.X = padding*2 +
                         (fistXLabelSize.X*0.5 - XOffset > longestYLabelSize.X
                             ? fistXLabelSize.X*0.5 - XOffset
                             : longestYLabelSize.X);
            PlotArea.Y = longestYLabelSize.Y*.5 + padding;
            PlotArea.Height = Math.Max(0, Canvas.DesiredSize.Height - (padding*2 + fistXLabelSize.Y) - PlotArea.Y);
            PlotArea.Width = Math.Max(0, Canvas.DesiredSize.Width - PlotArea.X - padding);
            var distanceToEnd = PlotArea.Width - (ToPlotArea(Max.X, AxisTags.X) - ToPlotArea(1, AxisTags.X));
            distanceToEnd -= XOffset + padding;
            var change = lastXLabelSize.X*.5 - distanceToEnd > 0 ? lastXLabelSize.X*.5 - distanceToEnd : 0;
            if (change <= PlotArea.Width)
                PlotArea.Width -= change;

            //calculate it again to get a better result
            unitW = ToPlotArea(1, AxisTags.X) - PlotArea.X + 5;
            //unitW = unitW > MaxColumnWidth*3 ? MaxColumnWidth*3 : unitW;
            XOffset = unitW/2;

            base.DrawAxes();
        }

        #endregion
    }
}
