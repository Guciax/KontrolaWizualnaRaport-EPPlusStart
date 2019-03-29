using KontrolaWizualnaRaport.CentalDataStorage;
using KontrolaWizualnaRaport.TabOperations.SMT_tabs;
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
    public class SMTOperations
    {
        public static void DownloadSqlDataAndMerge()
        {
            DataContainer.sqlDataByProcess.Smt = MST.MES.SqlDataReaderMethods.SMT.GetOrdersDateToDate(SharedComponents.Smt.smtStartDate.Value.Date, SharedComponents.Smt.smtEndDate.Value.Date);
            DataMerger.MergeData();
            PrepareFreshSmtData();
        }

        public static void PrepareFreshSmtData()
        {
            DataContainer.Smt.sortedTableByDayAndShift = SMTOperations.sortListByDayAndShift();
            DataContainer.Smt.EfficiencyNormPerModel = SmtEfficiencyCalculation.EfficiencyOutputPerHourNormPerModel();
            DataContainer.Smt.EfficiencyHistogramPerModel = SmtEfficiencyCalculation.EfficiencyHistogramPerModel(OrderStructureByOrderNo.ModelFamilyType.SameConnAndCCT);
            FillOutSmtComponents();
            LedWasteTabOperations.ledWasteDictionary = new SortedDictionary<DateTime, SortedDictionary<int, List<LedWasteTabOperations.LotLedWasteStruc>>>();
            ReLoadSmtTab();
        }

        private static void FillOutSmtComponents()
        {
            SharedComponents.Smt.ModelAnalysis.comboBoxSmtModels.Items.Clear();
            SharedComponents.Smt.ModelAnalysis.comboBoxSmtModels.Items.AddRange(DataContainer.Smt.EfficiencyHistogramPerModel.Select(model => model.Key).OrderBy(m=>m).ToArray());
        }

        public static void ReLoadSmtTab()
        {
            ReloadProductionReportsGrid();
            LedWasteTabOperations.RefreshOrReloadWasteData();
            FillOutStencilTable(SharedComponents.Smt.StencilsTab.dataGridViewSmtStencilUsage, DataContainer.mesModels);
            SmtCharts.DrawChartSmtProductionReport();
            ChangeOversOperations.FillOutGridChangeOvers();
            SmtEfficiencyReport.FillOutEfficiencyGrid();
        }

        public struct LotLedWaste
        {
            public Int32 ledExpectedUsageA;
            public Int32 droppedA;
            public Int32 ledExpectedUsageB;
            public Int32 droppedB;
        }

        public static SortedDictionary<DateTime, SortedDictionary<int, List<MST.MES.OrderStructureByOrderNo.SmtRecords>>> sortListByDayAndShift()
        {
            SortedDictionary<DateTime, SortedDictionary<int, List<MST.MES.OrderStructureByOrderNo.SmtRecords>>> result = new SortedDictionary<DateTime, SortedDictionary<int, List<MST.MES.OrderStructureByOrderNo.SmtRecords>>>();

            var dictByDate = DataContainer.sqlDataByProcess.Smt.SelectMany(rec=>rec.Value.smtOrders).
                                                                Where(order=>order.smtEndDate.Date >= SharedComponents.Smt.smtStartDate.Value & order.smtEndDate.Date <= SharedComponents.Smt.smtEndDate.Value).
                                                                GroupBy(o => o.smtEndDate.Date).
                                                                ToDictionary(k => k.Key, v => v.ToList());

            foreach (var dayEntry in dictByDate)
            {
                var dictByShift = dayEntry.Value.GroupBy(d => dateTools.whatDayShiftIsit(d.smtEndDate).shift).ToDictionary(x => x.Key, x => x.ToList());
                result.Add(dayEntry.Key,new SortedDictionary<int, List<MST.MES.OrderStructureByOrderNo.SmtRecords>>(dictByShift));
            }

            return result;
        }

        public static void ReloadProductionReportsGrid()
        {
            SortedDictionary<DateTime, SortedDictionary<int, List<MST.MES.OrderStructureByOrderNo.SmtRecords>>> sourceDic = DataContainer.Smt.sortedTableByDayAndShift;
            DataGridView grid = SharedComponents.Smt.productionReportTab.dataGridViewSmtProduction;
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

            System.Drawing.Color rowColor = System.Drawing.Color.White;
            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            foreach (var dayEntry in sourceDic)
            {
                if (dayEntry.Key < SharedComponents.Smt.smtStartDate.Value) continue;
                if (dayEntry.Key > SharedComponents.Smt.smtEndDate.Value) continue;
                rowColor = dgvTools.SwitchowColor(rowColor);

                var week = dateTools.WeekNumber(dayEntry.Key);

                foreach (var shiftEntry in dayEntry.Value)
                {
                    var filteredList = new List<OrderStructureByOrderNo.SmtRecords>();
                    if (SharedComponents.Smt.cbSmtLg.Checked)
                    {
                        filteredList.AddRange(shiftEntry.Value.Where(o => o.orderInfo.clientGroup == "LG"));
                    }
                    if (SharedComponents.Smt.cbSmtMst.Checked)
                    {
                        filteredList.AddRange(shiftEntry.Value.Where(o => o.orderInfo.clientGroup == "MST"));
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

                    var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
                    nfi.NumberGroupSeparator = " ";
                    var month = dateTools.productionMonthName(week, dayEntry.Key.Year).ToUpper();


                    grid.Rows.Add(month, week, dayEntry.Key.ToShortDateString(), shiftEntry.Key.ToString(),
                        qtyPerLine["SMT1"].ToString("#,0", nfi), qtyPerLine["SMT2"].ToString("#,0", nfi),
                        qtyPerLine["SMT3"].ToString("#,0", nfi), qtyPerLine["SMT4"].ToString("#,0", nfi),
                        qtyPerLine["SMT5"].ToString("#,0", nfi), qtyPerLine["SMT6"].ToString("#,0", nfi),
                        qtyPerLine["SMT7"].ToString("#,0", nfi), qtyPerLine["SMT8"].ToString("#,0", nfi));

                    Dictionary<string, DataTable> tagTablesPerLine = new Dictionary<string, DataTable>();
                    foreach (var smtLine in GlobalParameters.allLinesByHand)
                    {
                        tagTablesPerLine.Add(smtLine, tagTableTemplate.Clone());
                    }

                    foreach (var smtRecord in filteredList) {
                        tagTablesPerLine[smtRecord.smtLine].Rows.Add(smtRecord.smtStartDate, smtRecord.smtEndDate,
                            smtRecord.smtLine, smtRecord.operatorSmt, smtRecord.orderInfo.orderNo,
                            smtRecord.orderInfo.modelId, smtRecord.manufacturedQty, 0, smtRecord.stencilId);
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

        private static string StringFormatThousandSpaceSeparated(int input)
        {
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";
            return input.ToString("#,0", nfi); 
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
