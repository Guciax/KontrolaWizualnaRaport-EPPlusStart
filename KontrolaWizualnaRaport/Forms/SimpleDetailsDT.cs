using KontrolaWizualnaRaport.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport
{
    public partial class SimpleDetailsDT : Form
    {
        private readonly DataTable sourceTable;
        private readonly string title;
        private readonly int colIndexForColors;
        private readonly bool hyperlinkPcb;

        public SimpleDetailsDT(DataTable sourceTable, string title, int colIndexForColors, bool hyperlinkPcb)
        {
            InitializeComponent();
            this.sourceTable = sourceTable;
            this.title = title;
            this.colIndexForColors = colIndexForColors;
            this.hyperlinkPcb = hyperlinkPcb;
        }

        private void SimpleDetailsDT_Load(object sender, EventArgs e)
        {
            if (hyperlinkPcb & !Directory.Exists(@"P:\"))
            {
                Network.ConnectPDrive();
            }

            dataGridView1.DataSource = sourceTable;
            
            label1.Text = title;
            SMTOperations.autoSizeGridColumns(dataGridView1);

            MakeInterlacedColors();
            if (hyperlinkPcb) {
                MakeImageHyperlinks();
            }

            foreach (DataGridViewColumn col in dataGridView1.Columns) {
                col.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            }

            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            
        }

        

        private void MakeImageHyperlinks()
        {
            if (!System.IO.Directory.Exists(@"P:\"))
            {
                Network.ConnectPDrive();
            }
                //using (new Network.NetworkConnection(@"\\mstms005\Shared\", new System.Net.NetworkCredential("EPROD", "plfm!234","MST")))
                {
                    Dictionary<string, List<FileInfo>> listOfFilesInDict = new Dictionary<string, List<FileInfo>>();
                    dataGridView1.SuspendLayout();

                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        DateTime date = DateTime.ParseExact(row.Cells["Data"].Value.ToString(), "dd.MM.yyyy hh:mm", CultureInfo.InvariantCulture);
                        string serialNo = row.Cells["serialNo"].Value.ToString();
                        //var pcbDir = Path.Combine(@"P:\LED_Serwis", date.ToString("yyyy"), date.ToString("MMM"), date.ToString("dd"));
                        var pcbDir = Path.Combine(@"\\mstms005\Shared\LED_Serwis", date.ToString("yyyy"), date.ToString("MMM"), date.ToString("dd"));

                        List<FileInfo> fileList = new List<FileInfo>();



                    if (!listOfFilesInDict.TryGetValue(pcbDir, out fileList))
                    {
                        listOfFilesInDict.Add(pcbDir, new List<FileInfo>());
                        Debug.WriteLine("skan " + pcbDir);
                        if (System.IO.Directory.Exists(pcbDir))
                        {

                            DirectoryInfo dirNfo = new DirectoryInfo(pcbDir);
                            var files = dirNfo.GetFiles();
                            foreach (var file in files)
                            {

                                listOfFilesInDict[pcbDir].Add(file);

                            }
                        }

                    }


                        List<FileInfo> filesForThisSerial = new List<FileInfo>();

                        foreach (var file in listOfFilesInDict[pcbDir])
                        {
                            var splittedFilename = Path.GetFileNameWithoutExtension(file.Name).Split('_');
                            List<string> fractionsOfFileName = new List<string>();
                            for (int i = 0; i < splittedFilename.Length - 1; i++)
                            {
                                fractionsOfFileName.Add(splittedFilename[i]);
                            }

                            string serialFromFileName = string.Join("_", fractionsOfFileName);
                            if (serialFromFileName == serialNo)
                            {
                                filesForThisSerial.Add(file);
                            }
                        }

                        if (filesForThisSerial.Count > 0)
                        {
                            row.Cells["serialNo"].Style.ForeColor = Color.Blue;
                            //row.Cells["serialNo"].Style.Font = new Font(dataGridView1.Font, FontStyle.Underline);
                            row.Cells["serialNo"].Tag = filesForThisSerial;
                        }

                    }
                    dataGridView1.ResumeLayout();
                }

        }

        private void MakeInterlacedColors()
        {
            Color rowColor = Color.LightBlue;
            if (colIndexForColors >= 0) 
            {
                for (int r = 1; r < dataGridView1.Rows.Count; r++) 
                {
                    if (dataGridView1.Rows[r].Cells[colIndexForColors].Value.ToString() != dataGridView1.Rows[r - 1].Cells[colIndexForColors].Value.ToString())
                    {
                        if (rowColor == Color.LightBlue)
                        {
                            rowColor = Color.White;
                        }
                        else
                        {
                            rowColor = Color.LightBlue;
                        }
                    }

                    foreach (DataGridViewCell cell in dataGridView1.Rows[r].Cells)
                    {
                        cell.Style.BackColor = rowColor;
                        
                    }
                    
                    
                }
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 & e.ColumnIndex >= 0)
            {
                DataGridViewCell cell = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (cell.Style.ForeColor == Color.Blue)
                {
                    List<FileInfo> fileList = (List<FileInfo>)cell.Tag;
                    ShowImagesForm imgForm = new ShowImagesForm(fileList, cell.Value.ToString());
                    imgForm.ShowDialog();
                }
            }
        }
    }
}
