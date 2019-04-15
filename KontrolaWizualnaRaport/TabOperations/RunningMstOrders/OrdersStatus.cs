using KontrolaWizualnaRaport.TabOperations.RunningMstOrders;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static KontrolaWizualnaRaport.Form1;

namespace KontrolaWizualnaRaport.TabOperations
{
    public class OrdersStatus
    {
        public static void ShowStatusOfOrdersFromSql(CustomDataGridView grid)
        {
            var kittingData = MST.MES.SqlDataReaderMethods.Kitting.GetKittingDataForClientGroup(MST.MES.SqlDataReaderMethods.Kitting.clientGroup.MST, 20);
            string[] orders = kittingData.Select(o => o.Key).ToArray();
            var smtData = MST.MES.SqlDataReaderMethods.SMT.GetOrders(orders);
            var viData = MST.MES.SqlDataReaderMethods.VisualInspection.GetViRecordsForOrders(orders);
            var boxingData = MST.MES.SqlDataReaderMethods.Boxing.GetMstBoxingForOrders(orders);

            grid.Columns.Clear();
            grid.Columns.Add("Zlecenie", "Zlecenie");
            grid.Columns.Add("NC12", "12NC");
            grid.Columns.Add("Nazwa", "Nazwa");
            grid.Columns.Add("Poczatek", "Początek zlecenia");
            grid.Columns.Add("Ilosc", "Ilość zlecenia");
            DataGridViewImageColumn smtImgCol = new DataGridViewImageColumn();
            smtImgCol.Name = "SMT";
            smtImgCol.HeaderText = "SMT";
            smtImgCol.Width = 120;
            grid.Columns.Add(smtImgCol);

            DataGridViewImageColumn boxImgCol = new DataGridViewImageColumn();
            boxImgCol.Name = "Spakowane";
            boxImgCol.HeaderText = "Spakowane";
            boxImgCol.Width = 120;
            grid.Columns.Add(boxImgCol);

            //grid.Columns.Add("Spakowane", "Spakowane");
            grid.Columns.Add("Ilosckartonow", "Ilość kartonów");
            //grid.Columns.Add("SMT", "Ilość SMT");
            grid.Columns.Add("NG", "NG");
            grid.Columns.Add("SCR", "SCR");
            int firstFinishedRow = 0;
            foreach (var order in orders)
            {
                if (order.Trim() == "") continue;
                int currentRow = 0;

                if (kittingData[order].endDate > kittingData[order].kittingDate)
                {
                    grid.Rows.Insert(firstFinishedRow, order);
                    currentRow = firstFinishedRow;
                    dgvTools.SetRowColor(grid.Rows[currentRow], Color.LightGray, Color.Black);
                }
                else
                {
                    grid.Rows.Insert(0, order);
                    currentRow = 0;
                    firstFinishedRow++;
                }

                if (kittingData.ContainsKey(order))
                {
                    grid.Rows[currentRow].Cells["NC12"].Value = kittingData[order].modelId_12NCFormat;
                    grid.Rows[currentRow].Cells["Nazwa"].Value = kittingData[order].ModelName;
                    grid.Rows[currentRow].Cells["Poczatek"].Value = kittingData[order].kittingDate.ToString();
                    grid.Rows[currentRow].Cells["Ilosc"].Value = kittingData[order].orderedQty.ToString();
                }

                float smtProgress = 0;
                int smtCount = 0;
                if (smtData.ContainsKey(order))
                {
                    smtCount = smtData[order].totalManufacturedQty;
                    smtProgress = (float)smtCount / (float)kittingData[order].orderedQty;
                }
                ImageProgressBar.CreateProgressbar(smtProgress, smtCount, grid.Rows[currentRow].Cells["SMT"] as DataGridViewImageCell);

                if (viData.ContainsKey(order))
                {
                    grid.Rows[currentRow].Cells["NG"].Value = viData[order].ngCount.ToString();
                    grid.Rows[currentRow].Cells["SCR"].Value = viData[order].scrapCount.ToString();
                }

                float boxingProgress = 0;
                int boxedCount = 0;
                if (boxingData.ContainsKey(order))
                {
                    //grid.Rows[lastRow].Cells["Spakowane"].Value = boxingData[order].Count.ToString();
                    boxedCount = boxingData[order].Count();
                    grid.Rows[currentRow].Cells["Ilosckartonow"].Value = boxingData[order].Select(o => o.boxId).Distinct().Count().ToString();
                    boxingProgress = (float)boxedCount / (float)kittingData[order].orderedQty;
                }
                ImageProgressBar.CreateProgressbar(boxingProgress, boxedCount, grid.Rows[currentRow].Cells["Spakowane"] as DataGridViewImageCell);


            }

            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (column.Name == "SMT") continue;
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }
        public static void ShowStatusOfOrdersFromCache(CustomDataGridView grid)
        {
            //var kittingData = MST.MES.SqlDataReaderMethods.Kitting.GetKittingDataForClientGroup(MST.MES.SqlDataReaderMethods.Kitting.clientGroup.MST, 20);
            //var kittingData2 = DataContainer.sqlDataByOrder.Where(o => o.Value.kitting.odredGroup == "MST");
            //string[] orders = kittingData.Select(o => o.Key).ToArray();
            //var smtData = MST.MES.SqlDataReaderMethods.SMT.GetOrders(orders);
            //var viData = MST.MES.SqlDataReaderMethods.VisualInspection.GetViRecordsForOrders(orders);
            //var boxingData = MST.MES.SqlDataReaderMethods.Boxing.GetMstBoxingForOrders(orders);

            grid.Columns.Clear();
            grid.Columns.Add("Zlecenie", "Zlecenie");
            grid.Columns.Add("NC12", "12NC");
            grid.Columns.Add("Nazwa", "Nazwa");
            grid.Columns.Add("Poczatek", "Data wejścia");
            grid.Columns.Add("Koniec", "Data zakończenia");
            grid.Columns.Add("Ilosc", "Ilość zlecenia");
            DataGridViewImageColumn smtImgCol = new DataGridViewImageColumn();
            smtImgCol.Name = "SMT";
            smtImgCol.HeaderText = "SMT";
            smtImgCol.Width = 120;
            grid.Columns.Add(smtImgCol);

            DataGridViewImageColumn boxImgCol = new DataGridViewImageColumn();
            boxImgCol.Name = "Spakowane";
            boxImgCol.HeaderText = "Spakowane";
            boxImgCol.Width = 120;
            grid.Columns.Add(boxImgCol);

            //grid.Columns.Add("Spakowane", "Spakowane");
            grid.Columns.Add("Ilosckartonow", "Ilość kartonów");
            //grid.Columns.Add("SMT", "Ilość SMT");
            grid.Columns.Add("NG", "NG (naprawione)");
            grid.Columns.Add("SCR", "SCR");
            int firstFinishedRow = 0;
            foreach (var order in DataContainer.sqlDataByOrder)
            {
                
                int currentRow = 0;
                if (order.Value.kitting.odredGroup != "MST") continue;


                if (order.Value.kitting.endDate > order.Value.kitting.kittingDate)
                {
                    grid.Rows.Add(order.Key);
                    currentRow = grid.Rows.Count - 1;
                    dgvTools.SetRowColor(grid.Rows[grid.Rows.Count-1], Color.LightGray, Color.Black);
                }
                else
                {
                    grid.Rows.Insert(firstFinishedRow, order.Key);
                    currentRow = firstFinishedRow;
                    firstFinishedRow++;
                }


                    grid.Rows[currentRow].Cells["NC12"].Value = order.Value.kitting.modelId_12NCFormat;
                    grid.Rows[currentRow].Cells["Nazwa"].Value = order.Value.kitting.ModelName;
                    grid.Rows[currentRow].Cells["Poczatek"].Value = order.Value.kitting.kittingDate.ToString();
                grid.Rows[currentRow].Cells["Koniec"].Value = order.Value.kitting.endDate.Year > 1 ? order.Value.kitting.endDate.ToString() : "-";
                grid.Rows[currentRow].Cells["Ilosc"].Value = order.Value.kitting.orderedQty.ToString();

                float smtProgress = 0;
                int smtCount = 0;
                if (order.Value.smt.totalManufacturedQty > 0) 
                {
                    smtCount = order.Value.smt.totalManufacturedQty;
                    smtProgress = (float)smtCount / (float)order.Value.kitting.orderedQty;
                }
                ImageProgressBar.CreateProgressbar(smtProgress, smtCount, grid.Rows[currentRow].Cells["SMT"] as DataGridViewImageCell);

                string reworked = order.Value.visualInspection.reworkedOkCout > 0 ? $"({order.Value.visualInspection.reworkedOkCout.ToString()})" : "";

                    grid.Rows[currentRow].Cells["NG"].Value = $"{order.Value.visualInspection.ngCount}{reworked}";
                    grid.Rows[currentRow].Cells["SCR"].Value = order.Value.visualInspection.scrapCount.ToString();


                float boxingProgress = 0;
                int boxedCount = 0;

                    boxedCount = order.Value.ledsInBoxesList.Count();
                    grid.Rows[currentRow].Cells["Ilosckartonow"].Value = order.Value.ledsInBoxesList.Select(o => o.boxId).Distinct().Count().ToString();
                    boxingProgress = (float)boxedCount / (float)order.Value.kitting.orderedQty;

                ImageProgressBar.CreateProgressbar(boxingProgress, boxedCount, grid.Rows[currentRow].Cells["Spakowane"] as DataGridViewImageCell);

                if (grid.Rows.Count > 50) break;
            }

            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (column.Name == "SMT") continue;
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }
        
    }
}
