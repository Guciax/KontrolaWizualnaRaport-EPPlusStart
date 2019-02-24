using KontrolaWizualnaRaport.CentalDataStorage;
using MST.MES;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport.TabOperations.SMT_tabs
{
    public class LedWasteTabOperations
    {
        public static SortedDictionary<DateTime, SortedDictionary<int, List<LotLedWasteStruc>>> ledWasteDictionary = new SortedDictionary<DateTime, SortedDictionary<int, List<LotLedWasteStruc>>>();

        public class LotLedWasteStruc
        {
            public string lotId;
            public string smtLine;
            public string model;
            public int manufacturedModules;
            public int ledsUsed;
            public int ledsUsageFromBom;
            public float ledWaste
            {
                get
                {
                    return (float)Math.Round(((double)ledsUsed - (double)ledsUsageFromBom) / (double)ledsUsageFromBom * 100, 2);
                }
            }
        }

        public static void RefreshOrReloadWasteData()
        {
            if (ledWasteDictionary.Count > 0)
            {
                ReloadLedWasteTab();
                SharedComponents.Smt.LedWasteTab.comboBoxSmtLedWasteModels.Items.Add("Wszystkie");
                SharedComponents.Smt.LedWasteTab.comboBoxSmtLedWasteModels.Items.AddRange(DataContainer.sqlDataByProcess.Smt.Select(o => o.Value.modelId).Distinct().ToArray());
                SharedComponents.Smt.LedWasteTab.comboBoxSmtLedWasteLine.Text = "Wszystkie";
                SharedComponents.Smt.LedWasteTab.comboBoxSmtLedWasteLine.Items.AddRange(GlobalParameters.smtLines);
                SharedComponents.Smt.LedWasteTab.comboBoxSmtLedWasteLine.Items.Insert(0, "Wszystkie");
                SharedComponents.Smt.LedWasteTab.comboBoxSmtLedWasteLine.Text = "Wszystkie";
            }
            else
            {
                PrepareWasteData();
            }
        }

        private static void PrepareWasteData()
        {
            ledWasteDictionary = CreateLedWasteDictionary();
            ReloadLedWasteTab();
        }

        private static void ReloadLedWasteTab()
        {
            FillOutDailyLedWaste( SharedComponents.Smt.LedWasteTab.dataGridViewSmtLedDropped);
            FillOutLedWasteByModel( SharedComponents.Smt.LedWasteTab.dataGridViewSmtLedWasteByModel, SharedComponents.Smt.LedWasteTab.comboBoxSmtLedWasteLine.Text);
            FillOutLedWasteTotalWeekly( SharedComponents.Smt.LedWasteTab.dataGridViewSmtWasteTotal);

            Charting.DrawLedWasteChart();

            FillOutLedWasteTotalByLine( SharedComponents.Smt.LedWasteTab.dataGridViewSmtLedWasteTotalPerLine, SharedComponents.Smt.LedWasteTab.comboBoxSmtLedWasteModels.Text);
        }

        public static void FillOutLedWasteTotalByLine( DataGridView grid, string model)
        {
            
            grid.SuspendLayout();
            grid.Rows.Clear();

            Dictionary<string, List<LotLedWasteStruc>> ledsPerLine = ledWasteDictionary
                                                                    .SelectMany(s => s.Value)
                                                                    .SelectMany(o => o.Value)
                                                                    .GroupBy(r => r.smtLine)
                                                                    .ToDictionary(x => x.Key, v => v.ToList());

            Dictionary<string, DataTable> tagTable = new Dictionary<string, DataTable>();

            DataTable template = new DataTable();
            template.Columns.Add("LOT");
            template.Columns.Add("Model");
            template.Columns.Add("Data");
            template.Columns.Add("Linia");
            template.Columns.Add("Mont");
            template.Columns.Add("Odp%");

            grid.Columns.Clear();
            grid.Columns.Add("Poz", "");
            foreach (var item in GlobalParameters.allLinesByHand)
            {
                grid.Columns.Add(item, item);
                if (!ledsPerLine.ContainsKey(item))
                {
                    ledsPerLine.Add(item, new List<LotLedWasteStruc>());
                }
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

                        var waste = Math.Round((double)(lot.ledsUsed - lot.ledsUsageFromBom) / (double)lot.ledsUsageFromBom, 2);
                        tagTable[lot.smtLine].Rows.Add(lot.lotId, lot.model, dateEntry.Key.ToString("dd-MM-yyyy"), lot.smtLine, lot.ledsUsed, waste);
                    }
                }
            }



            grid.Rows.Add("Montaż LED");
            grid.Rows.Add("Odpad LED");

            foreach (var line in ledsPerLine)
            {
                double totalLedUsed = line.Value.Select(o => o.ledsUsed).Sum();
                double totalLedBom = line.Value.Select(o => o.ledsUsageFromBom).Sum();
                double waste = Math.Round((totalLedUsed - totalLedBom) / totalLedBom * 100, 2);

                grid.Rows[0].Cells[line.Key].Value = totalLedUsed;
                grid.Rows[1].Cells[line.Key].Value = waste + "%";
            }

            //autoSizeGridColumns(grid);
            grid.ResumeLayout();
        }

        public static void FillOutLedWasteByModel(DataGridView grid, string line)
        {
            grid.SuspendLayout();
            grid.Columns.Clear();
            grid.Columns.Add("Model", "Model");
            grid.Columns.Add("Mont_LED", "Mont.LED");
            grid.Columns.Add("Odp", "Odpad");

            DataTable template = new DataTable();
            template.Columns.Add("LOT");
            template.Columns.Add("Model");
            template.Columns.Add("Linia");
            template.Columns.Add("Mont_Led");
            template.Columns.Add("Odp_Led%");




            var grouppedByModel = ledWasteDictionary.SelectMany(o => o.Value).SelectMany(o => o.Value).Where(o => o.smtLine == line || o.smtLine == "Wszystkie").GroupBy(o => o.model).ToDictionary(k => k.Key, v => v.ToList());


            foreach (var modelEntry in grouppedByModel)
            {
                double mtdLed = modelEntry.Value.Select(o => o.ledsUsed).Sum();
                double bomLed = modelEntry.Value.Select(o => o.ledsUsageFromBom).Sum();
                float ledWaste = (float)Math.Round((mtdLed - bomLed) / bomLed * 100, 2);
                grid.Rows.Add(modelEntry.Key, mtdLed, ledWaste + "%");

                DataTable detailsTag = template.Clone();
                foreach (var order in modelEntry.Value)
                {
                    detailsTag.Rows.Add(order.lotId, order.model, order.smtLine, order.ledsUsed, order.ledWaste + "%");
                }

                foreach (DataGridViewCell cell in grid.Rows[grid.Rows.Count - 1].Cells)
                {
                    cell.Tag = detailsTag;
                }
            }

            dgvTools.ColumnsAutoSize (grid, DataGridViewAutoSizeColumnMode.AllCells);
            grid.ResumeLayout();
        }

        private static void FillOutLedWasteTotalWeekly( DataGridView grid)
        {
            grid.SuspendLayout();
            grid.Columns.Clear();
            grid.Columns.Add("Tydz", "Tydz");
            grid.Columns.Add("MontLED", "MontLED");
            grid.Columns.Add("%", "%");

            Dictionary<string, double> ledMounted = new Dictionary<string, double>();
            Dictionary<string, double> ledByBom = new Dictionary<string, double>();
            Dictionary<string, double> ledWaste = new Dictionary<string, double>();
            double monthMounted = 0;
            double monthwaste = 0;
            double monthByBom = 0;

            string monthName = "";

            foreach (var dateEntry in ledWasteDictionary)
            {
                if (dateEntry.Key.ToString("MMM", CultureInfo.InvariantCulture) != monthName & monthName != "")
                {
                    ledMounted.Add(monthName, monthMounted);
                    ledWaste.Add(monthName, monthwaste);
                    ledByBom.Add(monthName, monthwaste);
                    monthMounted = 0;
                    monthwaste = 0;
                    monthByBom = 0;
                }
                string week = dateTools.WeekNumber(dateEntry.Key).ToString();
                monthName = dateEntry.Key.ToString("MMM", CultureInfo.InvariantCulture);

                if (!ledMounted.ContainsKey(week))
                {
                    ledMounted.Add(week, 0);
                    ledWaste.Add(week, 0);
                    ledByBom.Add(week, 0);
                }

                foreach (var shiftEntry in dateEntry.Value)
                {
                    foreach (var lotData in shiftEntry.Value)
                    {

                        ledMounted[week] += lotData.ledsUsed;
                        ledByBom[week] += lotData.ledsUsageFromBom; ;
                        ledWaste[week] = Math.Round((ledMounted[week] - ledByBom[week]) / ledMounted[week] * 100, 2);
                        monthMounted += lotData.ledsUsed;
                        monthByBom += lotData.ledsUsageFromBom;
                        //monthwaste = Math.Round(ledDropped[week] / ledMounted[week] * 100, 2);
                        monthwaste = Math.Round((monthMounted - monthByBom) / monthMounted * 100, 2);
                    }
                }

            }



            foreach (var weekEntry in ledMounted)
            {
                grid.Rows.Add(weekEntry.Key, ledMounted[weekEntry.Key], ledWaste[weekEntry.Key]);
            }
            dgvTools.ColumnsAutoSize(grid, DataGridViewAutoSizeColumnMode.AllCells);
            grid.ResumeLayout();
        }

        private static void FillOutDailyLedWaste( DataGridView grid)
        {
            grid.SuspendLayout();
            grid.Columns.Clear();
            grid.Columns.Add("Data", "Data");
            grid.Columns.Add("Zm", "Zm");
            grid.Columns.Add("SMT2", "SMT2");
            grid.Columns.Add("SMT3", "SMT3");
            grid.Columns.Add("SMT4", "SMT4");
            grid.Columns.Add("SMT5", "SMT5");
            grid.Columns.Add("SMT6", "SMT6");
            grid.Columns.Add("SMT7", "SMT7");
            grid.Columns.Add("SMT8", "SMT8");

            DataTable tagTableTemplate = new DataTable();
            tagTableTemplate.Columns.Add("Zlecenie");
            tagTableTemplate.Columns.Add("Model");
            tagTableTemplate.Columns.Add("Ilość");
            tagTableTemplate.Columns.Add("Linia");
            tagTableTemplate.Columns.Add("Zużycie BOM");
            tagTableTemplate.Columns.Add("Rzecz. zużycie");
            tagTableTemplate.Columns.Add("Odpad szt.");
            tagTableTemplate.Columns.Add("Odpad %");

            foreach (var dateEntry in ledWasteDictionary)
            {
                foreach (var shiftEntry in dateEntry.Value)
                {

                    var grouppedByLine = shiftEntry.Value.GroupBy(o => o.smtLine).ToDictionary(l => l.Key, l => l.ToList());
                    Dictionary<string, float> wasteByLine = new Dictionary<string, float>();
                    foreach (var line in GlobalParameters.allLinesByHand)
                    {
                        if (!grouppedByLine.ContainsKey(line)) grouppedByLine.Add(line, new List<LotLedWasteStruc>());
                        if (!wasteByLine.ContainsKey(line)) wasteByLine.Add(line, 0);
                    }
                    Dictionary<string, DataTable> tagPerLine = new Dictionary<string, DataTable>();
                    tagPerLine.Add("SMT2", tagTableTemplate.Clone());
                    tagPerLine.Add("SMT3", tagTableTemplate.Clone());
                    tagPerLine.Add("SMT4", tagTableTemplate.Clone());
                    tagPerLine.Add("SMT5", tagTableTemplate.Clone());
                    tagPerLine.Add("SMT6", tagTableTemplate.Clone());
                    tagPerLine.Add("SMT7", tagTableTemplate.Clone());
                    tagPerLine.Add("SMT8", tagTableTemplate.Clone());
                    foreach (var smtLine in grouppedByLine)
                    {
                        int ledsUsed = 0;
                        int ledsByBom = 0;
                        foreach (var smtRecord in smtLine.Value)
                        {
                            ledsUsed += smtRecord.ledsUsed;
                            ledsByBom += smtRecord.ledsUsageFromBom;
                            tagPerLine[smtLine.Key].Rows.Add(smtRecord.lotId, smtRecord.model, smtRecord.manufacturedModules, smtRecord.smtLine, smtRecord.ledsUsageFromBom, smtRecord.ledsUsed, smtRecord.ledsUsed - smtRecord.ledsUsageFromBom, smtRecord.ledWaste + "%");
                        }

                        if (ledsByBom > 0)
                        {
                            wasteByLine[smtLine.Key] = (float)Math.Round(((double)ledsUsed - (double)ledsByBom) / (double)ledsByBom * 100, 2);
                        }
                    }

                    grid.Rows.Add(dateEntry.Key.ToString("dd-MM-yyyy"), shiftEntry.Key, wasteByLine["SMT2"], wasteByLine["SMT3"], wasteByLine["SMT4"], wasteByLine["SMT5"], wasteByLine["SMT6"], wasteByLine["SMT7"], wasteByLine["SMT8"]);
                    foreach (var smtLineEntry in tagPerLine)
                    {
                        grid.Rows[grid.Rows.Count - 1].Cells[smtLineEntry.Key].Tag = smtLineEntry.Value;
                    }
                }
            }
            dgvTools.ColumnsAutoSize(grid, DataGridViewAutoSizeColumnMode.AllCells);
            if (grid.Rows.Count > 0)
                grid.FirstDisplayedCell = grid.Rows[grid.Rows.Count - 1].Cells[0];
            grid.ResumeLayout();
        }

        private static SortedDictionary<DateTime, SortedDictionary<int, List<LotLedWasteStruc>>> CreateLedWasteDictionary( )
        {
            Dictionary<string, ModelInfo.ModelSpecification> mesModels = DataContainer.mesModels;
            SortedDictionary<DateTime, SortedDictionary<int, List<MST.MES.OrderStructureByOrderNo.SmtRecords>>> inputSmtData = DataContainer.Smt.sortedTableByDayAndShift;
            SortedDictionary<DateTime, SortedDictionary<int, List<LotLedWasteStruc>>> result = new SortedDictionary<DateTime, SortedDictionary<int, List<LotLedWasteStruc>>>();
            List<string> eachOrderOnlyOnce = new List<string>();

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
                    foreach (var smtRecord in shiftEntry.Value)
                    {
                        if (eachOrderOnlyOnce.Contains(smtRecord.orderInfo.orderNo)) continue;
                        eachOrderOnlyOnce.Add(smtRecord.orderInfo.orderNo);
                        //107577:2OPF00050A:0|107658:2OPF00050A:0#107580:2OPF00050A:27|107657:2OPF00050A:23
                        if (!SharedComponents.Smt.cbSmtLg.Checked)
                        {
                            if (smtRecord.orderInfo.clientGroup == "LG") continue;
                        }
                        if (!SharedComponents.Smt.cbSmtMst.Checked)
                        {
                            if (smtRecord.orderInfo.clientGroup == "MST") continue;
                        }
                        if (smtRecord.ledsUsed == 0) continue;

                        string lot = smtRecord.orderInfo.orderNo;
                        string model = smtRecord.orderInfo.modelId;
                        string clientGroup = smtRecord.orderInfo.clientGroup;
                        int manufacturedModules = smtRecord.orderInfo.totalManufacturedQty;
                        string smtLine = smtRecord.smtLine;

                        LotLedWasteStruc newItem = new LotLedWasteStruc();
                        newItem.lotId = lot;
                        newItem.ledsUsageFromBom = mesModels[smtRecord.orderInfo.modelId].ledCountPerModel * smtRecord.manufacturedQty;
                        newItem.ledsUsed = smtRecord.ledsUsed;
                        newItem.manufacturedModules = manufacturedModules;
                        newItem.smtLine = smtLine;
                        newItem.model = model;
                        var bomPerUsed = (double)newItem.ledsUsageFromBom / (double)newItem.ledsUsed;
                        if (bomPerUsed < 0.8 || bomPerUsed > 1.2) continue;
                        result[dateEntry.Key][shiftEntry.Key].Add(newItem);

                    }
                }
            }

            return result;
        }

    }
}
