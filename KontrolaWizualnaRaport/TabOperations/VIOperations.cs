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

        public static void RefreshViWasteLevelTab()
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

            DataContainer.VisualInspection.wasteReasonsByLineThenDateKey = grouppedByLineThenDate;
            Charting.DrawWasteLevel();
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
