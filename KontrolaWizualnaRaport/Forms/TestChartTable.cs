using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport.Forms
{
    public partial class TestChartTable : Form
    {
        private readonly DataTable sourceTable;

        public TestChartTable(DataTable sourceTable)
        {
            InitializeComponent();
            this.sourceTable = sourceTable;
        }

        private void TestChartTable_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = sourceTable;
        }
    }
}
