using KontrolaWizualnaRaport.CentalDataStorage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport.TabOperations.SMT_tabs
{
    public class ChangeOversOperations
    {
        public static void FillOutGridChangeOvers()
        {
            DataGridView grid = SharedComponents.Smt.changeoversTab.dataGridViewChangeOvers;
            SortedDictionary<DateTime, SortedDictionary<int, List<MST.MES.OrderStructureByOrderNo.SmtRecords>>> sourceDic = DataContainer.Smt.sortedTableByDayAndShift;

            grid.Rows.Clear();

            //DataTable tagTableTemplate = new DataTable();
            //tagTableTemplate.Columns.Add("Start");
            //tagTableTemplate.Columns.Add("Koniec");
            //tagTableTemplate.Columns.Add("Linia");
            //tagTableTemplate.Columns.Add("Operator");
            //tagTableTemplate.Columns.Add("Zlecenie");
            //tagTableTemplate.Columns.Add("Model");
            //tagTableTemplate.Columns.Add("Ilość");
            //tagTableTemplate.Columns.Add("NG");
            //tagTableTemplate.Columns.Add("Stencil");

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

            System.Drawing.Color rowColor = System.Drawing.Color.White;


            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            foreach (var dayEntry in sourceDic)
            {
                foreach (var shiftEntry in dayEntry.Value)
                {
                    Dictionary<string, int> numberOfModelsPerLine = new Dictionary<string, int>();
                    foreach (var smtLine in GlobalParameters.allLinesByHand)
                    {
                        numberOfModelsPerLine.Add(smtLine, 0);
                    }

                    grid.Rows.Add(dayEntry.Key.ToString("MMM").ToUpper(),
                                  dateTools.WeekNumber(dayEntry.Key),
                                  dayEntry.Key.ToShortDateString(),
                                  shiftEntry.Key);
                    var grouppedByLine = shiftEntry.Value.GroupBy(r => r.smtLine).ToDictionary(line => line.Key, rec => rec.ToList());
                    foreach (var lineEntry in grouppedByLine)
                    {
                        var cellHint = lineEntry.Value.Select(r => $"{r.orderInfo.modelId} {r.smtStartDate.ToString("HH:mm")}-{r.smtEndDate.ToString("HH:mm")} {r.manufacturedQty}szt.").ToArray();
                        int numberOfModels = cellHint.Count();
                        grid.Rows[grid.Rows.Count - 1].Cells[lineEntry.Key].Value = numberOfModels;
                        grid.Rows[grid.Rows.Count - 1].Cells[lineEntry.Key].ToolTipText = string.Join(Environment.NewLine, cellHint);
                    }
                    

                }
            }

        }
    }
}
