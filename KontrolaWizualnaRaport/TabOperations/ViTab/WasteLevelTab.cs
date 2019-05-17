using KontrolaWizualnaRaport.CentalDataStorage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static KontrolaWizualnaRaport.Form1;

namespace KontrolaWizualnaRaport.TabOperations.ViTab
{
    public class WasteLevelTab
    {
        private class wasteAndBoxing
        {
            public List<MST.MES.OrderStructureByOrderNo.NgInfo> ngList = new List<MST.MES.OrderStructureByOrderNo.NgInfo>();
            public List<MST.MES.OrderStructureByOrderNo.BoxingInfo> boxing = new List<MST.MES.OrderStructureByOrderNo.BoxingInfo>();
        }

        private static string[] MakeDateKeySortedCollection(DateTime startDate, DateTime endDate)
        {
            List<string> result = new List<string>();
            DateTime localStartDate = startDate;
            do
            {
                string dateKey = GetDateKey(localStartDate);
                if (!result.Contains(dateKey))
                {
                    result.Add(dateKey);
                }
                localStartDate = localStartDate.AddDays(1);
            } while (localStartDate <= endDate);

            return result.ToArray();
        }
        

        private static string GetDateKey(DateTime inputDate)
        {
            if (SharedComponents.VisualInspection.PoziomOdpaduTab.radioButtonDaily.Checked)
            {
                return inputDate.ToString("dd-MMM");
            }
            else if (SharedComponents.VisualInspection.PoziomOdpaduTab.radioButtonWeekly.Checked)
            {
                return dateTools.WeekNumber(inputDate).ToString();
            }
            else
            {
                return inputDate.ToString("MMM").ToUpper();
            }
        }

        private static Dictionary<string, Dictionary<string, wasteAndBoxing>> filterOrders(DateTime startDate, DateTime endDate, List<string> selectedLines)
        {
            Dictionary<string, Dictionary<string, wasteAndBoxing>> result = new Dictionary<string, Dictionary<string, wasteAndBoxing>>();
            string[] possibleDateKeys = MakeDateKeySortedCollection(startDate, endDate);
            result.Add("Total", new Dictionary<string, wasteAndBoxing>());

            foreach (var smtLine in GlobalParameters.allLinesByHand)
            {
                result.Add(smtLine, new Dictionary<string, wasteAndBoxing>());
            }

            foreach (var lineEntry in result)
            {
                foreach (var dateKey in possibleDateKeys)
                {
                    lineEntry.Value.Add(dateKey, new wasteAndBoxing());
                }
            }

            foreach (var orderEntry in DataContainer.sqlDataByOrder)
            {
                if (orderEntry.Value.ledsInBoxesList.Count == 0) continue;
                if (orderEntry.Value.ledsInBoxesList.Select(b => b.boxingDate).Min().Date < startDate) continue;
                if (orderEntry.Value.ledsInBoxesList.Select(b => b.boxingDate).Max().Date > endDate) continue;
                if (!orderEntry.Value.smt.smtLinesInvolved.Intersect(selectedLines).Any()) continue;

                if (!SharedComponents.VisualInspection.PoziomOdpaduTab.checkBoxViLevelLg.Checked)
                {
                    if (orderEntry.Value.kitting.odredGroup == "LG")
                        continue;
                }
                if (!SharedComponents.VisualInspection.PoziomOdpaduTab.checkBoxViLevelMst.Checked)
                {
                    if (orderEntry.Value.kitting.odredGroup == "MST")
                        continue;
                }

                var maxQtyOrder = orderEntry.Value.smt.smtOrders.Select(o => o.manufacturedQty).Max();
                string smtLine = orderEntry.Value.smt.smtOrders.Where(o => o.manufacturedQty == maxQtyOrder).First().smtLine;

                foreach (var ngInfo in orderEntry.Value.visualInspection.ngScrapList)
                {
                    string dateKey = GetDateKey(ngInfo.ngRegistrationDate);
                    if (!result[smtLine].ContainsKey(dateKey)) continue;
                        //if (!filteredWasteAndBoxingKeyLineDate[smtLine].ContainsKey(dateKey))
                        //{
                        //    filteredWasteAndBoxingKeyLineDate[smtLine].Add(dateKey, new wasteAndBoxing());
                        //}
                        //if (!filteredWasteAndBoxingKeyLineDate["Total"].ContainsKey(dateKey))
                        //{
                        //    filteredWasteAndBoxingKeyLineDate["Total"].Add(dateKey, new wasteAndBoxing());
                        //}

                        result[smtLine][dateKey].ngList.Add(ngInfo);
                    result["Total"][dateKey].ngList.Add(ngInfo);
                }

                foreach (var boxInfo in orderEntry.Value.ledsInBoxesList)
                {
                    string dateKey = GetDateKey(boxInfo.boxingDate);
                    //if (!filteredWasteAndBoxingKeyLineDate[smtLine].ContainsKey(dateKey))
                    //{
                    //    filteredWasteAndBoxingKeyLineDate[smtLine].Add(dateKey, new wasteAndBoxing());
                    //}
                    //if (!filteredWasteAndBoxingKeyLineDate["Total"].ContainsKey(dateKey))
                    //{
                    //    filteredWasteAndBoxingKeyLineDate["Total"].Add(dateKey, new wasteAndBoxing());
                    //}

                    result[smtLine][dateKey].boxing.Add(boxInfo);
                    result["Total"][dateKey].boxing.Add(boxInfo);
                }
            }

            return result;
        }

        public static void DrawWasteLevelAndFillOutGrid()
        {
            if (DataContainer.VisualInspection.ngByLineThenDateKey == null) return;

            var selectedLines = SharedComponents.VisualInspection.PoziomOdpaduTab.checkedListBoxViWasteLevelSmtLines.selectedLines;
            DateTime startDate = SharedComponents.VisualInspection.PoziomOdpaduTab.dateTimePickerWasteLevelBegin.Value.Date;
            DateTime endDate = SharedComponents.VisualInspection.PoziomOdpaduTab.dateTimePickerWasteLevelEnd.Value.Date;

            var filteredWasteAndBoxingKeyLineDate = filterOrders(startDate, endDate, selectedLines);

            Chart chart = SharedComponents.VisualInspection.PoziomOdpaduTab.chartWasteLevel;

            ChartArea ar = new ChartArea();
            ar.AxisX.LabelStyle.Interval = 1;
            ar.AxisY.MajorGrid.Interval = 0.5;
            ar.AxisY.MinorGrid.Interval = 0.1;
            ar.AxisY.MinorGrid.LineColor = System.Drawing.Color.Silver;
            ar.AxisY.MinorGrid.Enabled = true;
            ar.AxisY2.MajorGrid.Enabled = false;
            ar.AxisY.LabelStyle.Format = "{0.00} %";

            Series productionLevel = new Series();
            productionLevel.ChartType = SeriesChartType.Column;
            productionLevel.Name = "Poziom produkcji [szt.]";
            productionLevel.YAxisType = AxisType.Secondary;
            productionLevel.Color = System.Drawing.Color.FromArgb(50, 243, 156, 18);
            productionLevel.BorderColor = System.Drawing.Color.FromArgb(255, 243, 156, 18);

            chart.Series.Clear();
            chart.ChartAreas.Clear();

            chart.ChartAreas.Add(ar);

            Dictionary<string, Series> ngSeriesPerLine = new Dictionary<string, Series>();
            Dictionary<string, Series> scrapSeriesPerLine = new Dictionary<string, Series>();
            CustomDataGridView grid = SharedComponents.VisualInspection.PoziomOdpaduTab.dataGridViewWasteLevel;
            grid.SuspendLayout();
            grid.Rows.Clear();

            DataTable tagTemplate = new DataTable();
            tagTemplate.Columns.Add("Zlecenie");
            tagTemplate.Columns.Add("Id");
            tagTemplate.Columns.Add("Nazwa");
            tagTemplate.Columns.Add("Ilosc SMT");
            tagTemplate.Columns.Add("Data SMT");
            tagTemplate.Columns.Add("Linia SMT");
            tagTemplate.Columns.Add("NG");
            tagTemplate.Columns.Add("NG_przyczyny");
            tagTemplate.Columns.Add("SCR");
            tagTemplate.Columns.Add("SCR_przyczyny");

            if (filteredWasteAndBoxingKeyLineDate.Count() > 0) 
            {
                foreach (var dateEntry in filteredWasteAndBoxingKeyLineDate["Total"])
                {
                    DataTable tagTable = tagTemplate.Clone();
                    double ngCount = dateEntry.Value.ngList.Where(ng => ng.typeNgScrap == "ng").Count();
                    double scrCount = dateEntry.Value.ngList.Where(ng => ng.typeNgScrap == "scrap").Count();
                    double manufactured = dateEntry.Value.boxing.Count;

                    var ngByOrder = dateEntry.Value.ngList.GroupBy(ng => ng.orderNo).ToDictionary(o => o.Key, ng => ng.ToList());

                    foreach (var orderEntry in ngByOrder)
                    {
                        //ngCount += orderEntry.Value.Where(ng => ng.typeNgScrap == "ng").Count();
                        //scrCount += orderEntry.Value.Where(ng => ng.typeNgScrap == "scrap").Count();

                        tagTable.Rows.Add(orderEntry.Key,
                            DataContainer.sqlDataByOrder[orderEntry.Key].kitting.modelId,
                            DataContainer.sqlDataByOrder[orderEntry.Key].kitting.ModelName,
                            DataContainer.sqlDataByOrder[orderEntry.Key].smt.totalManufacturedQty,
                            DataContainer.sqlDataByOrder[orderEntry.Key].smt.latestEnd,
                            string.Join(", ", DataContainer.sqlDataByOrder[orderEntry.Key].smt.smtLinesInvolved),
                            orderEntry.Value.Where(ng => ng.typeNgScrap == "ng").Count(),
                            string.Join(Environment.NewLine, orderEntry.Value.Where(ng => ng.typeNgScrap == "ng").Select(x => x.ngReason).Distinct().ToArray()),
                            orderEntry.Value.Where(ng => ng.typeNgScrap == "scrap").Count(),
                            string.Join(Environment.NewLine, orderEntry.Value.Where(ng => ng.typeNgScrap == "scrap").Select(x => x.ngReason).Distinct().ToArray()));

                    }

                    productionLevel.Points.AddXY(dateEntry.Key, manufactured);

                    grid.Rows.Add(dateEntry.Key,
                                dateEntry.Value.boxing.Count,
                                ngCount,
                                Math.Round(ngCount / manufactured * 100, 2) + "%",
                                scrCount,
                                Math.Round(scrCount / manufactured * 100, 2) + "%"
                                );


                    foreach (DataGridViewCell cell in grid.Rows[grid.Rows.Count - 1].Cells)
                    {
                        cell.Tag = tagTable;
                    }

                }

                foreach (var lineEntry in filteredWasteAndBoxingKeyLineDate)
                {

                    Series ngSeries = new Series
                    {
                        ChartType = SeriesChartType.Line,
                        BorderWidth = 3,
                        Name = $"{lineEntry.Key} Total NG [%]",
                        MarkerStyle = MarkerStyle.Circle,
                        MarkerSize = 10,
                        LegendText = lineEntry.Key,
                        Color = GlobalParameters.smtLinesColors[lineEntry.Key]
                    };

                    Series scrapSeries = new Series
                    {
                        ChartType = SeriesChartType.Line,
                        BorderWidth = 3,
                        Name = $"{lineEntry.Key} Total SCRAP [%]",
                        MarkerStyle = MarkerStyle.Square,
                        MarkerSize = 10,
                        LegendText = lineEntry.Key,
                        Color = GlobalParameters.smtLinesColors[lineEntry.Key],
                    };

                    foreach (var dateEntry in lineEntry.Value)
                    {
                        string[] toolTips = MakeToolTip(dateEntry.Value);

                        DataPoint ngPoint = new DataPoint
                        {
                            MarkerStyle = MarkerStyle.Circle,
                            MarkerSize = 12,
                            MarkerBorderColor = Color.Red,//ControlPaint.Dark(ngSeries.Color, (float).2),
                            MarkerBorderWidth = 2
                        };

                        double ngCount = (double)dateEntry.Value.ngList.Where(ng => ng.typeNgScrap == "ng").Count();
                        double manufactured = (double)dateEntry.Value.boxing.Count;
                        double ngY = 0;
                        if (manufactured > 0) { ngY = ngCount / manufactured * 100; }
                        ngPoint.SetValueXY(dateEntry.Key, ngY);
                        string ngtoolTip = toolTips[0];
                        ngPoint.ToolTip = ngtoolTip;
                        ngSeries.Points.Add(ngPoint);

                        DataPoint scrapPoint = new DataPoint
                        {
                            MarkerStyle = MarkerStyle.Triangle,
                            MarkerSize = 12,
                            MarkerBorderColor = Color.Black,//ControlPaint.Dark(scrapSeries.Color, (float).2),
                            MarkerBorderWidth = 2
                        };
                        double scrCount = (double)dateEntry.Value.ngList.Where(ng => ng.typeNgScrap == "scrap").Count();
                        double scrapY = 0;
                        if (manufactured > 0)
                        {
                            scrapY= scrCount / manufactured * 100;
                        }
                        
                        scrapPoint.SetValueXY(dateEntry.Key, scrapY);
                        string scraptoolTip = toolTips[1];
                        scrapPoint.ToolTip = scraptoolTip;
                        scrapSeries.Points.Add(scrapPoint);


                    }

                    ngSeriesPerLine.Add(lineEntry.Key, ngSeries);
                    scrapSeriesPerLine.Add(lineEntry.Key, scrapSeries);
                }

                chart.Series.Add(productionLevel);
                foreach (var lineEntry in ngSeriesPerLine)
                {
                    if (SharedComponents.VisualInspection.PoziomOdpaduTab.radioButtonViLinesCumulated.Checked &
                        lineEntry.Key.Equals("Total"))
                    {
                        chart.Series.Add(ngSeriesPerLine[lineEntry.Key]);
                        chart.Series.Add(scrapSeriesPerLine[lineEntry.Key]);
                    }
                    else if (!SharedComponents.VisualInspection.PoziomOdpaduTab.radioButtonViLinesCumulated.Checked & !lineEntry.Key.Equals("Total"))
                    {
                        chart.Series.Add(ngSeriesPerLine[lineEntry.Key]);
                        chart.Series.Add(scrapSeriesPerLine[lineEntry.Key]);
                    }
                }

                grid.ResumeLayout();
                dgvTools.ColumnsAutoSize(grid, DataGridViewAutoSizeColumnMode.AllCellsExceptHeader);

                if (SharedComponents.VisualInspection.PoziomOdpaduTab.checkBoxEnableZoom.Checked)
                {
                    double maxY = 0;
                    foreach (Series ser in chart.Series)
                    {
                        if (ser.Name == "Poziom produkcji [szt.]") continue;
                        foreach (DataPoint pt in ser.Points)
                        {
                            if (pt.YValues.Max() > maxY)
                            {
                                maxY = pt.YValues.Max();
                            }
                        }
                    }

                    chart.ChartAreas[0].AxisY.Maximum =
                        maxY * (100 - SharedComponents.VisualInspection.PoziomOdpaduTab.vScrollBarZoomChart.Value) /
                        100;
                }

                chart.Legends.Clear();

            }
        }
        private static string[] MakeToolTip(wasteAndBoxing value)
        {
            string[] result = new string[] { "", "" };
            var ng = value.ngList.Where(entry=> entry.typeNgScrap=="ng")
                                 .GroupBy(o => o.orderNo)
                                 .ToDictionary(x => x.Key, v => v.ToList().Count)
                                 .OrderByDescending(o => o.Value);

            var scrap = value.ngList.Where(entry => entry.typeNgScrap == "scrap")
                                 .GroupBy(o => o.orderNo)
                                 .ToDictionary(x => x.Key, v => v.ToList().Count)
                                 .OrderByDescending(o => o.Value);

            //ng.ToList().Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
            //scrap.ToList().Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

            foreach (var orderEntry in ng)
            {
                result[0] += $"{DataContainer.sqlDataByOrder[orderEntry.Key].kitting.modelId} - {orderEntry.Value}szt." + Environment.NewLine;
            }
            foreach (var orderEntry in scrap)
            {
                result[1] += $"{DataContainer.sqlDataByOrder[orderEntry.Key].kitting.modelId} - {orderEntry.Value}szt." + Environment.NewLine;
            }
            return result;
        }

    }
}
