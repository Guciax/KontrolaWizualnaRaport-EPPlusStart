using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport
{
    public partial class ShowDetailsTable : Form
    {
        private readonly DataTable dtSource;
        private readonly string description;
        private readonly string topSummaryColCname;
        private readonly string bottomSummaryColName;
        private readonly string qtyColName;


        public ShowDetailsTable(DataTable dtSource, string description, string topSummaryColCname, string bottomSummaryColName, string qtyColName)
        {
            InitializeComponent();
            this.dtSource = dtSource;
            this.description = description;
            this.topSummaryColCname = topSummaryColCname;
            this.bottomSummaryColName = bottomSummaryColName;
            this.qtyColName = qtyColName;

        }

        private void SmtShiftDetails_Load(object sender, EventArgs e)
        {
            
            Dictionary<string, int> topSummary = new Dictionary<string, int>();
            Dictionary<string, int> bottomSummary = new Dictionary<string, int>();
            topSummary.Add("Razem", 0);
            bottomSummary.Add("Razem", 0);

            foreach (DataRow row in dtSource.Rows)
            {
                string topKey = row[topSummaryColCname].ToString();
                string bottomKey = row[bottomSummaryColName].ToString();
                int qty = int.Parse(row[qtyColName].ToString());
                if (!topSummary.ContainsKey(topKey)) { topSummary.Add(topKey, 0); }
                if (!bottomSummary.ContainsKey(bottomKey)) { bottomSummary.Add(bottomKey, 0); }
                topSummary[topKey] += qty;
                topSummary["Razem"] += qty;
                bottomSummary[bottomKey] += qty;
                bottomSummary["Razem"] += qty;
            }

            topSummary.ToList().Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
            bottomSummary.ToList().Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

            dataGridViewTopSummary.Columns.Add(topSummaryColCname, topSummaryColCname);
            dataGridViewTopSummary.Columns.Add("Ilosc", "Ilosc");
            foreach (var entry in topSummary)
            {
                dataGridViewTopSummary.Rows.Add(entry.Key, entry.Value);
            }
            dataGridViewBottomSummary.Columns.Add(bottomSummaryColName, bottomSummaryColName);
            dataGridViewBottomSummary.Columns.Add("Ilosc", "Ilosc");
            foreach (var entry in bottomSummary)
            {
                dataGridViewBottomSummary.Rows.Add(entry.Key, entry.Value);
            }

            dataGridViewShiftDetails.DataSource = dtSource;
            SMTOperations.autoSizeGridColumns(dataGridViewShiftDetails);
            label1.Text = description;
        }

        private void dataGridViewShiftDetails_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = sender as DataGridView;
            var rowIdx = (e.RowIndex + 1).ToString();

            var centerFormat = new StringFormat()
            {
                // right alignment might actually make more sense for numbers
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIdx, this.Font, SystemBrushes.ControlText, headerBounds, centerFormat);
        }
    }
}
