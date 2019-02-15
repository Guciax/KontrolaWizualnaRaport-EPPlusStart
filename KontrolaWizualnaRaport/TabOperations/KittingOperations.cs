using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport
{
    class KittingOperations
    {
        public static Dictionary<string, DataTable> TagForModelHistory(DataTable lotTable, ComboBox modelBox)
        {
            Dictionary<string, DataTable> modelTable = new Dictionary<string, DataTable>();
            foreach (DataRow row in lotTable.Rows)
            {
                string model = row["NC12_wyrobu"].ToString().Replace("LLFML","");
                if (!modelTable.ContainsKey(model))
                {
                    modelTable.Add(model, lotTable.Clone());
                }

                modelTable[model].Rows.Add(row.ItemArray);
            }

            modelBox.Items.Clear();
            modelBox.Items.AddRange(modelTable.Select(model => model.Key).OrderBy(m=>m).ToArray());
            return modelTable;
        }

        public static void FillGridWorkReport()
        {

            var filteredMstOrders = DataContainer.sqlDataByProcess.Kitting.Select(o => o.Value).Where(o => o.odredGroup == "MST" & SharedComponents.Kitting.checkBoxKittingMst.Checked);
            var filteredLgOrders = DataContainer.sqlDataByProcess.Kitting.Select(o => o.Value).Where(o => o.odredGroup == "LG" & SharedComponents.Kitting.checkBoxKittingLg.Checked);
            var joinedOrdered = filteredLgOrders.Union(filteredLgOrders).OrderBy(o => o.kittingDate);

            var groupByDay = joinedOrdered.GroupBy(o => dateTools.whatDayShiftIsit(o.kittingDate).fixedDate.Date).ToDictionary(x=>x.Key, v=>v.ToList());
            foreach (var dayEntry in groupByDay)
            {
                var groupByShift = dayEntry.Value.GroupBy(o=>dateTools.whatDayShiftIsit(o.kittingDate).shift).ToDictionary(x => x.Key, v => v.ToList());
                foreach (var shift in groupByShift)
                {
                    SharedComponents.Kitting.dataGridViewKitting.Rows.Add(dayEntry.Key, dateTools.GetIso8601WeekOfYear(dayEntry.Key), shift.Key, shift.Value.Count(), shift.Value.Select(o => o.orderedQty).Sum());
                }
                
            }

            SharedComponents.Kitting.dataGridViewKitting.FirstDisplayedScrollingRowIndex = SharedComponents.Kitting.dataGridViewKitting.RowCount - 1;
            SMTOperations.autoSizeGridColumns(SharedComponents.Kitting.dataGridViewKitting);
        }

        public static void FillGridReadyLots(DataGridView grid, DataTable lotTable, DataTable smtRecords)
        {
            Dictionary<string, Int32> qtyModulesPerModel = new Dictionary<string, int>();
            Dictionary<string, Int32> qtyLotsPerModel = new Dictionary<string, int>();
            Dictionary<string, DataTable> tagPerModel = new Dictionary<string, DataTable>();
            DataTable tagTemplate = new DataTable();
            tagTemplate.Columns.Add("Data_Poczatku_Zlecenia");
            tagTemplate.Columns.Add("DataCzasWydruku");
            tagTemplate.Columns.Add("LOT");
            tagTemplate.Columns.Add("NC12_wyrobu");
            tagTemplate.Columns.Add("Ilosc_wyrobu_zlecona");
            DateTime oldestLot = DateTime.Now;
            List<string> smtLots = new List<string>();
            foreach (DataRow row in smtRecords.Rows)
            {
                smtLots.Add(row["NrZlecenia"].ToString());
                DateTime date = DateTime.Parse(row["DataCzasKoniec"].ToString());
                if (date<oldestLot)
                {
                    oldestLot = date;
                }
            }


            foreach (DataRow row in lotTable.Rows)
            {
                string lot = row["Nr_Zlecenia_Produkcyjnego"].ToString();
                string endDate = row["Data_Konca_Zlecenia"].ToString();
                string startDate = row["Data_Poczatku_Zlecenia"].ToString();
                if (endDate != "") continue;
                string dateString = row["DataCzasWydruku"].ToString();
                if (dateString == "") continue;
                DateTime lotDate = DateTime.Parse(dateString);
                if (lotDate < oldestLot) continue;

                if (!smtLots.Contains(lot))
                {
                    string model = row["NC12_wyrobu"].ToString();
                    if (!qtyModulesPerModel.ContainsKey(model))
                    {
                        qtyModulesPerModel.Add(model, 0);
                        qtyLotsPerModel.Add(model, 0);
                        tagPerModel.Add(model, tagTemplate.Clone());
                    }
                    string qtyString = row["Ilosc_wyrobu_zlecona"].ToString();
                    qtyModulesPerModel[model] += Int32.Parse(qtyString);
                    qtyLotsPerModel[model]++;
                    tagPerModel[model].Rows.Add(startDate, dateString, lot, model, qtyString);
                }
            }

            grid.Columns.Clear();
            grid.Columns.Add("Model", "Model");
            grid.Columns.Add("Ilosc KITow", "Ilosc KITow");
            grid.Columns.Add("Ilosc wyrobow", "Ilosc wyrobow");

            foreach (var modelEntry in qtyLotsPerModel)
            {
                grid.Rows.Add(modelEntry.Key, modelEntry.Value, qtyModulesPerModel[modelEntry.Key]);
                foreach (DataGridViewCell cell in grid.Rows[grid.Rows.Count-1].Cells)
                {
                    cell.Tag = tagPerModel[modelEntry.Key];
                }
            }
            SMTOperations.autoSizeGridColumns(grid);
        }
    }
}
