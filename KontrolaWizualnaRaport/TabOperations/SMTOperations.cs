﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static KontrolaWizualnaRaport.dateTools;

namespace KontrolaWizualnaRaport
{
    class SMTOperations
    {
        public struct LotLedWasteStruc
        {
            public string lotId;
            public string smtLine;
            public string model;
            public int manufacturedModules;
            public int reelsUsed;
            public int ledsPerReel;
            public int requiredRankA;
            public int requiredRankB;
            public int ledLeftA;
            public int ledLeftB;
        }

        public static SortedDictionary<DateTime, SortedDictionary<int, List<LotLedWasteStruc>>> ledWasteDictionary = new SortedDictionary<DateTime, SortedDictionary<int, List<LotLedWasteStruc>>>();

        public static void ReLoadSmtTab(
            DataGridView dataGridViewSmtProduction, 
            DataGridView dataGridViewChangeOvers,
            DataGridView dataGridViewSmtLedDropped,
            DataGridView dataGridViewSmtLedWasteByModel,
            DataGridView dataGridViewSmtWasteTotal,
            DataGridView dataGridViewSmtLedWasteTotalPerLine,
            DataGridView dataGridViewSmtStencilUsage,
            DataTable smtRecords, 
            ref Dictionary<string, Dictionary<string, List<durationQuantity>>> smtModelLineQuantity, 
            RadioButton radioButtonSmtShowAllModels,
            Dictionary<string, MesModels> mesModels,
            ComboBox comboBoxSmtModels,
            ComboBox comboBoxSmtLedWasteLine,
            ComboBox comboBoxSmtLewWasteFreq,
            ComboBox comboBoxSmtLedWasteLines,
            Dictionary<string, Color> lineColors, 
            Chart chartLedWasteChart,
            Panel panelSmtLedWasteCheckContainer)
        {
            SortedDictionary<DateTime, SortedDictionary<int, DataTable>> sortedTableByDayAndShift = SMTOperations.sortTableByDayAndShift(smtRecords, "DataCzasKoniec");
            SMTOperations.shiftSummaryDataSource(sortedTableByDayAndShift, dataGridViewSmtProduction);

            smtModelLineQuantity = SMTOperations.smtQtyPerModelPerLine(smtRecords, radioButtonSmtShowAllModels.Checked, mesModels);
            comboBoxSmtModels.Items.AddRange(smtModelLineQuantity.Select(m => m.Key).OrderBy(m => m).ToArray());

            ChangeOverTools.BuildSmtChangeOverGrid(ChangeOverTools.BuildDateShiftLineDictionary(smtRecords), dataGridViewChangeOvers);
            
            ledWasteDictionary = LedWasteDictionary(sortedTableByDayAndShift, mesModels);
            SMTOperations.FillOutDailyLedWaste(ledWasteDictionary, dataGridViewSmtLedDropped);
            SMTOperations.FillOutLedWasteByModel(ledWasteDictionary, dataGridViewSmtLedWasteByModel, comboBoxSmtLedWasteLine.Text);
            SMTOperations.FillOutLedWasteTotalWeekly(ledWasteDictionary, dataGridViewSmtWasteTotal);
            Dictionary<string, bool> lineOptions = new Dictionary<string, bool>();
            lineColors = new Dictionary<string, Color>();

            foreach (Control c in panelSmtLedWasteCheckContainer.Controls)
            {
                if ((c is CheckBox))
                {
                    lineOptions.Add(c.Text.Trim(), ((CheckBox)c).Checked);
                    lineColors.Add(c.Text.Trim(), c.BackColor);
                }
            }

            Charting.DrawLedWasteChart(ledWasteDictionary, chartLedWasteChart, comboBoxSmtLewWasteFreq.Text, lineOptions, lineColors);

            comboBoxSmtLedWasteLine.Items.Add("Wszystkie");
            comboBoxSmtLedWasteLine.Items.AddRange(smtModelLineQuantity.SelectMany(m => m.Value).Select(l => l.Key).Distinct().OrderBy(l => l).ToArray());
            comboBoxSmtLedWasteLine.Text = "Wszystkie";
            comboBoxSmtLedWasteLines.Items.AddRange(ledWasteDictionary.SelectMany(date => date.Value).SelectMany(shift => shift.Value).Select(l => l.model).Distinct().OrderBy(l => l).ToArray());
            comboBoxSmtLedWasteLines.Items.Insert(0, "Wszystkie");
            comboBoxSmtLedWasteLines.Text = "Wszystkie";
            SMTOperations.FillOutLedWasteTotalByLine(ledWasteDictionary, dataGridViewSmtLedWasteTotalPerLine, comboBoxSmtLedWasteLines.Text);
            FillOutStencilTable(dataGridViewSmtStencilUsage, mesModels);
        }

        public static void FillOutLedWasteTotalByLine(SortedDictionary<DateTime, SortedDictionary<int, List<LotLedWasteStruc>>> ledWasteDictionary, DataGridView grid, string model)
        {
            grid.SuspendLayout();
            grid.Rows.Clear();
            List<string> lines = ledWasteDictionary.SelectMany(date => date.Value).SelectMany(shift => shift.Value).Select(l => l.smtLine).Distinct().OrderBy(l => l).ToList();
            
            Dictionary<string, double> producedPerLineA = new Dictionary<string, double>();
            Dictionary<string, double> producedPerLineB = new Dictionary<string, double>();
            Dictionary<string, double> wastePerLineA = new Dictionary<string, double>();
            Dictionary<string, double> wastePerLineB = new Dictionary<string, double>();
            Dictionary<string, DataTable> tagTable = new Dictionary<string, DataTable>();

            DataTable template = new DataTable();
            template.Columns.Add("LOT");
            template.Columns.Add("Model");
            template.Columns.Add("Data");
            template.Columns.Add("Linia");
            template.Columns.Add("Mont.A");
            template.Columns.Add("Odp_A");
            template.Columns.Add("Mont.B");
            template.Columns.Add("Odp_B");

            grid.Columns.Clear();
            grid.Columns.Add("Poz", "");
            foreach (var item in lines)
            {
                grid.Columns.Add(item, item);
                producedPerLineA.Add(item, 0);
                producedPerLineB.Add(item, 0);
                wastePerLineA.Add(item, 0);
                wastePerLineB.Add(item, 0);
                tagTable.Add(item, template.Clone());
                tagTable[item].Rows.Add();
            }

            foreach (var dateEntry in ledWasteDictionary)
            {
                foreach (var shiftEntry in dateEntry.Value)
                {
                    foreach (var lot in shiftEntry.Value)
                    {
                        if (lot.model != model & model != "Wszystkie") continue;
                        if (lot.manufacturedModules < 1) continue;

                        var lotWaste = CalculateLotLedWaste(lot);

                        producedPerLineA[lot.smtLine] += lotWaste.ledExpectedUsageA;
                        wastePerLineA[lot.smtLine] += lotWaste.droppedA;
                        producedPerLineB[lot.smtLine] += lotWaste.ledExpectedUsageB;
                        wastePerLineB[lot.smtLine] += lotWaste.droppedB;

                        tagTable[lot.smtLine].Rows.Add(lot.lotId, lot.model, dateEntry.Key.ToString("dd-MM-yyyy"), lot.smtLine, lotWaste.ledExpectedUsageA, lotWaste.droppedA, lotWaste.ledExpectedUsageB, lotWaste.droppedB);
                    }
                }
            }

            grid.Rows.Add(6);
            foreach (var lineEntry in producedPerLineA)
            {
                grid.Rows[0].Cells[0].Value = "Mont_A";
                grid.Rows[0].Cells[lineEntry.Key].Value = producedPerLineA[lineEntry.Key];
                grid.Rows[0].Cells[lineEntry.Key].Tag = tagTable[lineEntry.Key];

                grid.Rows[1].Cells[0].Value = "Odp_A";
                grid.Rows[1].Cells[lineEntry.Key].Value = wastePerLineA[lineEntry.Key];
                grid.Rows[1].Cells[lineEntry.Key].Tag = tagTable[lineEntry.Key];

                double wasteA = 0;
                if (producedPerLineA[lineEntry.Key]>0)
                {
                    wasteA = Math.Round(wastePerLineA[lineEntry.Key] / producedPerLineA[lineEntry.Key] * 100, 2);
                }
                grid.Rows[2].Cells[0].Value = "Odp%_A";
                grid.Rows[2].Cells[lineEntry.Key].Value = wasteA + "%";
                grid.Rows[2].Cells[lineEntry.Key].Tag = tagTable[lineEntry.Key];

                grid.Rows[3].Cells[0].Value = "Mont_B";
                grid.Rows[3].Cells[lineEntry.Key].Value = producedPerLineB[lineEntry.Key];
                grid.Rows[3].Cells[lineEntry.Key].Tag = tagTable[lineEntry.Key];

                grid.Rows[4].Cells[0].Value = "Odp_B";
                grid.Rows[4].Cells[lineEntry.Key].Value = wastePerLineB[lineEntry.Key];
                grid.Rows[4].Cells[lineEntry.Key].Tag = tagTable[lineEntry.Key];

                double wasteB = 0;
                if (producedPerLineB[lineEntry.Key] > 0)
                {
                    wasteB = Math.Round(wastePerLineB[lineEntry.Key] / producedPerLineB[lineEntry.Key] * 100, 2);
                }
                grid.Rows[5].Cells[0].Value = "Odp%_B";
                grid.Rows[5].Cells[lineEntry.Key].Value = wasteB + "%";
                grid.Rows[5].Cells[lineEntry.Key].Tag = tagTable[lineEntry.Key];
            }
            autoSizeGridColumns(grid);
            grid.ResumeLayout();
        }

        public struct LotLedWaste
        {
            public Int32 ledExpectedUsageA;
            public Int32 droppedA;
            public Int32 ledExpectedUsageB;
            public Int32 droppedB;
        }

        public static LotLedWaste CalculateLotLedWaste(LotLedWasteStruc lot)
        {
            int ledExpectedUsageA = lot.requiredRankA * lot.manufacturedModules;
            int ledExpectedUsageB = lot.requiredRankB * lot.manufacturedModules;
            int ledExpectedLeftoversA = lot.reelsUsed / 2 * lot.ledsPerReel - ledExpectedUsageA;
            int ledExpectedLeftoversB = lot.reelsUsed / 2 * lot.ledsPerReel - ledExpectedUsageB;
            int droppedA = ledExpectedLeftoversA - lot.ledLeftA;
            int droppedB = ledExpectedLeftoversB - lot.ledLeftB;

            LotLedWaste result = new LotLedWaste();
            result.droppedA = droppedA;
            result.droppedB = droppedB;
            result.ledExpectedUsageA = ledExpectedUsageA;
            result.ledExpectedUsageB = ledExpectedUsageB;

            return result;
        }

        public static void FillOutLedWasteByModel(SortedDictionary<DateTime, SortedDictionary<int, List<LotLedWasteStruc>>> ledWasteDictionary, DataGridView grid, string line)
        {
            grid.SuspendLayout();
            grid.Columns.Clear();
            grid.Columns.Add("Model", "Model");
            grid.Columns.Add("Mont_LED", "Mont.LED");
            grid.Columns.Add("Odp_LED", "Odpad LED");
            grid.Columns.Add("Odp", "Odpad");
            grid.Columns.Add("LED", "LED");

            Dictionary<string, double> mountedLed = new Dictionary<string, double>();
            Dictionary<string, DataTable> detailsTag = new Dictionary<string, DataTable>();
            Dictionary<string, double> droppedLed = new Dictionary<string, double>();
            Dictionary<string, double> ledWaste = new Dictionary<string, double>();
            Dictionary<string, string> ledPackage = new Dictionary<string, string>();

            DataTable template = new DataTable();
            template.Columns.Add("LOT");
            template.Columns.Add("Model");
            template.Columns.Add("Data");
            template.Columns.Add("Linia");
            template.Columns.Add("Mont.A");
            template.Columns.Add("Odp_A");
            template.Columns.Add("Mont.B");
            template.Columns.Add("Odp_B");

            foreach (var dateEntry in ledWasteDictionary)
            {
                
                foreach (var shiftEntry in dateEntry.Value)
                {
                    foreach (var lot in shiftEntry.Value)
                    {
                        if (lot.smtLine != line & line != "Wszystkie") continue;
                        string model = lot.model;
                        string pckg = "";

                        if (lot.ledsPerReel>3000)
                        {
                            pckg = "2835";
                        }
                        else
                        {
                            pckg = "5630";
                        }
                        if(!mountedLed.ContainsKey(model))
                        {
                            mountedLed.Add(model, 0);
                            droppedLed.Add(model, 0);
                            ledWaste.Add(model, 0);
                            detailsTag.Add(model, template.Clone());
                            detailsTag[model].Rows.Add(lot.model + " specA=" + lot.requiredRankA + " specB=" + lot.requiredRankB);
                            ledPackage.Add(model, pckg);
                        }

                        var lotWaste = CalculateLotLedWaste(lot);

                        detailsTag[model].Rows.Add(lot.lotId,lot.model, dateEntry.Key.ToString("dd-MM-yyyy"),lot.smtLine, lotWaste.ledExpectedUsageA, lotWaste.droppedA, lotWaste.ledExpectedUsageB, lotWaste.droppedB);
                        mountedLed[model] += lotWaste.ledExpectedUsageA + lotWaste.ledExpectedUsageB;
                        droppedLed[model] += lotWaste.droppedA + lotWaste.droppedB;
                        ledWaste[model] = Math.Round(droppedLed[model] / mountedLed[model] * 100, 2);
                    }
                }

                
            }

            foreach (var modelEntry in mountedLed)
            {
                grid.Rows.Add(modelEntry.Key, mountedLed[modelEntry.Key], droppedLed[modelEntry.Key], ledWaste[modelEntry.Key] + "%", ledPackage[modelEntry.Key]);
                foreach (DataGridViewCell cell in grid.Rows[grid.Rows.Count-1].Cells)
                {
                    cell.Tag = detailsTag[modelEntry.Key];
                }
            }

            autoSizeGridColumns(grid);
            grid.ResumeLayout();
        }

        public static void FillOutLedWasteTotalWeekly(SortedDictionary<DateTime, SortedDictionary<int, List<LotLedWasteStruc>>> ledWasteDictionary, DataGridView grid)
        {
            grid.SuspendLayout();
            grid.Columns.Clear();
            grid.Columns.Add("Tydz", "Tydz");
            grid.Columns.Add("MontLED", "MontLED");
            grid.Columns.Add("OdpadLED", "OdpadLED");
            grid.Columns.Add("%", "%");

            Dictionary<string, double> ledMounted = new Dictionary<string, double>();
            Dictionary<string, double> ledDropped = new Dictionary<string, double>();
            Dictionary<string, double> ledWaste = new Dictionary<string, double>();
            double monthMounted = 0;
            double monthDropped = 0;
            double monthwaste = 0;

            string monthName = "";

            foreach (var dateEntry in ledWasteDictionary)
            {
                if (dateEntry.Key.ToString("MMM", CultureInfo.InvariantCulture)!=monthName & monthName!="")
                {
                    ledMounted.Add(monthName, monthMounted);
                    ledDropped.Add(monthName, monthDropped);
                    ledWaste.Add(monthName, monthwaste);
                    monthMounted = 0;
                    monthDropped = 0;
                    monthwaste = 0;
                }
                string week = dateTools.GetIso8601WeekOfYear(dateEntry.Key).ToString();
                monthName = dateEntry.Key.ToString("MMM", CultureInfo.InvariantCulture);

                if (!ledMounted.ContainsKey(week))
                {
                    ledMounted.Add(week, 0);
                    ledDropped.Add(week, 0);
                    ledWaste.Add(week, 0);
                }

                foreach (var shiftEntry in dateEntry.Value)
                {
                    foreach (var lotData in shiftEntry.Value)
                    {
                        var lotWaste = CalculateLotLedWaste(lotData);

                        ledMounted[week] += lotWaste.ledExpectedUsageA + lotWaste.ledExpectedUsageB;
                        ledDropped[week] += lotWaste.droppedA + lotWaste.droppedB;
                        ledWaste[week] = Math.Round(ledDropped[week] / ledMounted[week] * 100, 2);
                        monthMounted += lotWaste.ledExpectedUsageA + lotWaste.ledExpectedUsageB;
                        monthDropped += lotWaste.droppedA + lotWaste.droppedB;
                        //monthwaste = Math.Round(ledDropped[week] / ledMounted[week] * 100, 2);
                        monthwaste = Math.Round(monthDropped / monthMounted * 100, 2);
                    }
                }

            }
            foreach (var weekEntry in ledMounted)
            {
                grid.Rows.Add(weekEntry.Key, ledMounted[weekEntry.Key], ledDropped[weekEntry.Key], ledWaste[weekEntry.Key]);
            }
            autoSizeGridColumns(grid);
            grid.ResumeLayout();
        }

        public static void FillOutDailyLedWaste(SortedDictionary<DateTime, SortedDictionary<int, List<LotLedWasteStruc>>> ledWasteDictionary, DataGridView grid)
        {
            grid.SuspendLayout();
            grid.Columns.Clear();
            grid.Columns.Add("Data", "Data");
            grid.Columns.Add("Zm", "Zm");
            grid.Columns.Add("SMT1", "SMT1");
            grid.Columns.Add("SMT2", "SMT2");
            grid.Columns.Add("SMT3", "SMT3");
            grid.Columns.Add("SMT5", "SMT5");
            grid.Columns.Add("SMT6", "SMT6");
            grid.Columns.Add("SMT7", "SMT7");
            grid.Columns.Add("SMT8", "SMT8");

            foreach (var dateEntry in ledWasteDictionary)
            {
                foreach (var shiftEntry in dateEntry.Value)
                {
                    Dictionary<string, double> ledDroppedPerLine = new Dictionary<string, double>();
                    Dictionary<string, double> ledUsedPerLine = new Dictionary<string, double>();
                    Dictionary<string, string> ledWastePerLine = new Dictionary<string, string>();

                    ledDroppedPerLine.Add("SMT1", 0);
                    ledDroppedPerLine.Add("SMT2", 0);
                    ledDroppedPerLine.Add("SMT3", 0);
                    ledDroppedPerLine.Add("SMT5", 0);
                    ledDroppedPerLine.Add("SMT6", 0);
                    ledDroppedPerLine.Add("SMT7", 0);
                    ledDroppedPerLine.Add("SMT8", 0);

                    ledUsedPerLine.Add("SMT1", 0);
                    ledUsedPerLine.Add("SMT2", 0);
                    ledUsedPerLine.Add("SMT3", 0);
                    ledUsedPerLine.Add("SMT5", 0);
                    ledUsedPerLine.Add("SMT6", 0);
                    ledUsedPerLine.Add("SMT7", 0);
                    ledUsedPerLine.Add("SMT8", 0);

                    ledWastePerLine.Add("SMT1", "");
                    ledWastePerLine.Add("SMT2", "");
                    ledWastePerLine.Add("SMT3", "");
                    ledWastePerLine.Add("SMT5", "");
                    ledWastePerLine.Add("SMT6", "");
                    ledWastePerLine.Add("SMT7", "");
                    ledWastePerLine.Add("SMT8", "");

                    foreach (var lotData in shiftEntry.Value)
                    {
                        var lotWaste = CalculateLotLedWaste(lotData);

                        if (lotWaste.droppedA + lotWaste.droppedB < 0) continue;

                        ledUsedPerLine[lotData.smtLine] += lotWaste.ledExpectedUsageA + lotWaste.ledExpectedUsageB;
                        ledDroppedPerLine[lotData.smtLine] += lotWaste.droppedA + lotWaste.droppedB;
                    }

                    foreach (var lineEntry in ledUsedPerLine)
                    {
                        if (ledUsedPerLine[lineEntry.Key] > 0)
                        {
                            ledWastePerLine[lineEntry.Key] = Math.Round(ledDroppedPerLine[lineEntry.Key] / ledUsedPerLine[lineEntry.Key] * 100, 2).ToString()+"%";
                        }
                        else
                        {
                            ledWastePerLine[lineEntry.Key] = "";
                        }
                    }

                    

                    grid.Rows.Add(dateEntry.Key.ToString("dd-MM-yyyy"), shiftEntry.Key, ledWastePerLine["SMT1"], ledWastePerLine["SMT2"] , ledWastePerLine["SMT3"], ledWastePerLine["SMT5"] , ledWastePerLine["SMT6"] , ledWastePerLine["SMT7"], ledWastePerLine["SMT8"]);
                }
            }
            autoSizeGridColumns(grid);
            grid.FirstDisplayedCell = grid.Rows[grid.Rows.Count - 1].Cells[0];
            grid.ResumeLayout();
        }

        public static SortedDictionary<DateTime, SortedDictionary<int, List<LotLedWasteStruc>>> LedWasteDictionary(SortedDictionary<DateTime, SortedDictionary<int, DataTable>> inputSmtData, Dictionary<string, MesModels> mesModels)
        {
            SortedDictionary<DateTime, SortedDictionary<int, List<LotLedWasteStruc>>> result = new SortedDictionary<DateTime, SortedDictionary<int, List<LotLedWasteStruc>>>();

            foreach (var dateEntry in inputSmtData)
            {
                if (!result.ContainsKey(dateEntry.Key))
                {
                    result.Add(dateEntry.Key, new SortedDictionary<int, List<LotLedWasteStruc>>());
                }
                foreach (var shiftEntry in dateEntry.Value)
                {
                    if (!result[dateEntry.Key].ContainsKey(shiftEntry.Key))
                    {
                        result[dateEntry.Key].Add(shiftEntry.Key, new List<LotLedWasteStruc>());
                    }
                    foreach (DataRow row in shiftEntry.Value.Rows)
                    {
                        //107577:2OPF00050A:0|107658:2OPF00050A:0#107580:2OPF00050A:27|107657:2OPF00050A:23
                        string lot = row["NrZlecenia"].ToString();
                        string model = row["Model"].ToString();
                        int requiredRankA = mesModels[model].LedAQty;
                        int requiredRankB = mesModels[model].LedBQty;
                        string[] ledDropped = row["KoncowkiLED"].ToString().Split('#');
                        int reelsUsed = ledDropped.Length * 2;
                        int ledALeftTotal = 0;
                        int ledBLeftTotal = 0;
                        int ledPerReel = 0;
                        int manufacturedModules = int.Parse(row["IloscWykonana"].ToString());
                        string smtLine = row["LiniaSMT"].ToString();
                        

                        foreach (var item in ledDropped) 
                        {
                            string[] ranks = item.Split('|');
                            string[] rankA = ranks[0].ToString().Split(':');
                            string[] rankB = ranks[1].ToString().Split(':');
                            int leftA = int.Parse(rankA[2]);
                            int leftB = int.Parse(rankB[2]);
                            string ledId = rankA[1];
                            if (ledId.Length>10)
                            {
                                ledPerReel = 3000;
                            }
                            else
                            {
                                ledPerReel = 3500;
                            }
                            ledALeftTotal += leftA;
                            ledBLeftTotal += leftB;
                        }

                        if (ledPerReel * reelsUsed / 2 - requiredRankA * manufacturedModules < ledALeftTotal || ledPerReel * reelsUsed / 2 - requiredRankB * manufacturedModules < ledBLeftTotal) 
                        {
                            continue;
                        }

                        LotLedWasteStruc newItem = new LotLedWasteStruc();
                        newItem.lotId = lot;
                        newItem.requiredRankA = requiredRankA;
                        newItem.requiredRankB = requiredRankB;
                        newItem.ledLeftA = ledALeftTotal;
                        newItem.ledLeftB = ledBLeftTotal;
                        newItem.ledsPerReel = ledPerReel;
                        newItem.manufacturedModules = manufacturedModules;
                        newItem.smtLine = smtLine;
                        newItem.reelsUsed = reelsUsed;
                        newItem.model = model;
                        result[dateEntry.Key][shiftEntry.Key].Add(newItem);
                    }
                }
            }

            return result;
        }


        public static SortedDictionary<DateTime, SortedDictionary<int, DataTable>> sortTableByDayAndShift(DataTable sqlTable,string dateColumnName)
        {
            //DataCzasStart,DataCzasKoniec,LiniaSMT,OperatorSMT,NrZlecenia,Model,IloscWykonana,NGIlosc,ScrapIlosc
            SortedDictionary<DateTime, SortedDictionary<int, DataTable>> summaryDic = new SortedDictionary<DateTime, SortedDictionary<int, DataTable>>();

            foreach (DataRow row in sqlTable.Rows)
            {
                string dateString = row[dateColumnName].ToString();
                if (dateString == "") continue;
                //DateTime endDate = DateTime.ParseExact(dateString, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.Parse(dateString);
                dateShiftNo endDateShiftInfo = whatDayShiftIsit(endDate);

                if (!summaryDic.ContainsKey(endDateShiftInfo.fixedDate.Date))
                {
                    summaryDic.Add(endDateShiftInfo.fixedDate.Date, new SortedDictionary<int, DataTable>());
                }
                if (!summaryDic[endDateShiftInfo.fixedDate.Date].ContainsKey(endDateShiftInfo.shift))
                {
                    summaryDic[endDateShiftInfo.fixedDate.Date].Add(endDateShiftInfo.shift, new DataTable());
                    summaryDic[endDateShiftInfo.fixedDate.Date][endDateShiftInfo.shift] = sqlTable.Clone();
                }
                summaryDic[endDateShiftInfo.fixedDate.Date][endDateShiftInfo.shift].Rows.Add(row.ItemArray);
            }

            return summaryDic;
        }

        public static void shiftSummaryDataSource(SortedDictionary<DateTime, SortedDictionary<int, DataTable>> sourceDic, DataGridView grid)
        {
            DataTable result = new DataTable();
            grid.Rows.Clear();
            grid.Columns.Clear();
            grid.Columns.Add("Mies", "Mies");
            grid.Columns.Add("Tydz", "Tydz");
            grid.Columns.Add("Dzien", "Dzien");
            grid.Columns.Add("Zmiana", "Zmiana");
            grid.Columns.Add("SMT1", "SMT1");
            grid.Columns.Add("SMT2", "SMT2");
            grid.Columns.Add("SMT3", "SMT3");
            grid.Columns.Add("SMT5", "SMT5");
            grid.Columns.Add("SMT6", "SMT6");
            grid.Columns.Add("SMT7", "SMT7");
            grid.Columns.Add("SMT8", "SMT8");

            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            System.Drawing.Color rowColor = System.Drawing.Color.LightBlue;


            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            foreach (var dayEntry in sourceDic)
            {
                if (rowColor == System.Drawing.Color.LightBlue)
                {
                    rowColor = System.Drawing.Color.White;
                }
                else
                {
                    rowColor = System.Drawing.Color.LightBlue;
                }
                var week = dateTools.GetIso8601WeekOfYear(dayEntry.Key);

                foreach (var shiftEntry in dayEntry.Value)
                {
                    Dictionary<string, double> qtyPerLine = new Dictionary<string, double>();
                    Dictionary<string, DataTable> detailPerLine = new Dictionary<string, DataTable>();

                    foreach (DataRow row in shiftEntry.Value.Rows)
                    {
                        string smtLine = row["LiniaSMT"].ToString();
                        if (!qtyPerLine.ContainsKey(smtLine))
                        {
                            qtyPerLine.Add(smtLine, 0);
                            detailPerLine.Add(smtLine, new DataTable());
                            detailPerLine[smtLine] = shiftEntry.Value.Clone();
                        }

                        double qty = double.Parse(row["IloscWykonana"].ToString());
                        qtyPerLine[smtLine] += qty;
                        detailPerLine[smtLine].Rows.Add(row.ItemArray);
                    }

                    double smt1 = 0;
                    double smt2 = 0;
                    double smt3 = 0;
                    double smt5 = 0;
                    double smt6 = 0;
                    double smt7 = 0;
                    double smt8 = 0;

                    foreach (var lineEntry in qtyPerLine)
                    {
                        qtyPerLine.TryGetValue("SMT1", out smt1);
                        qtyPerLine.TryGetValue("SMT2", out smt2);
                        qtyPerLine.TryGetValue("SMT3", out smt3);
                        qtyPerLine.TryGetValue("SMT5", out smt5);
                        qtyPerLine.TryGetValue("SMT6", out smt6);
                        qtyPerLine.TryGetValue("SMT7", out smt7);
                        qtyPerLine.TryGetValue("SMT8", out smt8);
                    }

                    grid.Rows.Add(dateTools.productionMonthNumber(week, dayEntry.Key.Year),week,dayEntry.Key.ToShortDateString(),  shiftEntry.Key.ToString(), smt1, smt2, smt3, smt5, smt6, smt7, smt8);

                    foreach (DataGridViewCell cell in grid.Rows[grid.Rows.Count-1].Cells)
                    {
                        cell.Style.BackColor = rowColor;
                        DataTable dt;
                        if (detailPerLine.TryGetValue(cell.OwningColumn.Name, out dt))
                        {
                            cell.Tag = dt;
                        }
                    }
                }
            }
            autoSizeGridColumns(grid);
            if (grid.Rows.Count > 0)
            {
                grid.FirstDisplayedScrollingRowIndex = grid.RowCount - 1;
            }
        }

        public static void autoSizeGridColumns(DataGridView grid)
        {
            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }

        public static Dictionary<string, Dictionary<string, List<durationQuantity>>> smtQtyPerModelPerLine (DataTable smtRecords, bool showAllModels, Dictionary<string, MesModels> mesModels)
        {
            Dictionary<string, Dictionary<string, List<durationQuantity>>> result = new Dictionary<string, Dictionary<string, List<durationQuantity>>>();
            Dictionary<string, durationQuantity> previousLotPerLine = new Dictionary<string, durationQuantity>();

            foreach (DataRow row in smtRecords.Rows)
            {
                string model = row["Model"].ToString();
                if (!mesModels.ContainsKey(model)) continue;

                string modelShort = model.Substring(0, 6) + "X0X" + model.Substring(9, 1);
                string line = row["LiniaSMT"].ToString();
                double qty = 0;
                if (!double.TryParse(row["IloscWykonana"].ToString(), out qty)) continue;

                //DataCzasStart,DataCzasKoniec
                DateTime dateStart = new DateTime();
                DateTime dateEnd = new DateTime();
                if (!DateTime.TryParse(row["DataCzasStart"].ToString(), out dateStart) || !DateTime.TryParse(row["DataCzasKoniec"].ToString(), out dateEnd)) continue;
                var lotDurationStartToEnd = (dateEnd - dateStart).TotalHours;
                durationQuantity previousLot;
                if (!previousLotPerLine.TryGetValue(line, out previousLot))
                {
                    previousLot.end = new DateTime(2017, 01, 01);
                }
                var lotDurationEndToEnd = (dateEnd - previousLot.end).TotalHours;

                int pcbOncarrier = mesModels[model].PcbsOnCarrier;
                double carriersPerLot = qty / (double)pcbOncarrier;

                double secondsPerCarrierStartToEnd = lotDurationStartToEnd * 3600 / carriersPerLot;
                double secondsPerCarrierEndToEnd = lotDurationEndToEnd * 3600 / carriersPerLot;
                
                if (secondsPerCarrierEndToEnd < 30 || secondsPerCarrierStartToEnd < 30) continue;

                //if (lotDurationStartToEnd < 0.25) continue;
                //if (lotDurationEndToEnd < 0.25) continue;

                //Debug.WriteLine(lotDuration);
                if (!result.ContainsKey(model) & showAllModels)
                {
                    result.Add(model, new Dictionary<string, List<durationQuantity>>());
                }
                if (!result.ContainsKey(modelShort))
                {
                    result.Add(modelShort, new Dictionary<string, List<durationQuantity>>());
                }
                if (showAllModels)
                {
                    if (!result[model].ContainsKey(line))
                    {
                        result[model].Add(line, new List<durationQuantity>());
                    }
                }
                if (!result[modelShort].ContainsKey(line))
                {
                    result[modelShort].Add(line, new List<durationQuantity>());
                }

                durationQuantity newItem = new durationQuantity();
                newItem.durationStartToEnd = lotDurationStartToEnd;
                newItem.duractionEndToEnd = lotDurationEndToEnd;
                newItem.quantity = qty;
                newItem.start = dateStart;
                newItem.end = dateEnd;
                newItem.lot = row["NrZlecenia"].ToString();
                if (!previousLotPerLine.ContainsKey(line))
                {
                    previousLotPerLine.Add(line, new durationQuantity());
                }
                previousLotPerLine[line] = newItem;

                if (showAllModels)
                {
                    result[model][line].Add(newItem);
                }
                result[modelShort][line].Add(newItem);
            }



            return result;
        }

        public struct durationQuantity
        {
            public double durationStartToEnd;
            public double duractionEndToEnd;
            public double quantity;
            public string lot;
            public DateTime start;
            public DateTime end;
        }

        public static DataTable MakeTableForModelEfficiency(Dictionary<string, Dictionary<string, List<durationQuantity>>> inputData,string model, bool perShift)
        {
            DataTable result = new DataTable();
            result.Columns.Add("Linia");
            result.Columns.Add("Ilość całkowita");
            result.Columns.Add("Średnia/h");
            result.Columns.Add("Min/h");
            result.Columns.Add("Max/h");
            double frequency = 1;
            if(!perShift)
            {
                frequency = 8;
            }
            

            foreach (var modelEntry in inputData)
            {
                if (modelEntry.Key != model) continue;
                foreach (var lineEntry in modelEntry.Value)
                {
                    List<double> checkList = new List<double>();
                    foreach (var lot in lineEntry.Value)
                    {
                        checkList.Add(lot.quantity / lot.durationStartToEnd * frequency);
                        Debug.WriteLine(lot.quantity + "szt. " + lot.start.ToShortTimeString() + "-" + lot.end.ToShortTimeString() + " " + lot.durationStartToEnd * 60 + "min. " + 8*lot.quantity / lot.durationStartToEnd + "szt./zm ");
                    }
                    

                    checkList.Sort();
                    double totalQty = lineEntry.Value.Select(q => q.quantity).Sum() * frequency;
                    double min = Math.Round(lineEntry.Value.Select(q => q.quantity / q.durationStartToEnd).Min(),0) * frequency;
                    double max = Math.Round(lineEntry.Value.Select(q => q.quantity / q.durationStartToEnd).Max(), 0) * frequency;
                    double avg = Math.Round(lineEntry.Value.Select(q => q.quantity / q.durationStartToEnd).Average(), 0) * frequency;
                    double median = Math.Round(checkList[checkList.Count / 2], 0) ;
                    result.Rows.Add(lineEntry.Key, totalQty, median ,min, max);
                }
            }
            return result;
        }

        public static DataTable MakeTableForModelEfficiency2(Dictionary<string, Dictionary<string, List<durationQuantity>>> inputData, string model, bool perShift)
        {
            DataTable result = new DataTable();
            result.Columns.Add("Linia");
            result.Columns.Add("Ilość całkowita");
            result.Columns.Add("Średnia/h");
            result.Columns.Add("Min/h");
            result.Columns.Add("Max/h");
            double frequency = 1;
            if (!perShift)
            {
                frequency = 8;
            }


            foreach (var modelEntry in inputData)
            {
                if (modelEntry.Key != model) continue;
                foreach (var lineEntry in modelEntry.Value)
                {
                    List<double> checkList = new List<double>();
                    foreach (var lot in lineEntry.Value)
                    {
                        if (lot.duractionEndToEnd > 2 || lot.duractionEndToEnd< 0.2) continue;
                        checkList.Add(lot.quantity / lot.duractionEndToEnd * frequency);
                        Debug.WriteLine(lot.quantity + "szt. " + lot.duractionEndToEnd * 60 + "min. " );
                    }

                    if (checkList.Count > 0)
                    {
                        checkList.Sort();
                        double totalQty = lineEntry.Value.Select(q => q.quantity).Sum() * frequency;
                        double min = Math.Round(lineEntry.Value.Select(q => q.quantity / q.duractionEndToEnd).Min(), 0) * frequency;
                        double max = Math.Round(lineEntry.Value.Select(q => q.quantity / q.duractionEndToEnd).Max(), 0) * frequency;
                        double avg = Math.Round(lineEntry.Value.Select(q => q.quantity / q.duractionEndToEnd).Average(), 0) * frequency;
                        double median = Math.Round(checkList[checkList.Count / 2], 0);
                        result.Rows.Add(lineEntry.Key, totalQty, median, min, max);
                    }
                }
            }
            return result;
        }

        public class stencilStruct
        {
            public Int32 cycleCOunt { get; set; }
            public DateTime dateStart { get; set; }
            public DateTime dateEnd { get; set; }
        }

        public static void FillOutGridTraceability(string[] ledIds, DataGridView grid)
        {
            DataTable sqlSmtTable = SQLoperations.GetSmtLedTraceability();
            DataTable sourceTable = new DataTable();

            sourceTable.Columns.Add("Dioda");
            sourceTable.Columns.Add("LOT");
            sourceTable.Columns.Add("SMT");
            sourceTable.Columns.Add("Box");
            sourceTable.Columns.Add("Końc.");
            List<string[]> rowsList = new List<string[]>();
            List<string[]> notFoundList = new List<string[]>();

            foreach (DataRow row in sqlSmtTable.Rows)
            {
                //DataCzasKoniec,LiniaSMT,NrZlecenia,Model
                //  94MWS59R80JZ3E-137099    :83|137100:94MWS59R80JZ3E:81#137105:94MWS59R80JZ3E:82|137106:94MWS59R80JZ3E:75

                string lot = row["NrZlecenia"].ToString();
                string smtLine = row["LiniaSMT"].ToString();
                string model = row["Model"].ToString();
                string date = row["DataCzasKoniec"].ToString();
                string ledLeftovers = row["KoncowkiLED"].ToString();
                string[] splitByheads = ledLeftovers.Split('#');
                List<string> ledIdList = new List<string>();
                List<string> lotsList = new List<string>();
                //94MWS59R80JZ3E-139693
                

                foreach (var head in splitByheads)
                {
                    string[] leds = head.Split('|');
                    foreach (var led in leds)
                    {
                        var props = led.Split(':');
                        var pair = props[1] + "-" + props[0];
                        var left = props[2];
                        if (!ledIds.Contains(pair))
                        {
                            break;
                        }
                        rowsList.Add(new string[] { pair, lot, model, smtLine, left });
                    }
                }
            }
            foreach (var id in ledIds)
            {
                if (id.Trim() == "") continue;
                bool idFound = false;
                foreach (var item in rowsList)
                {
                    if (id == item[0])
                    {
                        sourceTable.Rows.Add(item);
                        idFound = true;
                    }
                }
                if (!idFound)
                {
                    notFoundList.Add(id.Split('-'));
                    sourceTable.Rows.Add(id, "Nieznane", "Brak","Danych","SMT");
                }
            }

            var notFoundLots = SQLoperations.GetZlecenieString(notFoundList);

            foreach (DataRow row in sourceTable.Rows)
            {
                if (row["LOT"].ToString()=="Nieznane")
                {
                    string nc12Id = row["Dioda"].ToString();
                    foreach (var item in notFoundLots)
                    {
                        if (nc12Id == item[0]+"-"+item[1])
                        {
                            row["LOT"] = item[2];
                        }
                    }
                }
            }
                
                //foreach (DataRow gridRow in sourceTable.Rows)
                //{
                //    string lotNo = gridRow["LOT"].ToString();
                //    if (lotsList.Contains(lotNo) || lotNo == "") continue;
                //    List<string> serials = SQLoperations.lotToPcbSerialNo(lotNo);
                //    gridRow["Box"] = string.Join("|", SQLoperations.GetBoxPalletIdFromPcb(serials).ToArray());
                //    lotsList.Add(lotNo);

                //}

                grid.DataSource = sourceTable;

            
        }

        public static void FillOutStencilTable( DataGridView grid, Dictionary<string, MesModels> mesModels)
        {
            DataTable smtRecords = SQLoperations.GetStencilSmtRecords();
            Dictionary<string, stencilStruct> stencilDict = new Dictionary<string, stencilStruct>();

            foreach (DataRow row in smtRecords.Rows)
            {
                string stencilID = row["StencilQR"].ToString();
                string model = row["Model"].ToString();
                DateTime date = DateTime.Parse(row["DataCzasKoniec"].ToString());

                MesModels modelInfo;
                if (!mesModels.TryGetValue(model, out modelInfo)) continue;
                if (stencilID.Trim() == "") continue;
                int qty = 0;
                int.TryParse(row["IloscWykonana"].ToString(), out qty);

                if (qty > 0) 
                {
                    if (!stencilDict.ContainsKey(stencilID))
                    {
                        stencilDict.Add(stencilID, new stencilStruct());
                        stencilDict[stencilID].dateEnd = new DateTime(1700, 01, 01);
                        stencilDict[stencilID].dateStart = DateTime.Now;
                    }
                    stencilDict[stencilID].cycleCOunt += qty/modelInfo.PcbsOnCarrier;

                    if (date < stencilDict[stencilID].dateStart)
                    {
                        stencilDict[stencilID].dateStart = date;
                    }

                    if (date > stencilDict[stencilID].dateEnd)
                    {
                        stencilDict[stencilID].dateEnd = date;
                    }
                }
            }

            grid.Columns.Clear();
            grid.Columns.Add("StencilID", "StencilID");
            grid.Columns.Add("Ilosc", "Ilość cykli");
            grid.Columns.Add("Okres", "Okres dni");
            grid.Columns.Add("Avg", "Średnio na dzień");

            foreach (var stencilEntry in stencilDict)
            {
                grid.Rows.Add(stencilEntry.Key, stencilEntry.Value.cycleCOunt, Math.Round((DateTime.Now- stencilEntry.Value.dateStart).TotalDays,0,MidpointRounding.AwayFromZero),Math.Round( stencilEntry.Value.cycleCOunt/ Math.Round((DateTime.Now - stencilEntry.Value.dateStart).TotalDays, 0, MidpointRounding.AwayFromZero),1));
            }

            dgvTools.ColumnsAutoSize(grid, DataGridViewAutoSizeColumnMode.AllCells);
        }

        
    }
}
