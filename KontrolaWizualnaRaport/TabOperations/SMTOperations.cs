using KontrolaWizualnaRaport.CentalDataStorage;
using MST.MES;
using System;
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

        public static SortedDictionary<DateTime, SortedDictionary<int, List<LotLedWasteStruc>>> ledWasteDictionary = new SortedDictionary<DateTime, SortedDictionary<int, List<LotLedWasteStruc>>>();
        public static SortedDictionary<DateTime, SortedDictionary<int, List<MST.MES.OrderStructureByOrderNo.SmtRecords>>> sortedTableByDayAndShift = new SortedDictionary<DateTime, SortedDictionary<int, List<OrderStructureByOrderNo.SmtRecords>>>();

        private static void ReloadProductionReportsTab()
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            sortedTableByDayAndShift = SMTOperations.sortListByDayAndShift();
            Debug.WriteLine($"SMT sortedByDayAndShift done in {st.ElapsedMilliseconds}");
            st.Reset();
            SMTOperations.FillOutProductionReportGrid(sortedTableByDayAndShift, SharedComponents.Smt.productionReportTab.dataGridViewSmtProduction);
            Debug.WriteLine($"SMT wykonGrid filled in {st.ElapsedMilliseconds}");
            st.Stop();
        }

        private static void ReloadLedWasteTab()
        {
            ledWasteDictionary = CreateLedWasteDictionary(sortedTableByDayAndShift, DataContainer.mesModels);
            SMTOperations.FillOutDailyLedWaste(ledWasteDictionary, SharedComponents.Smt.LedWasteTab.dataGridViewSmtLedDropped);
            SMTOperations.FillOutLedWasteByModel(ledWasteDictionary, SharedComponents.Smt.LedWasteTab.dataGridViewSmtLedWasteByModel, SharedComponents.Smt.LedWasteTab.comboBoxSmtLedWasteLine.Text);
            SMTOperations.FillOutLedWasteTotalWeekly(ledWasteDictionary, SharedComponents.Smt.LedWasteTab.dataGridViewSmtWasteTotal);

            Charting.DrawLedWasteChart(ledWasteDictionary);

            SharedComponents.Smt.LedWasteTab.comboBoxSmtLedWasteModels.Items.Add("Wszystkie");
            SharedComponents.Smt.LedWasteTab.comboBoxSmtLedWasteModels.Items.AddRange(DataContainer.sqlDataByProcess.Smt.Select(o => o.Value.modelId).Distinct().ToArray());
            SharedComponents.Smt.LedWasteTab.comboBoxSmtLedWasteLine.Text = "Wszystkie";
            SharedComponents.Smt.LedWasteTab.comboBoxSmtLedWasteLine.Items.AddRange(GlobalParameters.smtLines);
            SharedComponents.Smt.LedWasteTab.comboBoxSmtLedWasteLine.Items.Insert(0, "Wszystkie");
            SharedComponents.Smt.LedWasteTab.comboBoxSmtLedWasteLine.Text = "Wszystkie";
            SMTOperations.FillOutLedWasteTotalByLine(ledWasteDictionary, SharedComponents.Smt.LedWasteTab.dataGridViewSmtLedWasteTotalPerLine, SharedComponents.Smt.LedWasteTab.comboBoxSmtLedWasteModels.Text);
        }

        public static void ReLoadSmtTab()
        {
            ReloadProductionReportsTab();
            ReloadLedWasteTab();

            //smtModelLineQuantity = SMTOperations.smtQtyPerModelPerLine(smtRecords, radioButtonSmtShowAllModels.Checked, mesModels);
            SharedComponents.Smt.ModelAnalysis.comboBoxSmtModels.Items.AddRange(GlobalParameters.allLinesByHand);
            //ChangeOverTools.BuildSmtChangeOverGrid(ChangeOverTools.BuildDateShiftLineDictionary(smtRecords), dataGridViewChangeOvers);
            FillOutStencilTable(SharedComponents.Smt.StencilsTab.dataGridViewSmtStencilUsage, DataContainer.mesModels);
        }

        public static void FillOutLedWasteTotalByLine(SortedDictionary<DateTime, SortedDictionary<int, List<LotLedWasteStruc>>> ledWasteDictionary, DataGridView grid, string model)
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
                grid.Rows[1].Cells[line.Key].Value = waste+"%";
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


        public static void FillOutLedWasteByModel(SortedDictionary<DateTime, SortedDictionary<int, List<LotLedWasteStruc>>> ledWasteDictionary, DataGridView grid, string line)
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

                foreach (DataGridViewCell cell in grid.Rows[grid.Rows.Count-1].Cells)
                {
                    cell.Tag = detailsTag;
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
                if (dateEntry.Key.ToString("MMM", CultureInfo.InvariantCulture)!=monthName & monthName!="")
                {
                    ledMounted.Add(monthName, monthMounted);
                    ledWaste.Add(monthName, monthwaste);
                    ledByBom.Add(monthName, monthwaste);
                    monthMounted = 0;
                    monthwaste = 0;
                    monthByBom = 0;
                }
                string week = dateTools.GetIso8601WeekOfYear(dateEntry.Key).ToString();
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
                        ledWaste[week] = Math.Round((ledMounted[week]- ledByBom[week]) / ledMounted[week] * 100, 2);
                        monthMounted += lotData.ledsUsed;
                        monthByBom += lotData.ledsUsageFromBom;
                        //monthwaste = Math.Round(ledDropped[week] / ledMounted[week] * 100, 2);
                        monthwaste = Math.Round((monthMounted - monthByBom) / monthMounted * 100, 2);
                    }
                }

            }

            

            foreach (var weekEntry in ledMounted)
            {
                grid.Rows.Add(weekEntry.Key, ledMounted[weekEntry.Key],  ledWaste[weekEntry.Key]);
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


                    var grouppedByLine = shiftEntry.Value.GroupBy(o => o.smtLine).ToDictionary(l => l.Key, l => l.ToList());
                    Dictionary<string, float> wasteByLine = new Dictionary<string, float>();
                    foreach (var line in GlobalParameters.allLinesByHand)
                    {
                        if (!grouppedByLine.ContainsKey(line)) grouppedByLine.Add(line, new List<LotLedWasteStruc>());
                        if (!wasteByLine.ContainsKey(line)) wasteByLine.Add(line, 0);
                    }

                    foreach (var smtLine in grouppedByLine)
                    {
                        int ledsUsed = 0;
                        int ledsByBom = 0;
                        foreach (var smtRecord in smtLine.Value)
                        {
                            ledsUsed += smtRecord.ledsUsed;
                            ledsByBom += smtRecord.ledsUsageFromBom;
                        }
                        wasteByLine[smtLine.Key] = (float)Math.Round(((double)ledsUsed - (double)ledsByBom) / (double)ledsByBom * 100, 2);
                    }

                    

                    grid.Rows.Add(dateEntry.Key.ToString("dd-MM-yyyy"), shiftEntry.Key, wasteByLine["SMT2"] , wasteByLine["SMT3"], wasteByLine["SMT5"] , wasteByLine["SMT6"] , wasteByLine["SMT7"], wasteByLine["SMT8"]);
                }
            }
            autoSizeGridColumns(grid);
            if (grid.Rows.Count>0)
            grid.FirstDisplayedCell = grid.Rows[grid.Rows.Count - 1].Cells[0];
            grid.ResumeLayout();
        }

        public static SortedDictionary<DateTime, SortedDictionary<int, List<LotLedWasteStruc>>> CreateLedWasteDictionary(SortedDictionary<DateTime, SortedDictionary<int, List<MST.MES.OrderStructureByOrderNo.SmtRecords>>> inputSmtData, Dictionary<string, ModelInfo.ModelSpecification> mesModels)
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
                    foreach (var smtRecord in shiftEntry.Value)
                    {
                        //107577:2OPF00050A:0|107658:2OPF00050A:0#107580:2OPF00050A:27|107657:2OPF00050A:23
                        if (!SharedComponents.Smt.cbSmtLg.Checked)
                        {
                            if (smtRecord.orderInfo.clientGroup == "LGI") continue;
                        }
                        if (!SharedComponents.Smt.cbSmtMst.Checked)
                        {
                            if (smtRecord.orderInfo.clientGroup == "MST") continue;
                        }

                        string lot = smtRecord.orderInfo.orderNo;
                        string model = smtRecord.orderInfo.modelId;
                        string clientGroup = smtRecord.orderInfo.clientGroup;
                        int manufacturedModules = smtRecord.manufacturedQty;
                        string smtLine = smtRecord.smtLine;



                        LotLedWasteStruc newItem = new LotLedWasteStruc();
                        newItem.lotId = lot;
                        newItem.ledsUsageFromBom = mesModels[smtRecord.orderInfo.modelId].ledCountPerModel * smtRecord.manufacturedQty;
                        newItem.ledsUsed = smtRecord.ledsUsed;
                        newItem.manufacturedModules = manufacturedModules;
                        newItem.smtLine = smtLine;
                        newItem.model = model;
                        result[dateEntry.Key][shiftEntry.Key].Add(newItem);
                    }
                }
            }

            return result;
        }


        public static SortedDictionary<DateTime, SortedDictionary<int, List<MST.MES.OrderStructureByOrderNo.SmtRecords>>> sortListByDayAndShift()
        {
            SortedDictionary<DateTime, SortedDictionary<int, List<MST.MES.OrderStructureByOrderNo.SmtRecords>>> result = new SortedDictionary<DateTime, SortedDictionary<int, List<MST.MES.OrderStructureByOrderNo.SmtRecords>>>();
            
            var dictByDate = DataContainer.sqlDataByProcess.Smt.SelectMany(o => o.Value.smtOrders).GroupBy(x => dateTools.whatDayShiftIsit(x.smtEndDate).fixedDate.Date).ToDictionary(x => x.Key, x => x.ToList());

            foreach (var dayEntry in dictByDate)
            {
                var dictByShift = dayEntry.Value.GroupBy(d => dateTools.whatDayShiftIsit(d.smtEndDate).shift).ToDictionary(x => x.Key, x => x.ToList());
                result.Add(dayEntry.Key,new SortedDictionary<int, List<MST.MES.OrderStructureByOrderNo.SmtRecords>>(dictByShift));
            }

            return result;
        }

        public static void FillOutProductionReportGrid(SortedDictionary<DateTime, SortedDictionary<int, List<MST.MES.OrderStructureByOrderNo.SmtRecords>>> sourceDic, DataGridView grid)
        {
            DataTable result = new DataTable();

            DataTable tagTableTemplate = new DataTable();
            tagTableTemplate.Columns.Add("Start");
            tagTableTemplate.Columns.Add("Koniec");
            tagTableTemplate.Columns.Add("Linia");
            tagTableTemplate.Columns.Add("Operator");
            tagTableTemplate.Columns.Add("Zlecenie");
            tagTableTemplate.Columns.Add("Model");
            tagTableTemplate.Columns.Add("Ilość");
            tagTableTemplate.Columns.Add("NG");
            tagTableTemplate.Columns.Add("Stencil");
             
            grid.Rows.Clear();
            grid.Columns.Clear();
            grid.Columns.Add("Mies", "Mies");
            grid.Columns.Add("Tydz", "Tydz");
            grid.Columns.Add("Dzien", "Dzien");
            grid.Columns.Add("Zmiana", "Zmiana");
            //grid.Columns.Add("SMT1", "SMT1");
            foreach (var smtLine in GlobalParameters.allLinesByHand)
            {
                grid.Columns.Add(smtLine, smtLine);
            }

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
                    var filteredList = shiftEntry.Value;
                    if (!SharedComponents.Smt.cbSmtLg.Checked)
                    {
                        filteredList.RemoveAll(o => o.orderInfo.clientGroup == "LGI");
                    }
                    if (!SharedComponents.Smt.cbSmtMst.Checked)
                    {
                        filteredList.RemoveAll(o => o.orderInfo.clientGroup == "MST");
                    }
                    SortedDictionary<string, int> qtyPerLine;
                    if (SharedComponents.Smt.productionReportTab.rbModelsCount.Checked)
                    {
                        qtyPerLine = new SortedDictionary<string, int>(filteredList.GroupBy(l => l.smtLine).ToDictionary(k => k.Key, l => l.ToList().Select(o => o.manufacturedQty).Sum()));
                    }
                    else
                    {
                        qtyPerLine =  new SortedDictionary<string, int>(filteredList.GroupBy(l => l.smtLine).ToDictionary(k => k.Key, l => l.ToList().Select(o => SmtRecordToMontedComponentsCount(o)).Sum()));
                    }
                     
                    foreach (var smtLine in GlobalParameters.allLinesByHand)
                    {
                        if (!qtyPerLine.ContainsKey(smtLine)) { qtyPerLine.Add(smtLine, 0); }
                    }


                    grid.Rows.Add(dateTools.productionMonthName(week, dayEntry.Key.Year).ToUpper(), week, dayEntry.Key.ToShortDateString(), shiftEntry.Key.ToString(),  qtyPerLine["SMT2"], qtyPerLine["SMT3"], qtyPerLine["SMT4"], qtyPerLine["SMT5"], qtyPerLine["SMT6"], qtyPerLine["SMT7"], qtyPerLine["SMT8"]);
                    Dictionary<string, DataTable> tagTablesPerLine = new Dictionary<string, DataTable>();
                    foreach (var smtLine in GlobalParameters.allLinesByHand)
                    {
                        tagTablesPerLine.Add(smtLine, tagTableTemplate.Clone());
                    }

                    foreach (var smtRecord in shiftEntry.Value)
                    {
                        tagTablesPerLine[smtRecord.smtLine].Rows.Add(smtRecord.smtStartDate, smtRecord.smtEndDate, smtRecord.smtLine, smtRecord.operatorSmt, smtRecord.orderInfo.orderNo, smtRecord.orderInfo.modelId, smtRecord.manufacturedQty, 0, smtRecord.stencilId);
                    }


                    foreach (DataGridViewCell cell in grid.Rows[grid.Rows.Count - 1].Cells)
                    {
                        cell.Style.BackColor = rowColor;

                        if (tagTablesPerLine.TryGetValue(cell.OwningColumn.Name, out tagTableTemplate))
                        {
                            cell.Tag = tagTableTemplate;
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

        private static int SmtRecordToMontedComponentsCount(MST.MES.OrderStructureByOrderNo.SmtRecords record)
        {
            var mesModel = DataContainer.mesModels[record.orderInfo.modelId];
            return record.manufacturedQty * mesModel.connectorCountPerModel + record.manufacturedQty * mesModel.ledCountPerModel + record.manufacturedQty * mesModel.resistorCountPerModel;
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

        public static void FillOutStencilTable( DataGridView grid, Dictionary<string, ModelInfo.ModelSpecification> mesModels)
        {
            DataTable smtRecords = SQLoperations.GetStencilSmtRecords();
            Dictionary<string, stencilStruct> stencilDict = new Dictionary<string, stencilStruct>();

            foreach (DataRow row in smtRecords.Rows)
            {
                string stencilID = row["StencilQR"].ToString();
                string model = row["Model"].ToString();
                DateTime date = DateTime.Parse(row["DataCzasKoniec"].ToString());

                ModelInfo.ModelSpecification modelInfo;
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
                    stencilDict[stencilID].cycleCOunt += qty/modelInfo.pcbCountPerMB;

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
