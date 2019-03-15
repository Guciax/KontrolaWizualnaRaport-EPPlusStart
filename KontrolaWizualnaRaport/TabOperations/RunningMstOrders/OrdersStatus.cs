using KontrolaWizualnaRaport.TabOperations.RunningMstOrders;
using System;
using System.Collections.Generic;
using System.Data;
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
            var kittingData = MST.MES.SqlDataReaderMethods.Kitting.GetKittingDataForNotFinishedOrderes(MST.MES.SqlDataReaderMethods.Kitting.clientGroup.MST, 20);
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
            

            
            

            foreach (var order in orders)
            {
                
                if (order.Trim() == "") continue;
                grid.Rows.Add(order);
                int lastRow = grid.Rows.Count - 1;

                if (kittingData.ContainsKey(order))
                {
                    grid.Rows[lastRow].Cells["NC12"].Value = kittingData[order].modelId_12NCFormat;
                    grid.Rows[lastRow].Cells["Nazwa"].Value = kittingData[order].ModelName;
                    grid.Rows[lastRow].Cells["Poczatek"].Value = kittingData[order].kittingDate.ToString();
                    grid.Rows[lastRow].Cells["Ilosc"].Value = kittingData[order].orderedQty.ToString();
                }

                float smtProgress = 0;
                int smtCount = 0;
                if (smtData.ContainsKey(order))
                {
                    smtCount = smtData[order].totalManufacturedQty;
                    smtProgress = (float)smtCount / (float)kittingData[order].orderedQty;
                }
                ImageProgressBar.CreateProgressbar(smtProgress, smtCount, grid.Rows[lastRow].Cells["SMT"] as DataGridViewImageCell);

                if (viData.ContainsKey(order))
                {
                    grid.Rows[lastRow].Cells["NG"].Value = viData[order].ngCount.ToString();
                    grid.Rows[lastRow].Cells["SCR"].Value = viData[order].scrapCount.ToString();
                }

                float boxingProgress = 0;
                int boxedCount = 0;
                if (boxingData.ContainsKey(order))
                {
                    //grid.Rows[lastRow].Cells["Spakowane"].Value = boxingData[order].Count.ToString();
                    boxedCount = boxingData[order].Count();
                    grid.Rows[lastRow].Cells["Ilosckartonow"].Value = boxingData[order].Select(o=>o.boxId).Distinct().Count().ToString();
                    boxingProgress = (float)boxedCount / (float)kittingData[order].orderedQty;
                }
                ImageProgressBar.CreateProgressbar(boxingProgress, boxedCount, grid.Rows[lastRow].Cells["Spakowane"] as DataGridViewImageCell);
            }

            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (column.Name == "SMT") continue;
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }
    }
}
