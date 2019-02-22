using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using static KontrolaWizualnaRaport.Form1;

namespace KontrolaWizualnaRaport
{
    internal class BoxingOperations
    {
        public static int GetIso8601WeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public static void FillOutBoxingLedQty()
        {
            CustomDataGridView grid = SharedComponents.Boxing.dataGridViewBoxing;
            grid.SuspendLayout();
            grid.Rows.Clear();
            grid.Columns.Clear();
            Color rowColor = Color.White;

            grid.Columns.Add("Tydz", "Tydz");
            grid.Columns.Add("Data", "Data");
            grid.Columns.Add("Zmiana", "Zmiana");
            grid.Columns.Add("Wszystkie", "Wszystkie");
            grid.Columns.Add("LinLED", "LinLED");
            grid.Columns.Add("RdLED", "RdLED");
            grid.Columns.Add("RecLED", "RecLED");
            grid.Columns.Add("Inne", "Inne");

            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            
            var orderedByTime = DataContainer.sqlDataByOrder.Where(o => o.Value.ledsInBoxesList != null).SelectMany(o => o.Value.ledsInBoxesList).OrderBy(o => o.boxingDate).ToList();
            if (!SharedComponents.Boxing.cbLg.Checked)
            {
                orderedByTime.RemoveAll(o => o.kittingInfo.odredGroup == "LG");
            }

            if (!SharedComponents.Boxing.cbMst.Checked)
            {
                orderedByTime.RemoveAll(o => o.kittingInfo.odredGroup == "MST");
            }

            if (orderedByTime.Count() > 0)
            {



                DateTime currentDay = dateTools.whatDayShiftIsit(orderedByTime.First().boxingDate).fixedDate.Date;
                int currentShift = dateTools.whatDayShiftIsit(orderedByTime.First().boxingDate).shift;

                var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
                nfi.NumberGroupSeparator = " ";

                DataTable tagTableTemplate = new DataTable();
                tagTableTemplate.Columns.Add("Data pierwszy");
                tagTableTemplate.Columns.Add("Data ostatni");
                tagTableTemplate.Columns.Add("Model ID");
                tagTableTemplate.Columns.Add("Nazwa");
                tagTableTemplate.Columns.Add("Zlecenie");
                tagTableTemplate.Columns.Add("Ilość SMT");
                tagTableTemplate.Columns.Add("Ilość spakowana");

                Dictionary<string, Dictionary<string, List<MST.MES.OrderStructureByOrderNo.BoxingInfo>>> infoPerTypePerOrderForTag = new Dictionary<string, Dictionary<string, List<MST.MES.OrderStructureByOrderNo.BoxingInfo>>>();
                infoPerTypePerOrderForTag.Add("Wszystkie", new Dictionary<string, List<MST.MES.OrderStructureByOrderNo.BoxingInfo>>());

                var grouppedByDay = orderedByTime.GroupBy(o => dateTools.whatDayShiftIsit(o.boxingDate).fixedDate);
                foreach (var dayEntry in grouppedByDay)
                {
                    var grouppedByShift = dayEntry.GroupBy(o => dateTools.whatDayShiftIsit(o.boxingDate).shift);
                    foreach (var shiftEntry in grouppedByShift)
                    {
                        Dictionary<string, int> qtyPerType = new Dictionary<string, int>();
                        qtyPerType.Add("LinLED", 0);
                        qtyPerType.Add("RdLED", 0);
                        qtyPerType.Add("RecLED", 0);
                        qtyPerType.Add("Inne", 0);
                        qtyPerType.Add("Wszystkie", 0);

                        var grouppedByOrder = shiftEntry.GroupBy(o => o.kittingInfo.orderNo).ToDictionary(k => k.Key, v => v.ToList());
                        var componentMultiplier = 1;
                        Dictionary<string, DataTable> tagTablePerType = new Dictionary<string, DataTable>();
                        tagTablePerType.Add("Wszystkie", tagTableTemplate.Clone());

                        foreach (var orderEntry in grouppedByOrder)
                        {
                            if (!SharedComponents.Boxing.rbModules.Checked)
                            {
                                componentMultiplier = orderEntry.Value.First().kittingInfo.modelSpec.connectorCountPerModel + orderEntry.Value.First().kittingInfo.modelSpec.resistorCountPerModel + orderEntry.Value.First().kittingInfo.modelSpec.ledCountPerModel;
                            }
                            var currentType = orderEntry.Value.First().kittingInfo.modelSpec.type;
                            qtyPerType[currentType] += componentMultiplier * orderEntry.Value.Count();
                            qtyPerType["Wszystkie"] += componentMultiplier * orderEntry.Value.Count();

                            if (!infoPerTypePerOrderForTag.ContainsKey(currentType)) infoPerTypePerOrderForTag.Add(currentType, new Dictionary<string, List<MST.MES.OrderStructureByOrderNo.BoxingInfo>>());
                            if (!infoPerTypePerOrderForTag[currentType].ContainsKey(orderEntry.Key)) infoPerTypePerOrderForTag[currentType].Add(orderEntry.Key, new List<MST.MES.OrderStructureByOrderNo.BoxingInfo>());
                            if (!infoPerTypePerOrderForTag["Wszystkie"].ContainsKey(orderEntry.Key)) infoPerTypePerOrderForTag["Wszystkie"].Add(orderEntry.Key, new List<MST.MES.OrderStructureByOrderNo.BoxingInfo>());

                            if (!tagTablePerType.ContainsKey(currentType))
                            {
                                tagTablePerType.Add(currentType, tagTableTemplate.Clone());
                            }
                            tagTablePerType[currentType].Rows.Add(orderEntry.Value.OrderBy(p => p.boxingDate).First().boxingDate, orderEntry.Value.OrderByDescending(p => p.boxingDate).First().boxingDate, orderEntry.Value.First().kittingInfo.modelId_12NCFormat, orderEntry.Value.First().kittingInfo.ModelName, orderEntry.Key, DataContainer.sqlDataByOrder[orderEntry.Key].smt.totalManufacturedQty, orderEntry.Value.Count);
                            tagTablePerType["Wszystkie"].Rows.Add(orderEntry.Value.OrderBy(p => p.boxingDate).First().boxingDate, orderEntry.Value.OrderByDescending(p => p.boxingDate).First().boxingDate, orderEntry.Value.First().kittingInfo.modelId_12NCFormat, orderEntry.Value.First().kittingInfo.ModelName, orderEntry.Key, DataContainer.sqlDataByOrder[orderEntry.Key].smt.totalManufacturedQty, orderEntry.Value.Count);
                        }

                        grid.Rows.Add(dateTools.WeekNumber(dayEntry.Key),
                                    dayEntry.Key.ToShortDateString(),
                                    shiftEntry.Key,
                                    qtyPerType["Wszystkie"].ToString("#,0", nfi),
                                    qtyPerType["LinLED"].ToString("#,0", nfi),
                                    qtyPerType["RdLED"].ToString("#,0", nfi),
                                    qtyPerType["RecLED"].ToString("#,0", nfi),
                                    qtyPerType["Inne"].ToString("#,0", nfi));
                        foreach (var typeEntry in tagTablePerType)
                        {
                            grid.Rows[grid.Rows.Count - 1].Cells[typeEntry.Key].Tag = typeEntry.Value;
                        }

                    }
                }

                SMTOperations.autoSizeGridColumns(grid);
                if (grid.Rows.Count > 0)
                {
                    grid.FirstDisplayedScrollingRowIndex = grid.RowCount - 1;
                }

                dgvTools.MakeAlternatingRowsColors(grid, 1);
                grid.ResumeLayout();
            }
        }

        public static void FillOutBoxingTable(Dictionary<DateTime, SortedDictionary<int, Dictionary<string, DataTable>>> boxingData, DataGridView grid)
        {
            grid.Rows.Clear();
            grid.Columns.Clear();
            Color rowColor = Color.White;

            grid.Columns.Add("Data", "Data");
            grid.Columns.Add("Tydz", "tydz");
            grid.Columns.Add("Zmiana", "Zmiana");
            grid.Columns.Add("Ilosc", "Ilosc");

            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            foreach (var dateEntry in boxingData)
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

                    DataTable shiftTable = new DataTable();
                    shiftTable.Columns.Add("Data");
                    shiftTable.Columns.Add("Zmiana");
                    shiftTable.Columns.Add("Ilosc");
                    shiftTable.Columns.Add("Model");
                    double shiftQty = 0;
                    Dictionary<string, double> qtyPerModel = new Dictionary<string, double>();
                    foreach (var modelEntry in shiftEntry.Value)
                    {
                        if (!qtyPerModel.ContainsKey(modelEntry.Key))
                        {
                            qtyPerModel.Add(modelEntry.Key, 0);
                        }
                        qtyPerModel[modelEntry.Key] = modelEntry.Value.Rows.Count;
                    }

                    foreach (var modelEntry in qtyPerModel)
                    {
                        shiftTable.Rows.Add(dateEntry.Key, shiftEntry.Key, modelEntry.Value, modelEntry.Key);
                        shiftQty += modelEntry.Value;
                    }

                    grid.Rows.Add(date, week, shift, shiftQty);
                    foreach (DataGridViewCell cell in grid.Rows[grid.Rows.Count - 1].Cells)
                    {
                        cell.Style.BackColor = rowColor;
                        if (cell.ColumnIndex > 1)
                        {
                            string tester = cell.OwningColumn.Name;
                            cell.Tag = shiftTable;
                        }
                    }
                }
            }
            SMTOperations.autoSizeGridColumns(grid);
            grid.FirstDisplayedScrollingRowIndex = grid.RowCount - 1;
        }
    }
}