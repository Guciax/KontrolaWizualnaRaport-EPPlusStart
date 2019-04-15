using KontrolaWizualnaRaport.CentalDataStorage;
using MST.MES;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport.TabOperations.SMT_tabs
{
    public class SmtEfficiencyReport
    {
        public static void FillOutEfficiencyGrid()
        {
            var sourceDic = DataContainer.Smt.sortedTableByDayAndShift;
            DataGridView grid = SharedComponents.Smt.SmtEfficiencyTab.dataGridViewSmtEfficiency;

            grid.Columns.Clear();
            grid.Columns.Add("Mies", "Mies");
            grid.Columns.Add("Tydz", "Tydz");
            grid.Columns.Add("Dzien", "Dzien");
            grid.Columns.Add("Zmiana", "Zmiana");

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

                    var grouppedByLine = filteredList.GroupBy(rec => rec.smtLine).ToDictionary(fam => fam.Key, rec => rec.ToList());
                    Dictionary<string, double> efficiencyByLine = new Dictionary<string, double>();
                    Dictionary<string, DataTable> tagTablePerLine = new Dictionary<string, DataTable>();
                    DataTable tagTemplate = new DataTable();
                    tagTemplate.Columns.Add("Zlecenie");
                    tagTemplate.Columns.Add("12NC");
                    tagTemplate.Columns.Add("Ilość");
                    tagTemplate.Columns.Add("Start");
                    tagTemplate.Columns.Add("Koniec");
                    tagTemplate.Columns.Add("Operator");
                    tagTemplate.Columns.Add("Wydajność");

                    foreach (var smtLine in GlobalParameters.allLinesByHand)
                    {
                        efficiencyByLine.Add(smtLine, 0);
                        tagTablePerLine.Add(smtLine, tagTemplate.Clone());
                    }


                    foreach (var lineEntry in grouppedByLine)
                    {
                        var grouppedByModelFamily = lineEntry.Value.GroupBy(rec => rec.orderInfo.ModelFamily(OrderStructureByOrderNo.ModelFamilyType.SameConnAndCCT)).ToDictionary(fam => fam.Key, rec => rec.ToList());
                        List<double> effcientyPerShift = new List<double>();
                        foreach (var familyEntry in grouppedByModelFamily)
                        {
                            var duration = (familyEntry.Value.Select(rec => rec.smtEndDate).Max()-familyEntry.Value.Select(rec => rec.smtStartDate).Min()).TotalHours;
                            var totalProduction = familyEntry.Value.Select(rec => rec.manufacturedQty).Sum();
                            float modelChangeOverTime = (float)0.5;
                            var numberOfLots = familyEntry.Value.Count;

                            var norm = SmtEfficiencyCalculation.NewWay.CalculateModelNormPerHour(familyEntry.Value.First().orderInfo.modelId, familyEntry.Value.First().smtLine);



                            effcientyPerShift.Add(totalProduction / duration / norm.outputPerHour);
                            //effcientyPerShift.Add(SmtEfficiencyCalculation.CalculateEfficiencyForOrders(familyEntry.Value));
                            foreach (var record in familyEntry.Value)
                            {
                                tagTablePerLine[lineEntry.Key].Rows.Add(record.orderInfo.orderNo,
                                                                        record.orderInfo.modelId,
                                                                        record.manufacturedQty,
                                                                        record.smtStartDate,
                                                                        record.smtEndDate,
                                                                        record.operatorSmt,
                                                                        SmtEfficiencyCalculation.CalculateEfficiencyForOneOrder(record));
                            }
                        }

                        efficiencyByLine[lineEntry.Key] = effcientyPerShift.Average();
                    }

                    grid.Rows.Add(dayEntry.Key.ToString("MMM").ToUpper(),
                                  dateTools.WeekNumber(dayEntry.Key),
                                  dayEntry.Key.ToString("dd-MMM-yyyy").ToUpper(),
                                  shiftEntry.Key);

                    foreach (var lineEntry in tagTablePerLine)
                    {
                        if (efficiencyByLine[lineEntry.Key] > 0)
                        {
                            grid.Rows[grid.Rows.Count - 1].Cells[lineEntry.Key].Value = Math.Round(efficiencyByLine[lineEntry.Key] * 100, 1) + "%";
                            grid.Rows[grid.Rows.Count - 1].Cells[lineEntry.Key].Tag = lineEntry.Value;
                        }
                        
                    }

                }

            }
            dgvTools.ColumnsAutoSize(grid, DataGridViewAutoSizeColumnMode.AllCells);
        }
    }
}
