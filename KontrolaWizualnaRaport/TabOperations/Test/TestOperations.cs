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
        public static void FillOutTesterTable()
        {
            var grid = SharedComponents.Test.DailyReport.dataGridViewTestProdReport;

            grid.Rows.Clear();
            grid.Columns.Clear();
            Color rowColor = Color.White;

            var testersList = DataContainer.sqlDataByProcess.Test.SelectMany(o => o.Value.pcbDict)
                                                                    .SelectMany(pcb => pcb.Value.testEntries)
                                                                    .Select(rec => rec.testerName).Distinct()
                                                                    .OrderBy(n=>n).ToArray();

            grid.Columns.Add("Mies", "Mies");
            grid.Columns.Add("Tydzt", "Tydzt");
            grid.Columns.Add("Data", "Data");
            grid.Columns.Add("Zmiana", "Zmiana");
            foreach (var testerName in testersList)
            {
                grid.Columns.Add(testerName, testerName);
            }

            DataTable tagTemplate = new DataTable();
            tagTemplate.Columns.Add("Data Start");
            tagTemplate.Columns.Add("Data Koniec");
            tagTemplate.Columns.Add("LOT");
            tagTemplate.Columns.Add("Model");
            tagTemplate.Columns.Add("Tester");
            tagTemplate.Columns.Add("Ilosc");
            tagTemplate.Columns.Add("Ilość cykli");

            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            var grouppedByDay = DataContainer.sqlDataByProcess.Test.SelectMany(o => o.Value.pcbDict)
                                                                    .SelectMany(pcb => pcb.Value.testEntries)
                                                                    .Where(rec=>rec.testTime<DateTime.Now)
                                                                    .OrderBy(rec=>rec.testTime)
                                                                    .GroupBy(rec => rec.testTime.Date)
                                                                    .ToDictionary(d => d.Key, rec => rec.ToList());

            
            foreach (var dayEntry in grouppedByDay)
            {
                var grouppedByShift = dayEntry.Value.GroupBy(rec => dateTools.whatDayShiftIsit(rec.testTime).shift).ToDictionary(s => s.Key, r => r.ToList());
                foreach (var shiftEntry in grouppedByShift)
                {
                    grid.Rows.Add(dayEntry.Key.ToString("MMM"), dateTools.WeekNumber(dayEntry.Key), dayEntry.Key.ToString("dd-MMM-yyyy"), shiftEntry.Key);
                    int lastRowIndex = grid.Rows.Count - 1;
                    var grouppedByTester = shiftEntry.Value.GroupBy(rec => rec.testerName).ToDictionary(t => t.Key, rec => rec.ToList());
                    Dictionary<string, DataTable> tagPerMachine = new Dictionary<string, DataTable>();
                    foreach (var testerName in testersList)
                    {
                        List<MST.MES.OrderStructureByOrderNo.TestRecord> testRecords;
                        grouppedByTester.TryGetValue(testerName, out testRecords);
                        int testedQuantity = 0;
                        if (testRecords != null) 
                        {
                            tagPerMachine.Add(testerName, tagTemplate.Clone());
                            
                            var grouppedByOrder = testRecords.Where(rec => rec.orderNo.Length == 7).GroupBy(rec => rec.orderNo).ToDictionary(m => m.Key, rec => rec.ToList());
                            foreach (var orderEntry in grouppedByOrder)
                            {
                                if (!DataContainer.sqlDataByOrder.ContainsKey(orderEntry.Key)) continue;
                                tagPerMachine[testerName].Rows.Add(orderEntry.Value.OrderBy(rec => rec.testTime).First().testTime,
                                                                   orderEntry.Value.OrderBy(rec => rec.testTime).Last().testTime,
                                                                   orderEntry.Value.First().orderNo,
                                                                   DataContainer.sqlDataByProcess.Kitting[orderEntry.Value.First().orderNo].ModelName,
                                                                   testerName,
                                                                   orderEntry.Value.Select(rec=>rec.serialNo).Distinct().Count(),
                                                                   orderEntry.Value.Count()
                                                                   );
                            }
                            testedQuantity = testRecords.Select(rec => rec.serialNo).Distinct().Count();
                        }
                        grid.Rows[lastRowIndex].Cells[testerName].Value = testedQuantity;
                        DataTable tag;
                        tagPerMachine.TryGetValue(testerName, out tag);
                        grid.Rows[lastRowIndex].Cells[testerName].Tag = tag;
                    }
                }
            }
                                                                    

            SMTOperations.autoSizeGridColumns(grid);
            dgvTools.MakeAlternatingRowsColors(grid, 2);
            grid.FirstDisplayedScrollingRowIndex = grid.RowCount - 1;
        }
    }
}
