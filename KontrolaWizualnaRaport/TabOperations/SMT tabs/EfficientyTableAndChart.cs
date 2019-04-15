using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace KontrolaWizualnaRaport.TabOperations.SMT_tabs
{
    public class EfficientyTableAndChart
    {
        public static void Show(string model)
        {
            SortedDictionary<double, int> modelHistogram = new SortedDictionary<double, int>();
            if  (DataContainer.Smt.EfficiencyHistogramPerModel.TryGetValue(model, out modelHistogram))
            {
                FillOutGrid(modelHistogram);
                DrawChart(modelHistogram);
            }
        }

        private static void FillOutGrid(SortedDictionary<double, int> modelHistogram)
        {
            DataGridView grid = SharedComponents.Smt.ModelAnalysis.dataGridViewSmtModelStats;
            grid.Rows.Clear();
            grid.Rows.Add("Wydajność na godzinę");
            grid.Rows.Add("Ilość zleceń:", modelHistogram.Select(v => v.Value).Sum());
            if (modelHistogram.Count > 0)
            {
                double min = Math.Round(modelHistogram.First().Key, 0);
                double max = Math.Round(modelHistogram.Last().Key, 0);
                double freq = Math.Round(modelHistogram.Where(v => v.Value == modelHistogram.Select(m => m.Value).Max()).First().Key, 0);
                grid.Rows.Add("Minimum:", min, min * 8);
                grid.Rows.Add("Maximum:", max, max*8);
                grid.Rows.Add("Najczęściej:", freq, freq*8);
                dgvTools.ColumnsAutoSize(grid, DataGridViewAutoSizeColumnMode.AllCells);
            }
            

            
        }

        private static void DrawChart(SortedDictionary<double, int> modelHistogram)
        {
            Chart chart = SharedComponents.Smt.ModelAnalysis.chartSmtModelAnalysis;
            chart.Series.Clear();

            if (modelHistogram.Count > 0)
            {

                chart.Series.Add(new Series
                {
                    ChartType = SeriesChartType.Spline,
                    BorderWidth = 3
                });

                foreach (var valueEntry in modelHistogram)
                {
                    DataPoint pt = new DataPoint();
                    pt.SetValueXY(valueEntry.Key, valueEntry.Value);
                    chart.Series[0].Points.Add(pt);
                }

                var yMin = chart.Series[0].Points.SelectMany(pt => pt.YValues).Min();
                var yMax = chart.Series[0].Points.SelectMany(pt => pt.YValues).Max();
                chart.ChartAreas[0].AxisY.Minimum = yMin * 0.9;
                chart.ChartAreas[0].AxisY.Maximum = yMax * 1.1;

                var xMin = chart.Series[0].Points.Select(pt => pt.XValue).Min();
                var xMax = chart.Series[0].Points.Select(pt => pt.XValue).Max();
                chart.ChartAreas[0].AxisX.Minimum = xMin * 0.9;
                chart.ChartAreas[0].AxisX.Maximum = xMax * 1.1;
                
                
                chart.ChartAreas[0].AxisY.Interval = (chart.ChartAreas[0].AxisY.Maximum - chart.ChartAreas[0].AxisY.Minimum) / 10;
            }
        }


    }
}
