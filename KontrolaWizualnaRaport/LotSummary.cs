using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static KontrolaWizualnaRaport.DgvImageButtonCell;

namespace KontrolaWizualnaRaport
{
    class LotSummary
    {
        public static void FillOutGrid(DataGridView grid, DataTable table, bool attachImages)
        {
            grid.Columns.Clear();
            grid.Columns.Add("Nazwa", "Nazwa");

            for (int r = 0; r < table.Rows.Count; r++) 
            {
                grid.Columns.Add("Wartosc" + r, "Wartosc" + r);

                for (int c = 0; c < table.Columns.Count; c++)
                {
                    grid.Rows.Add(table.Columns[c].ColumnName, table.Rows[r][c]);
                }
                if (attachImages)
                {
                    DataGridViewImageButtonSaveColumn columnImage = new DataGridViewImageButtonSaveColumn();

                    columnImage.Name = "Images";
                    columnImage.HeaderText = "";
                    grid.Columns.Add(columnImage);

                    string date = table.Rows[r]["Data_czas"].ToString().Substring(0, 10).Replace(".", "-");
                    string lot = table.Rows[r]["numerZlecenia"].ToString();
                    var fileImgList = VIOperations.TryGetFileInfoOfImagesForLot(lot, date);

                    if (fileImgList.Count > 0)
                    {
                        foreach (DataGridViewRow row in grid.Rows)
                        {
                            string rowFailureName = row.Cells[0].Value.ToString();
                            List<System.IO.FileInfo> tagList = new List<System.IO.FileInfo>();
                            foreach (var img in fileImgList)
                            {
                                string failureName = img.Name.Split('_')[1];

                                
                                if (failureName == rowFailureName)
                                {
                                    tagList.Add(img);
                                    
                                }
                            }
                            if (tagList.Count>0)
                            {
                                ((DataGridViewImageButtonSaveCell)(row.Cells[r + 2])).Enabled = true;
                                ((DataGridViewImageButtonSaveCell)(row.Cells[r + 2])).ButtonState = PushButtonState.Normal;

                                row.Cells[r + 2].Tag = tagList;
                            }
                        }

                    }
                }
            }



            SMTOperations.autoSizeGridColumns(grid);
        }

        public static void FillOutKitting(DataGridView grid, string lot)
        {
            DataTable kitTable = SQLoperations.GetKittingInfoForLot(lot);
            FillOutGrid(grid, kitTable,false);
        }

        public static void FillOutSmtSummary(DataGridView grid, string lot)
        {
            DataTable smtTable = SQLoperations.GetSmtRecordsForLot(lot);
            FillOutGrid(grid, smtTable, false);
        }

        public static void FillOutViSummary(DataGridView grid, string lot)
        {
            DataTable viTable = SQLoperations.GetVisInspForLotL(lot);
            FillOutGrid(grid, viTable, true);
        }

        public static void FilloutBoxingSummary(DataGridView grid, List<string> pcbs)
        {
            if (pcbs.Count > 0)
            {
                Dictionary<string, List<string>> boxes = SQLoperations.GetBoxingInfo(pcbs);

                grid.Columns.Clear();
                grid.Columns.Add("Pole", "");
                grid.Columns.Add("Wartość", "");

                foreach (var boxEntry in boxes)
                {
                    grid.Rows.Add(boxEntry.Key, boxEntry.Value.Count + "szt.");
                }

                SMTOperations.autoSizeGridColumns(grid);
            }
        }

        public static List<string> FillOutTestummaryReturnPcbOK(DataGridView grid, string lot)
        {
            Dictionary<string, Dictionary<string, DataTable>> testTable = SQLoperations.GetTestMeasurementsForLot(lot);
            DataTable table = new DataTable();
            table.Columns.Add("Start");
            table.Columns.Add("End");
            table.Columns.Add("Tester");
            table.Columns.Add("Tested");
            table.Columns.Add("OK");
            table.Columns.Add("NG");
            List<string> pcbOKList = new List<string>();

            foreach (var testerEntry in testTable)
            {
                int total = 0;
                int good = 0;
                int ng = 0;
                DateTime start = DateTime.Now;
                DateTime end = new DateTime(2000, 01, 01);
                List<Tuple<string, string>> ngList = new List<Tuple<string, string>>();
                

                foreach (var pcbEntry in testerEntry.Value)
                {
                    DateTime time = DateTime.Parse(pcbEntry.Value.Rows[0]["inspection_time"].ToString());

                    if (time > end)
                    {
                        end = time;
                    }
                    if (time < start)
                    {
                        start = time;
                    }

                    string result = pcbEntry.Value.Rows[0]["result"].ToString();
                    if (result == "OK")
                    {
                        pcbOKList.Add(pcbEntry.Key);
                        good++;
                    }
                    else
                    {
                        ng++;
                        ngList.Add(new Tuple<string, string>(pcbEntry.Key, result));
                    }
                    total++;
                }

                table.Rows.Add(start, end, testerEntry.Key, total, good, ng);
            }

            FillOutGrid(grid, table, false);
            return pcbOKList;
        }

        public static void FillOutBoxSummary(DataGridView grid, string lot)
        {
            
        }

    }
}
