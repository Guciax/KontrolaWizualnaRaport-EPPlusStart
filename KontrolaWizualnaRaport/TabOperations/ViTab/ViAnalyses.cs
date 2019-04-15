using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace KontrolaWizualnaRaport.TabOperations.ViTab
{
    public class ViAnalyses
    {
        public class ReasonAnalyses
        {
            static List<MST.MES.OrderStructureByOrderNo.NgInfo> viFilteredList = new List<MST.MES.OrderStructureByOrderNo.NgInfo>();
            public static void FilterList(string[] selectedReasons, string[] selectedModels)
            {
                viFilteredList = DataContainer.sqlDataByProcess.VisualInspection.SelectMany(o => o.Value.ngScrapList).ToList();
                if (selectedReasons.Length > 0)
                {
                    viFilteredList = viFilteredList.Where(ng => selectedReasons.Contains(ng.reasonName)).ToList();
                }
                if (selectedModels.Length > 0)
                {
                    viFilteredList = viFilteredList.Where(ng => selectedModels.Contains(ng.modelName)).ToList();
                }
            }

            public static void DrawChartWasteLevel()
            {
                Chart chart = SharedComponents.VisualInspection.AnalizaPoPrzyczynie.chartReasonLevel;
                chart.Series.Clear();
                chart.ChartAreas.Clear();

                ChartArea ar = new ChartArea();
                ar.AxisX.LabelStyle.Interval = 1;
                ar.AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
                ar.AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
                ar.AxisX.Interval = 1;
                ar.Position = new ElementPosition(0, 0, 100, 100);
                chart.ChartAreas.Add(ar);

                Series wasteLevelSeries = new Series();
                //wasteLevelSeries.ChartType = SeriesChartType.Line;
                wasteLevelSeries.IsValueShownAsLabel = true;

                Debug.WriteLine("filtring");
                var ordersOfViList = viFilteredList.Select(ng => ng.orderNo);

                var boxingByDay = DataContainer.sqlDataByProcess.Boxing.SelectMany(o => o.Value)
                                                                       .Where(m => ordersOfViList.Contains(m.orderNo))//veeeery slow
                                                                       .GroupBy(b => b.boxingDate.Date)
                                                                       .ToDictionary(d=>d.Key, b=>b.ToList());

                var viFilteredGrouppedByDay = viFilteredList.OrderBy(ng => ng.ngRegistrationDate).GroupBy(ng => ng.ngRegistrationDate.Date);

                Debug.WriteLine("foreach");
                foreach (var dateEntry in viFilteredGrouppedByDay)
                {
                    DataPoint pt = new DataPoint();

                    if (!boxingByDay.ContainsKey(dateEntry.Key)) continue;

                    var boxedThisDay = boxingByDay[dateEntry.Key].Count();

                    if (boxedThisDay == 0)
                        continue;

                    pt.SetValueXY(dateEntry.Key.ToString("dd-MM"), Math.Round((double)dateEntry.ToList().Count / (double)boxedThisDay) * 100, 2);
                    wasteLevelSeries.Points.Add(pt);
                }
                Debug.WriteLine("done");
                chart.Series.Add(wasteLevelSeries);
            }

            public static void DrawChartModelQuantityPareto()
            {
                Chart chart = SharedComponents.VisualInspection.AnalizaPoPrzyczynie.chartReasonPareto;
                chart.Series.Clear();
                chart.ChartAreas.Clear();

                Series ser = new Series();
                ser.ChartType = SeriesChartType.Column;

                ChartArea ar = new ChartArea();
                ar.AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
                ar.AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
                ar.AxisX.Interval = 1;
                ar.AxisX.LabelStyle.Interval = 1;
                ar.Position = new ElementPosition(0, 0, 100, 100);

                chart.ChartAreas.Add(ar);
                
                var ngQuantityPerModel = viFilteredList.GroupBy(ng => ng.modelName).ToDictionary(m => m.Key, q => q.ToList().Count()).OrderBy(m=>m.Value);
                foreach (var modelEntry in ngQuantityPerModel)
                {
                    ser.Points.AddXY(modelEntry.Key, modelEntry.Value);
                }

                chart.Series.Add(ser);
                
            }

            public static void DrawModelPercentagePareto()
            {
                Chart chart = SharedComponents.VisualInspection.AnalizaPoPrzyczynie.chartReasonsParetoPercentage;
                chart.Series.Clear();
                chart.ChartAreas.Clear();

                Series ser = new Series();
                ser.ChartType = SeriesChartType.Column;

                ChartArea ar = new ChartArea();
                ar.AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
                ar.AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
                ar.AxisX.Interval = 1;
                ar.AxisX.LabelStyle.Interval = 1;
                ar.Position = new ElementPosition(0, 0, 100, 100);

                chart.ChartAreas.Add(ar);

                var ngPercentagePerModel = viFilteredList.GroupBy(ng => ng.modelName).
                    ToDictionary(m => m.Key, q => (q.ToList().Count() / DataContainer.sqlDataByOrder[q.First().orderNo].ledsInBoxesList.Count))
                    .OrderBy(m=>m.Value);

                foreach (var modelEntry in ngPercentagePerModel)
                {
                    ser.Points.AddXY(modelEntry.Key, modelEntry.Value);
                }

                chart.Series.Add(ser);
            }
        }
        

        public void AnalyseReasonChart(string selectedReasons, string[] selectedModels)
        {
            
        }

    }
}
