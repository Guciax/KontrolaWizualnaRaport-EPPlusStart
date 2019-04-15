using KontrolaWizualnaRaport.CentalDataStorage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.VisualStyles;
using static KontrolaWizualnaRaport.DgvImageButtonCell;
using static KontrolaWizualnaRaport.Form1;

namespace KontrolaWizualnaRaport
{
    class VIOperations
    {
        public static List<excelOperations.order12NC> mstOrders = new List<excelOperations.order12NC>();
        public static DataTable masterVITable = new DataTable();
        public static Dictionary<string, string> lotToSmtLine = new Dictionary<string, string>();

        private static void PrepareViTabComponents()
        {
            DataContainer.VisualInspection.finishedOrders = DataContainer.sqlDataByOrder.Where(o => o.Value.smt.ledsUsed > 0)
                                                                                        .OrderBy(o => o.Value.kitting.endDate)
                                                                                        .ToDictionary(k => k.Key, v => v.Value);
        }

        public static void RefreshViWasteLevelTab()
        {
            PrepareViTabComponents();
            
            var wasteReasons = DataContainer.VisualInspection.finishedOrders.SelectMany(o => o.Value.visualInspection.allReasons)
                                                                            .Select(r => r.Key)
                                                                            .Distinct()
                                                                            .Select(r=>r.Replace("ng","")
                                                                            .Replace("scrap",""))
                                                                            .Distinct();
            foreach (var reason in wasteReasons)
            {
                SharedComponents.VisualInspection.AnalizaPoPrzyczynie.cBListViReasonList.Items.Add(reason);
            }
            

            // group by date Key
            var grouppedByLineThenDate = new Dictionary<string, Dictionary<string, List<MST.MES.OrderStructureByOrderNo.OneOrderData>>>();
            foreach (var smtLine in GlobalParameters.allLinesByHand)
            {
                grouppedByLineThenDate.Add(smtLine, new Dictionary<string, List<MST.MES.OrderStructureByOrderNo.OneOrderData>>());
            }

            grouppedByLineThenDate.Add("Total", new Dictionary<string, List<MST.MES.OrderStructureByOrderNo.OneOrderData>>());

            foreach (var orderEntry in DataContainer.VisualInspection.finishedOrders)
            {
                DateTime orderDate = orderEntry.Value.kitting.endDate;
                if (orderDate.Year < 2017)
                    continue;
                string dateKey = "";
                if (SharedComponents.VisualInspection.PoziomOdpaduTab.radioButtonDaily.Checked)
                {
                    dateKey = orderDate.ToString("dd-MMM");
                }
                else if (SharedComponents.VisualInspection.PoziomOdpaduTab.radioButtonWeekly.Checked)
                {
                    dateKey = dateTools.WeekNumber(orderDate).ToString();
                }
                else
                {
                    dateKey = orderDate.ToString("MMM").ToUpper();
                }

                string lineKey = orderEntry.Value.smt.smtLinesInvolved.First();


                foreach (var lineEntry in grouppedByLineThenDate)
                {
                    if (!lineEntry.Value.ContainsKey(dateKey))
                    {
                        lineEntry.Value.Add(dateKey, new List<MST.MES.OrderStructureByOrderNo.OneOrderData>());
                        //Debug.WriteLine(dateKey);
                    }
                }

                grouppedByLineThenDate[lineKey][dateKey].Add(orderEntry.Value);
                grouppedByLineThenDate["Total"][dateKey].Add(orderEntry.Value);

            }

            var or = grouppedByLineThenDate.SelectMany(l => l.Value).SelectMany(d => d.Value)
                .Select(o => o.kitting.orderNo).ToList();

            var ord = grouppedByLineThenDate.SelectMany(l => l.Value).SelectMany(d => d.Value)
                .Select(o => o.kitting.orderNo).Distinct().ToList();

            DataContainer.VisualInspection.ngByLineThenDateKey = grouppedByLineThenDate;
            Charting.DrawWasteLevel();
            FillOutGridLatestLots();
        }


        public static void RefreshReworkChart(List<WasteDataStructure> inspectionData, Chart chartServiceVsNg, bool chartDaily, DataGridView dataGridViewServiceVsNg)
        {
            Rework.FillOutServiceVsNgGridAndDrawChart(inspectionData, chartServiceVsNg, chartDaily, dataGridViewServiceVsNg);
        }

        public static string[] Load12hOperatorsList()
        {
            string[] result = new string[0];
            if (System.IO.File.Exists("operatos12h.txt"))
            {
                result = System.IO.File.ReadAllLines("operatos12h.txt");

            }
            return result;
        }

        public static void Save12hOperators(DataGridView grid)
        {
            List<string> ops = new List<string>();
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.Cells["h/zmiane"].Value.ToString() == "12")
                {
                    ops.Add(row.Cells["Operator"].Value.ToString());
                }
            }
            try
            {
                System.IO.File.WriteAllLines("operatos12h.txt", ops.ToArray());
            }
            catch (Exception ex) { }
        }

        public static List<Image> TryGetFailureImagesForPcb(string lot, string serial, string date)
        {
            List<Image> result = new List<Image>();
            var path = Path.Combine(@"P:\Kontrola_Wzrokowa", date, lot);

            DirectoryInfo dirNfo = new DirectoryInfo(path);
            if (!dirNfo.Exists) return new List<Image>();
            var files = dirNfo.GetFiles();

            foreach (var file in files)
            {
                if (file.Name.Split('_')[0] == serial)
                {
                    result.Add(Image.FromFile(file.FullName));
                }
            }

            return result;
        }

        public static List<Image> TryGetFailureImagesForLot(string lot,  string date)
        {
            List<Image> result = new List<Image>();
            var path = Path.Combine(@"P:\Kontrola_Wzrokowa", date, lot);

            DirectoryInfo dirNfo = new DirectoryInfo(path);
            if (!dirNfo.Exists) return new List<Image>();
            var files = dirNfo.GetFiles();

            foreach (var file in files)
            {
                if (file.Extension != ".jpg") continue;
                Image img = Image.FromFile(file.FullName);
                img.Tag = file.Name;
                    result.Add(img);
            }

            return result;
        }

        public static List<FileInfo> TryGetFileInfoOfImagesForLot(string lot, string date)
        {
            List<FileInfo> result = new List<FileInfo>();
            var path = Path.Combine(@"P:\Kontrola_Wzrokowa", date, lot);

            DirectoryInfo dirNfo = new DirectoryInfo(path);
            if (!dirNfo.Exists) {
                Network.ConnectPDrive();
                if (!dirNfo.Exists) {
                    return new List<FileInfo>();
                }
            }
            var files = dirNfo.GetFiles();

            foreach (var file in files)
            {
                if (file.Extension != ".jpg") continue;
                //Image img = Image.FromFile(file.FullName);
                //img.Tag = file.Name;
                result.Add(file);
            }

            return result;
        }

        public static void FillOutGridLatestLots()
        {
            DataGridView grid = SharedComponents.VisualInspection.latestOrders.dataGridViewLatestLots;
            if (!System.IO.Directory.Exists(@"P:\"))
            {
                Network.ConnectPDrive();
            }
            grid.Columns.Clear();
            grid.Columns.Add("DataP", "Początek zlecenia");
            grid.Columns.Add("DataK", "Koniec zlecenia");
            grid.Columns.Add("Model", "Model ID");
            grid.Columns.Add("Model", "Model");
            grid.Columns.Add("Lot", "Nr zlecenia");
            grid.Columns.Add("Operator", "Operator");
            grid.Columns.Add("Linia", "Linia SMT");
            grid.Columns.Add("Linia", "Data SMT");
            grid.Columns.Add("Ilosc", "Ilosc SMT");
            grid.Columns.Add("z", "Rejestracja NG");
            grid.Columns.Add("NG", "NG");
            grid.Columns.Add("Scrap", "Scrap");
            DataGridViewImageButtonSaveColumn columnImage = new DataGridViewImageButtonSaveColumn();

            columnImage.Name = "Images";
            columnImage.HeaderText = "";
            grid.Columns.Insert(0,columnImage);
            var orderedOrders = DataContainer.sqlDataByOrder.//Where(o=>o.Value.smt.ledsUsed>0).
                                                             Where(o=>o.Value.visualInspection.ngScrapList.Count>0).
                                                             OrderByDescending(o => o.Value.visualInspection.ngScrapList.Select(i=>i.ngRegistrationDate).OrderByDescending(d=>d).First()).
                                                             Take(30);
            grid.SuspendLayout();
            foreach (var orderEntry in orderedOrders)
            {
                var fileImgList = TryGetFileInfoOfImagesForLot(orderEntry.Value.kitting.orderNo, orderEntry.Value.kitting.kittingDate.ToString("dd-MM-yyyy"));
                grid.Rows.Add(null,
                              orderEntry.Value.kitting.kittingDate,
                              orderEntry.Value.kitting.endDate,
                              orderEntry.Value.kitting.modelId_12NCFormat,
                              orderEntry.Value.kitting.ModelName,
                              orderEntry.Value.kitting.orderNo,
                              string.Join(", ",orderEntry.Value.visualInspection.ngScrapList.Select(i=>i.viOperator).Where(o=>o!="")),
                              string.Join(", ", orderEntry.Value.smt.smtLinesInvolved),
                              orderEntry.Value.smt.earliestStart +" - "+ orderEntry.Value.smt.latestEnd,
                              orderEntry.Value.smt.totalManufacturedQty,
                              string.Join(", ",orderEntry.Value.visualInspection.ngScrapList.Select(i=>i.ngRegistrationDate)),
                              orderEntry.Value.visualInspection.ngCount,
                              orderEntry.Value.visualInspection.scrapCount);

                if (fileImgList.Count > 0)
                {
                    ((DataGridViewImageButtonSaveCell)(grid.Rows[grid.Rows.GetLastRow(DataGridViewElementStates.None)].Cells["Images"])).Enabled = true;
                    ((DataGridViewImageButtonSaveCell)(grid.Rows[grid.Rows.GetLastRow(DataGridViewElementStates.None)].Cells["Images"])).ButtonState = PushButtonState.Normal;

                    //grid.Rows[grid.Rows.GetLastRow(DataGridViewElementStates.None)].Cells["Zdjecia"] = new DataGridViewButtonCell();
                    //grid.Rows[grid.Rows.GetLastRow(DataGridViewElementStates.None)].Cells["Zdjecia"] = new DataGridViewImageButtonCell();
                    grid.Rows[grid.Rows.GetLastRow(DataGridViewElementStates.None)].Cells["Images"].Tag = fileImgList;
                }
            }

            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (column.Name == "z")
                {
                    continue;
                }
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            }
            grid.Columns["z"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid.ResumeLayout();
        }
    }
}
