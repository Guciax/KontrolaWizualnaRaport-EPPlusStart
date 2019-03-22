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
        public static void ShowStatusOfOrders(CustomDataGridView grid)
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
                    grid.Rows.Insert(firstFinishedRow,order);
                    currentRow = firstFinishedRow;
                    dgvTools.SetRowColor(grid.Rows[currentRow], Color.LightGray, Color.Black);
                }
                else
                {
                    grid.Rows.Insert(0,order);
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
                    grid.Rows[currentRow].Cells["Ilosckartonow"].Value = boxingData[order].Select(o=>o.boxId).Distinct().Count().ToString();
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
    }
}
