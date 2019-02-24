using KontrolaWizualnaRaport.CentalDataStorage;
using KontrolaWizualnaRaport.TabOperations.SMT_tabs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static KontrolaWizualnaRaport.Form1;
using static KontrolaWizualnaRaport.SharedComponents.Smt;
using static KontrolaWizualnaRaport.SMTOperations;
using static KontrolaWizualnaRaport.TabOperations.SMT_tabs.LedWasteTabOperations;

namespace KontrolaWizualnaRaport
{
    class Charting
    {
        //public static DataTable DrawCapaChart(Chart chart, List<WasteDataStructure> inputData, string oper, Dictionary<string, string> modelDictionary, bool customerLGI, List<excelOperations.order12NC> mstOrders)
        //{
        //    chart.Series.Clear();
        //    chart.ChartAreas.Clear();

        //    Series serColumn = new Series();
        //    serColumn.IsVisibleInLegend = false;
        //    serColumn.IsValueShownAsLabel = false;
        //    serColumn.Color = System.Drawing.Color.Blue;
        //    serColumn.BorderColor = System.Drawing.Color.Black;

        //    ChartArea area = new ChartArea();
        //    area.AxisX.IsLabelAutoFit = true;
        //    area.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.LabelsAngleStep45;
        //    area.AxisX.LabelStyle.Enabled = true;
        //    area.AxisX.LabelStyle.Font = new System.Drawing.Font("Arial", 10);
        //    area.AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
        //    area.AxisY.MajorGrid.LineColor = System.Drawing.Color.Black;
        //    area.AxisY.MajorGrid.LineWidth = 1;
        //    area.AxisY.MinorGrid.Enabled = true;
        //    area.AxisY.MinorGrid.LineColor = System.Drawing.Color.Silver;
        //    area.AxisY.MinorGrid.Interval = 1000;
        //    area.AxisY.MajorGrid.Interval = 5000;
        //    area.AxisX.LabelStyle.Interval = 1;
        //    area.AxisY.LabelStyle.Interval = 5000;

        //    Dictionary<DateTime, int> qtyPerDayForAll = new Dictionary<DateTime, int>();
        //    Dictionary<DateTime, Dictionary<string, int>> qtyPerDayPerModel = new Dictionary<DateTime, Dictionary<string, int>>();
        //    DataTable gridTable = new DataTable();
        //    gridTable.Columns.Add("Data");
        //    gridTable.Columns.Add("Ilość");

        //    if (oper == "Wszyscy")
        //    {
        //        Dictionary<DateTime, string> labelDict = new Dictionary<DateTime, string>();
        //        foreach (var item in inputData)
        //        {
        //            if (item.Oper == oper || oper == "Wszyscy")
        //            {
        //               // string orderList = string.Join(Environment.NewLine, mstOrders.Select(o => o.order).ToArray());
        //                if (customerLGI & mstOrders.Select(o => o.order).ToList().Contains(item.NumerZlecenia)) continue;
        //                if (!customerLGI & !mstOrders.Select(o => o.order).ToList().Contains(item.NumerZlecenia)) continue;

        //                    string model = "???";
        //                if (modelDictionary.ContainsKey(item.NumerZlecenia))
        //                    model = modelDictionary[item.NumerZlecenia];

        //                if (!qtyPerDayForAll.ContainsKey(item.FixedDateTime.Date))
        //                {
        //                    qtyPerDayForAll.Add(item.FixedDateTime.Date, 0);
        //                    labelDict.Add(item.FixedDateTime.Date, model);
        //                }
        //                qtyPerDayForAll[item.FixedDateTime.Date] += item.AllQty;
        //                if (!labelDict[item.FixedDateTime.Date].Contains(model))
        //                    labelDict[item.FixedDateTime.Date] += Environment.NewLine + model;
        //            }
        //        }

        //        serColumn.ChartType = SeriesChartType.Column;
        //        foreach (var key in qtyPerDayForAll)
        //        {
        //            serColumn.Points.AddXY(key.Key, key.Value);
        //            gridTable.Rows.Add(key.Key.Date.ToShortDateString(), key.Value);
        //        }
        //        chart.Series.Add(serColumn);
        //    }
        //    else
        //    {
        //        gridTable.Columns.Add("NG");
        //        gridTable.Columns.Add("NG%");
        //        gridTable.Columns.Add("Scrap");
        //        gridTable.Columns.Add("Scrap%");
        //        HashSet<string> uniqueModels = new HashSet<string>();
        //        HashSet<DateTime> uniqueDates = new HashSet<DateTime>();
        //        Dictionary<string, Dictionary<DateTime, int>> dictFirstModelThenDate = new Dictionary<string, Dictionary<DateTime, int>>();

        //        foreach (var item in inputData)
        //        {
        //            if (customerLGI & mstOrders.Select(o => o.order).ToList().Contains(item.NumerZlecenia)) continue;
        //            if (!customerLGI & !mstOrders.Select(o => o.order).ToList().Contains(item.NumerZlecenia)) continue;

        //            if (item.Oper != oper) continue;

        //                if (item.Oper == oper)
        //            {
        //                if (!qtyPerDayPerModel.ContainsKey(item.FixedDateTime.Date))
        //                {
        //                    qtyPerDayPerModel.Add(item.FixedDateTime.Date, new Dictionary<string, int>());
        //                }
        //                string model = "??";

        //                if (modelDictionary.ContainsKey(item.NumerZlecenia))
        //                    model = modelDictionary[item.NumerZlecenia].Replace("LLFML", "");

        //                uniqueModels.Add(model);
        //                uniqueDates.Add(item.FixedDateTime.Date);

        //                if (!qtyPerDayPerModel[item.FixedDateTime.Date].ContainsKey(model))
        //                    qtyPerDayPerModel[item.FixedDateTime.Date].Add(model, 0);

        //                qtyPerDayPerModel[item.FixedDateTime.Date][model] += item.AllQty;
        //            }
        //        }

        //        serColumn.ChartType = SeriesChartType.StackedColumn;
        //        Dictionary<DateTime, int> qtyPerDayPerOperator = new Dictionary<DateTime, int>();
        //        Dictionary<DateTime, int> qtyNgPerDayPerOperator = new Dictionary<DateTime, int>();
        //        Dictionary<DateTime, int> qtyScrapPerDayPerOperator = new Dictionary<DateTime, int>();
        //        foreach (var model in uniqueModels)
        //        {

        //            dictFirstModelThenDate.Add(model, new Dictionary<DateTime, int>());
        //            foreach (var date in uniqueDates)
        //            {
        //                dictFirstModelThenDate[model].Add(date, 0);

        //                if (!qtyPerDayPerOperator.ContainsKey(date))
        //                {
        //                    qtyPerDayPerOperator.Add(date, 0);
        //                    qtyNgPerDayPerOperator.Add(date, 0);
        //                    qtyScrapPerDayPerOperator.Add(date, 0);

        //                }
        //            }
        //        }

        //        foreach (var item in inputData)
        //        {
        //            if (item.Oper == oper)
        //            {

        //                string model = "??";
        //                if (modelDictionary.ContainsKey(item.NumerZlecenia))
        //                {
        //                    model = modelDictionary[item.NumerZlecenia].Replace("LLFML", "");
        //                }
        //                if (!dictFirstModelThenDate.ContainsKey(model)) continue;
        //                dictFirstModelThenDate[model][item.FixedDateTime.Date] += item.AllQty;
        //                qtyPerDayPerOperator[item.FixedDateTime.Date] += item.AllQty;
        //                qtyNgPerDayPerOperator[item.FixedDateTime.Date] += item.AllNg;
        //                qtyScrapPerDayPerOperator[item.FixedDateTime.Date] += item.AllScrap;
        //            }
        //        }

        //        int total = qtyPerDayPerOperator.Select(q => q.Value).Sum();
        //        int ngTotal = qtyNgPerDayPerOperator.Select(q => q.Value).Sum();
        //        double totalNgRate = Math.Round((double)ngTotal / (double)total * 100, 2);
        //        int scrapTotal = qtyScrapPerDayPerOperator.Select(q => q.Value).Sum();
        //        double totalScrapRate = Math.Round((double)scrapTotal / (double)total * 100, 2);
        //        gridTable.Rows.Add("TOTAL", total, ngTotal, totalNgRate, scrapTotal, totalScrapRate);

        //        foreach (var keyEntry in qtyPerDayPerOperator)
        //        {
        //            int ng = qtyNgPerDayPerOperator[keyEntry.Key];
        //            double ngRate = Math.Round((double)ng / (double)keyEntry.Value * 100, 2);
        //            int scrap = qtyScrapPerDayPerOperator[keyEntry.Key];
        //            double scrapRate = Math.Round((double)scrap / (double)keyEntry.Value * 100, 2);

        //            gridTable.Rows.Add(keyEntry.Key.ToString("dd-MM-yyyy"), keyEntry.Value, ng, ngRate, scrap, scrapRate);

        //        }

        //        foreach (var model in dictFirstModelThenDate)
        //        {
        //            chart.Series.Add(new Series(model.Key));
        //            chart.Series[model.Key].ChartType = SeriesChartType.StackedColumn;
        //            chart.Series[model.Key].IsValueShownAsLabel = true;
        //            chart.Series[model.Key].ToolTip = model.Key;

        //            foreach (var date in model.Value)
        //            {
        //                {
        //                    //DataPoint point = new DataPoint();
        //                    //point.SetValueXY(date.Key, date.Value);
                            
        //                   // if (date.Value > 0)
        //                        //point.Label = date.Value + " " + model.Key;

        //                    //chart.Series[model.Key].Points.Add(point);
        //                    chart.Series[model.Key].Points.AddXY(date.Key, date.Value);
        //                }
        //            }

        //            foreach (var point in chart.Series[model.Key].Points)
        //            {
        //                if (point.YValues[0] == 0) point.IsValueShownAsLabel = false;
        //            }
        //        }

        //        area.AxisY.LabelStyle.Interval = 500;
        //        area.AxisY.MinorGrid.Interval = 100;
        //        area.AxisY.MajorGrid.Interval = 500;
        //    }
        //    chart.ChartAreas.Add(area);
        //    //chart.Legends[0].DockedToChartArea = chart.ChartAreas[0].Name;
        //    //chart.Legends[0].TableStyle = LegendTableStyle.Auto;
        //    chart.Legends.Clear();

           
        //    return gridTable;
        //}

        private class WasteStruc
        {
            public string name;
            public int qty;
        }

        public static void DrawLedWasteForDetailView (DataTable inputTable, Chart chart)
        {
            chart.ChartAreas.Clear();
            chart.Series.Clear();

            ChartArea ar = new ChartArea();
            //ar.AxisX.LabelStyle.Interval = 1;
           // ar.AxisX.MajorGrid.Interval = 1;
            //ar.AxisY.MajorGrid.Interval = 0.5;
            //ar.AxisY.MinorGrid.Interval = 0.1;
            //ar.AxisY.MajorGrid.Interval = 0.5;

            ar.AxisY.MinorGrid.LineColor = System.Drawing.Color.Silver;
            ar.AxisY.MajorGrid.LineColor = System.Drawing.Color.Gray;
            ar.AxisY.MinorGrid.Enabled = true;
            ar.AxisY.LabelStyle.Format = "{0.00} %";
            ar.AxisX.IsMarginVisible = false;

            chart.ChartAreas.Add(ar);

            Series lineSeriesA = new Series();
            lineSeriesA.ChartType = SeriesChartType.Line;
            lineSeriesA.BorderWidth = 3;
            lineSeriesA.Name = "RankA";

            Series lineSeriesB = new Series();
            lineSeriesB.ChartType = SeriesChartType.Line;
            lineSeriesB.BorderWidth = 3;
            lineSeriesB.Name = "RankB";

            //template.Columns.Add("Mont.A");
            //template.Columns.Add("Odpad_A");

            foreach (DataRow row in inputTable.Rows)
            {
                string date = row["Data"].ToString();
                double valueA = Math.Round(double.Parse(row["Odp_A"].ToString()) / double.Parse(row["Mont.A"].ToString()) * 100, 2);
                double valueB = Math.Round(double.Parse(row["Odp_B"].ToString()) / double.Parse(row["Mont.B"].ToString()) * 100, 2);
                DataPoint ptA = new DataPoint();
                ptA.SetValueXY(date, valueA);
                lineSeriesA.Points.Add(ptA);

                DataPoint ptB = new DataPoint();
                ptB.SetValueXY(date, valueB);
                lineSeriesB.Points.Add(ptB);

            }

            chart.Series.Add(lineSeriesA);
            chart.Series.Add(lineSeriesB);
        }

        private class ledUsedBom
        {
            public int ledsUsed=0;
            public int ledsBom=0;
        }

        public static void DrawLedWasteChart()
        {
            SortedDictionary<DateTime, SortedDictionary<int, List<LotLedWasteStruc>>> ledWasteDictionary = LedWasteTabOperations.ledWasteDictionary;
            Chart chart = SharedComponents.Smt.LedWasteTab.chartLedWasteChart;
            string frequency = SharedComponents.Smt.LedWasteTab.comboBoxSmtLewWasteFreq.Text;
            Dictionary<string, bool> selectedLines = SharedComponents.Smt.LedWasteTab.selectedLines;
            Dictionary<string, Dictionary<string, double>> dataPointsProd = new Dictionary<string, Dictionary<string, double>>();
            Dictionary<string, Dictionary<string, ledUsedBom>> dataPointsWaste = new Dictionary<string, Dictionary<string, ledUsedBom>>();


            foreach (var smtLine in GlobalParameters.allLinesByHand)
            {
                dataPointsWaste.Add(smtLine, new Dictionary<string, ledUsedBom>());
            }
            dataPointsWaste.Add("Total", new Dictionary<string, ledUsedBom>());

            //dataPointsProd.Add("Total", new Dictionary<string, double>());
            //dataPointsDropped.Add("Total", new Dictionary<string, double>());

            
            foreach (var dayEntry in ledWasteDictionary)
            {

                string chartFrequency = SharedComponents.Smt.LedWasteTab.comboBoxSmtLewWasteFreq.Text;
                string dateKey = dayEntry.Key.ToString("dd-MMM");
                if (chartFrequency == "Tygodniowo") { dateKey = dateTools.WeekNumber(dayEntry.Key).ToString(); }
                if (chartFrequency == "Miesiecznie") { dateKey = dayEntry.Key.ToString("MMM"); }

                foreach (var shiftEntry in dayEntry.Value)
                {
                    var grouppedByLine = shiftEntry.Value.GroupBy(k => k.smtLine).ToDictionary(k => k.Key, v => v.ToList());
                    foreach (var smtLine in grouppedByLine)
                    {
                        if (!selectedLines[smtLine.Key]) continue;
                        if (!dataPointsWaste[smtLine.Key].ContainsKey(dateKey))
                        {
                            dataPointsWaste[smtLine.Key].Add(dateKey, new ledUsedBom());
                            if (selectedLines["Total"])
                            {
                                if (!dataPointsWaste["Total"].ContainsKey(dateKey))
                                {
                                    dataPointsWaste["Total"].Add(dateKey, new ledUsedBom());
                                }
                            }
                        }
                        dataPointsWaste[smtLine.Key][dateKey].ledsUsed += smtLine.Value.Select(o => o.ledsUsed).Sum();
                        dataPointsWaste[smtLine.Key][dateKey].ledsBom += smtLine.Value.Select(o => o.ledsUsageFromBom).Sum();
                        if (selectedLines["Total"])
                        {
                            dataPointsWaste["Total"][dateKey].ledsBom += smtLine.Value.Select(o => o.ledsUsageFromBom).Sum();
                        }
                        
                    }
                }
            }
   
            chart.Series.Clear();
            chart.ChartAreas.Clear();

            ChartArea ar = new ChartArea();
            ar.AxisX.LabelStyle.Interval = 1;
            ar.AxisX.MajorGrid.Interval = 1;
            ar.AxisY.MajorGrid.Interval = 0.5;
            ar.AxisY.MinorGrid.Interval = 0.1;
            ar.AxisY.MajorGrid.Interval = 0.5;

            ar.AxisY.MinorGrid.LineColor = System.Drawing.Color.Silver;
            ar.AxisY.MajorGrid.LineColor = System.Drawing.Color.Gray;
            ar.AxisY.MinorGrid.Enabled = true;
            ar.AxisY2.Enabled = AxisEnabled.True;

            ar.AxisY2.MajorGrid.Enabled = false;
            ar.AxisY.LabelStyle.Format = "{0.00} %";
            ar.AxisY2.LabelStyle.Format = "{0.00} %";
            ar.AxisX.IsMarginVisible = false;
            ar.AxisY.IsMarginVisible = false;
            ar.Position.X = 0;
            ar.Position.Width = 100;
            ar.Position.Height = 100;
            ar.Position.Y = 0;


            chart.ChartAreas.Add(ar);
            double maxY = 0;
            foreach (var lineEntry in dataPointsWaste)
            {

                Series lineSeries = new Series();
                lineSeries.ChartType = SeriesChartType.Line;
                lineSeries.BorderWidth = 3;
                lineSeries.Name = lineEntry.Key;
                lineSeries.Color = GlobalParameters.smtLinesColors[lineEntry.Key];


                foreach (var dateKeyEntry in lineEntry.Value)
                {
                    DataPoint ngPoint = new DataPoint();
                    double waste = Math.Round((double)(dataPointsWaste[lineEntry.Key][dateKeyEntry.Key].ledsUsed - dataPointsWaste[lineEntry.Key][dateKeyEntry.Key].ledsBom) / (double)dataPointsWaste[lineEntry.Key][dateKeyEntry.Key].ledsBom * 100, 2);
                    ngPoint.MarkerSize = 8;
                    ngPoint.MarkerStyle = MarkerStyle.Circle;
                    if (waste > maxY) maxY = waste;
                    ngPoint.SetValueXY(dateKeyEntry.Key, waste);
                    //ngPoint.ToolTip = ngtoolTip;
                    lineSeries.Points.Add(ngPoint);
                }
                chart.Series.Add(lineSeries);
            }
            chart.ChartAreas[0].AxisY.Maximum = maxY * 1.1;

            
            Series productionLevel = new Series();
            productionLevel.ChartType = SeriesChartType.Column;
            productionLevel.Name = "Użycie LED [szt.]";
            productionLevel.YAxisType = AxisType.Secondary;
            productionLevel.Color = System.Drawing.Color.AliceBlue;
            productionLevel.BorderColor = System.Drawing.Color.Silver;

            foreach (var dateKey in dataPointsWaste["Total"])
            {
                DataPoint pt = new DataPoint();
                pt.SetValueXY(dateKey.Key, dateKey.Value.ledsUsed);
                productionLevel.Points.Add(pt);
            }


            chart.Series.Add(productionLevel);

        }
            
        private static WasteStruc CreateWasteStruc(string name)
        {
            WasteStruc result = new WasteStruc();
            result.name = name;
            result.qty = 0;
            return result;
        }

        private static int FindIndexOfWaste(string name, List<WasteStruc> searchList)
        {
            int result = 0;
            for (int i = 0; i < searchList.Count; i++)
            {
                if (searchList[i].name == name)
                {
                    return i;

                }
            }
            return result;
        }

        public class WasteLevelChartStruct
        {
            public int ngCount { get; set; }
            public int scrapCount { get; set; }
            public int totalProdThisLine { get; set; }
            public Dictionary<string, Tuple<int, int>> ngTooltip { get; set; }
            public Dictionary<string, Tuple<int, int>> scrapToolTip { get; set; }
        }


        public static void DrawWasteLevel()
        {
            if (DataContainer.VisualInspection.wasteReasonsByLineThenDateKey == null) return;
            var filteredOrders = new Dictionary<string, Dictionary<string, List<MST.MES.OrderStructureByOrderNo.OneOrderData>>>();
            var selectedLines = SharedComponents.VisualInspection.PoziomOdpaduTab.checkedListBoxViWasteLevelSmtLines.selectedLines;

            filteredOrders.Add("Total", new Dictionary<string, List<MST.MES.OrderStructureByOrderNo.OneOrderData>>());

            foreach (var lineEntry in DataContainer.VisualInspection.wasteReasonsByLineThenDateKey)
            {
                Debug.WriteLine(lineEntry.Key + " " + lineEntry.Value.Keys.First() + "-" + lineEntry.Value.Keys.Last());
                foreach (var dateEntry in lineEntry.Value)
                {
                    if (!filteredOrders["Total"].ContainsKey(dateEntry.Key))
                    {
                        filteredOrders["Total"].Add(dateEntry.Key, new List<MST.MES.OrderStructureByOrderNo.OneOrderData>());
                    }
                    foreach (var orderEntry in dateEntry.Value)
                    {
                        if (!SharedComponents.VisualInspection.PoziomOdpaduTab.checkBoxViLevelLg.Checked)
                        {
                            if (orderEntry.kitting.odredGroup == "LG")
                                continue;
                        }
                        if (!SharedComponents.VisualInspection.PoziomOdpaduTab.checkBoxViLevelMst.Checked)
                        {
                            if (orderEntry.kitting.odredGroup == "MST")
                                continue;
                        }
                        if (!SharedComponents.VisualInspection.PoziomOdpaduTab.radioButtonViLinesCumulated.Checked)
                        {
                            if (!orderEntry.smt.smtOrders.Select(o => o.smtLine).Intersect(selectedLines).Any())
                                continue;
                        }

                        if (!filteredOrders.ContainsKey(lineEntry.Key))
                        {
                            filteredOrders.Add(lineEntry.Key, new Dictionary<string, List<MST.MES.OrderStructureByOrderNo.OneOrderData>>());
                        }
                        if (!filteredOrders[lineEntry.Key].ContainsKey(dateEntry.Key))
                        {
                            filteredOrders[lineEntry.Key].Add(dateEntry.Key, new List<MST.MES.OrderStructureByOrderNo.OneOrderData>());
                        }
                        
                        

                        filteredOrders[lineEntry.Key][dateEntry.Key].Add(orderEntry);
                        filteredOrders["Total"][dateEntry.Key].Add(orderEntry);
                        Debug.WriteLine($"adding day {dateEntry.Key}");
                    }
                }
            }

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
            productionLevel.Color = System.Drawing.Color.AliceBlue;
            productionLevel.BorderColor = System.Drawing.Color.Silver;

            chart.Series.Clear();
            chart.ChartAreas.Clear();

            chart.ChartAreas.Add(ar);
            if (SharedComponents.VisualInspection.PoziomOdpaduTab.radioButtonViLinesCumulated.Checked)
            {
                Series ngSeries = new Series();
                Series scrapSeries = new Series();
                ngSeries.ChartType = SeriesChartType.Line;
                ngSeries.BorderWidth = 3;
                ngSeries.Name = "Total NG [%]";
                ngSeries.MarkerStyle = MarkerStyle.Circle;
                ngSeries.MarkerSize = 10;

                scrapSeries.ChartType = SeriesChartType.Line;
                scrapSeries.BorderWidth = 3;
                scrapSeries.Name = "Total SCRAP [%]";
                scrapSeries.MarkerStyle = MarkerStyle.Square;
                scrapSeries.MarkerSize = 10;

                foreach (var dateKey in filteredOrders["Total"])
                {
                    DataPoint ngPoint = new DataPoint();
                    double ngY = (double)dateKey.Value.Select(o => o.visualInspection.ngCount).Sum() / (double)dateKey.Value.Select(o => o.smt.totalManufacturedQty).Sum() * 100;

                    ngPoint.SetValueXY(dateKey.Key, ngY);
                    string[] toolTips = MakeToolTip(dateKey.Value);
                    string ngtoolTip = toolTips[0];
                    ngPoint.ToolTip = ngtoolTip;
                    ngSeries.Points.Add(ngPoint);

                    DataPoint scrapPoint = new DataPoint();
                    double scrapY = (double)dateKey.Value.Select(o => o.visualInspection.scrapCount).Sum() / (double)dateKey.Value.Select(o => o.smt.totalManufacturedQty).Sum() * 100;
                    scrapPoint.SetValueXY(dateKey.Key, scrapY);
                    string scraptoolTip = toolTips[1];
                    scrapPoint.ToolTip = scraptoolTip;
                    scrapSeries.Points.Add(scrapPoint);

                    productionLevel.Points.AddXY(dateKey.Key, dateKey.Value.Select(o => o.smt.totalManufacturedQty).Sum());
                }
                chart.Series.Add(productionLevel);
                chart.Series.Add(ngSeries);
                chart.Series.Add(scrapSeries);
            }
            else
            {
                foreach (var dateKey in filteredOrders["Total"])
                {
                    productionLevel.Points.AddXY(dateKey.Key, dateKey.Value.Select(o => o.smt.totalManufacturedQty).Sum());
                }
                chart.Series.Add(productionLevel);

                foreach (var lineEntry in filteredOrders)
                {
                    if (lineEntry.Key == "Total") continue;
                    Series ngSeries = new Series();
                    Series scrapSeries = new Series();
                    ngSeries.ChartType = SeriesChartType.Line;
                    ngSeries.BorderWidth = 3;
                    ngSeries.Name = $"{lineEntry.Key} NG [%]";
                    ngSeries.MarkerStyle = MarkerStyle.Circle;
                    ngSeries.MarkerSize = 10;
                    ngSeries.Color = GlobalParameters.smtLinesColors[lineEntry.Key];

                    scrapSeries.ChartType = SeriesChartType.Line;
                    scrapSeries.BorderWidth = 3;
                    scrapSeries.Name = $"{lineEntry.Key} SCRAP [%]";
                    scrapSeries.MarkerStyle = MarkerStyle.Square;
                    scrapSeries.MarkerSize = 10;
                    scrapSeries.Color = GlobalParameters.smtLinesColors[lineEntry.Key];

                    foreach (var dateKey in lineEntry.Value)
                    {
                        DataPoint ngPoint = new DataPoint();
                        double ngY = (double)dateKey.Value.Select(o => o.visualInspection.ngCount).Sum() / (double)dateKey.Value.Select(o => o.smt.totalManufacturedQty).Sum() * 100;

                        ngPoint.SetValueXY(dateKey.Key, ngY);
                        string[] toolTips = MakeToolTip(dateKey.Value);
                        string ngtoolTip = toolTips[0];
                        ngPoint.ToolTip = ngtoolTip;
                        ngSeries.Points.Add(ngPoint);

                        DataPoint scrapPoint = new DataPoint();
                        double scrapY = (double)dateKey.Value.Select(o => o.visualInspection.scrapCount).Sum() / (double)dateKey.Value.Select(o => o.smt.totalManufacturedQty).Sum() * 100;
                        scrapPoint.SetValueXY(dateKey.Value, scrapY);
                        string scraptoolTip = toolTips[1];
                        scrapPoint.ToolTip = scraptoolTip;
                        scrapSeries.Points.Add(scrapPoint);
                    }
                    chart.Series.Add(ngSeries);
                    chart.Series.Add(scrapSeries);
                }

                
            }
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
                chart.ChartAreas[0].AxisY.Maximum = maxY * (100-SharedComponents.VisualInspection.PoziomOdpaduTab.vScrollBarZoomChart.Value)/100;
            }
                chart.Legends.Clear();
        }

        private static string[] MakeToolTip(List<MST.MES.OrderStructureByOrderNo.OneOrderData> value)
        {
            string[] result = new string[] { "", "" };
            var ng = value.GroupBy(o => o.kitting.modelId).ToDictionary(x => x.Key, v => v.Select(o => o.visualInspection.ngCount).Sum()).OrderByDescending(o=>o.Value);
            var scrap = value.GroupBy(o => o.kitting.modelId).ToDictionary(x => x.Key, v => v.Select(o => o.visualInspection.scrapCount).Sum()).OrderByDescending(o => o.Value);
            ng.ToList().Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
            scrap.ToList().Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
            foreach (var waste in ng)
            {
                result[0] += $"{waste.Key} - {waste.Value}szt." + Environment.NewLine;
            }
            foreach (var waste in scrap)
            {
                result[1] += $"{waste.Key} - {waste.Value}szt." + Environment.NewLine;
            }
            return result;
        }

        public static void DrawWasteReasonsCHart()
        {
            var filteredOrders = new Dictionary<string, Dictionary<string, List<MST.MES.OrderStructureByOrderNo.OneOrderData>>>();
            var selectedLines = SharedComponents.VisualInspection.PrzyczynyOdpaduTab.checkedListBoxViWasteReasonsSmtLines.selectedLines;

            filteredOrders.Add("Total", new Dictionary<string, List<MST.MES.OrderStructureByOrderNo.OneOrderData>>());
            foreach (var lineEntry in DataContainer.VisualInspection.wasteReasonsByLineThenDateKey)
            {
                foreach (var dateEntry in lineEntry.Value)
                {
                    foreach (var orderEntry in dateEntry.Value)
                    {
                        if (!SharedComponents.VisualInspection.PrzyczynyOdpaduTab.checkBoxViReasonsLg.Checked)
                        {
                            if (orderEntry.kitting.odredGroup == "LG") continue;
                        }
                        if (!SharedComponents.VisualInspection.PrzyczynyOdpaduTab.checkBoxViReasonsMst.Checked)
                        {
                            if (orderEntry.kitting.odredGroup == "MST") continue;
                        }
                        if (!orderEntry.smt.smtOrders.Select(o => o.smtLine).Intersect(selectedLines).Any()) continue;

                        if (!filteredOrders.ContainsKey(lineEntry.Key))
                        {
                            filteredOrders.Add(lineEntry.Key, new Dictionary<string, List<MST.MES.OrderStructureByOrderNo.OneOrderData>>());
                        }
                        if (!filteredOrders[lineEntry.Key].ContainsKey(dateEntry.Key))
                        {
                            filteredOrders[lineEntry.Key].Add(dateEntry.Key, new List<MST.MES.OrderStructureByOrderNo.OneOrderData>());
                        }

                        if (!filteredOrders["Total"].ContainsKey(dateEntry.Key))
                        {
                            filteredOrders["Total"].Add(dateEntry.Key, new List<MST.MES.OrderStructureByOrderNo.OneOrderData>());
                        }

                        filteredOrders[lineEntry.Key][dateEntry.Key].Add(orderEntry);
                        filteredOrders["Total"][dateEntry.Key].Add(orderEntry);
                    }
                }
            }

            var ngOrders = filteredOrders.SelectMany(o => o.Value).SelectMany(o => o.Value).Where(o => o.visualInspection.ngCount > 0 || o.visualInspection.scrapCount > 0);
            
            Dictionary<string, List<MST.MES.OrderStructureByOrderNo.OneOrderData>> ngPerReason = new Dictionary<string, List<MST.MES.OrderStructureByOrderNo.OneOrderData>>();
            Dictionary<string, List<MST.MES.OrderStructureByOrderNo.OneOrderData>> scrapPerReason = new Dictionary<string, List<MST.MES.OrderStructureByOrderNo.OneOrderData>>();
            List<string> controlListNgOrdersAdded = new List<string>();
            List<string> controlListScrapOrdersAdded = new List<string>();

            foreach (var order in ngOrders)
            {
                foreach (var reason in order.visualInspection.allReasons)
                {
                    if (reason.Key.StartsWith("ng"))
                    {
                        if (!ngPerReason.ContainsKey(reason.Key)) ngPerReason.Add(reason.Key, new List<MST.MES.OrderStructureByOrderNo.OneOrderData>());
                        {
                            if (controlListNgOrdersAdded.Contains(order.kitting.orderNo)) continue;
                            ngPerReason[reason.Key].Add(order);
                            controlListNgOrdersAdded.Add(order.kitting.orderNo);
                        }
                    }
                    else
                    {
                        if (!scrapPerReason.ContainsKey(reason.Key)) scrapPerReason.Add(reason.Key, new List<MST.MES.OrderStructureByOrderNo.OneOrderData>());
                        {
                            if (controlListScrapOrdersAdded.Contains(order.kitting.orderNo)) continue;
                            scrapPerReason[reason.Key].Add(order);
                            controlListScrapOrdersAdded.Add(order.kitting.orderNo);
                        }
                    }
                }

            }

            ngPerReason = ngPerReason.OrderByDescending(q => q.Value.Select(o => o.visualInspection.allReasons[q.Key]).Sum()).ToDictionary(k => k.Key, v => v.Value);
            scrapPerReason = scrapPerReason.OrderByDescending(q => q.Value.Select(o => o.visualInspection.allReasons[q.Key]).Sum()).ToDictionary(k => k.Key, v => v.Value);
            //////////////////////

            Chart ngChart = SharedComponents.VisualInspection.PrzyczynyOdpaduTab.chartPrzyczynyOdpaduNg;
            Chart scrapChart = SharedComponents.VisualInspection.PrzyczynyOdpaduTab.chartPrzyczynyOdpaduScrap;

            ngChart.Series.Clear();
            ngChart.ChartAreas.Clear();

            scrapChart.Series.Clear();
            scrapChart.ChartAreas.Clear();

            Series ngSeries = new Series();
            ngSeries.ChartType = SeriesChartType.Column;
            ngSeries.LegendText = "NG";

            Series scrapSeries = new Series();
            scrapSeries.ChartType = SeriesChartType.Column;
            scrapSeries.LegendText = "SCRAP";

            ChartArea ngArea = new ChartArea();
            ngArea.AxisX.LabelStyle.Interval = 1;
            ngArea.AxisX.IsLabelAutoFit = true;
            ngArea.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.LabelsAngleStep30;
            ngArea.AxisX.LabelStyle.Font = new System.Drawing.Font(ngArea.AxisX.LabelStyle.Font.Name, 10f);

            ChartArea scrapArea = new ChartArea();
            scrapArea.AxisX.LabelStyle.Interval = 1;
            scrapArea.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.LabelsAngleStep30;
            scrapArea.AxisX.LabelStyle.Font = new System.Drawing.Font(scrapArea.AxisX.LabelStyle.Font.Name, 10f);

            CustomDataGridView grid = SharedComponents.VisualInspection.PrzyczynyOdpaduTab.dataGridViewNgScrapReasons;
            grid.Rows.Clear();
            grid.Rows.Add("NG");
            foreach (DataGridViewCell cell in grid.Rows[grid.Rows.Count-1].Cells)
            {
                cell.Style.BackColor = Color.Red;
                cell.Style.ForeColor = Color.White;
            }

            double allNg = ngPerReason.SelectMany(ng => ng.Value).Select(o => o.visualInspection.ngCount).Sum();
            double allScr = ngPerReason.SelectMany(ng => ng.Value).Select(o => o.visualInspection.scrapCount).Sum();

            foreach (var ngKey in ngPerReason)
            {
                var ngQty = ngKey.Value.Select(o => o.visualInspection.allReasons[ngKey.Key]).Sum();
                DataPoint ngPt = new DataPoint();
                ngPt.SetValueXY(ngKey.Key, ngQty);
                ngPt.Tag = ngKey.Value;
                ngSeries.Points.Add(ngPt);

                grid.Rows.Add(ngKey.Key.Replace("ng",""), ngQty, Math.Round(ngQty / allNg * 100, 1));
            }

            grid.Rows.Add();
            grid.Rows.Add("SCRAP");
            foreach (DataGridViewCell cell in grid.Rows[grid.Rows.Count - 1].Cells)
            {
                cell.Style.BackColor = Color.Black;
                cell.Style.ForeColor = Color.White;
            }

            foreach (var scrapKey in scrapPerReason)
            {
                var scrapQty = scrapKey.Value.Select(o => o.visualInspection.allReasons[scrapKey.Key]).Sum();
                DataPoint scrapPt = new DataPoint();
                scrapPt.SetValueXY(scrapKey.Key, scrapQty);
                scrapPt.Tag = scrapKey.Value;
                scrapSeries.Points.Add(scrapPt);

                grid.Rows.Add(scrapKey.Key.Replace("scrap", ""), scrapQty, Math.Round(scrapQty / allScr * 100, 1));
            }

            autoSizeGridColumns(grid);

            ngChart.ChartAreas.Add(ngArea);
            scrapChart.ChartAreas.Add(scrapArea);
            ngChart.Series.Add(ngSeries);
            scrapChart.Series.Add(scrapSeries);
            ngChart.Legends[0].Docking = Docking.Top;
            scrapChart.Legends[0].Docking = Docking.Top;
            //ngChart.Legends[0].Position.Auto = false;
        }

        public static void DrawWasteLevelPerReason(Chart targetChart, string targetModel, List<WasteDataStructure> inputData, string[] reasons, Dictionary<string, string> modelDictionary, string[] smtLines)
        {
            DataTable result = new DataTable();
            Dictionary<DateTime, Dictionary<string, double>> wasteInDayPerModel = new Dictionary<DateTime, Dictionary<string, double>>();
            Dictionary<DateTime, Dictionary<string, double>> scrapInDayPerModel = new Dictionary<DateTime, Dictionary<string, double>>();
            Dictionary<DateTime, Dictionary<string, double>> totalInDayPerModel = new Dictionary<DateTime, Dictionary<string, double>>();

            foreach (var record in inputData)
            {
                if (!smtLines.Contains(record.SmtLine)) continue;

                if (!wasteInDayPerModel.ContainsKey(record.FixedDateTime.Date))
                {
                    wasteInDayPerModel.Add(record.FixedDateTime.Date, new Dictionary<string, double>());
                    totalInDayPerModel.Add(record.FixedDateTime.Date, new Dictionary<string, double>());
                    scrapInDayPerModel.Add(record.FixedDateTime.Date, new Dictionary<string, double>());
                }

                if (targetModel != "all")
                {
                    if (targetModel != record.Model) continue;
                }

                if (!wasteInDayPerModel[record.FixedDateTime.Date].ContainsKey(record.Model))
                {
                    wasteInDayPerModel[record.FixedDateTime.Date].Add(record.Model, 0);
                    totalInDayPerModel[record.FixedDateTime.Date].Add(record.Model, 0);
                    scrapInDayPerModel[record.FixedDateTime.Date].Add(record.Model, 0);
                }

                var typ = typeof(WasteDataStructure);
                string reasonNg = "ng" + reasons;
                string reasonScrap = "ccrap" + reasons;

                foreach (var wasteReason in record.WastePerReason)
                {
                    foreach (var reason in reasons)
                    {
                        if (wasteReason.Key.Contains(reason))
                        {
                            double value = (double)wasteReason.Value;
                            if (wasteReason.Key.StartsWith("ng"))
                            {
                                wasteInDayPerModel[record.FixedDateTime.Date][record.Model] += value;
                            }
                            else
                            {
                                scrapInDayPerModel[record.FixedDateTime.Date][record.Model] += value;
                            }
                        }
                    }
                }

                totalInDayPerModel[record.FixedDateTime.Date][record.Model] += record.AllQty;
            }

            Series ngSeries = new Series();
            ngSeries.ChartType = SeriesChartType.Line;
            ngSeries.BorderWidth = 3;
            ngSeries.Name = "NG [%]";

            Series scrapSeries = new Series();
            scrapSeries.ChartType = SeriesChartType.Line;
            scrapSeries.BorderWidth = 3;
            scrapSeries.Name = "SCRAP [%]";
            
            ChartArea ar = new ChartArea();
            ar.AxisX.LabelStyle.Interval = 1;
            ar.AxisX.MajorGrid.Interval = 1;
            ar.AxisY.MajorGrid.Interval = 0.5;
            ar.AxisY.MinorGrid.Interval = 0.1;
            ar.AxisY.MinorGrid.LineColor = System.Drawing.Color.Silver;
            ar.AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            ar.AxisY.MinorGrid.Enabled = true;
            ar.AxisX.IntervalType = DateTimeIntervalType.Days;
            ar.AxisY.LabelStyle.Format = "{0.00} %";
            ar.Position = new ElementPosition(0, 0, 100, 100);

            foreach (var dateEntry in wasteInDayPerModel)
            {
                double totalNg = wasteInDayPerModel[dateEntry.Key].Select(m => m.Value).Sum(s => s);
                double totalTotal = totalInDayPerModel[dateEntry.Key].Select(m => m.Value).Sum(s => s);
                double totalScrap = scrapInDayPerModel[dateEntry.Key].Select(m => m.Value).Sum(s => s);

                DataPoint ngPoint = new DataPoint();
                ngPoint.MarkerStyle = MarkerStyle.Circle;
                ngPoint.MarkerSize = 10;
                ngPoint.SetValueXY(dateEntry.Key, (totalNg / totalTotal) * 100);

                //List<string> ngToolTip = new List<string>();
                List<Tuple<double, string>> NgToolTipTupleList = new List<Tuple<double, string>>();
                foreach (var modelEntry in wasteInDayPerModel[dateEntry.Key])
                {
                    NgToolTipTupleList.Add(new Tuple<double, string>(Math.Round( modelEntry.Value/ totalInDayPerModel[dateEntry.Key][modelEntry.Key]*100,1), modelEntry.Value+@"/"+totalInDayPerModel[dateEntry.Key][modelEntry.Key] + "szt. - " + modelEntry.Key));
                    //ngToolTip.Add(modelEntry.Value + @"/" + totalInDayPerModel[dateEntry.Key][modelEntry.Key] + "szt. - " + modelEntry.Key);
                }

                NgToolTipTupleList = NgToolTipTupleList.OrderByDescending(o => o.Item1).ToList();
 
                //ngToolTip = ngToolTip.OrderByDescending(o => o).ToList(); ;
                string tip = string.Join(Environment.NewLine, NgToolTipTupleList.Select(t => string.Format("{0}% - {1}", t.Item1, t.Item2)));
                ngPoint.ToolTip = tip;
                ngSeries.Points.Add(ngPoint);

                DataPoint scrapPoint = new DataPoint();
                scrapPoint.MarkerStyle = MarkerStyle.Circle;
                scrapPoint.MarkerSize = 10;
                scrapPoint.SetValueXY(dateEntry.Key, (totalScrap / totalTotal) * 100);

                List<string> scrapToolTip = new List<string>();
                foreach (var modelEntry in scrapInDayPerModel[dateEntry.Key])
                {
                    scrapToolTip.Add(modelEntry.Value+ @"/" + totalInDayPerModel[dateEntry.Key][modelEntry.Key] + "szt. - " + modelEntry.Key);
                }
                scrapToolTip = scrapToolTip.OrderByDescending(o => o).ToList(); ;
                scrapPoint.ToolTip = string.Join(Environment.NewLine, scrapToolTip.ToArray());
                scrapSeries.Points.Add(scrapPoint);
            }



            // var dictNg = wasteInDayPerModel.Select(item => new { Key = item.Value.Keys, wartosc = item.Value.Values }).ToDictionary(item => item, item=> item.wartosc);
            // var dictScrap = scrapInDayPerModel.SelectMany(sel => sel.Value).ToDictionary(p => p.Key, p => p.Value);

            targetChart.Series.Clear();
            targetChart.ChartAreas.Clear();

            targetChart.Series.Add(ngSeries);
            targetChart.Series.Add(scrapSeries);
            targetChart.ChartAreas.Add(ar);
            targetChart.Legends[0].DockedToChartArea = targetChart.ChartAreas[0].Name;
            //targetChart.Legends[0].BackColor = System.Drawing.Color.Transparent;
            targetChart.Legends[0].Position = new ElementPosition(0, 0, targetChart.Legends[0].Position.Width, targetChart.Legends[0].Position.Height);

            

        }

        public static void DrawWasteParetoPerReason(Chart paretoQtyChart, Chart paretoPercentageChart, List<WasteDataStructure> inputData, string[] reasons, Dictionary<string, string> modelDictionary, string[] smtLines)
        {
            DataTable result = new DataTable();

            Dictionary<string, double> modelWastePareto = new Dictionary<string, double>();
            Dictionary<string, double> modelProductionPareto = new Dictionary<string, double>();

            foreach (var record in inputData)
            {
                if (!smtLines.Contains(record.SmtLine)) continue;
                if (record.Model == "") continue;

                if (!modelProductionPareto.ContainsKey(record.Model))
                    modelProductionPareto.Add(record.Model, 0);
                modelProductionPareto[record.Model] += record.AllQty;

                var typ = typeof(WasteDataStructure);
                string reasonNg = "ng" + reasons;
                string reasonScrap = "scrap" + reasons;

                foreach (var reasonEntry in record.WastePerReason)
                {
                    //only ng for now:(
                    if (reasonEntry.Key.StartsWith("scrap")) continue;
                        foreach (var reason in reasons)
                    {
                        if (reasonEntry.Key.Contains(reason))
                        {
                            double value = (double)reasonEntry.Value;
                            if (!modelWastePareto.ContainsKey(record.Model))
                            {
                                modelWastePareto.Add(record.Model, 0);

                            }
                            modelWastePareto[record.Model] += value;
                        }
                    }
                }
            }

            //modelPareto
            List<Tuple<double, string>> paretoQtyList = new List<Tuple<double, string>>();
            List<Tuple<double, string>> paretoPercentageList = new List<Tuple<double, string>>();

            foreach (var keyentry in modelWastePareto)
            {
                paretoQtyList.Add(new Tuple<double, string>(keyentry.Value, keyentry.Key));
                paretoPercentageList.Add(new Tuple<double, string>(keyentry.Value / modelProductionPareto[keyentry.Key], keyentry.Key));
            }

            paretoQtyList = paretoQtyList.OrderByDescending(o => o.Item1).ToList();
            paretoPercentageList = paretoPercentageList.OrderByDescending(o => o.Item1).ToList();

            paretoQtyChart.Series.Clear();
            paretoQtyChart.ChartAreas.Clear();
            paretoQtyChart.Legends.Clear();

            Series seriesParetoNg = new Series();
            seriesParetoNg.ChartType = SeriesChartType.Column;

            ChartArea areaPareto = new ChartArea();
            areaPareto.AxisX.LabelStyle.Interval = 1;
            areaPareto.AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            areaPareto.AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;

            foreach (var item in paretoQtyList)
            {
                if (item.Item1 > 0)
                    seriesParetoNg.Points.AddXY(item.Item2, item.Item1);
            }

            paretoQtyChart.ChartAreas.Add(areaPareto);
            paretoQtyChart.Series.Add(seriesParetoNg);

            //
            paretoPercentageChart.Series.Clear();
            paretoPercentageChart.ChartAreas.Clear();
            paretoPercentageChart.Legends.Clear();

            Series seriesParetoPrcNg = new Series();
            seriesParetoPrcNg.ChartType = SeriesChartType.Column;

            ChartArea areaParetoPrc = new ChartArea();
            areaParetoPrc.AxisX.LabelStyle.Interval = 1;
            areaParetoPrc.AxisY.LabelStyle.Format = "{0.0}%";
            areaParetoPrc.AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            areaParetoPrc.AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;

            foreach (var item in paretoPercentageList)
            {
                if (item.Item1 > 0)
                    seriesParetoPrcNg.Points.AddXY(item.Item2, item.Item1 * 100);
            }

            paretoPercentageChart.ChartAreas.Add(areaParetoPrc);
            paretoPercentageChart.Series.Add(seriesParetoPrcNg);

        }

        public static void DrawWasteLevelPerModel (Chart chartLevel, string targetReason,List<WasteDataStructure> inputData, Dictionary<string, string> modelDictionary, string selectedModel, string[] smtLines)
        {
            Dictionary<DateTime, double> wastePerDay = new Dictionary<DateTime, double>();
            Dictionary<DateTime, double> scrapPerDay = new Dictionary<DateTime, double>();
            Dictionary<DateTime, double> totalPerDay = new Dictionary<DateTime, double>();

            foreach (var record in inputData)
            {
                if (record.Model=="") continue;
                if (!smtLines.Contains(record.SmtLine)) continue;

                //if (targetReason != "all")
                //    if (targetReason != model) continue;

                if (record.Model.Contains(selectedModel))
                {
                    if (!wastePerDay.ContainsKey(record.FixedDateTime.Date))
                    {
                        wastePerDay.Add(record.FixedDateTime.Date, 0);
                        totalPerDay.Add(record.FixedDateTime.Date, 0);
                        scrapPerDay.Add(record.FixedDateTime.Date, 0);
                    }
                    totalPerDay[record.FixedDateTime.Date] += record.AllQty;

                    if (targetReason == "all")
                    {
                        wastePerDay[record.FixedDateTime.Date] += record.AllNg;
                        scrapPerDay[record.FixedDateTime.Date] += record.AllScrap;
                    }
                    else
                    {
                        foreach (var reasonEntry in record.WastePerReason)
                        {
                            if (reasonEntry.Key.StartsWith("ng"))
                            {
                                if (reasonEntry.Key == targetReason)
                                {
                                    wastePerDay[record.FixedDateTime.Date] += reasonEntry.Value;
                                }
                            } else if (reasonEntry.Key.StartsWith("scrap"))
                            {
                                if (reasonEntry.Key == targetReason)
                                {
                                    scrapPerDay[record.FixedDateTime.Date] += reasonEntry.Value;
                                }
                            }
                        }
                    }
                }
            }
            
            chartLevel.Series.Clear();
            //chartLevel.Legends.Clear();
            chartLevel.ChartAreas.Clear();

            Series seriesNg = new Series();
            seriesNg.ChartType = SeriesChartType.Line;
            seriesNg.BorderWidth = 3;
            seriesNg.Name = "NG";

            Series seriesScrap = new Series();
            seriesScrap.ChartType = SeriesChartType.Line;
            seriesScrap.BorderWidth = 3;
            seriesScrap.Name = "Scrap";

            ChartArea arLevel = new ChartArea();
            arLevel.AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            arLevel.AxisX.IntervalType = DateTimeIntervalType.Days;
            arLevel.AxisX.Interval = 1;
            arLevel.AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
            arLevel.AxisY.LabelStyle.Format = "{0.00} %";
            arLevel.Position = new ElementPosition(0, 0, 100, 100);

            foreach (var dayEntry in wastePerDay)
            {
                DataPoint pNg = new DataPoint();
                pNg.MarkerStyle = MarkerStyle.Circle;
                pNg.MarkerSize = 10;
                pNg.SetValueXY(dayEntry.Key, dayEntry.Value / totalPerDay[dayEntry.Key]*100);
                seriesNg.Points.Add(pNg);

                DataPoint pSc = new DataPoint();
                pSc.MarkerStyle = MarkerStyle.Circle;
                pSc.MarkerSize = 10;
                pSc.SetValueXY(dayEntry.Key, scrapPerDay[dayEntry.Key] / totalPerDay[dayEntry.Key] * 100);
                seriesScrap.Points.Add(pSc);
            }

            chartLevel.ChartAreas.Add(arLevel);
            chartLevel.Series.Add(seriesNg);
            chartLevel.Series.Add(seriesScrap);
            chartLevel.Legends[0].DockedToChartArea = chartLevel.ChartAreas[0].Name;
            //chartLevel.ChartAreas[0].AxisY.MajorGrid.Interval = 0.01;// (chartLevel.ChartAreas[0].AxisY.Maximum - chartLevel.ChartAreas[0].AxisY.Minimum) / 10;
        }

        public static void DrawWasteReasonsPerModel(Chart chartReasonsNg, Chart chartReasonsScrap, List<WasteDataStructure> inputData, Dictionary<string, string> modelDictionary, string selectedModel)
        {
            Dictionary<string, double> wasteReasonsNg = new Dictionary<string, double>();
            Dictionary<string, double> wasteReasonsScrap = new Dictionary<string, double>();

            foreach (var record in inputData)
            {
                if (record.Model == "") continue;

                if (record.Model.Contains(selectedModel))
                {
                    foreach (var reasonEntry in record.WastePerReason)
                    {
                        if (reasonEntry.Key.StartsWith("ng"))
                        {
                            if (!wasteReasonsNg.ContainsKey(reasonEntry.Key))
                            {
                                wasteReasonsNg.Add(reasonEntry.Key, 0);
                            }
                            wasteReasonsNg[reasonEntry.Key] += reasonEntry.Value ;
                        } else if (reasonEntry.Key.StartsWith("scrap"))
                        {
                            if (!wasteReasonsScrap.ContainsKey(reasonEntry.Key))
                            {
                                wasteReasonsScrap.Add(reasonEntry.Key, 0);
                            }
                            wasteReasonsScrap[reasonEntry.Key] += reasonEntry.Value;
                        }
                    } 
                }
            }

            List<Tuple<double, string>> reasonsListNg = new List<Tuple<double, string>>();
            List<Tuple<double, string>> reasonsListScrap = new List<Tuple<double, string>>();

            foreach (var reasonEntry in wasteReasonsNg)
            {
                reasonsListNg.Add(new Tuple<double, string>(reasonEntry.Value, reasonEntry.Key));
            }
            foreach (var reasonEntry in wasteReasonsScrap)
            {
                reasonsListScrap.Add(new Tuple<double, string>(reasonEntry.Value, reasonEntry.Key));
            }

            reasonsListNg = reasonsListNg.OrderByDescending(o => o.Item1).ToList();
            reasonsListScrap = reasonsListScrap.OrderByDescending(o => o.Item1).ToList();

            chartReasonsNg.Series.Clear();
            chartReasonsNg.Legends.Clear();
            chartReasonsNg.ChartAreas.Clear();
            ///
            Series seriesNg = new Series();
            seriesNg.ChartType = SeriesChartType.Column;

            ChartArea arNg = new ChartArea();
            arNg.AxisX.LabelStyle.Interval = 1;
            arNg.AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            arNg.AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
            arNg.AxisX.Interval = 1;
            arNg.Position = new ElementPosition(0, 0, 100, 100);

            foreach (var item in reasonsListNg)
            {
                seriesNg.Points.AddXY(item.Item2, item.Item1);
            }
            chartReasonsNg.ChartAreas.Add(arNg);
            chartReasonsNg.Series.Add(seriesNg);
            //Scrap
            chartReasonsScrap.Series.Clear();
            chartReasonsScrap.Legends.Clear();
            chartReasonsScrap.ChartAreas.Clear();
            ///
            Series seriesScrap = new Series();
            seriesScrap.ChartType = SeriesChartType.Column;

            ChartArea arScrap = new ChartArea();
            arScrap.AxisX.LabelStyle.Interval = 1;
            arScrap.AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            arScrap.AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
            arScrap.AxisX.Interval = 1;
            arScrap.Position = new ElementPosition(0, 0, 100, 100);

            foreach (var item in reasonsListScrap)
            {
                seriesScrap.Points.AddXY(item.Item2, item.Item1);
            }
            chartReasonsScrap.ChartAreas.Add(arScrap);
            chartReasonsScrap.Series.Add(seriesScrap);
        }

        public static void DrawSmtEfficiencyHistogramForModel(Chart chart, Dictionary<string, List<durationQuantity>> inputData, bool perHour)
        {
            double frequency = 1;
            if (!perHour) frequency = 8;
            double minValue = 99999999;
            double maxValue = 0;
            foreach (var lineEntry in inputData)
            {
                foreach (var lot in lineEntry.Value)
                {
                    if (lot.duractionEndToEnd > 2 || lot.duractionEndToEnd < 0.2) continue;
                    double eff = lot.quantity / lot.duractionEndToEnd * frequency;
                    if (eff > maxValue) maxValue = eff;
                    if (eff < minValue) minValue = eff;
                }
            }

            int step = (int)Math.Round((maxValue - minValue) / 15, 0);
            List<int> histogramValues = new List<int>();
            for (int i = 0; i < 15; i++) 
            {
                histogramValues.Add((int)Math.Round(minValue + step * i, 0));
            }

            Dictionary<string, SortedDictionary<int, int>> pointsPerLine = new Dictionary<string, SortedDictionary<int, int>>();
            foreach (var lineEntry in inputData)
            {
                if (!pointsPerLine.ContainsKey(lineEntry.Key))
                {
                    pointsPerLine.Add(lineEntry.Key, new SortedDictionary<int, int>());
                }

                foreach (var lot in lineEntry.Value)
                {
                    if (lot.duractionEndToEnd > 2 || lot.duractionEndToEnd < 0.2) continue;
                    int value = GetClosetsPOint(lot.quantity/ lot.duractionEndToEnd * frequency, histogramValues);
                    if (pointsPerLine[lineEntry.Key].ContainsKey(value))
                    {
                        pointsPerLine[lineEntry.Key][value]++;
                    }
                    else
                    {
                        pointsPerLine[lineEntry.Key][value] = 1;
                    }
                }
            }

            chart.Series.Clear();
            chart.ChartAreas.Clear();
            ChartArea area = new ChartArea();
            //area.AxisX.LabelStyle.Interval = 1;
            area.AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            area.AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
            //area.AxisX.Interval = 1;
            area.Position = new ElementPosition(0, 0, 100, 100);

            chart.ChartAreas.Add(area);

            foreach (var lineEntry in pointsPerLine)
            {
                Series newSeries = new Series();
                newSeries.Name = lineEntry.Key;
                newSeries.ChartType = SeriesChartType.Spline;
                newSeries.BorderWidth = 3;
                foreach (var point in lineEntry.Value)
                {
                    int maximum = lineEntry.Value.Select(p => p.Value).ToList().Max();
                    DataPoint pt = new DataPoint();
                    pt.SetValueXY(point.Key, point.Value);
                    pt.MarkerStyle = MarkerStyle.Circle;
                    pt.MarkerSize = 10;
                    if (point.Value == maximum)
                    {
                        pt.IsValueShownAsLabel = true;
                        pt.Label = point.Key.ToString();
                    }
                    newSeries.Points.Add(pt);
                }
                chart.Series.Add(newSeries);

            }
        }

        public static void DrawTestEfficiencyHistogramForModel(Chart chart, DataGridView grid,  bool perHour, string model)
        {
            if (model != "" & chart.Tag!=null)
            {
                Dictionary<string, Dictionary<string, List<double>>> cycleTimeListByMachineModel = (Dictionary<string, Dictionary<string, List<double>>>)chart.Tag;
                grid.Columns.Clear();
                grid.Columns.Add("Tester", "Tester");
                grid.Columns.Add("Min", "Min");
                grid.Columns.Add("Max", "Max");
                grid.Columns.Add("Sred", "Sred");

                double frequency = 1;
                if (!perHour) frequency = 8;

                Dictionary<string, Dictionary<double, double>> histogramsPerTester = new Dictionary<string, Dictionary<double, double>>();
                foreach (var testerEntry in cycleTimeListByMachineModel)
                {
                    string testerId = testerEntry.Key;

                    List<double> cycleList = new List<double>();
                    if (!testerEntry.Value.TryGetValue(model, out cycleList) || testerEntry.Value[model].Count < 5) 
                    {
                        continue;
                    }

                    if (cycleList.Count > 0)
                    {
                        cycleList = cycleList.Select(c => 28800 / c).ToList();
                        grid.Rows.Add(testerId, Math.Round(cycleList.Min(), 0), Math.Round(cycleList.Max(), 0), Math.Round(cycleList[cycleList.Count / 2], 0));
                        if (!histogramsPerTester.ContainsKey(testerId))
                        {
                            histogramsPerTester.Add(testerId, new Dictionary<double, double>());
                        }
                        histogramsPerTester[testerId] = CreateHistogram(10, cycleList);
                    }
                }

                chart.Series.Clear();
                chart.ChartAreas.Clear();
                ChartArea area = new ChartArea();
                //area.AxisX.LabelStyle.Interval = 1;
                area.AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
                area.AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
                //area.AxisX.Interval = 1;
                area.Position = new ElementPosition(0, 0, 100, 100);
                chart.ChartAreas.Add(area);

                foreach (var testerEntry in histogramsPerTester)
                {
                    Series newSeries = new Series();
                    newSeries.Name = testerEntry.Key;
                    newSeries.ChartType = SeriesChartType.Spline;
                    newSeries.BorderWidth = 3;
                    foreach (var point in testerEntry.Value)
                    {
                        DataPoint pt = new DataPoint();
                        pt.SetValueXY(Math.Round(point.Key, 2), Math.Round(point.Value, 2));
                        pt.MarkerStyle = MarkerStyle.Circle;
                        pt.MarkerSize = 10;
                        newSeries.Points.Add(pt);
                    }
                    chart.Series.Add(newSeries);
                }
            }
        }

        public static int GetClosetsPOint(double inputValue, List<int> valuesArray)
        {
            List<Tuple<int, int>> substractionList = new List<Tuple<int, int>>();

            foreach (var arrayValue in valuesArray)
            {
                substractionList.Add(new Tuple<int, int>(arrayValue, (int)Math.Round(Math.Abs(arrayValue - inputValue),0)));
            }
            substractionList.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            return substractionList[substractionList.Count-1].Item1;
        }

        public static Dictionary<double, double> CreateHistogram(double numberOfSteps, List<double> values)
        {
            Dictionary<double, double> result = new Dictionary<double, double>();
            double min = values.Min();
            double max = values.Max();
            double stepSize = (max - min) / (numberOfSteps - 1);
            List<double> steps = new List<double>();
            for (int i = 0; i <= numberOfSteps; i++)
            {
                var step = min + stepSize * i;
                steps.Add(step);
                result.Add(step, 0);
            }

            for (int i = 0; i < values.Count; i++)
            {
                double step = GetClosestStepInHistogram(steps.ToArray(), values[i]);
                if (!result.ContainsKey(step))
                {
                    result.Add(step, 0);
                }
                result[step]++;
            }
            return result;
        }

        public static double GetClosestStepInHistogram(double[] steps, double value)
        {
            List<Tuple<double, double>> deltaList = new List<Tuple<double, double>>();
            double result = -1;
            foreach (var item in steps)
            {
                deltaList.Add(new Tuple<double, double>(item - value, item));
            }
            deltaList.Sort((x, y) => y.Item1.CompareTo(x.Item1));

            for (int i = 0; i < deltaList.Count; i++) 
            {
                if (deltaList[i].Item1 >= 0) 
                {
                    result = deltaList[i].Item2;
                }
            }

            return result;
        }

        public static void DrawNgChartForOneOperator(DataTable operatorTable, Chart chart)
        {
            Dictionary<string, Tuple<double, double>> ngScrapPerDay = new Dictionary<string, Tuple<double, double>>();

            foreach (DataRow row in operatorTable.Rows)
            {
                if (row["Data"].ToString() == "") continue;
                string day = row["Data"].ToString();

                double ng = double.Parse(row["NG"].ToString());
                double qty = double.Parse(row["Ilość"].ToString());
                double scrap = double.Parse(row["Scrap"].ToString());

                ngScrapPerDay.Add(day, new Tuple<double, double>(Math.Round(ng / qty * 100, 2), Math.Round(scrap / qty * 100, 2)));
            }

            ChartArea ar = new ChartArea();
            ar.AxisX.LabelStyle.Interval = 1;
            ar.AxisY.MajorGrid.Interval = 0.5;
            ar.AxisY.MinorGrid.Interval = 0.1;
            ar.AxisY.MinorGrid.LineColor = System.Drawing.Color.Silver;
            ar.AxisY.MinorGrid.Enabled = true;
            ar.AxisY2.MajorGrid.Enabled = false;
            ar.AxisY.LabelStyle.Format = "{0.00} %";
            //ar.AxisY.Maximum = 10;

            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Legends.Clear();
            chart.ChartAreas.Add(ar);

            Series ngSeries = new Series();
            Series scrapSeries = new Series();
            ngSeries.ChartType = SeriesChartType.Line;
            ngSeries.BorderWidth = 3;
            ngSeries.MarkerStyle = MarkerStyle.Circle;
            ngSeries.MarkerSize = 10;

            scrapSeries.ChartType = SeriesChartType.Line;
            scrapSeries.BorderWidth = 3;
            scrapSeries.MarkerStyle = MarkerStyle.Square;
            scrapSeries.MarkerSize = 10;

            foreach (var dayEntry in ngScrapPerDay)
            {
                DataPoint ngPoint = new DataPoint();
                ngPoint.SetValueXY(dayEntry.Key, (double)dayEntry.Value.Item1);
                ngSeries.Points.Add(ngPoint);

                DataPoint scrapPoint = new DataPoint();
                scrapPoint.SetValueXY(dayEntry.Key, (double)dayEntry.Value.Item2);
                scrapSeries.Points.Add(scrapPoint);
            }
            chart.Series.Add(ngSeries);
            chart.Series.Add(scrapSeries);
        }
    }
}
