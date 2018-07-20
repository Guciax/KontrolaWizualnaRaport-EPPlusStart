﻿using System;
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
using static KontrolaWizualnaRaport.SMTOperations;

namespace KontrolaWizualnaRaport
{
    class Charting
    {
        public static DataTable DrawCapaChart(Chart chart, List<WasteDataStructure> inputData, string oper, Dictionary<string, string> modelDictionary, bool customerLGI, List<excelOperations.order12NC> mstOrders)
        {
            chart.Series.Clear();
            chart.ChartAreas.Clear();

            Series serColumn = new Series();
            serColumn.IsVisibleInLegend = false;
            serColumn.IsValueShownAsLabel = false;
            serColumn.Color = System.Drawing.Color.Blue;
            serColumn.BorderColor = System.Drawing.Color.Black;

            ChartArea area = new ChartArea();
            area.AxisX.IsLabelAutoFit = true;
            area.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.LabelsAngleStep45;
            area.AxisX.LabelStyle.Enabled = true;
            area.AxisX.LabelStyle.Font = new System.Drawing.Font("Arial", 10);
            area.AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            area.AxisY.MajorGrid.LineColor = System.Drawing.Color.Black;
            area.AxisY.MajorGrid.LineWidth = 1;
            area.AxisY.MinorGrid.Enabled = true;
            area.AxisY.MinorGrid.LineColor = System.Drawing.Color.Silver;
            area.AxisY.MinorGrid.Interval = 1000;
            area.AxisY.MajorGrid.Interval = 5000;
            area.AxisX.LabelStyle.Interval = 1;
            area.AxisY.LabelStyle.Interval = 5000;

            Dictionary<DateTime, int> qtyPerDayForAll = new Dictionary<DateTime, int>();
            Dictionary<DateTime, Dictionary<string, int>> qtyPerDayPerModel = new Dictionary<DateTime, Dictionary<string, int>>();
            DataTable gridTable = new DataTable();
            gridTable.Columns.Add("Data");
            gridTable.Columns.Add("Ilość");

            if (oper == "Wszyscy")
            {
                Dictionary<DateTime, string> labelDict = new Dictionary<DateTime, string>();
                foreach (var item in inputData)
                {
                    if (item.Oper == oper || oper == "Wszyscy")
                    {
                       // string orderList = string.Join(Environment.NewLine, mstOrders.Select(o => o.order).ToArray());
                        if (customerLGI & mstOrders.Select(o => o.order).ToList().Contains(item.NumerZlecenia)) continue;
                        if (!customerLGI & !mstOrders.Select(o => o.order).ToList().Contains(item.NumerZlecenia)) continue;

                            string model = "???";
                        if (modelDictionary.ContainsKey(item.NumerZlecenia))
                            model = modelDictionary[item.NumerZlecenia];

                        if (!qtyPerDayForAll.ContainsKey(item.FixedDateTime.Date))
                        {
                            qtyPerDayForAll.Add(item.FixedDateTime.Date, 0);
                            labelDict.Add(item.FixedDateTime.Date, model);
                        }
                        qtyPerDayForAll[item.FixedDateTime.Date] += item.AllQty;
                        if (!labelDict[item.FixedDateTime.Date].Contains(model))
                            labelDict[item.FixedDateTime.Date] += Environment.NewLine + model;
                    }
                }

                serColumn.ChartType = SeriesChartType.Column;
                foreach (var key in qtyPerDayForAll)
                {
                    serColumn.Points.AddXY(key.Key, key.Value);
                    gridTable.Rows.Add(key.Key.Date.ToShortDateString(), key.Value);
                }
                chart.Series.Add(serColumn);
            }
            else
            {
                gridTable.Columns.Add("NG");
                gridTable.Columns.Add("NG%");
                gridTable.Columns.Add("Scrap");
                gridTable.Columns.Add("Scrap%");
                HashSet<string> uniqueModels = new HashSet<string>();
                HashSet<DateTime> uniqueDates = new HashSet<DateTime>();
                Dictionary<string, Dictionary<DateTime, int>> dictFirstModelThenDate = new Dictionary<string, Dictionary<DateTime, int>>();

                foreach (var item in inputData)
                {
                    if (customerLGI & mstOrders.Select(o => o.order).ToList().Contains(item.NumerZlecenia)) continue;
                    if (!customerLGI & !mstOrders.Select(o => o.order).ToList().Contains(item.NumerZlecenia)) continue;

                    if (item.Oper != oper) continue;

                        if (item.Oper == oper)
                    {
                        if (!qtyPerDayPerModel.ContainsKey(item.FixedDateTime.Date))
                        {
                            qtyPerDayPerModel.Add(item.FixedDateTime.Date, new Dictionary<string, int>());
                        }
                        string model = "??";

                        if (modelDictionary.ContainsKey(item.NumerZlecenia))
                            model = modelDictionary[item.NumerZlecenia].Replace("LLFML", "");

                        uniqueModels.Add(model);
                        uniqueDates.Add(item.FixedDateTime.Date);

                        if (!qtyPerDayPerModel[item.FixedDateTime.Date].ContainsKey(model))
                            qtyPerDayPerModel[item.FixedDateTime.Date].Add(model, 0);

                        qtyPerDayPerModel[item.FixedDateTime.Date][model] += item.AllQty;
                    }
                }

                serColumn.ChartType = SeriesChartType.StackedColumn;
                Dictionary<DateTime, int> qtyPerDayPerOperator = new Dictionary<DateTime, int>();
                Dictionary<DateTime, int> qtyNgPerDayPerOperator = new Dictionary<DateTime, int>();
                Dictionary<DateTime, int> qtyScrapPerDayPerOperator = new Dictionary<DateTime, int>();
                foreach (var model in uniqueModels)
                {

                    dictFirstModelThenDate.Add(model, new Dictionary<DateTime, int>());
                    foreach (var date in uniqueDates)
                    {
                        dictFirstModelThenDate[model].Add(date, 0);

                        if (!qtyPerDayPerOperator.ContainsKey(date))
                        {
                            qtyPerDayPerOperator.Add(date, 0);
                            qtyNgPerDayPerOperator.Add(date, 0);
                            qtyScrapPerDayPerOperator.Add(date, 0);

                        }
                    }
                }

                foreach (var item in inputData)
                {
                    if (item.Oper == oper)
                    {

                        string model = "??";
                        if (modelDictionary.ContainsKey(item.NumerZlecenia))
                        {
                            model = modelDictionary[item.NumerZlecenia].Replace("LLFML", "");
                        }
                        if (!dictFirstModelThenDate.ContainsKey(model)) continue;
                        dictFirstModelThenDate[model][item.FixedDateTime.Date] += item.AllQty;
                        qtyPerDayPerOperator[item.FixedDateTime.Date] += item.AllQty;
                        qtyNgPerDayPerOperator[item.FixedDateTime.Date] += item.AllNg;
                        qtyScrapPerDayPerOperator[item.FixedDateTime.Date] += item.AllScrap;
                    }
                }

                int total = qtyPerDayPerOperator.Select(q => q.Value).Sum();
                int ngTotal = qtyNgPerDayPerOperator.Select(q => q.Value).Sum();
                double totalNgRate = Math.Round((double)ngTotal / (double)total * 100, 2);
                int scrapTotal = qtyScrapPerDayPerOperator.Select(q => q.Value).Sum();
                double totalScrapRate = Math.Round((double)scrapTotal / (double)total * 100, 2);
                gridTable.Rows.Add("TOTAL", total, ngTotal, totalNgRate, scrapTotal, totalScrapRate);

                foreach (var keyEntry in qtyPerDayPerOperator)
                {
                    int ng = qtyNgPerDayPerOperator[keyEntry.Key];
                    double ngRate = Math.Round((double)ng / (double)keyEntry.Value * 100, 2);
                    int scrap = qtyScrapPerDayPerOperator[keyEntry.Key];
                    double scrapRate = Math.Round((double)scrap / (double)keyEntry.Value * 100, 2);

                    gridTable.Rows.Add(keyEntry.Key.ToString("dd-MM-yyyy"), keyEntry.Value, ng, ngRate, scrap, scrapRate);

                }

                foreach (var model in dictFirstModelThenDate)
                {
                    chart.Series.Add(new Series(model.Key));
                    chart.Series[model.Key].ChartType = SeriesChartType.StackedColumn;
                    chart.Series[model.Key].IsValueShownAsLabel = true;
                    chart.Series[model.Key].ToolTip = model.Key;

                    foreach (var date in model.Value)
                    {
                        {
                            //DataPoint point = new DataPoint();
                            //point.SetValueXY(date.Key, date.Value);
                            
                           // if (date.Value > 0)
                                //point.Label = date.Value + " " + model.Key;

                            //chart.Series[model.Key].Points.Add(point);
                            chart.Series[model.Key].Points.AddXY(date.Key, date.Value);
                        }
                    }

                    foreach (var point in chart.Series[model.Key].Points)
                    {
                        if (point.YValues[0] == 0) point.IsValueShownAsLabel = false;
                    }
                }

                area.AxisY.LabelStyle.Interval = 500;
                area.AxisY.MinorGrid.Interval = 100;
                area.AxisY.MajorGrid.Interval = 500;
            }
            chart.ChartAreas.Add(area);
            //chart.Legends[0].DockedToChartArea = chart.ChartAreas[0].Name;
            //chart.Legends[0].TableStyle = LegendTableStyle.Auto;
            chart.Legends.Clear();

           
            return gridTable;
        }

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

        public static void DrawLedWasteChart(SortedDictionary<DateTime, SortedDictionary<int, List<LotLedWasteStruc>>> ledWasteDictionary, Chart chart, string frequency, Dictionary<string, bool> lineOptions, Dictionary<string, System.Drawing.Color> lineColors)
        {
            Dictionary<string, Dictionary<string, double>> dataPointsProd = new Dictionary<string, Dictionary<string, double>>();
            Dictionary<string, Dictionary<string, double>> dataPointsDropped = new Dictionary<string, Dictionary<string, double>>();

            //dataPointsProd.Add("Total", new Dictionary<string, double>());
           // dataPointsDropped.Add("Total", new Dictionary<string, double>());
            List<DateTime> allDates = ledWasteDictionary.Select(date => date.Key).ToList();
            List<string> allLines = ledWasteDictionary.SelectMany(date => date.Value).SelectMany(shift => shift.Value).Select(l => l.smtLine).Distinct().OrderBy(l => l).ToList();
            allLines.Add("Total");

            foreach (var line in allLines)
            {
                dataPointsProd.Add(line, new Dictionary<string, double>());
                dataPointsDropped.Add(line, new Dictionary<string, double>());
                foreach (var date in allDates)
                {
                    string dateKey = date.ToString("dd-MM-yyyy");
                    if (frequency == "Tygodniowo")
                    {
                        dateKey = dateTools.GetIso8601WeekOfYear(date).ToString();
                    }
                    if (frequency == "Miesiecznie")
                    {
                        dateKey = date.ToString("MMM", CultureInfo.InvariantCulture);
                    }
                    if (dataPointsProd[line].ContainsKey(dateKey)) continue;
                    dataPointsProd[line].Add(dateKey, 0);
                    dataPointsDropped[line].Add(dateKey, 0);
                }
            }

            foreach (var dateEntry in ledWasteDictionary)
            {
                string dateKey = dateEntry.Key.ToString("dd-MM-yyyy");
                if (frequency=="Tygodniowo")
                {
                    dateKey = dateTools.GetIso8601WeekOfYear(dateEntry.Key).ToString();
                }
                if (frequency=="Miesiecznie")
                {
                    dateKey = dateEntry.Key.ToString("MMM", CultureInfo.InvariantCulture);
                }

                
                foreach (var shiftEntry in dateEntry.Value)
                {
                    foreach (var lotData in shiftEntry.Value)
                    {
                        string line = lotData.smtLine;
                        

                        int ledExpectedUsageA = lotData.requiredRankA * lotData.manufacturedModules;
                        int ledExpectedUsageB = lotData.requiredRankB * lotData.manufacturedModules;
                        int ledExpectedLeftoversA = lotData.reelsUsed / 2 * lotData.ledsPerReel - ledExpectedUsageA;
                        int ledExpectedLeftoversB = lotData.reelsUsed / 2 * lotData.ledsPerReel - ledExpectedUsageB;
                        int droppedA = ledExpectedLeftoversA - lotData.ledLeftA;
                        int droppedB = ledExpectedLeftoversB - lotData.ledLeftB;

                        if (lineOptions["Total"])
                        {
                            if (!dataPointsProd["Total"].ContainsKey(dateKey))
                            {
                                dataPointsProd["Total"].Add(dateKey, 0);
                                dataPointsDropped["Total"].Add(dateKey, 0);
                            }
                            dataPointsProd["Total"][dateKey] += ledExpectedUsageA + ledExpectedUsageB;
                            dataPointsDropped["Total"][dateKey] += droppedA + droppedB;
                        }

                        if (!lineOptions[line]) continue;

                        if (!dataPointsProd.ContainsKey(line))
                        {
                            dataPointsProd.Add(line, new Dictionary<string, double>());
                            dataPointsDropped.Add(line, new Dictionary<string, double>());
                        }

                        if (!dataPointsProd[line].ContainsKey(dateKey))
                        {
                            dataPointsProd[line].Add(dateKey, 0);
                            dataPointsDropped[line].Add(dateKey, 0);
                        }


                        dataPointsProd[line][dateKey] += ledExpectedUsageA + ledExpectedUsageB;
                        dataPointsDropped[line][dateKey] += droppedA + droppedB;

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
            foreach (var lineEntry in dataPointsProd)
            {
                Series lineSeries = new Series();
                lineSeries.ChartType = SeriesChartType.Line;
                lineSeries.BorderWidth = 3;
                lineSeries.Name = lineEntry.Key;
                lineSeries.Color = lineColors[lineEntry.Key];


                foreach (var dateKeyEntry in lineEntry.Value)
                {
                    DataPoint ngPoint = new DataPoint();
                    double waste = Math.Round(dataPointsDropped[lineEntry.Key][dateKeyEntry.Key] / dateKeyEntry.Value * 100, 2);
                    ngPoint.MarkerSize = 8;
                    ngPoint.MarkerStyle = MarkerStyle.Circle;
                    if (waste > maxY) maxY = waste;
                    ngPoint.SetValueXY(dateKeyEntry.Key, waste);
                    //ngPoint.ToolTip = ngtoolTip;
                    lineSeries.Points.Add(ngPoint);
                    if (lineEntry.Key=="SMT2")
                    {
                       // MessageBox.Show("");
                    }
                }
                chart.Series.Add(lineSeries);
            }
            chart.ChartAreas[0].AxisY.Maximum = maxY * 1.1;

            Series productionLevel = new Series();
            productionLevel.ChartType = SeriesChartType.Column;
            productionLevel.Name = "Poziom produkcji [szt.]";
            productionLevel.YAxisType = AxisType.Secondary;
            productionLevel.Color = System.Drawing.Color.AliceBlue;
            productionLevel.BorderColor = System.Drawing.Color.Silver;

            foreach (var dateKeyEnrtry in dataPointsProd)
            {
                DataPoint pt = new DataPoint();
                pt.SetValueXY(dateKeyEnrtry.Key, dateKeyEnrtry.Value);
                productionLevel.Points.Add(pt);
            }
            //chart.Series.Add(productionLevel);

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

        public static DataTable DrawWasteLevel(string frequency, Chart chartWasteLevel, List<WasteDataStructure> inputData, DateTime dateBegin, DateTime dateEnd, Dictionary<string, string> modelDictionary, ComboBox comboModel, string[] smtLines, bool linesCumulated, bool customerLGI, List<excelOperations.order12NC> mstOrders, Dictionary<string, Color> lineColors)
        {
            DataTable result = new DataTable();
            Dictionary<string, Dictionary<string, WasteLevelChartStruct>> lineDateWaste = new Dictionary<string, Dictionary<string, WasteLevelChartStruct>>();

            string frequencyKey = "";

            result.Columns.Add("Data");
            result.Columns.Add("Ilość");
            result.Columns.Add("Produkcja");
            result.Columns.Add("%");

            string[] mstOrdersList = mstOrders.Select(o => o.order).ToArray();
            string lineKey = "Total";
            foreach (var wasteEntry in inputData)
            {
                if (customerLGI & mstOrdersList.Contains(wasteEntry.NumerZlecenia)) continue;
                if (!customerLGI & !mstOrdersList.Contains(wasteEntry.NumerZlecenia)) continue;
                if (wasteEntry.FixedDateTime.Date < dateBegin || wasteEntry.FixedDateTime.Date > dateEnd) continue;

                string line = wasteEntry.SmtLine;
                if (line == "") continue;
                if (!linesCumulated & !smtLines.Contains(wasteEntry.SmtLine)) continue;
                if (!linesCumulated)
                {
                    lineKey = wasteEntry.SmtLine;
                }

                string model = "";
                if (!modelDictionary.TryGetValue(wasteEntry.NumerZlecenia, out model)) model = "???";

                switch (frequency)
                {
                    case "daily":
                        {
                            frequencyKey = wasteEntry.FixedDateTime.Date.ToShortDateString();
                            break;
                        }
                    case "weekly":
                        {
                            frequencyKey = dateTools.GetIso8601WeekOfYear(wasteEntry.FixedDateTime).ToString();
                            break;
                        }
                    case "monthly":
                        {
                            frequencyKey = dateTools.productionMonthNumber(dateTools.GetIso8601WeekOfYear(wasteEntry.FixedDateTime), wasteEntry.FixedDateTime.Year).ToString();
                            break;
                        }
                }

                //line key
                if (!lineDateWaste.ContainsKey(lineKey))
                {
                    lineDateWaste.Add(lineKey, new Dictionary<string, WasteLevelChartStruct>());
                }

                //frequency Key
                if (!lineDateWaste[lineKey].ContainsKey(frequencyKey))
                {
                    lineDateWaste[lineKey].Add(frequencyKey, new WasteLevelChartStruct());
                    lineDateWaste[lineKey][frequencyKey].ngTooltip = new Dictionary<string, Tuple<int, int>>();
                    lineDateWaste[lineKey][frequencyKey].scrapToolTip = new Dictionary<string, Tuple<int, int>>();
                }

                lineDateWaste[lineKey][frequencyKey].ngCount += wasteEntry.AllNg;
                lineDateWaste[lineKey][frequencyKey].scrapCount += wasteEntry.AllScrap;
                lineDateWaste[lineKey][frequencyKey].totalProdThisLine += wasteEntry.AllQty;

                if(!lineDateWaste[lineKey][frequencyKey].ngTooltip.ContainsKey(model))
                {
                    lineDateWaste[lineKey][frequencyKey].ngTooltip.Add(model, new Tuple<int, int>(0,0));
                }

                if (!lineDateWaste[lineKey][frequencyKey].scrapToolTip.ContainsKey(model))
                {
                    lineDateWaste[lineKey][frequencyKey].scrapToolTip.Add(model, new Tuple<int, int>(0, 0));
                }

                Tuple<int, int> ngEntry = new Tuple<int, int>(lineDateWaste[lineKey][frequencyKey].ngTooltip[model].Item1 + wasteEntry.AllNg, lineDateWaste[lineKey][frequencyKey].ngTooltip[model].Item2 + wasteEntry.AllQty);
                Tuple<int, int> scrapEntry = new Tuple<int, int>(lineDateWaste[lineKey][frequencyKey].scrapToolTip[model].Item1 + wasteEntry.AllNg, lineDateWaste[lineKey][frequencyKey].scrapToolTip[model].Item2 + wasteEntry.AllQty);

                lineDateWaste[lineKey][frequencyKey].ngTooltip[model] = ngEntry;
                lineDateWaste[lineKey][frequencyKey].scrapToolTip[model] = scrapEntry;
            }

            Dictionary<string, double> totalProduction = new Dictionary<string, double>();
            foreach (var lineEntry in lineDateWaste)
            {
                foreach (var frequencyEntry in lineEntry.Value)
                {
                    if (!totalProduction.ContainsKey(frequencyEntry.Key))
                    {
                        totalProduction.Add(frequencyEntry.Key, 0);
                    }
                    totalProduction[frequencyEntry.Key] += frequencyEntry.Value.totalProdThisLine;
                }
                
            }

            ChartArea ar = new ChartArea();
            ar.AxisX.LabelStyle.Interval = 1;
            ar.AxisY.MajorGrid.Interval = 0.5;
            ar.AxisY.MinorGrid.Interval = 0.1;
            ar.AxisY.MinorGrid.LineColor = System.Drawing.Color.Silver;
            ar.AxisY.MinorGrid.Enabled = true;
            ar.AxisY2.MajorGrid.Enabled = false;
            ar.AxisY.LabelStyle.Format = "{0.00} %";
            DataTable tempScrapTable = result.Clone();

            Series productionLevel = new Series();
            productionLevel.Name = "productionLevel";
            productionLevel.ChartType = SeriesChartType.Column;
            productionLevel.Name = "Poziom produkcji [szt.]";
            productionLevel.YAxisType = AxisType.Secondary;
            productionLevel.Color = System.Drawing.Color.AliceBlue;
            productionLevel.BorderColor = System.Drawing.Color.Silver;

            chartWasteLevel.Series.Clear();
            chartWasteLevel.ChartAreas.Clear();

            chartWasteLevel.ChartAreas.Add(ar);

            foreach (var dateEntry in totalProduction)
            {
                productionLevel.Points.AddXY(dateEntry.Key, dateEntry.Value);

            }
            chartWasteLevel.Series.Add(productionLevel);

            foreach (var lineEntry in lineDateWaste)
            {
                string line = lineEntry.Key;

                Color seriesColor = Color.Blue;
                lineColors.TryGetValue(lineEntry.Key, out seriesColor);

                Series ngSeries = new Series();
                Series scrapSeries = new Series();
                ngSeries.ChartType = SeriesChartType.Line;
                ngSeries.BorderWidth = 3;
                ngSeries.Name = line + " NG [%]";
                ngSeries.Color = seriesColor;
                ngSeries.MarkerStyle = MarkerStyle.Circle;
                ngSeries.MarkerSize = 10;

                scrapSeries.ChartType = SeriesChartType.Line;
                scrapSeries.BorderWidth = 3;
                scrapSeries.Name = line + " SCRAP [%]";
                scrapSeries.Color = seriesColor;
                scrapSeries.MarkerStyle = MarkerStyle.Square;
                scrapSeries.MarkerSize = 10;

                foreach (var frequencyEntry in lineEntry.Value)
                {

                    DataPoint ngPoint = new DataPoint();
                    ngPoint.SetValueXY(frequencyEntry.Key, (double)frequencyEntry.Value.ngCount/ (double)frequencyEntry.Value.totalProdThisLine*100);
                    string ngtoolTip = MakeToolTip(frequencyEntry.Value.ngTooltip);
                    ngPoint.ToolTip = ngtoolTip;
                    ngSeries.Points.Add(ngPoint);

                    DataPoint scrapPoint = new DataPoint();
                    scrapPoint.SetValueXY(frequencyEntry.Key, (double)frequencyEntry.Value.scrapCount / (double)frequencyEntry.Value.totalProdThisLine*100);
                    string scraptoolTip = MakeToolTip(frequencyEntry.Value.scrapToolTip);
                    scrapPoint.ToolTip = scraptoolTip;
                    scrapSeries.Points.Add(scrapPoint);


                }

                result.Rows.Add("NG:");
                foreach (var frequencyEntry in lineEntry.Value)
                {
                    result.Rows.Add(frequencyEntry.Key, frequencyEntry.Value.totalProdThisLine, frequencyEntry.Value.ngCount, Math.Round((double)frequencyEntry.Value.ngCount / (double)frequencyEntry.Value.totalProdThisLine * 100, 2).ToString()+"%");
                }
                result.Rows.Add("SCRAP:");
                foreach (var frequencyEntry in lineEntry.Value)
                {
                    result.Rows.Add(frequencyEntry.Key, frequencyEntry.Value.totalProdThisLine, frequencyEntry.Value.scrapCount, Math.Round((double)frequencyEntry.Value.scrapCount / (double)frequencyEntry.Value.totalProdThisLine * 100, 2).ToString()+"%");
                }


                chartWasteLevel.Series.Add(ngSeries);
                chartWasteLevel.Series.Add(scrapSeries);
            }


            chartWasteLevel.Legends[0].DockedToChartArea = chartWasteLevel.ChartAreas[0].Name;
            chartWasteLevel.Legends[0].Position.Auto = false;
            chartWasteLevel.Legends[0].Position = new ElementPosition(8, 0, 30, 10);
            chartWasteLevel.Legends[0].BackColor = System.Drawing.Color.Transparent;

            return result;
        }

        public static string MakeToolTip(Dictionary<string, Tuple<int, int>> inputDict)
        {
            string result = "";
            List<Tuple<double, string>> list = new List<Tuple<double, string>>();
            foreach (var modelEntry in inputDict)
            {
                if(modelEntry.Value.Item1>0)
                {
                    list.Add(new Tuple<double, string>(Math.Round((double)modelEntry.Value.Item1 / (double)modelEntry.Value.Item2*100,2), modelEntry.Value.Item1.ToString() + @"/" + modelEntry.Value.Item2.ToString() + " " + modelEntry.Key));
                }
            }

            list = list.OrderByDescending(o => o.Item1).ToList();

            foreach (var item in list)
            {
                result += item.Item1.ToString() + "% - " + item.Item2 + Environment.NewLine;
            }
            return result;
        }

        public static DataTable DrawWasteLevelDoWyjebania(string frequency, Chart chartWasteLevel, List<WasteDataStructure> inputData, DateTime dateBegin, DateTime dateEnd, Dictionary<string, string> modelDictionary, ComboBox comboModel, string[] smtLines, bool linesCumulated, bool customerLGI, List<excelOperations.order12NC> mstOrders)
        {
            DataTable result = new DataTable();
            Dictionary<string, Dictionary<string, double>> ngLineDateKey = new Dictionary<string, Dictionary<string, double>>();
            Dictionary<string, Dictionary<string, double>> scrapLineDateKey = new Dictionary<string, Dictionary<string, double>>();
            Dictionary<string, Dictionary<string, double>> prodLineDateKey = new Dictionary<string, Dictionary<string, double>>();

            Dictionary<string, Dictionary<string, Dictionary<string, double>>> ngLineDateKeyModel = new Dictionary<string, Dictionary<string, Dictionary<string, double>>>();
            Dictionary<string, Dictionary<string, Dictionary<string, double>>> scrapLineDateKeyModel = new Dictionary<string, Dictionary<string, Dictionary<string, double>>>();
            Dictionary<string, Dictionary<string, Dictionary<string, double>>> prodLineDateKeyModel = new Dictionary<string, Dictionary<string, Dictionary<string, double>>>();

            //Dictionary<string, Dictionary<string, double>> ngLevelPerLine = new Dictionary<string, Dictionary<string, double>>();
            //Dictionary<string, Dictionary<string, double>> scrapLevelPerLine = new Dictionary<string, Dictionary<string, double>>();


            Dictionary<string, double> totalProdDateKey = new Dictionary<string, double>();

            string frequencyKey = "";

            result.Columns.Add("Data");
            result.Columns.Add("Ilość");
            result.Columns.Add("Produkcja");
            result.Columns.Add("%");

            string[] mstOrdersList = mstOrders.Select(o => o.order).ToArray();
            string lineKey = "Total";
            foreach (var item in inputData)
            {
                if (customerLGI & mstOrdersList.Contains(item.NumerZlecenia)) continue;
                if (!customerLGI & !mstOrdersList.Contains(item.NumerZlecenia)) continue;
                if (item.FixedDateTime.Date < dateBegin || item.FixedDateTime.Date > dateEnd) continue;

                    string line = item.SmtLine;
                if (line == "") continue;
                if (!linesCumulated & !smtLines.Contains(item.SmtLine)) continue;
                if (!linesCumulated)
                {
                    lineKey = item.SmtLine;
                }

                string model = "";
                if (!modelDictionary.TryGetValue(item.NumerZlecenia, out model)) model = "???";

                switch (frequency)
                {
                    case "daily":
                        {
                            frequencyKey = item.FixedDateTime.Date.ToShortDateString();
                            break;
                        }
                    case "weekly":
                        {
                            frequencyKey = dateTools.GetIso8601WeekOfYear(item.FixedDateTime).ToString();
                            break;
                        }
                    case "monthly":
                        {
                            frequencyKey = dateTools.productionMonthNumber(dateTools.GetIso8601WeekOfYear(item.FixedDateTime), item.FixedDateTime.Year).ToString();
                            break;
                        }
                }

                //line key
                if (!ngLineDateKey.ContainsKey(lineKey))
                {
                    ngLineDateKey.Add(lineKey, new Dictionary<string, double>());
                    scrapLineDateKey.Add(lineKey, new Dictionary<string, double>());
                    prodLineDateKey.Add(lineKey, new Dictionary<string, double>());

                    ngLineDateKeyModel.Add(lineKey, new Dictionary<string, Dictionary<string, double>>());
                    scrapLineDateKeyModel.Add(lineKey, new Dictionary<string, Dictionary<string, double>>());
                    prodLineDateKeyModel.Add(lineKey, new Dictionary<string, Dictionary<string, double>>());
                }




                //frequency Key
                if (!ngLineDateKey[lineKey].ContainsKey(frequencyKey))
                {
                    ngLineDateKey[lineKey].Add(frequencyKey, 0);
                    scrapLineDateKey[lineKey].Add(frequencyKey, 0);
                    prodLineDateKey[lineKey].Add(frequencyKey, 0);

                    ngLineDateKeyModel[lineKey].Add(frequencyKey, new Dictionary<string, double>());
                    scrapLineDateKeyModel[lineKey].Add(frequencyKey, new Dictionary<string, double>());
                    prodLineDateKeyModel[lineKey].Add(frequencyKey, new Dictionary<string, double>());
                }
                if (!totalProdDateKey.ContainsKey(frequencyKey))
                {
                    totalProdDateKey.Add(frequencyKey, 0);
                }

                //modelKey
                if (!ngLineDateKeyModel[lineKey][frequencyKey].ContainsKey(model))
                {
                    ngLineDateKeyModel[lineKey][frequencyKey].Add(model, 0);
                    scrapLineDateKeyModel[lineKey][frequencyKey].Add(model, 0);
                    prodLineDateKeyModel[lineKey][frequencyKey].Add(model, 0);
                }



                if (model.Replace("LLFML", "") == comboModel.Text || comboModel.Text == "")
                {
                    ngLineDateKey[lineKey][frequencyKey] += item.AllNg;
                    scrapLineDateKey[lineKey][frequencyKey] += item.AllScrap;
                    prodLineDateKey[lineKey][frequencyKey] += item.AllQty;

                    ngLineDateKeyModel[lineKey][frequencyKey][model] += item.AllNg;
                    scrapLineDateKeyModel[lineKey][frequencyKey][model] += item.AllScrap;
                    prodLineDateKeyModel[lineKey][frequencyKey][model] += item.AllQty;

                    totalProdDateKey[frequencyKey] += item.AllQty;

                    ngLineDateKey[lineKey][frequencyKey] += item.AllNg;
                    scrapLineDateKey[lineKey][frequencyKey] += item.AllScrap;
                }

            }


            ChartArea ar = new ChartArea();
            ar.AxisX.LabelStyle.Interval = 1;
            ar.AxisY.MajorGrid.Interval = 0.5;
            ar.AxisY.MinorGrid.Interval = 0.1;
            ar.AxisY.MinorGrid.LineColor = System.Drawing.Color.Silver;
            ar.AxisY.MinorGrid.Enabled = true;
            ar.AxisY2.MajorGrid.Enabled = false;
            ar.AxisY.LabelStyle.Format = "{0.00} %";
            DataTable tempScrapTable = result.Clone();
            Series productionLevel = new Series();
            productionLevel.Name = "productionLevel";
            productionLevel.ChartType = SeriesChartType.Column;
            productionLevel.Name = "Poziom produkcji [szt.]";
            productionLevel.YAxisType = AxisType.Secondary;
            productionLevel.Color = System.Drawing.Color.AliceBlue;
            productionLevel.BorderColor = System.Drawing.Color.Silver;

            chartWasteLevel.Series.Clear();
            chartWasteLevel.ChartAreas.Clear();

            chartWasteLevel.ChartAreas.Add(ar);

            foreach (var keyEntry in totalProdDateKey)
            {
                productionLevel.Points.AddXY(keyEntry.Key, keyEntry.Value);
                
            }
            chartWasteLevel.Series.Add(productionLevel);

            foreach (var lineEntry in ngLineDateKey)
            {
                string line = lineEntry.Key;

                Series ngCumulatedSeries = new Series();
                Series scrapCumulatedSeries = new Series();
                ngCumulatedSeries.ChartType = SeriesChartType.Line;
                ngCumulatedSeries.BorderWidth = 3;
                ngCumulatedSeries.Name = line+" NG [%]";

                scrapCumulatedSeries.ChartType = SeriesChartType.Line;
                scrapCumulatedSeries.BorderWidth = 3;
                scrapCumulatedSeries.Name = line+" SCRAP [%]";

                foreach (var frequencyEntry in lineEntry.Value)
                {
                    double ng = 0;
                    double scrap = 0;

                    if (prodLineDateKey[line][frequencyEntry.Key] > 0)
                    {
                        ng = ((double)frequencyEntry.Value / (double)prodLineDateKey[line][frequencyEntry.Key]) * 100;
                        scrap = ((double)scrapLineDateKey[line][frequencyEntry.Key] / (double)prodLineDateKey[line][frequencyEntry.Key]) * 100;
                    }

                    List<string> ngPerModelToolTip = new List<string>();
                    List<Tuple<double, string>> scrapPerModelTooltip = new List<Tuple<double, string>>();

                    foreach (var model in scrapLineDateKeyModel[line][frequencyEntry.Key])
                    {
                        scrapPerModelTooltip.Add(new Tuple<double, string>(Math.Round(model.Value / prodLineDateKeyModel[line][frequencyEntry.Key][model.Key] * 100, 1), "% - " + model.Value + @"/" + prodLineDateKeyModel[line][frequencyEntry.Key][model.Key] + " - " + model.Key));
                    }

                    scrapPerModelTooltip = scrapPerModelTooltip.OrderByDescending(o => o.Item1).ToList();
                    foreach (var model in ngLineDateKeyModel[line][frequencyEntry.Key])
                    {
                        ngPerModelToolTip.Add(Math.Round((model.Value / prodLineDateKeyModel[line][frequencyEntry.Key][model.Key]) * 100, 1).ToString() + "% - " + model.Value + @"/" + prodLineDateKeyModel[line][frequencyEntry.Key][model.Key] + " - " + model.Key);
                    }

                    ngPerModelToolTip = ngPerModelToolTip.OrderByDescending(o => o).ToList();

                    string ngtoolTip = "";
                    foreach (var item in ngPerModelToolTip)
                    {
                        ngtoolTip += item + Environment.NewLine;
                    }
                    DataPoint ngPoint = new DataPoint();
                    ngPoint.SetValueXY(frequencyEntry.Key, ng);
                    ngPoint.ToolTip = ngtoolTip;
                    ngCumulatedSeries.Points.Add(ngPoint);

                    //scrap
                    string scrapTooltip = "";
                    foreach (var item in scrapPerModelTooltip)
                    {
                        scrapTooltip += item.Item1.ToString() + item.Item2 + Environment.NewLine;
                    }

                    DataPoint scrapPoint = new DataPoint();
                    scrapPoint.SetValueXY(frequencyEntry.Key, scrap);
                    scrapPoint.ToolTip = scrapTooltip;

                    scrapCumulatedSeries.Points.Add(scrapPoint);
                    result.Rows.Add("NG:");
                    result.Rows.Add(frequencyEntry.Key, frequencyEntry.Value, (double)prodLineDateKey[line][frequencyEntry.Key], Math.Round(ng, 2) + "%");
                    tempScrapTable.Rows.Add(frequencyEntry.Key, scrapLineDateKey[line][frequencyEntry.Key], (double)prodLineDateKey[line][frequencyEntry.Key], Math.Round(scrap, 2) + "%");

                    result.Rows.Add();
                    result.Rows.Add("SCRAP:");

                    foreach (DataRow r in tempScrapTable.Rows)
                    {
                        result.Rows.Add(r[0].ToString(), r[1].ToString(), r[2].ToString(), r[3].ToString());
                    }

                }
                chartWasteLevel.Series.Add(ngCumulatedSeries);
                chartWasteLevel.Series.Add(scrapCumulatedSeries);
            }
            

            chartWasteLevel.Legends[0].DockedToChartArea = chartWasteLevel.ChartAreas[0].Name;
            chartWasteLevel.Legends[0].Position.Auto = false;
            chartWasteLevel.Legends[0].Position = new ElementPosition(8, 0, 30, 10);
            chartWasteLevel.Legends[0].BackColor = System.Drawing.Color.Transparent;

            foreach (var point in chartWasteLevel.Series[2].Points)
            {
                point.MarkerStyle = MarkerStyle.Circle;
                point.MarkerSize = 10;
            }

            foreach (var point in chartWasteLevel.Series[1].Points)
            {
                point.MarkerStyle = MarkerStyle.Circle;
                point.MarkerSize = 10;
            }










            //var ngLimit = new HorizontalLineAnnotation();
            //ngLimit.AxisY = ar.AxisY;
            //ngLimit.AxisX = ar.AxisX;
            //ngLimit.ClipToChartArea = chartWasteLevel.ChartAreas[0].Name;
            //ngLimit.LineColor = System.Drawing.Color.Red;
            //ngLimit.LineWidth = 2;
            //ngLimit.AnchorY = 0;
            //ngLimit.IsSizeAlwaysRelative = true;
            //var scrapLimit = new HorizontalLineAnnotation();

            //chartWasteLevel.Annotations.Add(ngLimit);



            return result;
        }

        public static DataTable DrawWasteLevelOld(string frequency, Chart chartWasteLevel, List<WasteDataStructure> inputData, DateTime dateBegin, DateTime dateEnd, Dictionary<string, string> modelDictionary, ComboBox comboModel, string[] smtLines,bool linesCumulated,  bool customerLGI, List<excelOperations.order12NC> mstOrders)
        {
            DataTable result = new DataTable();
            Dictionary<string, double> ngLevel = new Dictionary<string, double>();
            Dictionary<string, double> scrapLevel = new Dictionary<string, double>();
            Dictionary<string, double> allLevel = new Dictionary<string, double>();

            Dictionary<string, Dictionary<string, double>> ngLevelPerModel = new Dictionary<string, Dictionary<string, double>>();
            Dictionary<string, Dictionary<string, double>> scrapLevelPerModel = new Dictionary<string, Dictionary<string, double>>();
            Dictionary<string, Dictionary<string, double>> allLevelPerModel = new Dictionary<string, Dictionary<string, double>>();

            Dictionary<string, Dictionary<string, double>> ngLevelPerLine = new Dictionary<string, Dictionary<string, double>>();
            Dictionary<string, Dictionary<string, double>> scrapLevelPerLine = new Dictionary<string, Dictionary<string, double>>();


            Dictionary<string, double> totalProdQuantity = new Dictionary<string, double>();

            string dictionaryKey = "";

            result.Columns.Add("Data");
            result.Columns.Add("Ilość");
            result.Columns.Add("Produkcja");
            result.Columns.Add("%");

            string[] mstOrdersList = mstOrders.Select(o => o.order).ToArray();
            foreach (var item in inputData)
            {
                if (customerLGI & mstOrdersList.Contains(item.NumerZlecenia)) continue;
                if (!customerLGI & !mstOrdersList.Contains(item.NumerZlecenia)) continue;
                if (!linesCumulated & !smtLines.Contains(item.SmtLine)) continue;

                string line = item.SmtLine;
                if (line == "") continue;

                if (item.FixedDateTime.Date >= dateBegin & item.FixedDateTime.Date <= dateEnd)
                {
                    string model = "";
                    if (!modelDictionary.TryGetValue(item.NumerZlecenia, out model)) model = "???";
                    
                    switch (frequency)
                    {
                        case "daily":
                            {
                                dictionaryKey = item.FixedDateTime.Date.ToShortDateString();
                                break;
                            }
                        case "weekly":
                            {
                                dictionaryKey = dateTools.GetIso8601WeekOfYear(item.FixedDateTime).ToString();
                                break;
                            }
                        case "monthly":
                            {
                                dictionaryKey = dateTools.productionMonthNumber(dateTools.GetIso8601WeekOfYear(item.FixedDateTime), item.FixedDateTime.Year).ToString();
                                break;
                            }
                    }


                    if (!ngLevelPerLine.ContainsKey(line))
                    {
                        ngLevelPerLine.Add(line, new Dictionary<string, double>());
                        scrapLevelPerLine.Add(line, new Dictionary<string, double>());
                    }

                    if (!ngLevel.ContainsKey(dictionaryKey))
                    {
                        ngLevel.Add(dictionaryKey, 0);
                        scrapLevel.Add(dictionaryKey, 0);
                        allLevel.Add(dictionaryKey, 0);

                        ngLevelPerModel.Add(dictionaryKey, new Dictionary<string, double>());
                        scrapLevelPerModel.Add(dictionaryKey, new Dictionary<string, double>());
                        allLevelPerModel.Add(dictionaryKey, new Dictionary<string, double>());

                        totalProdQuantity.Add(dictionaryKey, 0);

                        ngLevelPerLine[line].Add(dictionaryKey, 0);
                        scrapLevelPerLine[line].Add(dictionaryKey, 0);
                    }

                    if (!ngLevelPerModel[dictionaryKey].ContainsKey(model))
                    {
                        ngLevelPerModel[dictionaryKey].Add(model, 0);
                        scrapLevelPerModel[dictionaryKey].Add(model, 0);
                        allLevelPerModel[dictionaryKey].Add(model, 0);
                    }

                    if (model.Replace("LLFML", "") == comboModel.Text || comboModel.Text == "")
                    {
                        ngLevel[dictionaryKey] += item.AllNg;
                        scrapLevel[dictionaryKey] += item.AllScrap;
                        allLevel[dictionaryKey] += item.AllQty;

                        ngLevelPerModel[dictionaryKey][model] += item.AllNg;
                        scrapLevelPerModel[dictionaryKey][model] += item.AllScrap;
                        allLevelPerModel[dictionaryKey][model] += item.AllQty;

                        totalProdQuantity[dictionaryKey] += item.AllQty;

                        ngLevelPerLine[line][dictionaryKey] += item.AllNg;
                        scrapLevelPerLine[line][dictionaryKey] += item.AllScrap;
                    }
                }
            }

            ChartArea ar = new ChartArea();
            ar.AxisX.LabelStyle.Interval = 1;
            ar.AxisY.MajorGrid.Interval = 0.5;
            ar.AxisY.MinorGrid.Interval = 0.1;
            ar.AxisY.MinorGrid.LineColor = System.Drawing.Color.Silver;
            ar.AxisY.MinorGrid.Enabled = true;
            ar.AxisY2.MajorGrid.Enabled = false;
            ar.AxisY.LabelStyle.Format = "{0.00} %";
            DataTable tempScrapTable = result.Clone();
            Series productionLevel = new Series();
            productionLevel.Name = "productionLevel";
            productionLevel.ChartType = SeriesChartType.Column;
            productionLevel.Name = "Poziom produkcji [szt.]";
            productionLevel.YAxisType = AxisType.Secondary;
            productionLevel.Color = System.Drawing.Color.AliceBlue;
            productionLevel.BorderColor = System.Drawing.Color.Silver;

            chartWasteLevel.Series.Clear();
            chartWasteLevel.ChartAreas.Clear();

            if (linesCumulated)
            {
                Series ngCumulatedSeries = new Series();
                Series scrapCumulatedSeries = new Series();
                ngCumulatedSeries.ChartType = SeriesChartType.Line;
                ngCumulatedSeries.BorderWidth = 3;
                ngCumulatedSeries.Name = "NG [%]";

                scrapCumulatedSeries.ChartType = SeriesChartType.Line;
                scrapCumulatedSeries.BorderWidth = 3;
                scrapCumulatedSeries.Name = "SCRAP [%]";

                foreach (var keyEntry in ngLevel)
                {
                    double ng = 0;
                    double scrap = 0;

                    if (allLevel[keyEntry.Key] > 0)
                    {
                        ng = ((double)keyEntry.Value / (double)allLevel[keyEntry.Key]) * 100;
                        scrap = ((double)scrapLevel[keyEntry.Key] / (double)allLevel[keyEntry.Key]) * 100;
                    }

                    List<string> ngPerModelToolTip = new List<string>();
                    List<Tuple<double, string>> scrapPerModelTooltip = new List<Tuple<double, string>>();

                    foreach (var model in scrapLevelPerModel[keyEntry.Key])
                    {
                        scrapPerModelTooltip.Add(new Tuple<double, string>(Math.Round(model.Value / allLevelPerModel[keyEntry.Key][model.Key] * 100, 1), "% - " + model.Value + @"/" + allLevelPerModel[keyEntry.Key][model.Key] + " - " + model.Key));
                    }
                    
                    scrapPerModelTooltip = scrapPerModelTooltip.OrderByDescending(o => o.Item1).ToList();
                    foreach (var model in ngLevelPerModel[keyEntry.Key])
                    {
                        ngPerModelToolTip.Add(Math.Round((model.Value / allLevelPerModel[keyEntry.Key][model.Key]) * 100, 1).ToString() + "% - " + model.Value + @"/" + allLevelPerModel[keyEntry.Key][model.Key] + " - " + model.Key);
                    }

                    ngPerModelToolTip = ngPerModelToolTip.OrderByDescending(o => o).ToList();

                    string ngtoolTip = "";
                    foreach (var item in ngPerModelToolTip)
                    {
                        ngtoolTip += item + Environment.NewLine;
                    }
                    DataPoint ngPoint = new DataPoint();
                    ngPoint.SetValueXY(keyEntry.Key, ng);
                    ngPoint.ToolTip = ngtoolTip;
                    ngCumulatedSeries.Points.Add(ngPoint);
                    //scrap

                    string scrapTooltip = "";
                    foreach (var item in scrapPerModelTooltip)
                    {
                        scrapTooltip += item.Item1.ToString() + item.Item2 + Environment.NewLine;
                    }

                    DataPoint scrapPoint = new DataPoint();
                    scrapPoint.SetValueXY(keyEntry.Key, scrap);
                    scrapPoint.ToolTip = scrapTooltip;

                    scrapCumulatedSeries.Points.Add(scrapPoint);
                    result.Rows.Add("NG:");
                    result.Rows.Add(keyEntry.Key, keyEntry.Value, (double)allLevel[keyEntry.Key], Math.Round(ng, 2) + "%");
                    tempScrapTable.Rows.Add(keyEntry.Key, scrapLevel[keyEntry.Key], (double)allLevel[keyEntry.Key], Math.Round(scrap, 2) + "%");

                    productionLevel.Points.AddXY(keyEntry.Key, totalProdQuantity[keyEntry.Key]);

                    result.Rows.Add();
                    result.Rows.Add("SCRAP:");

                    foreach (DataRow r in tempScrapTable.Rows)
                    {
                        result.Rows.Add(r[0].ToString(), r[1].ToString(), r[2].ToString(), r[3].ToString());
                    }
                    chartWasteLevel.Series.Add(productionLevel);
                    chartWasteLevel.ChartAreas.Add(ar);
                    chartWasteLevel.Series.Add(ngCumulatedSeries);
                    chartWasteLevel.Series.Add(scrapCumulatedSeries);

                    chartWasteLevel.Legends[0].DockedToChartArea = chartWasteLevel.ChartAreas[0].Name;
                    chartWasteLevel.Legends[0].Position.Auto = false;
                    chartWasteLevel.Legends[0].Position = new ElementPosition(8, 0, 30, 10);
                    chartWasteLevel.Legends[0].BackColor = System.Drawing.Color.Transparent;

                    foreach (var point in chartWasteLevel.Series[2].Points)
                    {
                        point.MarkerStyle = MarkerStyle.Circle;
                        point.MarkerSize = 10;
                    }

                    foreach (var point in chartWasteLevel.Series[1].Points)
                    {
                        point.MarkerStyle = MarkerStyle.Circle;
                        point.MarkerSize = 10;
                    }

                }
            }
            else
            {
                List<Series> ngSeriesPerLine = new List<Series>();
                foreach (var line in smtLines)
                {
                    Series ngLineSeries = new Series();
                    ngLineSeries.ChartType = SeriesChartType.Line;
                    ngLineSeries.BorderWidth = 3;
                    ngLineSeries.Name = "NG [%]";
                }
            }




            




           
            

            //var ngLimit = new HorizontalLineAnnotation();
            //ngLimit.AxisY = ar.AxisY;
            //ngLimit.AxisX = ar.AxisX;
            //ngLimit.ClipToChartArea = chartWasteLevel.ChartAreas[0].Name;
            //ngLimit.LineColor = System.Drawing.Color.Red;
            //ngLimit.LineWidth = 2;
            //ngLimit.AnchorY = 0;
            //ngLimit.IsSizeAlwaysRelative = true;
            //var scrapLimit = new HorizontalLineAnnotation();

            //chartWasteLevel.Annotations.Add(ngLimit);

            

            return result;
        }

        public static DataTable DrawWasteReasonsCHart(Chart ngChart, Chart scrapChart, List<WasteDataStructure> inputData, DateTime dateBegin, DateTime dateEnd, Dictionary<string, string> modelDictionary, string[] smtLines, bool customerLGI, List<excelOperations.order12NC> mstOrders)
        {
            DataTable result = new DataTable();
            result.Columns.Add("Nazwa");
            result.Columns.Add("Ilość");
            List<string> mstOrderss = mstOrders.Select(o => o.order).ToList();
            Dictionary<string, WastePerReasonStructure> wastePerReason = new Dictionary<string, WastePerReasonStructure>();

            foreach (var wasteRecord in inputData)
            {
                if (wasteRecord.FixedDateTime.Date < dateBegin.Date || wasteRecord.FixedDateTime.Date > dateEnd.Date) continue;

                if (customerLGI & mstOrderss.Contains(wasteRecord.NumerZlecenia)) continue;
                if (!customerLGI & !mstOrderss.Contains(wasteRecord.NumerZlecenia)) continue;

                if (!smtLines.Contains( wasteRecord.SmtLine) & customerLGI) continue;

                //only once
                if (wastePerReason.Count==0)
                {
                    foreach (var reasonKey in wasteRecord.WastePerReason)
                    {
                        wastePerReason.Add(reasonKey.Key, new WastePerReasonStructure(new List<WasteDataStructure>(),0));
                    }
                }

                foreach (var reasonKey in wasteRecord.WastePerReason)
                {
                    if (wasteRecord.WastePerReason[reasonKey.Key] == 0) continue;

                    wastePerReason[reasonKey.Key].Lots.Add(wasteRecord);
                    wastePerReason[reasonKey.Key].Quantity += wasteRecord.WastePerReason[reasonKey.Key];
                }
            }

            wastePerReason = wastePerReason.OrderByDescending(o => o.Value.Quantity).ToDictionary(k => k.Key, o => o.Value);
            foreach (var reasonEntry in wastePerReason)
            {
                reasonEntry.Value.Lots = reasonEntry.Value.Lots.OrderByDescending(q => q.WastePerReason[reasonEntry.Key]).ToList();
            }

            ngChart.Series.Clear();
            ngChart.ChartAreas.Clear();

            scrapChart.Series.Clear();
            scrapChart.ChartAreas.Clear();

            Series ngSeries = new Series();
            ngSeries.ChartType = SeriesChartType.Column;

            Series scrapSeries = new Series();
            scrapSeries.ChartType = SeriesChartType.Column;


            ChartArea ngArea = new ChartArea();
            ngArea.AxisX.LabelStyle.Interval = 1;
            ngArea.AxisX.IsLabelAutoFit = true;
            ngArea.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.LabelsAngleStep30;
            ngArea.AxisX.LabelStyle.Font = new System.Drawing.Font(ngArea.AxisX.LabelStyle.Font.Name, 10f);

            ChartArea scrapArea = new ChartArea();
            scrapArea.AxisX.LabelStyle.Interval = 1;
            scrapArea.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.LabelsAngleStep30;
            scrapArea.AxisX.LabelStyle.Font = new System.Drawing.Font(scrapArea.AxisX.LabelStyle.Font.Name, 10f);

            DataTable scrapTempTable = result.Clone();
            result.Rows.Add("NG", "");
            foreach (var wasteReason in wastePerReason)
            {
                if (wasteReason.Key.Substring(0, 2) == "ng")
                {
                    Dictionary<string, int> labelDictNg = new Dictionary<string, int>();
                    Dictionary<string, int> labelDictTotal = new Dictionary<string, int>();
                    foreach (var lot in wasteReason.Value.Lots)
                    {
                        if (!labelDictNg.ContainsKey(lot.Model))
                        {
                            labelDictNg.Add(lot.Model, 0);
                            labelDictTotal.Add(lot.Model, 0);
                        }

                        labelDictNg[lot.Model] += lot.WastePerReason[wasteReason.Key];
                        labelDictTotal[lot.Model] += lot.GoodQty ;
                    }

                    labelDictNg=labelDictNg.OrderByDescending(q => q.Value).ToDictionary(k => k.Key, q => q.Value);

                    string label =  string.Join(Environment.NewLine, labelDictNg.Select(m=>(m.Key+"-"+m.Value+"/"+ labelDictTotal[m.Key])).ToArray());


                    DataPoint ngPoint = new DataPoint();
                    ngPoint.SetValueXY(wasteReason.Key, wasteReason.Value.Quantity);
                    ngPoint.ToolTip = label;
                    ngPoint.Tag = wastePerReason[wasteReason.Key];
                    ngSeries.Points.Add(ngPoint);

                    result.Rows.Add(wasteReason.Key, wasteReason.Value.Quantity);
                }

                else if (wasteReason.Key.Substring(0, 2) == "sc")
                {
                    Dictionary<string, int> labelDictNg = new Dictionary<string, int>();
                    Dictionary<string, int> labelDictTotal = new Dictionary<string, int>();
                    foreach (var lot in wasteReason.Value.Lots)
                    {
                        if (!labelDictNg.ContainsKey(lot.Model))
                        {
                            labelDictNg.Add(lot.Model, 0);
                            labelDictTotal.Add(lot.Model, 0);
                        }

                        labelDictNg[lot.Model] += lot.WastePerReason[wasteReason.Key];
                        labelDictTotal[lot.Model] += lot.GoodQty;
                    }

                    labelDictNg = labelDictNg.OrderByDescending(q => q.Value).ToDictionary(k => k.Key, q => q.Value);

                    string label = string.Join(Environment.NewLine, labelDictNg.Select(m => (m.Key + "-" + m.Value + "/" + labelDictTotal[m.Key])).ToArray());

                    DataPoint scrapPoint = new DataPoint();
                    scrapPoint.SetValueXY(wasteReason.Key, wasteReason.Value.Quantity);
                    scrapPoint.ToolTip = label;
                    scrapPoint.Tag = wastePerReason[wasteReason.Key];
                    scrapSeries.Points.Add(scrapPoint);
                    scrapTempTable.Rows.Add(wasteReason.Key, wasteReason.Value.Quantity);
                }
            }

            result.Rows.Add();
            result.Rows.Add("SCRAP:", "");
            foreach (DataRow row in scrapTempTable.Rows)
            {
                result.Rows.Add(row[0].ToString(), row[1].ToString());
            }

            ngChart.Series.Add(ngSeries);
            ngChart.ChartAreas.Add(ngArea);
            ngChart.Legends.Clear();

            scrapChart.Series.Add(scrapSeries);
            scrapChart.ChartAreas.Add(scrapArea);
            scrapChart.Legends.Clear();



            return result;
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
    }
}
