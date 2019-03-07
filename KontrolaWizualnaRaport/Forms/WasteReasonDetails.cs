﻿using KontrolaWizualnaRaport.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static KontrolaWizualnaRaport.DgvImageButtonCell;

namespace KontrolaWizualnaRaport
{
    public partial class WasteReasonDetails : Form
    {
        private readonly List<MST.MES.OrderStructureByOrderNo.OneOrderData> inputWasteData;
        private readonly string reason;
        private Dictionary<string, List<FileInfo>> imagesPerLot = new Dictionary<string, List<FileInfo>>();

        public WasteReasonDetails(List<MST.MES.OrderStructureByOrderNo.OneOrderData> inputWasteData, string reason)
        {
            InitializeComponent();
            this.inputWasteData = inputWasteData;
            this.reason = reason;
        }

        private void WasteReasonDetails_Load(object sender, EventArgs e)
        {
            DataTable sourceTable = new DataTable();
            sourceTable.Columns.Add("Dobrych");
            sourceTable.Columns.Add("Ng");
            sourceTable.Columns.Add("LOT");
            sourceTable.Columns.Add("Model");
            sourceTable.Columns.Add("Data SMT");
            sourceTable.Columns.Add("LiniaSMT");
            //sourceTable.Columns.Add("Zdjecia");

            sourceTable.Columns["Dobrych"].DataType = typeof(Int32);

            Dictionary<string, Int32> qtyPerModel = new Dictionary<string, int>();
            Dictionary<string, Int32> qtyPerLine = new Dictionary<string, int>();

            foreach (var lot in inputWasteData.OrderByDescending(o=>o.smt.latestEnd))
            {
                string model = lot.kitting.modelId_12NCFormat;
                string line = string.Join(", ", lot.smt.smtLinesInvolved);

                if (!qtyPerLine.ContainsKey(line)) qtyPerLine.Add(line, 0);
                if (!qtyPerModel.ContainsKey(model)) qtyPerModel.Add(model, 0);

                Int32 qty = lot.visualInspection.allReasons[reason];
                qtyPerLine[line] += qty;
                qtyPerModel[model] += qty;

                var imageList = VIOperations.TryGetFileInfoOfImagesForLot(lot.kitting.orderNo, lot.kitting.kittingDate.ToString("dd-MM-yyyy"));
                if (imageList.Count > 0) 
                {
                    if (!imagesPerLot.ContainsKey(lot.kitting.orderNo))
                    {
                        imagesPerLot.Add(lot.kitting.orderNo, new List<FileInfo>());
                    }
                    imagesPerLot[lot.kitting.orderNo].AddRange(imageList);
                }
                
                sourceTable.Rows.Add(lot.smt.totalManufacturedQty - lot.visualInspection.ngCount - lot.visualInspection.scrapCount, qty, lot.kitting.orderNo, model, lot.smt.latestEnd, string.Join(", ", lot.smt.smtLinesInvolved) );
            }

            dataGridView1.DataSource = sourceTable;

            label1.Text = reason;

            DataTable modelSource = new DataTable();
            modelSource.Columns.Add("Model");
            modelSource.Columns.Add("Ilość", typeof (Int32));

            DataTable lineSource = new DataTable();
            lineSource.Columns.Add("Linia");
            lineSource.Columns.Add("Ilość", typeof(Int32));

            foreach (var modelEntry in qtyPerModel)
            {
                modelSource.Rows.Add(modelEntry.Key, modelEntry.Value);
            }

            foreach (var lineEntry in qtyPerLine)
            {
                lineSource.Rows.Add(lineEntry.Key, lineEntry.Value);
            }

            dataGridViewModel.DataSource = modelSource;
            dataGridViewLine.DataSource = lineSource;

            SMTOperations.autoSizeGridColumns(dataGridView1);
            SMTOperations.autoSizeGridColumns(dataGridViewLine);
            SMTOperations.autoSizeGridColumns(dataGridViewModel);

            dataGridViewLine.Sort(this.dataGridViewLine.Columns["Ilość"], ListSortDirection.Descending);
            dataGridViewModel.Sort(this.dataGridViewModel.Columns["Ilość"], ListSortDirection.Descending);
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (dataGridView1.Columns.Count < 8)
            {
                DataGridViewImageButtonSaveColumn columnImage =
                    new DataGridViewImageButtonSaveColumn();

                columnImage.Name = "Zdjecia";
                columnImage.HeaderText = "";
                dataGridView1.Columns.Add(columnImage);
            }
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                

                string lot = row.Cells["LOT"].Value.ToString();
                List<FileInfo> imgList = new List<FileInfo>();
                if (!imagesPerLot.TryGetValue(lot, out imgList)) continue;

                
                    ((DataGridViewImageButtonSaveCell)(row.Cells["Zdjecia"])).Enabled = true;
                ((DataGridViewImageButtonSaveCell)(row.Cells["Zdjecia"])).ButtonState = PushButtonState.Normal;
                row.Cells["Zdjecia"].Tag = imgList;

                //DataGridViewButtonCell butCell = new DataGridViewButtonCell();

                //butCell.Tag = imgList;
                //butCell.Style.BackColor = Color.Lime;


                //row.Cells["Zdjecia"] = butCell;
                //row.Cells["Zdjecia"].Value = "ZDJĘCIA";
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1 & e.ColumnIndex > -1)
            {
                DataGridViewCell cell = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (cell.OwningColumn.Name == "Zdjecia" & cell.Tag != null)
                {
                    string lot = dataGridView1.Rows[e.RowIndex].Cells["LOT"].Value.ToString();
                    List<FileInfo> imgList = (List<FileInfo>)cell.Tag;
                    ShowImagesForm imgForm = new ShowImagesForm(imgList, $"LOT: {lot}");
                    imgForm.ShowDialog();
                }
            }
        }
    }
}
