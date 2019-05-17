using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport.TabOperations.Kitting
{
    public partial class ShowHistoryDetails : Form
    {
        private readonly List<MST.MES.OrderStructureByOrderNo.Kitting> ordersList;

        public ShowHistoryDetails(List<MST.MES.OrderStructureByOrderNo.Kitting> ordersList)
        {
            InitializeComponent();
            this.ordersList = ordersList;
        }

        private void ShowHistoryDetails_Load(object sender, EventArgs e)
        {
            foreach (var order in ordersList)
            {
                dataGridView1.Rows.Add(order.orderNo, order.orderedQty, order.kittingDate, order.endDate);
            }

            dgvTools.ColumnsAutoSize(dataGridView1, DataGridViewAutoSizeColumnMode.AllCells);
        }
    }
}
