using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport.TabOperations.Kitting
{
    public class OrdersHistory
    {
        public static void FillOutGrid(DataGridView grid, bool mstOrders, bool lgOrders, int daysBack)
        {
            var sqlOrders = MST.MES.SqlDataReaderMethods.Kitting.GetOrdersInfoByDataReader(daysBack);
            List<MST.MES.OrderStructureByOrderNo.Kitting> filteredOrders = new List<MST.MES.OrderStructureByOrderNo.Kitting>();
            if (lgOrders)
            {
                filteredOrders.AddRange(sqlOrders.Where(o => (o.Value.odredGroup == "LG")).Select(o => o.Value));
            }
            if (mstOrders)
            {
                filteredOrders.AddRange(sqlOrders.Where(o => (o.Value.odredGroup == "MST")).Select(o => o.Value));
            }

            var grouppedByModel = filteredOrders.GroupBy(o => o.modelId).ToDictionary(m => m.Key, o => o.ToList()).OrderByDescending(m=>m.Value.Select(o=>o.orderedQty).Sum());

            grid.Rows.Clear();
            foreach (var modelEntry in grouppedByModel)
            {
                
                var dtModel = DataContainer.DevTools.devToolsDb.Where(m => m.nc12 == modelEntry.Key + "00");
                string mb = "";
                if (dtModel.Count() >0)
                {
                    mb = MST.MES.DtTools.GetPcb12NC(dtModel.First());
                }

                grid.Rows.Add(modelEntry.Key,
                              modelEntry.Value.First().ModelName,
                              modelEntry.Value.Count(),
                              modelEntry.Value.Select(o => o.orderedQty).Sum(),
                              mb);

                grid.Rows[grid.Rows.Count - 1].Cells[0].Tag = modelEntry.Value;
            }

            dgvTools.ColumnsAutoSize(grid, DataGridViewAutoSizeColumnMode.AllCells);
        }
    }
}
