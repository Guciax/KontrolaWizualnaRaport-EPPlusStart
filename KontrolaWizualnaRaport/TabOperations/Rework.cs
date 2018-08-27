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
using static KontrolaWizualnaRaport.SMTOperations;

namespace KontrolaWizualnaRaport
{
    public class Rework
    {
        public struct PcbReworkData
        {
            public string id;
            public List<ReworkOperationDetails> operations;
        }

        public struct ReworkOperationDetails
        {
            public DateTime fixedDate;
            public DateTime realDate;
            public string operation;
            public string model;
            public string compRef;
            public string oper;
            public bool result;
            public string pcbId;
        }

        private static Dictionary<DateTime, Dictionary<int,List<ReworkOperationDetails>>> SortSqlTableBydateShiftPcb(DataTable sqlTable)
        {
            Dictionary<DateTime, Dictionary<int, List<ReworkOperationDetails>>> result = new Dictionary<DateTime, Dictionary<int, List<ReworkOperationDetails>>>();

            foreach (DataRow row in sqlTable.Rows)
            {
                string pcbId = row["SerialNo"].ToString();
                DateTime realDate = DateTime.Parse(row["Data"].ToString());
                DateTime fixedDate = ViDataLoader.FixedShiftDate(realDate);
                string operation = row["OpisNaprawy"].ToString();
                string compRef = row["NaprawianyKomponent"].ToString();
                string oper = row["Operator"].ToString();
                string model = row["Model"].ToString();
                bool repairResult = true;

                if (row["WynikNaprawy"].ToString() != "OK") { repairResult = false; }

                if (!result.ContainsKey(fixedDate.Date))
                {
                    result.Add(fixedDate.Date, new Dictionary<int, List<ReworkOperationDetails>>());
                }
                dateShiftNo shiftInfo = dateTools.whatDayShiftIsit(realDate);
                if (!result[fixedDate.Date].ContainsKey(shiftInfo.shift))
                {
                    result[fixedDate.Date].Add(shiftInfo.shift, new List<ReworkOperationDetails>());
                }

                


                PcbReworkData reworkItem = new PcbReworkData();
                reworkItem.operations = new List<ReworkOperationDetails>();
                reworkItem.id = pcbId;
                ReworkOperationDetails newOperation = new ReworkOperationDetails();

                newOperation.fixedDate = fixedDate;
                newOperation.realDate = realDate;
                newOperation.operation = operation;
                newOperation.model = model;
                newOperation.compRef = compRef;
                newOperation.oper = oper;
                newOperation.result = repairResult;
                newOperation.pcbId = pcbId;
                reworkItem.operations.Add(newOperation);

                    result[fixedDate.Date][shiftInfo.shift].Add(newOperation);
                
            }

            return result;
        }

        private static void FillOutGridReworkByOperator(Dictionary<DateTime, Dictionary<int, List<ReworkOperationDetails>>> reworkDataByDate, DataGridView grid)
        {
            Dictionary<string, List<ReworkOperationDetails>> reworkInfoByOperator = new Dictionary<string, List<ReworkOperationDetails>>();
            grid.Columns.Clear();
            grid.Columns.Add("Operator", "Operator");
            grid.Columns.Add("Operacje", "Operacje");
            grid.Columns.Add("PCB", "PCB");
            grid.Columns.Add("Uszodzone", "Uszodzone");
            grid.Columns.Add("Uszodzone%", "Uszodzone%");
            foreach (var dateEntry in reworkDataByDate)
            {
                foreach (var shiftEntry in dateEntry.Value)
                {
                    foreach (var pcbEntry in shiftEntry.Value)
                    {
                        string oper = pcbEntry.oper;
                        if (!reworkInfoByOperator.ContainsKey(oper))
                        {
                            reworkInfoByOperator.Add(oper, new List<ReworkOperationDetails>());
                        }
                        reworkInfoByOperator[oper].Add(pcbEntry);
                    }
                }
            }

            foreach (var operatorEntry in reworkInfoByOperator)
            {
                int totalOperations = operatorEntry.Value.Count;
                int totalPcbs = operatorEntry.Value.Select(pcb => pcb.pcbId).Distinct().ToList().Count;
                //int okCount = operatorEntry.Value.Select(pcb => pcb.result == true).ToList().Count;
                int mistakeCount = operatorEntry.Value.Where(pcb => pcb.operation == "SCRAP - uszkodzenie podczas naprawy.").ToList().Count;
                grid.Rows.Add(operatorEntry.Key, totalOperations, totalPcbs,  mistakeCount, Math.Round((double)mistakeCount/(double)totalPcbs*100,2)+"%");
            }
        }
        
        

        public static void FillOutGridDailyProdReport(DataGridView dailyReportGrid, DataGridView reportByOperatorGrid, DataTable sqlTable)
        {
            Dictionary<DateTime, Dictionary<int, List<ReworkOperationDetails>>> reworkDataByDate = SortSqlTableBydateShiftPcb(sqlTable);
            FillOutGridReworkByOperator(reworkDataByDate, reportByOperatorGrid);

            dailyReportGrid.Columns.Clear();
            dailyReportGrid.Columns.Add("Data", "Data");
            dailyReportGrid.Columns.Add("Tydz", "Tydz");
            dailyReportGrid.Columns.Add("Zmiana", "Zmiana");
            dailyReportGrid.Columns.Add("PCB", "PCB");
            dailyReportGrid.Columns.Add("Operacji", "Operacji");

            Color rowColor = Color.White;

            foreach (var dateEntry in reworkDataByDate)
            {
                if (rowColor==Color.White)
                {
                    rowColor = Color.LightBlue;
                }
                else
                {
                    rowColor = Color.White;
                }

                foreach (var shiftEntry in dateEntry.Value)
                {
                    DataTable shiftTagTable = new DataTable();

                    shiftTagTable.Columns.Add("#pcb");
                    shiftTagTable.Columns.Add("#operacja");
                    shiftTagTable.Columns.Add("Data");
                    shiftTagTable.Columns.Add("Model");
                    shiftTagTable.Columns.Add("serialNo");
                    shiftTagTable.Columns.Add("Operator");
                    shiftTagTable.Columns.Add("Naprawa");
                    shiftTagTable.Columns.Add("Komponent");
                    shiftTagTable.Columns.Add("Wynik");

                    int operationCount = shiftEntry.Value.Count;
                    int pcbCount = 0;

                    string prevPcb = "";

                    for (int op=0;op<shiftEntry.Value.Count;op++)
                    {
                        if (shiftEntry.Value[op].pcbId!=prevPcb)
                        {
                            pcbCount++;
                            prevPcb = shiftEntry.Value[op].pcbId;
                        }

                        string wynik = "OK";
                        if (!shiftEntry.Value[op].result) wynik = "NG";

                        shiftTagTable.Rows.Add(pcbCount, op+1, shiftEntry.Value[op].realDate.ToString("dd.MM.yyyy hh:mm"), shiftEntry.Value[op].model, shiftEntry.Value[op].pcbId, shiftEntry.Value[op].oper, shiftEntry.Value[op].operation, shiftEntry.Value[op].compRef, wynik);
                    }

                    

                    dailyReportGrid.Rows.Add(dateEntry.Key.ToString("dd-MM-yyyy"), dateTools.GetIso8601WeekOfYear(dateEntry.Key), shiftEntry.Key, pcbCount, operationCount);
                    foreach (DataGridViewCell cell in dailyReportGrid.Rows[dailyReportGrid.Rows.Count-1].Cells)
                    {
                        cell.Style.BackColor = rowColor;
                        if (cell.OwningColumn.HeaderText.ToLower()!="data" & cell.OwningColumn.HeaderText.ToLower() != "tydz" & cell.OwningColumn.HeaderText.ToLower() != "zmiana")
                        {
                            cell.Tag = shiftTagTable;
                        }
                    }
                }
            }
            SMTOperations.autoSizeGridColumns(dailyReportGrid);
            dailyReportGrid.FirstDisplayedCell = dailyReportGrid.Rows[dailyReportGrid.Rows.Count - 1].Cells[0];
        }
    }
}
