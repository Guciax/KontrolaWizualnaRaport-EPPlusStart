﻿using KontrolaWizualnaRaport.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static KontrolaWizualnaRaport.SMTOperations;

namespace KontrolaWizualnaRaport
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            sqloperations = new SQLoperations(this, textBox1);
            checkedListBoxViWasteLevelSmtLines.MouseEnter += checkedListBox1_MouseEnter;
            checkedListBoxViWasteLevelSmtLines.MouseLeave += checkedListBox1_MouseLeave;
            checkedListBoxViWasteLevelSmtLines.Size = new Size(120, 20);
            checkedListBoxViWasteLevelSmtLines.Parent = tabPage3;
            checkedListBoxViWasteLevelSmtLines.BringToFront();
            checkedListBoxViWasteLevelSmtLines.Location = new Point(965, 13);
            checkedListBoxViWasteLevelSmtLines.ForeColor = Color.Black;
            checkedListBoxViWasteLevelSmtLines.CheckOnClick = true;
            checkedListBoxViWasteLevelSmtLines.SelectedIndexChanged += checkedListBoxViWasteLevel_SelectedIndexChanged;
        }

        DataTable masterVITable = new DataTable();
        Dictionary<string, string> lotModelDictionary = new Dictionary<string, string>();
        Dictionary<string, string> planModelDictionary = new Dictionary<string, string>();
        Dictionary<string, string> lotToOrderedQty = new Dictionary<string, string>();
        public static List<WasteDataStructure> inspectionData = new List<WasteDataStructure>();
        List<excelOperations.order12NC> mstOrders = new List<excelOperations.order12NC>();
        private SQLoperations sqloperations;
        DataTable smtRecords = new DataTable();
        Dictionary<string, Dictionary<string, List<durationQuantity>>> smtModelLineQuantity = new Dictionary<string, Dictionary<string, List<durationQuantity>>>();
        DataTable lotTable = new DataTable();
        Dictionary<DateTime, SortedDictionary<int, Dictionary<string, Dictionary<string, DataTable>>>> testData = new Dictionary<DateTime, SortedDictionary<int, Dictionary<string, Dictionary<string, DataTable>>>>();
        Dictionary<DateTime, SortedDictionary<int, Dictionary<string, DataTable>>> boxingData = new Dictionary<DateTime, SortedDictionary<int, Dictionary<string, DataTable>>>();
        Dictionary<string, MesModels> mesModels = new Dictionary<string, MesModels>();
        //SortedDictionary<DateTime, SortedDictionary<int, List<LotLedWasteStruc>>> ledWasteDictionary = new SortedDictionary<DateTime, SortedDictionary<int, List<LotLedWasteStruc>>>();
        static Dictionary<string, Color> lineColors = new Dictionary<string, Color>();
        CustomCheckedListBox checkedListBoxViWasteLevelSmtLines = new CustomCheckedListBox();

        private void Form1_Load(object sender, EventArgs e)
        {
#if DEBUG
            ImageToByteArray.ImgToByteArray();
#endif
            dateTimePickerSmtStart.Value = DateTime.Now.Date.AddDays(-60);
            smtRecords = SQLoperations.GetSmtRecordsFromDb(dateTimePickerSmtStart.Value, dateTimePickerSmtEnd.Value);
            lotTable = SQLoperations.lotTable();
            Dictionary<string, string>[] lotList = VIOperations.lotArray(lotTable);
            lotModelDictionary = lotList[0];
            lotToOrderedQty = lotList[1];
            planModelDictionary = lotList[3];
            mesModels = SQLoperations.GetMesModels();
            dateTimePickerViOperatorEfiiciencyStart.Value = DateTime.Now.AddDays(-7);
            cBListViReasonAnalysesSmtLines.Parent = tabPage6;
            cBListViReasonList.Parent = tabPage6;
            cBListViReasonAnalysesSmtLines.BringToFront();
            cBListViReasonList.BringToFront();

            checkedListBoxViReasons.Parent = tabPage2;
            checkedListBoxViReasons.BringToFront();

            cBListViModelAnalysesSmtLines.Parent = tabPage7;
            cBListViModelAnalysesSmtLines.BringToFront();

            lineColors.Add("SMT1", Color.Maroon);
            lineColors.Add("SMT2", Color.OrangeRed);
            lineColors.Add("SMT3", Color.SandyBrown);
            lineColors.Add("SMT4", Color.Violet);
            lineColors.Add("SMT5", Color.LimeGreen);
            lineColors.Add("SMT6", Color.DarkGreen);
            lineColors.Add("SMT7", Color.SteelBlue);
            lineColors.Add("SMT8", Color.MediumTurquoise);
        }

        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabControl2.SelectedTab.Text)
            {
                case "SMT":
                    {
                        if (smtModelLineQuantity.Count < 1)
                        {
                            SMTOperations.ReLoadSmtTab(dataGridViewSmtProduction,
                                dataGridViewChangeOvers,
                                dataGridViewSmtLedDropped, dataGridViewSmtLedWasteByModel,
                                dataGridViewSmtWasteTotal,
                                dataGridViewSmtLedWasteTotalPerLine,
                                dataGridViewSmtStencilUsage,
                                smtRecords,
                                ref smtModelLineQuantity,
                                radioButtonSmtShowAllModels,
                                mesModels,
                                comboBoxSmtModels,
                                comboBoxSmtLedWasteLine,
                                comboBoxSmtLewWasteFreq,
                                comboBoxSmtLedWasteLines,
                                lineColors,
                                chartLedWasteChart,
                                panelSmtLedWasteCheckContainer);

                        }
                            break;
                    }
                case "KITTING":
                    {
                        if (dataGridViewKitting.Rows.Count == 0)
                        {
                            KittingOperations.FillGridWorkReport(lotTable, dataGridViewKitting);
                            KittingOperations.FillGridReadyLots(dataGridViewKittingReadyLots, lotTable, smtRecords);
                            comboBoxKittingModels.Tag = KittingOperations.TagForModelHistory(lotTable, comboBoxKittingModels);
                        }
                        break;
                    }
                case "BOXING":
                    {
                        if (dataGridViewBoxing.Rows.Count == 0)
                        {
                            loadDone = false;
                            PictureBox loadPB = new PictureBox();
                            Image loadImg = KontrolaWizualnaRaport.Properties.Resources.load;

                            loadPB.Size = loadImg.Size;
                            loadPB.Parent = dataGridViewBoxing;
                            loadPB.Location = new Point(0, 0);
                            loadPB.Image = loadImg;
                            timerBoxLoadDone.Enabled = true;
                            dataGridViewBoxing.Tag = loadPB;
                            new Thread(() =>
                            {
                                Thread.CurrentThread.IsBackground = true;
                                boxingData = SQLoperations.GetBoxing(20);
                                
                                loadDone = true;
                            }).Start();
                            
                        }
                        break;
                    }
                case "TEST":
                    {
                        if (dataGridViewTestProdReport.Rows.Count == 0)
                        {
                            loadDone = false;
                            PictureBox loadPB = new PictureBox();
                            Image loadImg = KontrolaWizualnaRaport.Properties.Resources.load;

                            loadPB.Size = loadImg.Size;
                            loadPB.Parent = dataGridViewTestProdReport;
                            loadPB.Location = new Point(0,0);
                            loadPB.Image = loadImg;
                            timerTestLoadDone.Enabled = true;
                            dataGridViewTestProdReport.Tag = loadPB;
                            new Thread(() => 
                            {
                                Thread.CurrentThread.IsBackground = true;
                                testData = SQLoperations.GetTestMeasurements(60);
                                loadDone = true;
                            }).Start();
                        }
                        break;
                    }
                case "SPLITTING":
                    {
                        if (dataGridViewSplitting.Rows.Count == 0)
                        {
                            SplittingOperations.FillGrid(lotTable, dataGridViewSplitting);
                        }
                        break;
                    }
                case "KONTROLA WZROKOWA":
                    {
                        if (inspectionData.Count < 1)
                        {
                            VIOperations.ReLoadViTab(
                                ref lotModelDictionary,
                                ref inspectionData,
                                ref mstOrders,
                                comboBoxViOperatorsCapa,
                                comboBoxModel,
                                comboBoxViModelAnalFamily,
                                comboBoxViModelAnalModel,

                                checkedListBoxViWasteLevelSmtLines,
                                checkedListBoxViReasons,
                                cBListViReasonAnalysesSmtLines,
                                cBListViModelAnalysesSmtLines,
                                cBListViReasonList,

                                dateTimePickerPrzyczynyOdpaduOd,
                                dateTimePickerWasteLevelBegin,
                                dataGridViewDuplikaty,
                                dataGridViewPomylkiIlosc,
                                dataGridViewPowyzej50,
                                dataGridViewBledyNrZlec,
                                dataGridViewMstOrders,
                                dataGridViewViOperatorsTotal,
                                dataGridViewLatestLots,
                                dateTimePickerViOperatorEfiiciencyStart,
                                dateTimePickerViOperatorEfiiciencyEnd,
                                numericUpDownMoreThan50Scrap,
                                numericUpDownMoreThan50Ng,
                                lotToOrderedQty, dataGridViewReworkDailyReport, 
                                dataGridViewReworkByOperator,
                                dgvServiceVsNg,
                                chartServiceVsNg,
                                radioButtonReworkVsNgDaily.Checked
                                );
                        }
                        break;
                    }
            }
        }

        bool loadDone = false;
        private void timerTestLoadDone_Tick(object sender, EventArgs e)
        {
            if (loadDone)
            {
                if (testData.Count > 0)
                {
                    TestOperations.FillOutTesterTable(testData, dataGridViewTestProdReport, lotModelDictionary);
                    timerTestLoadDone.Enabled = false;
                    PictureBox loadPB = (PictureBox)dataGridViewTestProdReport.Tag;
                    dataGridViewTestProdReport.Controls.Remove(loadPB);
                    var tag = TestOperations.InspectionTimeByMachineModel(testData, lotModelDictionary);
                    chartTestEfficiencyModel.Tag = tag;
                    comboBoxTestEfficiencyModels.Items.AddRange(tag.SelectMany(t => t.Value.Select(m => m.Key)).Distinct().ToArray());
                    TestOperations.FillOutGridWaitingForTest(dgvTestWaitingForTest, testData, smtRecords);

                }
            }
        }

        private void buttonSmtRefresh_Click(object sender, EventArgs e)
        {
            smtRecords = SQLoperations.GetSmtRecordsFromDb(dateTimePickerSmtStart.Value, dateTimePickerSmtEnd.Value);
            SMTOperations.ReLoadSmtTab(dataGridViewSmtProduction,
                                dataGridViewChangeOvers,
                                dataGridViewSmtLedDropped, dataGridViewSmtLedWasteByModel,
                                dataGridViewSmtWasteTotal,
                                dataGridViewSmtLedWasteTotalPerLine,
                                dataGridViewSmtStencilUsage,
                                smtRecords,
                                ref smtModelLineQuantity,
                                radioButtonSmtShowAllModels,
                                mesModels,
                                comboBoxSmtModels,
                                comboBoxSmtLedWasteLine,
                                comboBoxSmtLewWasteFreq,
                                comboBoxSmtLedWasteLines,
                                lineColors,
                                chartLedWasteChart,
                                panelSmtLedWasteCheckContainer);
        }

        

        private void timerBoxLoadDone_Tick(object sender, EventArgs e)
        {
            if (loadDone)
            {
                BoxingOperations.FillOutBoxingTable(boxingData, dataGridViewBoxing);
                timerBoxLoadDone.Enabled = false;
                PictureBox loadPB = (PictureBox)dataGridViewBoxing.Tag;
                dataGridViewBoxing.Controls.Remove(loadPB);
                BoxingOperations.FillOutBoxingLedQty(boxingData, mesModels, dataGridViewBoxingLedQty);
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            //tab przyczyny odpadu
            chartPrzyczynyOdpaduScrap.Height = tabPage2.Height / 2;

            //tab analiza po przyczynie
            chartReasonLevel.Height = tabPage6.Height / 2;
            chartReasonPareto.Width = tabPage6.Width / 2;

            //tab analiza po modelu
            chartModelLevel.Height = tabPage7.Height / 2;
            chartModelReasonsNg.Width = panel13.Width / 2;
        }

       private Dictionary<string, Color> PrepareSmtLinesColros()
        {
            Dictionary<string, Color> result = new Dictionary<string, Color>();
            foreach (object ctrl in panelSmtLedWasteCheckContainer.Controls)
            {
                if (ctrl is CheckBox)
                {
                    CheckBox cb = (CheckBox)ctrl;
                    result.Add(cb.Text.Trim(),cb.BackColor);
                }
            }
            return result;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabControl1.SelectedIndex)
            {
                case 0://tab wydajnosc
                    chartEfficiency.Width = this.Width - panel1.Width;
                    dataGridViewEffciency.Height = panel1.Height - comboBoxViOperatorsCapa.Height;
                    break;
                case 1://tab przyczyny odpadu
                    chartPrzyczynyOdpaduScrap.Height = tabPage2.Height / 2;
                    break;
                case 5://tab analiza po przyczynie
                    chartReasonLevel.Height = tabPage6.Height / 2;
                    chartReasonPareto.Width = tabPage6.Width / 2;
                    break;
                case 6://tab analiza po modelu
                    chartModelLevel.Height = tabPage7.Height / 2;
                    chartModelReasonsNg.Width = panel13.Width / 2;
                    break;
            }
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridViewEffciency.DataSource = Charting.DrawCapaChart(chartEfficiency, inspectionData, comboBoxViOperatorsCapa.Text, lotModelDictionary, radioButtonCapaLGI.Checked, mstOrders);
            foreach (DataGridViewColumn col in dataGridViewEffciency.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }

        private void dateTimePickerPrzyczynyOdpaduOd_ValueChanged(object sender, EventArgs e)
        {
            dataGridViewNgScrapReasons.DataSource = Charting.DrawWasteReasonsCHart(chartPrzyczynyOdpaduNg, chartPrzyczynyOdpaduScrap, inspectionData, dateTimePickerPrzyczynyOdpaduOd.Value, dateTimePickerPrzyczynyOdpaduDo.Value, lotModelDictionary, checkedListBoxViReasons.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray(),  radioButtonReasonsLg.Checked, mstOrders);
            dataGridViewNgScrapReasons.Columns[0].Width = 150;
            dataGridViewNgScrapReasons.Columns[1].Width = 35;
        }

        private void dateTimePickerPrzyczynyOdpaduDo_ValueChanged(object sender, EventArgs e)
        {
            dataGridViewNgScrapReasons.DataSource = Charting.DrawWasteReasonsCHart(chartPrzyczynyOdpaduNg, chartPrzyczynyOdpaduScrap, inspectionData, dateTimePickerPrzyczynyOdpaduOd.Value, dateTimePickerPrzyczynyOdpaduDo.Value, lotModelDictionary, checkedListBoxViReasons.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray(),  radioButtonReasonsLg.Checked, mstOrders);
            dataGridViewNgScrapReasons.Columns[0].Width = 150;
            dataGridViewNgScrapReasons.Columns[1].Width = 35;
        }

        private void dateTimePickerWasteLevelBegin_ValueChanged(object sender, EventArgs e)
        {
            dataGridViewWasteLevel.DataSource = Charting.DrawWasteLevel(VIOperations.chartFrequency(groupBoxFrequency), chartWasteLevel, inspectionData, dateTimePickerWasteLevelBegin.Value.Date, dateTimePickerWasteLevelEnd.Value.Date, lotModelDictionary, comboBoxModel, checkedListBoxViWasteLevelSmtLines.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray(), radioButtonViLinesCumulated.Checked,  radioButtonWasteLevelLG.Checked, mstOrders, lineColors, vScrollBar1.Value/vScrollBar1.Maximum);
            dgvTools.ColumnsAutoSize(dataGridViewWasteLevel, DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader);
        }

        

        private void copyChartToClipboard(Chart chrt)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                chrt.SaveImage(ms, ChartImageFormat.Bmp);
                Bitmap bm = new Bitmap(ms);
                Clipboard.SetImage(bm);
            }
        }

        private void radioButtonDaily_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void comboBoxModel_TextChanged(object sender, EventArgs e)
        {

        }

        private void dateTimePickerWasteLevelEnd_ValueChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            Charting.DrawWasteLevelPerModel(chartModelLevel, "all", inspectionData, lotModelDictionary, comboBoxViModelAnalFamily.Text, cBListViModelAnalysesSmtLines.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray());
            Charting.DrawWasteReasonsPerModel(chartModelReasonsNg, chartModelReasonsScrap, inspectionData, lotModelDictionary, comboBoxViModelAnalFamily.Text);
            comboBoxViModelAnalModel.Text = "";
        }


        string currentReasonOnChart = "";
        private void chartModelReasons_MouseMove(object sender, MouseEventArgs e)
        {
            var results = chartModelReasonsNg.HitTest(e.X, e.Y, false, ChartElementType.DataPoint);
            string model = comboBoxViModelAnalFamily.Text + comboBoxViModelAnalModel.Text;
            foreach (var result in results)
            {
                if (result.ChartElementType != ChartElementType.DataPoint)
                {
                    if (currentReasonOnChart != "all")
                    {
                        foreach (var pt in chartModelReasonsNg.Series[0].Points)
                        {
                            pt.BorderWidth = 0;
                        }


                        Charting.DrawWasteLevelPerModel(chartModelLevel, "all", inspectionData, lotModelDictionary, model, cBListViModelAnalysesSmtLines.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray());
                        currentReasonOnChart = "all";
                    }
                    continue;
                }

                var p = (DataPoint)(result.Object);
                if (currentReasonOnChart != p.AxisLabel)
                {
                    Charting.DrawWasteLevelPerModel(chartModelLevel, p.AxisLabel, inspectionData, lotModelDictionary, model, cBListViModelAnalysesSmtLines.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray());
                    currentReasonOnChart = p.AxisLabel;
                    Debug.WriteLine(p.AxisLabel);
                    p.BorderWidth = 4;
                    p.BorderColor = System.Drawing.Color.Red;
                }
                break;

            }
        }

        private void chartModelReasonsScrap_MouseMove(object sender, MouseEventArgs e)
        {
            var results = chartModelReasonsScrap.HitTest(e.X, e.Y, false, ChartElementType.DataPoint);


            foreach (var result in results)
            {
                if (result.ChartElementType != ChartElementType.DataPoint)
                {
                    if (currentReasonOnChart != "all")
                    {
                        foreach (var pt in chartModelReasonsScrap.Series[0].Points)
                        {
                            pt.BorderWidth = 0;
                        }

                        Charting.DrawWasteLevelPerModel(chartModelLevel, "all", inspectionData, lotModelDictionary, comboBoxViModelAnalFamily.Text, cBListViModelAnalysesSmtLines.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray());
                        currentReasonOnChart = "all";
                    }
                    continue;

                }

                var p = (DataPoint)(result.Object);
                if (currentReasonOnChart != p.AxisLabel)
                {
                    Charting.DrawWasteLevelPerModel(chartModelLevel, p.AxisLabel, inspectionData, lotModelDictionary, comboBoxViModelAnalFamily.Text, cBListViModelAnalysesSmtLines.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray());
                    currentReasonOnChart = p.AxisLabel;
                    Debug.WriteLine(p.AxisLabel);
                    p.BorderWidth = 4;
                    p.BorderColor = System.Drawing.Color.Red;
                }
                break;

            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            dataGridViewPowyzej50.DataSource = VIOperations.MoreThan50(numericUpDownMoreThan50Scrap, numericUpDownMoreThan50Ng, lotModelDictionary, inspectionData);
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            dataGridViewPowyzej50.DataSource = VIOperations.MoreThan50(numericUpDownMoreThan50Scrap, numericUpDownMoreThan50Ng, lotModelDictionary, inspectionData);
        }

        

        private void chartReasonsParetoPercentage_MouseMove(object sender, MouseEventArgs e)
        {
            var results = chartReasonsParetoPercentage.HitTest(e.X, e.Y, false, ChartElementType.DataPoint);


            foreach (var result in results)
            {
                if (result.ChartElementType != ChartElementType.DataPoint)
                {
                    if (currentReasonOnChart != "all")
                    {
                        foreach (var pt in chartReasonsParetoPercentage.Series[0].Points)
                        {
                            pt.BorderWidth = 0;
                        }

                        Charting.DrawWasteLevelPerReason(chartReasonLevel, "all", inspectionData, cBListViReasonList.CheckedItems.OfType<object>().Select(w => w.ToString()).ToArray(), lotModelDictionary, cBListViReasonAnalysesSmtLines.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray());
                        currentReasonOnChart = "all";
                    }
                    continue;

                }

                var p = (DataPoint)(result.Object);
                if (currentReasonOnChart != p.AxisLabel)
                {
                    Charting.DrawWasteLevelPerReason(chartReasonLevel, p.AxisLabel, inspectionData, cBListViReasonList.CheckedItems.OfType<object>().Select(w => w.ToString()).ToArray(), lotModelDictionary, cBListViReasonAnalysesSmtLines.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray());
                    currentReasonOnChart = p.AxisLabel;
                    //Debug.WriteLine(p.AxisLabel);
                    p.BorderWidth = 4;
                    p.BorderColor = System.Drawing.Color.Red;
                }
                break;

            }
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            Charting.DrawWasteLevelPerModel(chartModelLevel, "all", inspectionData, lotModelDictionary, comboBoxViModelAnalModel.Text, cBListViModelAnalysesSmtLines.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray());
            Charting.DrawWasteReasonsPerModel(chartModelReasonsNg, chartModelReasonsScrap, inspectionData, lotModelDictionary, comboBoxViModelAnalModel.Text);
            comboBoxViModelAnalFamily.Text = "";
        }

        private void chartPrzyczynyOdpaduNg_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void chartWasteLevel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                copyChartToClipboard(chartWasteLevel);
            }
        }

        private void chartEfficiency_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                copyChartToClipboard(chartEfficiency);
            }

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        
        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {

        }


        

        private void radioButtonCapaLGI_CheckedChanged(object sender, EventArgs e)
        {
            dataGridViewEffciency.DataSource = Charting.DrawCapaChart(chartEfficiency, inspectionData, comboBoxViOperatorsCapa.Text, lotModelDictionary, radioButtonCapaLGI.Checked, mstOrders);
            foreach (DataGridViewColumn col in dataGridViewEffciency.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            dgvTools.SumOfSelectedCells(dataGridViewSmtProduction, label1SmtSelectedSum);
        }

        

        private void CopyLabelTagToClipboard(Label lbl)
        {
            string lblText = (string)lbl.Tag;
            string bresultString = Regex.Match(lblText, @"\d+").Value;
            Clipboard.SetText(bresultString);
        }

        

        public static Bitmap chartToBitmap(Chart chrt)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                chrt.SaveImage(ms, ChartImageFormat.Bmp);
                Bitmap bm = new Bitmap(ms);
                return bm;
            }
        }
        private void contextMenuStripPrint_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "Drukuj")
            {
                //Drukuj printForm = new Drukuj(chartPrzyczynyOdpaduNg);
                //printForm.ShowDialog();

                System.IO.MemoryStream myStream = new System.IO.MemoryStream();
                Chart chartCopy = new Chart();
                chartPrzyczynyOdpaduNg.Serializer.Save(myStream);
                chartCopy.Serializer.Load(myStream);

                PrintDocument pd = new PrintDocument();
                pd.DefaultPageSettings.Landscape = true;

                chartCopy.Width = pd.DefaultPageSettings.PaperSize.Height;
                chartCopy.Height = pd.DefaultPageSettings.PaperSize.Width - 50;

                chartCopy.Tag = "Przyczyny odpadu okres: " + dateTimePickerPrzyczynyOdpaduOd.Value.ToShortDateString() + " - " + dateTimePickerPrzyczynyOdpaduDo.Value.ToShortDateString();
                pd.PrintPage += (sender2, args) => printing_PrintPage(chartCopy, args);

                PrintDialog printdlg = new PrintDialog();
                PrintPreviewDialog printPrvDlg = new PrintPreviewDialog();


                printPrvDlg.Document = pd;
                printPrvDlg.ShowDialog(); 

                printdlg.Document = pd;

                if (printdlg.ShowDialog() == DialogResult.OK)
                {
                    pd.Print();
                }

            }
        }

        private void printing_PrintPage(object sender, PrintPageEventArgs e)
        {
            Chart chart = sender as Chart;
            Single leftMargin = e.MarginBounds.Left;
            Single topMargin = e.MarginBounds.Top;
            Image img = Form1.chartToBitmap(chart);
            int textYPos = 20;
            int w = e.PageBounds.Width;
            int h = e.PageBounds.Height;
            string title = chart.Tag.ToString();

            using (Font printFont = new Font("Arial", 20.0f))
            {
                e.Graphics.DrawImage(img, new Point(5, 55));
                e.Graphics.DrawRectangle(new Pen(Color.Black, 2), new Rectangle(5, 5, 70, 70));
                e.Graphics.DrawRectangle(new Pen(Color.Black, 2), new Rectangle(5, 5, w-100, 70));
                e.Graphics.DrawRectangle(new Pen(Color.Black, 2), new Rectangle(5, 5, w-10, 70));
                e.Graphics.DrawString("MST", printFont, Brushes.Black, 6, textYPos, new StringFormat());
                e.Graphics.DrawString(title, printFont, Brushes.Black, 100, textYPos, new StringFormat());
            }

        }

        private void contextMenuStripPrintPoziomOdpadu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "Drukuj")
            {
                //Drukuj printForm = new Drukuj(chartPrzyczynyOdpaduNg);
                //printForm.ShowDialog();
                System.IO.MemoryStream myStream = new System.IO.MemoryStream();
                Chart chartCopy = new Chart();
                chartWasteLevel.Serializer.Save(myStream);
                chartCopy.Serializer.Load(myStream);
                chartCopy.Series.RemoveAt(2);
                PrintDocument pd = new PrintDocument();
                pd.DefaultPageSettings.Landscape = true;

                chartCopy.Width = pd.DefaultPageSettings.PaperSize.Height;
                chartCopy.Height = pd.DefaultPageSettings.PaperSize.Width - 50;

                chartCopy.Tag = "Tygodniowy poziom odpadu " + dateTimePickerWasteLevelBegin.Value.ToShortDateString() + " - " + dateTimePickerWasteLevelEnd.Value.ToShortDateString();
                pd.PrintPage += (sender2, args) => printing_PrintPage(chartCopy, args);

                PrintDialog printdlg = new PrintDialog();
                PrintPreviewDialog printPrvDlg = new PrintPreviewDialog();


                printPrvDlg.Document = pd;
                printPrvDlg.ShowDialog();

                printdlg.Document = pd;

                if (printdlg.ShowDialog() == DialogResult.OK)
                {
                    pd.Print();
                }

            }
        }

        private void comboBoxSmtModels_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowSmtStatistics();
        }

        private void radioButtonSmtPerHour_CheckedChanged(object sender, EventArgs e)
        {
            ShowSmtStatistics();
        }

        private void ShowSmtStatistics()
        {
            if (comboBoxSmtModels.Text != "")
            {
                dataGridViewSmtModelStats.DataSource = SMTOperations.MakeTableForModelEfficiency2(smtModelLineQuantity, comboBoxSmtModels.Text, radioButtonSmtPerHour.Checked);
                Charting.DrawSmtEfficiencyHistogramForModel(chartSmt, smtModelLineQuantity[comboBoxSmtModels.Text], radioButtonSmtPerHour.Checked);
            }
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            showJobDetails(e, dataGridViewSmtProduction, "SMT");
        }

        private void dataGridViewKitting_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            showJobDetails(e, dataGridViewKitting, "Kitting");
        }

        private void showJobDetails(DataGridViewCellEventArgs e, DataGridView grid, string station)
        {
            if (e.RowIndex > -1 & e.ColumnIndex > -1)
            {
                string description = "";
                string[] dateColumns = new string[] { "mies", "tydz", "dzien", "zmiana", "data" };
                DataGridViewCell cell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (cell.Value != null)
                {
                    if (cell.Value.ToString() != "0")
                    {
                        if (!dateColumns.Contains(cell.OwningColumn.HeaderText.ToLower()))
                        {
                            DataTable dt = (DataTable)cell.Tag;
                            if (dt.Columns.Contains("NC12_wyrobu"))
                                dt.Columns["NC12_wyrobu"].ColumnName = "model";
                            if (dt.Columns.Contains("Ilosc_wyrobu_zlecona"))
                                dt.Columns["Ilosc_wyrobu_zlecona"].ColumnName = "Ilosc";
                            if (dt.Columns.Contains("IloscWykonana"))
                                dt.Columns["IloscWykonana"].ColumnName = "Ilosc";


                            if (cell.OwningColumn.Name.StartsWith("SMT"))
                            {
                                description = cell.OwningColumn.Name + " " + dataGridViewSmtProduction.Rows[e.RowIndex].Cells[0].Value.ToString() + " Zm." + dataGridViewSmtProduction.Rows[e.RowIndex].Cells[1].Value.ToString();
                            }
                            else
                            {
                                description = station + " " + grid.Rows[e.RowIndex].Cells[0].Value.ToString() + " Zm." + grid.Rows[e.RowIndex].Cells[1].Value.ToString();
                            }
                            SmtShiftDetails detailsForm = new SmtShiftDetails(dt, description);
                            detailsForm.Show();
                        }
                        else
                        {
                            DataGridViewCell[] dayCells = dgvTools.GetCellsForSameKeyValue(cell, grid);
                            Dictionary<string, double> quantityPerModel = new Dictionary<string, double>();
                            DataTable combinedTable = new DataTable();
                            foreach (var c in dayCells)
                            {
                                DataTable table = (DataTable)c.Tag;
                                if (table.Columns.Contains("NC12_wyrobu"))
                                    table.Columns["NC12_wyrobu"].ColumnName = "model";
                                if (table.Columns.Contains("Ilosc_wyrobu_zlecona"))
                                    table.Columns["Ilosc_wyrobu_zlecona"].ColumnName = "Ilosc";
                                if (table.Columns.Contains("IloscWykonana"))
                                    table.Columns["IloscWykonana"].ColumnName = "Ilosc";
                                if (combinedTable.Columns.Count == 0)
                                {
                                    combinedTable = table.Clone();
                                }

                                foreach (DataRow row in table.Rows)
                                {
                                    combinedTable.Rows.Add(row.ItemArray);
                                }
                                description = cell.OwningColumn.HeaderText + " " + cell.Value.ToString();
                            }




                            SmtShiftDetails detailsForm = new SmtShiftDetails(combinedTable, description);
                            detailsForm.Show();
                        }
                    }
                }
            }
        }

        private void dataGridViewSplitting_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            showJobDetails(e, dataGridViewSplitting, "Splitting");
        }

        private void dataGridViewKitting_SelectionChanged(object sender, EventArgs e)
        {
            dgvTools.SumOfSelectedCells(dataGridViewKitting, labelKittingSelectedSum);
        }

        private void dataGridViewSplitting_SelectionChanged(object sender, EventArgs e)
        {
            dgvTools.SumOfSelectedCells(dataGridViewSplitting, labelSplittingSelectedSum);
        }

        private void buttonShowOneLot_Click(object sender, EventArgs e)
        {
            DataTable oneLotDt = smtRecords.Clone();

            foreach (DataRow row in smtRecords.Rows)
            {
                if (row["NrZlecenia"].ToString() == textBoxSmtLot.Text)
                {
                    oneLotDt.Rows.Add(row.ItemArray);
                    break;
                }
            }

            if (oneLotDt.Rows.Count > 0)
            {
                oneLotDt.Columns["IloscWykonana"].ColumnName = "Ilosc";

                SmtShiftDetails detailsForm = new SmtShiftDetails(oneLotDt, "LOT: " + textBoxSmtLot.Text);
                detailsForm.ShowDialog();
            }
            else
            { MessageBox.Show("Brak zlecenia " + textBoxSmtLot.Text + " w bazie danych"); }
        }

        private void dataGridViewTest_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            showJobDetails(e, dataGridViewTestProdReport, "TEST");
        }

        private void dataGridViewTest_SelectionChanged(object sender, EventArgs e)
        {
            dgvTools.SumOfSelectedCells(dataGridViewTestProdReport, labelTest);
        }

        private void dataGridViewBoxing_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            showJobDetails(e, dataGridViewBoxing, "BOXING");
        }

        private void dataGridViewBoxing_SelectionChanged(object sender, EventArgs e)
        {
            dgvTools.SumOfSelectedCells(dataGridViewBoxing, labelBoxing);
        }

        

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
           
        }

        private void radioButtonSmtShowAllModels_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxSmtModels.Items.Clear();
            comboBoxSmtModels.Items.AddRange(smtModelLineQuantity.Select(m => m.Key).OrderBy(m => m).ToArray());
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButtonReasonsLg_CheckedChanged(object sender, EventArgs e)
        {
            dataGridViewNgScrapReasons.DataSource = Charting.DrawWasteReasonsCHart(chartPrzyczynyOdpaduNg, chartPrzyczynyOdpaduScrap, inspectionData, dateTimePickerPrzyczynyOdpaduOd.Value, dateTimePickerPrzyczynyOdpaduDo.Value, lotModelDictionary, checkedListBoxViReasons.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray(), radioButtonReasonsLg.Checked, mstOrders);
        }

        private void radioButtonWasteLevelLG_CheckedChanged(object sender, EventArgs e)
        {
            dataGridViewWasteLevel.DataSource = Charting.DrawWasteLevel(VIOperations.chartFrequency(groupBoxFrequency), chartWasteLevel, inspectionData, dateTimePickerWasteLevelBegin.Value.Date, dateTimePickerWasteLevelEnd.Value.Date, lotModelDictionary, comboBoxModel, checkedListBoxViWasteLevelSmtLines.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray(), radioButtonViLinesCumulated.Checked, radioButtonWasteLevelLG.Checked, mstOrders, lineColors, (double)vScrollBar1.Value / (double)vScrollBar1.Maximum);
        }

        private void dataGridViewChangeOvers_SelectionChanged(object sender, EventArgs e)
        {
            dgvTools.SumOfSelectedCells(dataGridViewChangeOvers, labelSumOfSelectedChangeOver);
        }

        private void dataGridViewBoxingLedQty_SelectionChanged(object sender, EventArgs e)
        {
            dgvTools.SumOfSelectedCells(dataGridViewBoxingLedQty, labelBoxingLedQty); 
        }

        private void chartPrzyczynyOdpaduNg_DoubleClick(object sender, EventArgs e)
        {
            
        }

        private void chartReasonPareto_MouseMove(object sender, MouseEventArgs e)
        {
            var results = chartReasonPareto.HitTest(e.X, e.Y, false, ChartElementType.DataPoint);


            foreach (var result in results)
            {
                if (result.ChartElementType != ChartElementType.DataPoint)
                {
                    if (currentReasonOnChart != "all")
                    {
                        foreach (var pt in chartReasonPareto.Series[0].Points)
                        {
                            pt.BorderWidth = 0;
                        }

                        Charting.DrawWasteLevelPerReason(chartReasonLevel, "all", inspectionData, cBListViReasonList.CheckedItems.OfType<object>().Select(w => w.ToString()).ToArray(), lotModelDictionary, cBListViReasonAnalysesSmtLines.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray());
                        currentReasonOnChart = "all";
                    }
                    continue;

                }

                var p = (DataPoint)(result.Object);
                if (currentReasonOnChart != p.AxisLabel)
                {
                    Charting.DrawWasteLevelPerReason(chartReasonLevel, p.AxisLabel, inspectionData, cBListViReasonList.CheckedItems.OfType<object>().Select(w => w.ToString()).ToArray(), lotModelDictionary, cBListViReasonAnalysesSmtLines.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray());
                    currentReasonOnChart = p.AxisLabel;
                    //Debug.WriteLine(p.AxisLabel);
                    p.BorderWidth = 4;
                    p.BorderColor = System.Drawing.Color.Red;
                }
                break;

            }
        }

        private void chartPrzyczynyOdpaduNg_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            var results = chartPrzyczynyOdpaduNg.HitTest(e.X, e.Y, false, ChartElementType.DataPoint);

            foreach (var result in results)
            {
                //Debug.WriteLine(result.ChartElementType.ToString());
                if (result.ChartElementType == ChartElementType.DataPoint)
                {

                    DataPoint pt = (DataPoint)result.Object;
                    WasteReasonDetails detailForm = new WasteReasonDetails((WastePerReasonStructure)pt.Tag, pt.AxisLabel);
                    detailForm.Show();
                    break;
                }
            }
        }

        private void chartPrzyczynyOdpaduScrap_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var results = chartPrzyczynyOdpaduScrap.HitTest(e.X, e.Y, false, ChartElementType.DataPoint);

            foreach (var result in results)
            {
                Debug.WriteLine(result.ChartElementType.ToString());
                if (result.ChartElementType == ChartElementType.DataPoint)
                {

                    DataPoint pt = (DataPoint)result.Object;
                    WasteReasonDetails detailForm = new WasteReasonDetails((WastePerReasonStructure)pt.Tag, pt.AxisLabel);
                    detailForm.Show();
                    break;
                }
            }
        }

        private void comboBoxSmtLewWasteFreq_SelectedIndexChanged(object sender, EventArgs e)
        {
            Dictionary<string, bool> lineOptions = new Dictionary<string, bool>();
            foreach (Control c in panelSmtLedWasteCheckContainer.Controls)
            {
                if ((c is CheckBox))
                {
                    lineOptions.Add(c.Text.Trim(), ((CheckBox)c).Checked);
                }
            }
            Charting.DrawLedWasteChart(ledWasteDictionary, chartLedWasteChart, comboBoxSmtLewWasteFreq.Text, lineOptions, lineColors);
        }

        private void checkBoxSmt1_CheckStateChanged(object sender, EventArgs e)
        {
            Dictionary<string, bool> lineOptions = new Dictionary<string, bool>();
            foreach (Control c in panelSmtLedWasteCheckContainer.Controls)
            {
                if ((c is CheckBox))
                {
                    lineOptions.Add(c.Text.Trim(), ((CheckBox)c).Checked);
                }
            }
            Charting.DrawLedWasteChart(ledWasteDictionary, chartLedWasteChart, comboBoxSmtLewWasteFreq.Text, lineOptions, lineColors);
        }

        private void comboBoxSmtLedWasteLine_SelectedIndexChanged(object sender, EventArgs e)
        {
            SMTOperations.FillOutLedWasteByModel(ledWasteDictionary, dataGridViewSmtLedWasteByModel, comboBoxSmtLedWasteLine.Text);
        }

        private void dataGridViewSmtLedWasteByModel_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCell cell = dataGridViewSmtLedWasteByModel.Rows[e.RowIndex].Cells[e.ColumnIndex];
            if (cell.Tag != null)
            {
                DataTable tagTable = (DataTable)cell.Tag;
                LedWasteDetails detailsForm = new LedWasteDetails(tagTable, dataGridViewSmtLedWasteByModel.Rows[e.RowIndex].Cells["Model"].Value.ToString());
                detailsForm.ShowDialog();
            }
        }

        private void comboBoxSmtLedWasteLines_SelectedIndexChanged(object sender, EventArgs e)
        {
            SMTOperations.FillOutLedWasteTotalByLine(ledWasteDictionary, dataGridViewSmtLedWasteTotalPerLine, comboBoxSmtLedWasteLines.Text);
        }

        private void dataGridViewSmtLedWasteTotalPerLine_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCell cell = dataGridViewSmtLedWasteTotalPerLine.Rows[e.RowIndex].Cells[e.ColumnIndex];
            if (cell.Tag != null)
            {
                DataTable tagTable = (DataTable)cell.Tag;
                LedWasteDetails detailsForm = new LedWasteDetails(tagTable, "");
                detailsForm.Show();
            }
        }

        private void labelKittingSelectedSum_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                CopyLabelTagToClipboard(labelKittingSelectedSum);
            }
        }

        private void label1SmtSelectedSum_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                
                contextMenuCopySumOfSelected.Items[0].Tag = label1SmtSelectedSum.Tag;
                //CopyLabelTagToClipboard(label1SmtSelectedSum);
            }
        }

        private void labelSumOfSelectedChangeOver_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                CopyLabelTagToClipboard(labelSumOfSelectedChangeOver);
            }
        }

        private void labelTest_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                CopyLabelTagToClipboard(labelTest);
            }
        }

        private void labelSplittingSelectedSum_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                CopyLabelTagToClipboard(labelSplittingSelectedSum);
            }
        }

        private void labelBoxing_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                CopyLabelTagToClipboard(labelBoxing);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //dataGridViewSmtEditLot.Rows.Clear();
            DataTable oneLotDt = SQLoperations.GetSmtRecordsForLot(textBoxSmtEditLot.Text);

            
            if (oneLotDt.Rows.Count > 0)
            {
                List<string> rowValues = new List<string>();
                for (int c = 0; c < oneLotDt.Columns.Count; c++)
                {
                    rowValues.Add(oneLotDt.Rows[0][c].ToString());
                }
                dataGridViewSmtEditLot.Tag = rowValues.ToArray();
                dataGridViewSmtEditLot.DataSource = oneLotDt;

                SMTOperations.autoSizeGridColumns(dataGridViewSmtEditLot);
                dataGridViewSmtEditLot.Rows[0].Cells["NrZlecenia"].Style.BackColor = Color.Silver;
                dataGridViewSmtEditLot.Rows[0].Cells["NrZlecenia"].ReadOnly = true;
                foreach (DataGridViewColumn col in dataGridViewSmtEditLot.Columns)
                {
                    col.SortMode = DataGridViewColumnSortMode.NotSortable;
                }
            }
        }

        private void dataGridViewSmtEditLot_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            string[] oryginalValues = (string[])dataGridViewSmtEditLot.Tag;

            DataGridViewCell changedCell = dataGridViewSmtEditLot.Rows[e.RowIndex].Cells[e.ColumnIndex];
            string oryginalValue = oryginalValues[e.ColumnIndex];
            bool cellsHasBeenChanged = false;
            for(int c=0; c< dataGridViewSmtEditLot.Columns.Count;c++) 
            {
                if (dataGridViewSmtEditLot.Rows[0].Cells[c].Value.ToString() != oryginalValues[c])
                {
                    dataGridViewSmtEditLot.Rows[0].Cells[c].Style.BackColor = Color.Orange;
                    cellsHasBeenChanged = true;
                }
                else
                {
                    dataGridViewSmtEditLot.Rows[0].Cells[c].Style.BackColor = Color.White;
                }
                dataGridViewSmtEditLot.Rows[0].Cells[c].Selected = false;
            }

            if(cellsHasBeenChanged)
            {
                buttonSmtEditLotSaveChanges.BackColor = Color.Orange;
            }
            else
            {
                buttonSmtEditLotSaveChanges.BackColor = Color.LightGray;
            }
        }

        private void buttonSmtEditLotRestore_Click(object sender, EventArgs e)
        {
            string[] oryginalValues = (string[])dataGridViewSmtEditLot.Tag;
            if (oryginalValues!=null)
            {
                for (int c = 0; c < dataGridViewSmtEditLot.Columns.Count; c++)
                {
                    dataGridViewSmtEditLot.Rows[0].Cells[c].Value = oryginalValues[c];

                }
            }
        }

        private void buttonSmtEditLotSaveChanges_Click(object sender, EventArgs e)
        {
            if (dataGridViewSmtEditLot.Rows.Count == 1)
            {
                List<Tuple<string, string>> colValuePairList = new List<Tuple<string, string>>();
                foreach (DataGridViewCell cell in dataGridViewSmtEditLot.Rows[0].Cells)
                {
                    if (cell.Style.BackColor == Color.Orange)
                    {
                        Tuple<string, string> newPair = new Tuple<string, string>(cell.OwningColumn.Name, cell.Value.ToString());
                        colValuePairList.Add(newPair);
                    }
                }
                if (SQLoperations.UpdateSmtRecord(colValuePairList , dataGridViewSmtEditLot.Rows[0].Cells["NrZlecenia"].Value.ToString()))
                {
                    foreach (DataGridViewRow row in dataGridViewSmtEditLot.Rows)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            cell.Style.BackColor = Color.White;
                        }
                    }
                    buttonSmtEditLotSaveChanges.BackColor = Color.LightGray;
                    buttonSmtEditLoadLot.PerformClick();
                }
            }
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode== Keys.Return)
            {
                string lot = textBox3.Text;
                LotSummary.FillOutKitting(dataGridViewSummaryKitting, lot);
                LotSummary.FillOutSmtSummary(dataGridViewSummarySmt, lot);
                LotSummary.FillOutViSummary(dataGridViewSummaryVi, lot);
                List<string> pcbOK = LotSummary.FillOutTestummaryReturnPcbOK(dataGridViewSummaryTest, lot);
                LotSummary.FilloutBoxingSummary(dataGridViewSummaryBox, pcbOK);
            }
        }

        private void dataGridViewSummaryTest_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewSummaryTest.Rows)
            {
                if ((row.Cells[0].Value.ToString().ToUpper().StartsWith("NG") || row.Cells[0].Value.ToString().ToUpper().StartsWith("SCRAP")) & row.Cells[1].Value.ToString() != "0")
                {
                    row.Cells[1].Style.BackColor = Color.Red;
                    row.Cells[1].Style.ForeColor = Color.White;
                }
            }
            Debug.WriteLine("test bind compl");
        }

        private void dataGridViewSummaryVi_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewSummaryVi.Rows)
            {
                if ((row.Cells[0].Value.ToString().ToUpper().StartsWith("NG") || row.Cells[0].Value.ToString().ToUpper().StartsWith("SCRAP")) & row.Cells[1].Value.ToString() != "0")
                {
                    row.Cells[1].Style.BackColor = Color.Red;
                    row.Cells[1].Style.ForeColor = Color.White;
                }
            }
            Debug.WriteLine("vi bind compl");
        }

        private void dateTimePickerViOperatorEfiiciencyStart_ValueChanged(object sender, EventArgs e)
        {
            VIOperations.ngRatePerOperator(inspectionData, dateTimePickerViOperatorEfiiciencyStart.Value, dateTimePickerViOperatorEfiiciencyEnd.Value, dataGridViewViOperatorsTotal);
            
        }

        private void dateTimePickerViOperatorEfiiciencyEnd_ValueChanged(object sender, EventArgs e)
        {
            VIOperations.ngRatePerOperator(inspectionData, dateTimePickerViOperatorEfiiciencyStart.Value, dateTimePickerViOperatorEfiiciencyEnd.Value, dataGridViewViOperatorsTotal);
        }

        private void dataGridViewViOperatorsTotal_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewViOperatorsTotal.Rows)
            {
                if (row.Cells["h / zmiane"].Value.ToString()=="12")
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cell.Style.BackColor = Color.LightBlue;
                    }
                }
            }
        }

        private void checkedListBox1_MouseEnter(object sender, EventArgs e)
        {
            checkedListBoxViWasteLevelSmtLines.Height = 120;
        }

        private void checkedListBox1_MouseLeave(object sender, EventArgs e)
        {
            checkedListBoxViWasteLevelSmtLines.Height = 20;
        }
        
        private void checkedListBoxViWasteLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            string[] smtLines = checkedListBoxViWasteLevelSmtLines.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray();

            if (smtLines.Length > 0)
            {
                dataGridViewWasteLevel.DataSource = Charting.DrawWasteLevel(VIOperations.chartFrequency(groupBoxFrequency), chartWasteLevel, inspectionData, dateTimePickerWasteLevelBegin.Value.Date, dateTimePickerWasteLevelEnd.Value.Date, lotModelDictionary, comboBoxModel, smtLines, radioButtonViLinesCumulated.Checked, radioButtonWasteLevelLG.Checked, mstOrders, lineColors, checkBoxyAxis.Checked ? 1-(double)vScrollBar1.Value / (double)vScrollBar1.Maximum : 0);
                foreach (DataGridViewColumn col in dataGridViewWasteLevel.Columns)
                {
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
            }
            else
            {
                chartWasteLevel.Series.Clear();
            }
        }

        private void checkedListBoxViReasons_MouseEnter(object sender, EventArgs e)
        {
            checkedListBoxViReasons.Height = 120;
        }

        private void checkedListBoxViReasons_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridViewNgScrapReasons.DataSource = Charting.DrawWasteReasonsCHart(chartPrzyczynyOdpaduNg, chartPrzyczynyOdpaduScrap, inspectionData, dateTimePickerPrzyczynyOdpaduOd.Value, dateTimePickerPrzyczynyOdpaduDo.Value, lotModelDictionary, checkedListBoxViReasons.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray(), radioButtonReasonsLg.Checked, mstOrders);
        }

        private void checkedListBoxViReasons_MouseLeave(object sender, EventArgs e)
        {
            checkedListBoxViReasons.Height = 20;
        }

        private void dataGridViewMstOrders_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (dataGridViewMstOrders.Rows.Count > 0)
                { 
            foreach (DataGridViewRow row in dataGridViewMstOrders.Rows)
                {
                    if (row.Cells["Kontrola wzrokowa"].Value.ToString() == "NIE")
                    {
                        row.Cells["Kontrola wzrokowa"].Style.BackColor = Color.Red;
                        row.Cells["Kontrola wzrokowa"].Style.ForeColor = Color.White;
                    }
                }
                dataGridViewMstOrders.FirstDisplayedCell = dataGridViewMstOrders.Rows[dataGridViewMstOrders.Rows.Count - 1].Cells[0];
            }
        }

        private void cBListViReasonAnalysesSmtLines_MouseEnter(object sender, EventArgs e)
        {
            cBListViReasonAnalysesSmtLines.Height = 120;
        }

        private void cBListViReasonAnalysesSmtLines_MouseLeave(object sender, EventArgs e)
        {
            cBListViReasonAnalysesSmtLines.Height = 20;
        }

        private void cBListViReasonList_MouseEnter(object sender, EventArgs e)
        {
            cBListViReasonList.Height = 230;
        }

        private void cBListViReasonList_MouseLeave(object sender, EventArgs e)
        {
            cBListViReasonList.Height = 20;
        }

        private void cBListViReasonAnalysesSmtLines_SelectedIndexChanged(object sender, EventArgs e)
        {
            Charting.DrawWasteLevelPerReason(chartReasonLevel, "all", inspectionData, cBListViReasonList.CheckedItems.OfType<object>().Select(w => w.ToString()).ToArray(), lotModelDictionary, cBListViReasonAnalysesSmtLines.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray());
            Charting.DrawWasteParetoPerReason(chartReasonPareto, chartReasonsParetoPercentage, inspectionData, cBListViReasonList.CheckedItems.OfType<object>().Select(w => w.ToString()).ToArray(), lotModelDictionary, cBListViReasonAnalysesSmtLines.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray());
        }

        private void cBListViReasonList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Charting.DrawWasteLevelPerReason(chartReasonLevel, "all", inspectionData, cBListViReasonList.CheckedItems.OfType<object>().Select(w => w.ToString()).ToArray(), lotModelDictionary, cBListViReasonAnalysesSmtLines.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray());
            Charting.DrawWasteParetoPerReason(chartReasonPareto, chartReasonsParetoPercentage, inspectionData, cBListViReasonList.CheckedItems.OfType<object>().Select(w => w.ToString()).ToArray(), lotModelDictionary, cBListViReasonAnalysesSmtLines.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray());
        }

        private void dataGridViewViOperatorsTotal_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex==3)
            {
                double h = double.Parse( dataGridViewViOperatorsTotal.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
                double dayAvg  = double.Parse(dataGridViewViOperatorsTotal.Rows[e.RowIndex].Cells[2].Value.ToString());
                dataGridViewViOperatorsTotal.Rows[e.RowIndex].Cells[4].Value = Math.Round(dayAvg / h, 0);
                Color rowClr = Color.LightBlue;
                if (h==8)
                {
                    rowClr = Color.White ;
                }

                foreach (DataGridViewCell cell in dataGridViewViOperatorsTotal.Rows[e.RowIndex].Cells)
                {
                    cell.Style.BackColor = rowClr;
                }
            }
        }

        private void dataGridViewViOperatorsTotal_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridViewViOperatorsTotal_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewViOperatorsTotal.SelectedCells.Count > 0)
            {
                int rowInd = dataGridViewViOperatorsTotal.SelectedCells[0].RowIndex;
                if (rowInd >= 0)
                {
                    if (dataGridViewViOperatorsTotal.Rows[rowInd].Cells[0].Tag != null)
                    {
                        DataTable tagTable = (DataTable)dataGridViewViOperatorsTotal.Rows[rowInd].Cells[0].Tag;
                        
                        dataGridViewViOperatorDetails.DataSource = tagTable;
                        if (tagTable.Rows.Count > 0)
                        {
                            dataGridViewViOperatorDetails.FirstDisplayedCell = dataGridViewViOperatorDetails.Rows[dataGridViewViOperatorDetails.Rows.Count - 1].Cells[0];
                            VIOperations.Save12hOperators(dataGridViewViOperatorsTotal);
                        }
                    }
                }
            }
        }

        private void dataGridViewViOperatorDetails_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            Color rowCol = Color.LightBlue;

            foreach (DataGridViewRow row in dataGridViewViOperatorDetails.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Style.BackColor = rowCol;
                }

                if (row.Cells["Data"].Value != null)
                {
                    if (row.Cells["Data"].Value.ToString() != "")
                    {
                        if (rowCol == Color.LightBlue)
                        {
                            rowCol = Color.White;
                        }
                        else
                        {
                            rowCol = Color.LightBlue;
                        }
                    }
                }
            }

            SMTOperations.autoSizeGridColumns(dataGridViewViOperatorDetails);
            Charting.DrawNgChartForOneOperator((DataTable)dataGridViewViOperatorDetails.DataSource, chartNgRateForOneOperator);

        }

        private void dataGridViewKittingReadyLots_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex > -1 & e.RowIndex > -1)
            {
                DataGridViewCell cell = dataGridViewKittingReadyLots.Rows[e.RowIndex].Cells[e.ColumnIndex];
                DataTable sourceTable = (DataTable)cell.Tag;
                string title = dataGridViewKittingReadyLots.Rows[e.RowIndex].Cells["Model"].Value.ToString();
                SimpleDetailsDT dtForm = new SimpleDetailsDT(sourceTable, title, -1, false);
                dtForm.Show();
            }
        }

        private void dataGridViewReworkDailyReport_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void cBListViModelAnalysesSmtLines_MouseEnter(object sender, EventArgs e)
        {
            cBListViModelAnalysesSmtLines.Height = 120;
        }

        private void cBListViModelAnalysesSmtLines_MouseLeave(object sender, EventArgs e)
        {
            cBListViModelAnalysesSmtLines.Height = 20;
        }

        private void cBListViModelAnalysesSmtLines_SelectedIndexChanged(object sender, EventArgs e)
        {
            Charting.DrawWasteLevelPerModel(chartModelLevel, "all", inspectionData, lotModelDictionary, comboBoxViModelAnalModel.Text, cBListViModelAnalysesSmtLines.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray());
            Charting.DrawWasteReasonsPerModel(chartModelReasonsNg, chartModelReasonsScrap, inspectionData, lotModelDictionary, comboBoxViModelAnalModel.Text);
        }

        private void dataGridViewReworkDailyReport_SelectionChanged(object sender, EventArgs e)
        {
            
        }

        private void comboBoxTestEfficiencyModels_SelectedIndexChanged(object sender, EventArgs e)
        {
            Charting.DrawTestEfficiencyHistogramForModel(chartTestEfficiencyModel, dataGridViewTestModelEfficiency, false, comboBoxTestEfficiencyFamily.Text + comboBoxTestEfficiencyModels.Text);
        }

        private void contextMenuCopySumOfSelected_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "Kopiuj") 
            {

                    Clipboard.SetText((string)contextMenuCopySumOfSelected.Tag);
            }
        }

        private void contextMenuCopySumOfSelected_Opening(object sender, CancelEventArgs e)
        {
            contextMenuCopySumOfSelected.Tag = (string)contextMenuCopySumOfSelected.SourceControl.Tag;
        }

        private void comboBoxKittingModels_SelectedIndexChanged(object sender, EventArgs e)
        {
            Dictionary<string, DataTable> kittingTable = (Dictionary<string, DataTable>)comboBoxKittingModels.Tag;
            if (kittingTable.ContainsKey(comboBoxKittingModels.Text))
            {
                dataGridViewKittingHistory.DataSource = kittingTable[comboBoxKittingModels.Text];
                dgvTools.ColumnsAutoSize(dataGridViewKittingHistory, DataGridViewAutoSizeColumnMode.AllCellsExceptHeader);
            }
        }

        private void radioButtonWeekly_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButtonDaily_CheckedChanged_1(object sender, EventArgs e)
        {
            string[] smtLines = checkedListBoxViWasteLevelSmtLines.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray();

            if (smtLines.Length > 0)
            {
                dataGridViewWasteLevel.DataSource = Charting.DrawWasteLevel(VIOperations.chartFrequency(groupBoxFrequency), chartWasteLevel, inspectionData, dateTimePickerWasteLevelBegin.Value.Date, dateTimePickerWasteLevelEnd.Value.Date, lotModelDictionary, comboBoxModel, smtLines, radioButtonViLinesCumulated.Checked, radioButtonWasteLevelLG.Checked, mstOrders, lineColors, checkBoxyAxis.Checked ? 1-(double)vScrollBar1.Value / (double)vScrollBar1.Maximum : 0);
                foreach (DataGridViewColumn col in dataGridViewWasteLevel.Columns)
                {
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
            }
            else
            {
                chartWasteLevel.Series.Clear();
            }
        }

        private void dataGridViewSmtProduction_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            
        }

        public class CustomCheckedListBox : CheckedListBox
        {
            protected override void OnDrawItem(DrawItemEventArgs e)
            {
                base.OnDrawItem(e);

                if (Items.Count <= 0)
                    return;
                string name = Convert.ToString(Items[e.Index]);
                Color rowColor = Color.White;
                lineColors.TryGetValue(name, out rowColor);
                SolidBrush rowBrush = new SolidBrush(rowColor);

                var contentRect = e.Bounds;
                contentRect.X = 16;
                e.Graphics.FillRectangle(rowBrush, contentRect);
                e.Graphics.DrawString(Convert.ToString(Items[e.Index]), e.Font, Brushes.White, contentRect);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            string[] smtLines = checkedListBoxViWasteLevelSmtLines.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray();
            dataGridViewWasteLevel.DataSource = Charting.DrawWasteLevel(VIOperations.chartFrequency(groupBoxFrequency), chartWasteLevel, inspectionData, dateTimePickerWasteLevelBegin.Value.Date, dateTimePickerWasteLevelEnd.Value.Date, lotModelDictionary, comboBoxModel, smtLines, radioButtonViLinesCumulated.Checked, radioButtonWasteLevelLG.Checked, mstOrders, lineColors, checkBoxyAxis.Checked ?1- (double)vScrollBar1.Value / (double)vScrollBar1.Maximum : 0);
        }

        private void buttonSmtTraceOK_Click(object sender, EventArgs e)
        {
            SMTOperations.FillOutGridTraceability(textBoxSmtTraceLedId.Lines, dataGridViewSmtTrace);
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            string[] smtLines = checkedListBoxViWasteLevelSmtLines.CheckedItems.OfType<object>().Select(li => li.ToString()).ToArray();
            dataGridViewWasteLevel.DataSource = Charting.DrawWasteLevel(VIOperations.chartFrequency(groupBoxFrequency), chartWasteLevel, inspectionData, dateTimePickerWasteLevelBegin.Value.Date, dateTimePickerWasteLevelEnd.Value.Date, lotModelDictionary, comboBoxModel, smtLines, radioButtonViLinesCumulated.Checked, radioButtonWasteLevelLG.Checked, mstOrders, lineColors, checkBoxyAxis.Checked ? 1- (double)vScrollBar1.Value / (double)vScrollBar1.Maximum : 0);
        }

        private void dataGridViewReworkDailyReport_SelectionChanged_1(object sender, EventArgs e)
        {
            dgvTools.SumOfSelectedCells(dataGridViewReworkDailyReport, labelReworkProdReportSumOfSelected);
        }

        private void dataGridViewReworkDailyReport_CellDoubleClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex > -1 & e.RowIndex > -1)
            {
                DataGridViewCell cell = dataGridViewReworkDailyReport.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (cell.Tag != null)
                {
                    DataTable sourceTable = (DataTable)cell.Tag;
                    SimpleDetailsDT detailForm = new SimpleDetailsDT(sourceTable, "", 4, true);
                    detailForm.Show();
                }
            }
        }

        private void radioButtonReworkVsNgDaily_CheckedChanged(object sender, EventArgs e)
        {
            VIOperations.RefreshReworkChart(inspectionData, chartServiceVsNg, radioButtonReworkVsNgDaily.Checked, dgvServiceVsNg);
        }

        private void dataGridViewLatestLots_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1 & e.ColumnIndex > -1)
            {
                DataGridViewCell cell = dataGridViewLatestLots.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (cell.Tag != null)
                {
                    List<FileInfo> imgFilesList = (List<FileInfo>)cell.Tag;
                    ShowImagesForm imgForm = new ShowImagesForm(imgFilesList, "");
                    imgForm.ShowDialog();
                }
            }
        }

        private void dataGridViewSummaryVi_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex > -1 & e.RowIndex > -1) 
            {
                DataGridViewCell cell = dataGridViewSummaryVi.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (cell.Tag!=null)
                {
                    List<FileInfo> imgFilesList = (List<FileInfo>)cell.Tag;
                    ShowImagesForm imgForm = new ShowImagesForm(imgFilesList, "");
                    imgForm.ShowDialog();
                }
            }
        }
    }
}
