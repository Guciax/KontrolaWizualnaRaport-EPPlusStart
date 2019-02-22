using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static KontrolaWizualnaRaport.dateTools;

namespace KontrolaWizualnaRaport
{
    public class TestOperations
    {
        public static string testerIdToName(string lineID)
        {
            string testerID = "";
            switch (lineID)
            {
                case "1":
                    {
                        testerID = "Optical";
                        break;
                    }
                case "2":
                    {
                        testerID = "Manual-2";
                        break;
                    }
                case "3":
                    {
                        testerID = "Manual-1";
                        break;
                    }
                case "4":
                    {
                        testerID = "test_SMT5";
                        break;
                    }
                case "5":
                    {
                        testerID = "test_SMT6";
                        break;
                    }
                case "0":
                    {
                        testerID = "Splitting";
                        break;
                    }
            }
            return testerID;
        }

        public static string TestModelFamily(string model)
        {
            var splittedModel = model.Split('-');
            return splittedModel[0];
        }

        public static Dictionary<string, DataTable> InspectionRecordsPerMachine(Dictionary<DateTime, SortedDictionary<int, Dictionary<string, Dictionary<string, DataTable>>>> testerData)
        {
            Dictionary<string, DataTable> result = new Dictionary<string, DataTable>();

            foreach (var dateEntry in testerData)
            {
                foreach (var shiftEntry in dateEntry.Value)
                {
                    foreach (var machineEntry in shiftEntry.Value)
                    {
                        DataTable template = new DataTable();
                        template.Columns.Add("date", typeof(DateTime));
                        template.Columns.Add("id", typeof(string));
                        template.Columns.Add("lot", typeof(string));

                        if (!result.ContainsKey(machineEntry.Key))
                        {
                            result.Add(machineEntry.Key, template.Clone());
                        }

                        foreach (var lotEntry in machineEntry.Value)
                        {
                            foreach (DataRow row in lotEntry.Value.Rows)
                            {
                                DateTime date = DateTime.Parse(row["Data"].ToString());
                                string id = row["PCB"].ToString();
                                result[machineEntry.Key].Rows.Add(date, id, lotEntry.Key);
                            }
                        }
                    }
                }
            }


            return result;
        }

        public static Dictionary<string, Dictionary<string, List<double>>> InspectionTimeByMachineModel (Dictionary<DateTime, SortedDictionary<int, Dictionary<string, Dictionary<string, DataTable>>>> testerData, Dictionary<string, string> lotModelDictionary)
        {
            Dictionary<string, DataTable> inspectionRecordsPerMachine = InspectionRecordsPerMachine( testerData);
            Dictionary<string, Dictionary<string, List<double>>> result = new Dictionary<string, Dictionary<string, List<double>>>();

            foreach (var machineEntry in inspectionRecordsPerMachine)
            {
                result.Add(machineEntry.Key, new Dictionary<string, List<double>>());
                string previousLot = "";
                DateTime lotStart = DateTime.Now;
                DateTime lotEnd = new DateTime(1800, 1, 1); 
                int testCyclesCount = 0;
                string model = "";
                var lastRow = machineEntry.Value.Rows.Count - 1;
                double duration = 0;

                foreach (DataRow row in machineEntry.Value.Rows)
                {
                    string currentLot = row["lot"].ToString();
                    if (currentLot == "empty") continue;
                    if (!lotModelDictionary.TryGetValue(row["lot"].ToString(), out model)) continue;
                    model = TestModelFamily(model);
                    if (model == null) continue;

                    if (previousLot != "" & previousLot != currentLot) 
                    {


                        if (!result[machineEntry.Key].ContainsKey(model))
                        {
                            result[machineEntry.Key].Add(model, new List<double>());
                        }

                        duration = (lotEnd - lotStart).TotalSeconds;

                        if (duration > 0 & duration/testCyclesCount<30 & testCyclesCount > 10)
                        {
                            result[machineEntry.Key][model].Add(duration / testCyclesCount);
                            //string msg = string.Format("start:{0} end:{1} dur:{2} qty:{3}", lotStart, lotEnd, duration, testCyclesCount);
                            //string msg = string.Format("{0};{1};{2};{3};{4};{5}", machineEntry.Key, model, lotStart, lotEnd, duration, testCyclesCount);
                            //Debug.WriteLine(msg);
                        }
                        testCyclesCount = 0;
                        lotStart = DateTime.Now;
                        lotEnd = new DateTime(1800, 1, 1);
                    }

                    DateTime rowDate = DateTime.Parse(row["date"].ToString());

                    if (rowDate < lotStart) 
                    {
                        lotStart = rowDate;
                    }
                    if (rowDate > lotEnd) 
                    {
                        lotEnd = rowDate;
                    }
                    testCyclesCount++;
                    previousLot = currentLot;
                }
                //last row
                if (testCyclesCount > 0)
                {
                    if (!result[machineEntry.Key].ContainsKey(model))
                    {
                        result[machineEntry.Key].Add(model, new List<double>());
                    }
                    duration = (lotEnd - lotStart).TotalSeconds;
                    result[machineEntry.Key][model].Add(duration / testCyclesCount);
                    //Debug.WriteLine("Adding: " + machineEntry.Key + " Model: " + model + " Duration: " + duration);
                }
            }
            return result;
        }

        //public static Dictionary<string, Dictionary<string, List<double>>> cycleTimeListByMachineModel(Dictionary<DateTime, SortedDictionary<int, Dictionary<string, Dictionary<string, DataTable>>>> testerData, Dictionary<string, string> lotModelDictionary)
        //{
        //    Dictionary<string, DataTable> inspectionRecordsPerMachine = InspectionRecordsPerMachine(testerData);
        //    Dictionary<string, Dictionary<string,  List<double>>> inspectionTimeByMachineModelShift = InspectionTimeByMachineModel(inspectionRecordsPerMachine, lotModelDictionary);

        //    Dictionary<string, Dictionary<string, List<double>>> result = new Dictionary<string, Dictionary<string, List<double>>>();
        //    double maxBreakPeriod = 30*60;
        //    double minModulesTakenToCalc = 20;

        //    foreach (var machineEntry in inspectionTimeByMachineModelShift)
        //    {
        //        result.Add(machineEntry.Key, new Dictionary<string, List<double>>());
        //        foreach (var modelEntry in machineEntry.Value)
        //        {
        //            result[machineEntry.Key].Add(modelEntry.Key, new List<double>());
                    
        //            foreach (var shiftEntry in modelEntry.Value)
        //            {
        //                List<double> fractalList = new List<double>();

        //                for (int i = 0; i < shiftEntry.Value.Count; i++)
        //                {
        //                    if (i == 0)
        //                    {
        //                        //fractalList.Add(shiftEntry.Value[i]);
        //                    }
        //                    else
        //                    {
        //                        DateTime prevDate = shiftEntry.Value[i - 1];
        //                        DateTime currentDate = shiftEntry.Value[i];
        //                        double duration = (currentDate - prevDate).TotalSeconds;
                                

        //                        if (duration < maxBreakPeriod) 
        //                        {
        //                            if (duration > 0)
        //                            {
        //                                fractalList.Add(duration);
        //                                //Debug.WriteLine("Adding " + duration);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (fractalList.Count > minModulesTakenToCalc)
        //                            {
        //                                //double avgCycleTime = fractalList.Average();
        //                                result[machineEntry.Key][modelEntry.Key].AddRange(fractalList.ToArray());
        //                                //Debug.WriteLine("Test Eff: added " +fractalList.Count + "results");
        //                            }
        //                            else
        //                            {
        //                                //Debug.WriteLine("Test Eff: discarted " + fractalList.Count + "results");
        //                            }
        //                            fractalList.Clear();
        //                            //fractalList.Add(currentDate);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return result;
        //}

        public static void FillOutGridWaitingForTest(DataGridView grid, Dictionary<DateTime, SortedDictionary<int, Dictionary<string, Dictionary<string, DataTable>>>> testerData, DataTable smtRecords)
        {

            HashSet<string> testedLots = new HashSet<string>();
            Dictionary<string, DataTable> lotsWaitingForTest = new Dictionary<string, DataTable>();

            foreach (var DateEntry in testerData)
            {
                foreach (var shiftEntry in DateEntry.Value)
                {
                    foreach (var lineEntry in shiftEntry.Value)
                    {
                        foreach (var modelEntry in lineEntry.Value)
                        {
                            foreach (DataRow row in modelEntry.Value.Rows)
                            {
                                string lot = row["LOT"].ToString();
                                testedLots.Add(lot);
                            }
                            
                        }
                    }
                }
            }

            foreach (DataRow row in smtRecords.Rows)
            {
                string smtLot = row["NrZlecenia"].ToString();
                DateTime smtDate = DateTime.Parse(row["DataCzasKoniec"].ToString());
                if ((DateTime.Now - smtDate).TotalDays > 14) continue;
                if (testedLots.Contains(smtLot)) { continue; }
                Debug.WriteLine(smtLot + "  "+row["DataCzasKoniec"].ToString());

                string model = row["Model"].ToString();
                if(!lotsWaitingForTest.ContainsKey(model))
                {
                    lotsWaitingForTest.Add(model, smtRecords.Clone());
                }

                lotsWaitingForTest[model].Rows.Add(row.ItemArray);
            }
            grid.Columns.Clear();
            grid.Columns.Add("Model", "Model");
            grid.Columns.Add("Ilosc", "Ilość");
            grid.Columns.Add("IloscLot", "LOTów");

            foreach (var modelEntry in lotsWaitingForTest)
            {
                int qty = 0;
                foreach (DataRow row in modelEntry.Value.Rows)
                {
                    qty += int.Parse(row["IloscWykonana"].ToString());
                }
                grid.Rows.Add(modelEntry.Key,qty , modelEntry.Value.Rows.Count);

                grid.Rows[grid.Rows.Count - 1].Cells[1].Tag = modelEntry.Value;
                grid.Rows[grid.Rows.Count - 1].Cells[2].Tag = modelEntry.Value;
            }
        }

        public static void FillOutTesterTable(Dictionary<DateTime, SortedDictionary<int, Dictionary<string, Dictionary<string, DataTable>>>> testerData, DataGridView grid, Dictionary<string, string> lotModelDictionary)
        {
            grid.Rows.Clear();
            grid.Columns.Clear();
            Color rowColor = Color.White;

            grid.Columns.Add("Data", "Data");
            grid.Columns.Add("Tydzt", "Tydz");
            grid.Columns.Add("Zmiana", "Zmiana");
            grid.Columns.Add("Optical", "Optical");
            grid.Columns.Add("Manual-1", "Manual-1");
            grid.Columns.Add("Manual-2", "Manual-2");
            grid.Columns.Add("test_SMT5", "test_SMT5");
            grid.Columns.Add("test_SMT6", "test_SMT6");

            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            foreach (var dateEntry in testerData)
            {
                if (rowColor == System.Drawing.Color.LightBlue)
                {
                    rowColor = System.Drawing.Color.White;
                }
                else
                {
                    rowColor = System.Drawing.Color.LightBlue;
                }
                var week = dateTools.WeekNumber(dateEntry.Key);
                foreach (var shiftEntry in dateEntry.Value)
                {
                    string date = dateEntry.Key.Date.ToString("yyyy-MM-dd");
                    string shift = shiftEntry.Key.ToString();

                    Dictionary<string, double> qtyPerMachine = new Dictionary<string, double>();
                    qtyPerMachine.Add("Optical", 0);
                    qtyPerMachine.Add("Manual-1", 0);
                    qtyPerMachine.Add("Manual-2", 0);
                    qtyPerMachine.Add("test_SMT5", 0);
                    qtyPerMachine.Add("test_SMT6", 0);


                    DataTable shiftTable = new DataTable();
                    shiftTable.Columns.Add("Data Start");
                    shiftTable.Columns.Add("Data Koniec");
                    shiftTable.Columns.Add("LOT");
                    shiftTable.Columns.Add("Model");
                    shiftTable.Columns.Add("Tester");
                    shiftTable.Columns.Add("Ilosc");
                    shiftTable.Columns.Add("Ilość cykli");
                    Dictionary<string, DataTable> tagPerMachine = new Dictionary<string, DataTable>();
                    tagPerMachine.Add("Optical", shiftTable.Clone());
                    tagPerMachine.Add("Manual-1", shiftTable.Clone());
                    tagPerMachine.Add("Manual-2", shiftTable.Clone());
                    tagPerMachine.Add("test_SMT5", shiftTable.Clone());
                    tagPerMachine.Add("test_SMT6", shiftTable.Clone());

                    foreach (var machineEntry in shiftEntry.Value)
                    {
                        if (!qtyPerMachine.ContainsKey(machineEntry.Key)) continue;
                        HashSet<string> pcbPerMachine = new HashSet<string>();
                        
                        foreach (var lotEntry in machineEntry.Value)
                        {
                            List<string> pcbPerLot = new List<string>();
                            DateTime start = DateTime.Now;
                            DateTime koniec = new DateTime(1970,1,1);
                            string model = "";
                            lotModelDictionary.TryGetValue(lotEntry.Key, out model);
                            
                            foreach (DataRow row in lotEntry.Value.Rows)
                            {
                                DateTime testDate = DateTime.Parse(row["Data"].ToString());
                                if (testDate > koniec) koniec = testDate;
                                if (testDate < start) start = testDate;
                                pcbPerMachine.Add(row["PCB"].ToString());
                                pcbPerLot.Add(row["PCB"].ToString());
                            }
                            tagPerMachine[machineEntry.Key].Rows.Add(start, koniec, lotEntry.Key, model, machineEntry.Key, pcbPerLot.Distinct().Count(), pcbPerLot.Count);
                        }

                        qtyPerMachine[machineEntry.Key] += pcbPerMachine.Count;
                    }
                    grid.Rows.Add(date, week, shift, qtyPerMachine["Optical"], qtyPerMachine["Manual-1"], qtyPerMachine["Manual-2"], qtyPerMachine["test_SMT5"], qtyPerMachine["test_SMT6"]);
                    foreach (DataGridViewCell cell in grid.Rows[grid.Rows.Count - 1].Cells)
                    {
                        cell.Style.BackColor = rowColor;
                        if (cell.ColumnIndex > 2)
                        {
                            string tester = cell.OwningColumn.Name;
                            cell.Tag = tagPerMachine[tester];
                        }
                    }
                    
                }
            }
            SMTOperations.autoSizeGridColumns(grid);
            grid.FirstDisplayedScrollingRowIndex = grid.RowCount - 1;
        }
    }
}
