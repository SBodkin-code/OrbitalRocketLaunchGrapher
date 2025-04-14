using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
namespace OrbitalManeuverOptimizer
{
    class HelperFunctions
    {
        public static double[] ProcessPythonArray(PyObject Array)
        {
            double[] arrayResult = new double[Array.Length()];
            try
            {
                using (Py.GIL())
                {
                    // Import numpy for conversion
                    dynamic np = Py.Import("numpy");

                    // Ensure consistent numpy array type
                    dynamic array = np.array(Array, dtype: np.float64);
                    int lengthValue = array.size;
                    // Get values from numpy list
                    for (int x = 0; x < lengthValue; x++)
                    {
                        arrayResult[x] = array[x];
                    }
                    return arrayResult;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to process PyObject Array: {ex.Message}",
                    ex
                );
            }
        }

        public static void PopulateChartFromPython(dynamic function, Chart chart, string seriesName)
        {
            var Arrays = function;
            // Earth X and Y Coords
            double[] X = HelperFunctions.ProcessPythonArray(Arrays["planetX"]);
            double[] Y = HelperFunctions.ProcessPythonArray(Arrays["planetY"]);
            var chartSeries = new Series(chart.Text)
            {
                Name = seriesName,
            };

            for (int x = 0; x < X.Length; x++)
            {
                chartSeries.Points.AddXY(X[x], Y[x]);
            }
            chartSeries.ChartType = SeriesChartType.Line;
            chart.Series.Add(chartSeries);
            CenterGraph(chart, X, Y);
        }

        public static void CenterGraph(Chart chart, double[] xValues, double[] yValues)
        {
            if (Math.Abs(yValues.Min()) + Math.Abs(yValues.Max()) > Math.Abs(xValues.Min()) + Math.Abs(xValues.Max()))
            {
                if (Math.Abs(yValues.Min()) > Math.Abs(yValues.Max()))
                {
                    chart.ChartAreas[0].AxisY.Minimum = yValues.Min();
                    chart.ChartAreas[0].AxisY.Maximum = Math.Abs(yValues.Min());
                    chart.ChartAreas[0].AxisX.Minimum = yValues.Min();
                    chart.ChartAreas[0].AxisX.Maximum = Math.Abs(yValues.Min());
                }
                else
                {
                    chart.ChartAreas[0].AxisY.Minimum = -1 * yValues.Max();
                    chart.ChartAreas[0].AxisY.Maximum = yValues.Max();
                    chart.ChartAreas[0].AxisX.Minimum = -1 * yValues.Max();
                    chart.ChartAreas[0].AxisX.Maximum = yValues.Max();

                }
            }
            else
            {
                if (Math.Abs(xValues.Min()) > Math.Abs(xValues.Max()))
                {
                    chart.ChartAreas[0].AxisX.Minimum = xValues.Min();
                    chart.ChartAreas[0].AxisX.Maximum = Math.Abs(xValues.Min());
                    chart.ChartAreas[0].AxisY.Minimum = xValues.Min();
                    chart.ChartAreas[0].AxisY.Maximum = Math.Abs(xValues.Min());
                }
                else
                {
                    chart.ChartAreas[0].AxisX.Minimum = -1 * xValues.Max();
                    chart.ChartAreas[0].AxisX.Maximum = xValues.Max();
                    chart.ChartAreas[0].AxisY.Minimum = -1 * xValues.Max();
                    chart.ChartAreas[0].AxisY.Maximum = xValues.Max();
                }
            }

            // Add tollerance for X and Y

            chart.ChartAreas[0].AxisX.Maximum += xValues.Max() / 10;
            chart.ChartAreas[0].AxisX.Minimum += xValues.Min() / 10;
            chart.ChartAreas[0].AxisY.Maximum += yValues.Max() / 10;
            chart.ChartAreas[0].AxisY.Minimum += yValues.Min() / 10;
        }

        public static Chart CreateChart(string chartName, string title, Color lineColor, string xAxisTitle, string yAxisTitle)
        {
            Chart chart = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Name = chartName
            };

            ChartArea chartArea = new ChartArea();
            chartArea.AxisX.Title = xAxisTitle;
            chartArea.AxisY.Title = yAxisTitle;
            chartArea.AxisX.LabelStyle.Format = "0.0";
            chartArea.AxisY.LabelStyle.Format = "0.0";
            chart.ChartAreas.Add(chartArea);


            Title chartTitle = new Title();
            chartTitle.Alignment = ContentAlignment.TopCenter;
            chartTitle.Font = new Font("Arial", 16F, System.Drawing.FontStyle.Bold);
            chartTitle.Name = title;
            chartTitle.Text = title;
            chart.Titles.Add(chartTitle);

            return chart;
        }

        public static void RealignSeries(Chart chart)
        {
            if (chart.Series.Count == 0)
            {
                return;
            }
            var noSeries = chart.Series.Count;
            var PointsX = chart.Series[0].Points.Select(x => x.XValue);
            var PointsY = chart.Series[0].Points.Select(x => x.YValues[0]);
            var minX = PointsX.Min();
            var minY = PointsY.Min();
            var maxX = PointsX.Max();
            var maxY = PointsY.Max();

            chart.ChartAreas[0].AxisX.Minimum = maxX - (maxX - minX) * 2;
            chart.ChartAreas[0].AxisX.Maximum = Math.Abs(maxX) + Math.Abs(minX / 100);


            chart.ChartAreas[0].AxisY.Minimum = maxY - (maxY - minY) * 2;
            chart.ChartAreas[0].AxisY.Maximum = maxY;
        }

        public static void PopulateChart(Chart chart, string chartName, double[] XValues, double[] YValues, bool clearPreviousSeries = true)
        {
            if (clearPreviousSeries)
            {
                foreach (Series series in chart.Series.ToList())
                {
                    if (series.Name.StartsWith(chartName))
                    {
                        chart.Series.Remove(series);
                    }
                }
            }

            Series chartSeries = new Series
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 2,
                Name = chartName,
                MarkerSize = 2,
            };

            for (int x = 0; x < XValues.Length; x++)
            {
                chartSeries.Points.AddXY(XValues[x], YValues[x]);
            }
            chartSeries.ChartType = SeriesChartType.Line;
            chart.Series.Add(chartSeries);

            // for Earth Perspective frame make the X and Y axis same to not stretch the earth
            if (chartName == "satPositionChart")
            {
                HelperFunctions.CenterGraph(chart, XValues, YValues);
            }

        }
    }
      
}