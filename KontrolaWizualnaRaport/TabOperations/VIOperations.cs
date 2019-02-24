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

        public static void ReLoadViTab(
            ref Dictionary<string, string> lotModelDictionary,
            ref List<WasteDataStructure> inspectionData,
            ref List<excelOperations.order12NC> mstOrders,
            ComboBox comboBoxViOperatorsCapa,
            ComboBox comboBoxModel,
            ComboBox comboBoxViModelAnalFamily,
            ComboBox comboBoxViModelAnalModel,

            CheckedListBox checkedListBoxViWasteLevelSmtLines,
            CheckedListBox checkedListBoxViReasons,
            CheckedListBox cBListViReasonAnalysesSmtLines,
            CheckedListBox cBListViModelAnalysesSmtLines,
            CheckedListBox cBListViReasonList,

            DateTimePicker dateTimePickerPrzyczynyOdpaduOd,
            DateTimePicker dateTimePickerWasteLevelBegin,
            DataGridView dataGridViewDuplikaty,
            DataGridView dataGridViewPomylkiIlosc,
            DataGridView dataGridViewPowyzej50,
            DataGridView dataGridViewBledyNrZlec,
            DataGridView dataGridViewMstOrders,
            DataGridView dataGridViewViOperatorsTotal,
            DataGridView gridLatestLots,
            DateTimePicker dateTimePickerViOperatorEfiiciencyStart,
            DateTimePicker dateTimePickerViOperatorEfiiciencyEnd,
            NumericUpDown numericUpDownMoreThan50Scrap,
            NumericUpDown numericUpDownMoreThan50Ng,
            Dictionary<string, string> lotToOrderedQty,

            //rework
            DataGridView dataGridViewReworkDailyReport,
            DataGridView dataGridViewReworkByOperator,
            DataGridView dataGridViewServiceVsNg,
            Chart chartServiceVsNg,
            bool chartDaily
            )
        {
            DataContainer.VisualInspection.finishedOrders = DataContainer.sqlDataByOrder.Where(o => o.Value.smt.ledsUsed > 0).ToDictionary(x=>x.Key, v=>v.Value);

        }

        public static void RefreshViWasteLevelChart()
        {
            DataContainer.VisualInspection.finishedOrders = DataContainer.sqlDataByOrder.Where(o => o.Value.smt.ledsUsed > 0).OrderBy(o=>o.Value.kitting.endDate).ToDictionary(k => k.Key, v => v.Value);
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
                if (orderDate.Year < 2017) continue;
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
            
            DataContainer.VisualInspection.wasteReasonsByLineThenDateKey = grouppedByLineThenDate;
            Charting.DrawWasteLevel();
            FilloutWasteLevelGrid();
        }

        public static void FilloutWasteLevelGrid()
        {
            CustomDataGridView grid = SharedComponents.VisualInspection.PoziomOdpaduTab.dataGridViewWasteLevel;
            grid.SuspendLayout();
            grid.Rows.Clear();
            grid.Rows.Add("Data", "Prod.", "NG", "NG%");
            foreach (DataGridViewCell cell in grid.Rows[0].Cells)
            {
                cell.Style.BackColor = Color.Red;
                cell.Style.ForeColor = Color.White;
            }

            Dictionary<string, double> ngPerDateKey = new Dictionary<string, double>();
            Dictionary<string, double> scrapPerDateKey = new Dictionary<string, double>();
            Dictionary<string, double> prodPerDateKey = new Dictionary<string, double>();

            //foreach (var lineEntry in DataContainer.VisualInspection.wasteReasonsByLineThenDateKey)
            {
                foreach (var dateEntry in DataContainer.VisualInspection.wasteReasonsByLineThenDateKey["Total"])
                {

                    //var ngRepaired = dateEntry.Value.SelectMany(o => o.visualInspection.ngScrapList).Where(ng => ng.reworkOK == true).Count();
                    //double ngCount = dateEntry.Value.Select(o => o.visualInspection.ngCount).Sum();
                    //double totalProduction = dateEntry.Value.Select(o => o.smt.totalManufacturedQty).Sum();
                    //double scrapCount = dateEntry.Value.Select(o => o.visualInspection.scrapCount).Sum();
                    double ngCount = 0;
                    double totalProduction = 0;
                    double scrapCount = 0;

                    foreach (var order in dateEntry.Value)
                    {
                        if (!SharedComponents.VisualInspection.PoziomOdpaduTab.checkBoxViLevelLg.Checked)
                        {
                            if (order.kitting.odredGroup == "LG") continue;
                        }
                        if (!SharedComponents.VisualInspection.PoziomOdpaduTab.checkBoxViLevelMst.Checked)
                        {
                            if (order.kitting.odredGroup == "MST") continue;
                        }
                        if (!SharedComponents.VisualInspection.PoziomOdpaduTab.radioButtonViLinesCumulated.Checked)
                        {
                            if (!order.smt.smtLinesInvolved.Intersect(SharedComponents.VisualInspection.PoziomOdpaduTab.checkedListBoxViWasteLevelSmtLines.selectedLines).Any()) continue;
                        }

                        ngCount += order.visualInspection.ngCount;
                        totalProduction += order.smt.totalManufacturedQty;
                        scrapCount += order.visualInspection.scrapCount;
                    }

                    if (!ngPerDateKey.ContainsKey(dateEntry.Key)) ngPerDateKey.Add(dateEntry.Key, 0);
                    if (!scrapPerDateKey.ContainsKey(dateEntry.Key)) scrapPerDateKey.Add(dateEntry.Key, 0);
                    if (!prodPerDateKey.ContainsKey(dateEntry.Key)) prodPerDateKey.Add(dateEntry.Key, 0);

                    ngPerDateKey[dateEntry.Key] += ngCount;
                    scrapPerDateKey[dateEntry.Key] += scrapCount;
                    prodPerDateKey[dateEntry.Key] += totalProduction;
                }
            }

            foreach (var ng in ngPerDateKey)
            {
                grid.Rows.Add(ng.Key, prodPerDateKey[ng.Key], ngPerDateKey[ng.Key], Math.Round(ngPerDateKey[ng.Key] / prodPerDateKey[ng.Key] * 100, 2) + "%");
            }

            grid.Rows.Add();
            grid.Rows.Add("Data", "Prod.", "SCR", "SCR%");

            
            foreach (DataGridViewCell cell in grid.Rows[grid.Rows.Count-1].Cells)
            {
                cell.Style.BackColor = Color.Black;
                cell.Style.ForeColor = Color.White;
            }

            foreach (var scr in scrapPerDateKey)
            {
                grid.Rows.Add(scr.Key, prodPerDateKey[scr.Key], scrapPerDateKey[scr.Key], Math.Round(scrapPerDateKey[scr.Key] / prodPerDateKey[scr.Key] * 100, 2) + "%");
            }


            dgvTools.ColumnsAutoSize(grid, DataGridViewAutoSizeColumnMode.AllCellsExceptHeader);
            grid.ResumeLayout();
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
            if (!dirNfo.Exists) return new List<FileInfo>();
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

        public static void FillOutGridLatestLots(DataGridView grid, List<WasteDataStructure> inspectionData)
        {
            if (!System.IO.Directory.Exists(@"P:\"))
            {
                Network.ConnectPDrive();
            }
            grid.Columns.Clear();
            grid.Columns.Add("Data", "Data");
            grid.Columns.Add("Model", "Model");
            grid.Columns.Add("Lot", "Lot");
            grid.Columns.Add("Operator", "Operator");
            grid.Columns.Add("Linia", "Linia");
            grid.Columns.Add("IloscOK", "IloscOK");
            grid.Columns.Add("NG", "NG");
            grid.Columns.Add("Scrap", "Scrap");
            DataGridViewImageButtonSaveColumn columnImage = new DataGridViewImageButtonSaveColumn();

            columnImage.Name = "Images";
            columnImage.HeaderText = "";
            grid.Columns.Add(columnImage);
            //grid.Columns.Add("Zdjecia", "Zdjecia");
            foreach (var inspectionRecord in inspectionData.Skip(inspectionData.Count-40).Reverse())
            {
                var fileImgList = TryGetFileInfoOfImagesForLot(inspectionRecord.NumerZlecenia, inspectionRecord.RealDateTime.ToString("dd-MM-yyyy"));
                grid.Rows.Add(inspectionRecord.RealDateTime, inspectionRecord.Model, inspectionRecord.NumerZlecenia, inspectionRecord.Oper, inspectionRecord.SmtLine, inspectionRecord.GoodQty, inspectionRecord.AllNg, inspectionRecord.AllScrap);

                if (fileImgList.Count>0)
                {
                    ((DataGridViewImageButtonSaveCell)(grid.Rows[grid.Rows.GetLastRow(DataGridViewElementStates.None)].Cells["Images"])).Enabled = true;
                    ((DataGridViewImageButtonSaveCell)(grid.Rows[grid.Rows.GetLastRow(DataGridViewElementStates.None)].Cells["Images"])).ButtonState = PushButtonState.Normal;

                    //grid.Rows[grid.Rows.GetLastRow(DataGridViewElementStates.None)].Cells["Zdjecia"] = new DataGridViewButtonCell();
                    //grid.Rows[grid.Rows.GetLastRow(DataGridViewElementStates.None)].Cells["Zdjecia"] = new DataGridViewImageButtonCell();
                    grid.Rows[grid.Rows.GetLastRow(DataGridViewElementStates.None)].Cells["Images"].Tag = fileImgList;
                }
            }

            dgvTools.ColumnsAutoSize(grid, DataGridViewAutoSizeColumnMode.AllCells);
        }
    }
}
