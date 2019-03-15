using KontrolaWizualnaRaport.CentalDataStorage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace KontrolaWizualnaRaport.TabOperations.SMT_tabs
{
    public class  SmtCharts
    {
        public static void DrawChartSmtProductionReport()
        {
            SortedDictionary<DateTime, SortedDictionary<int, List<MST.MES.OrderStructureByOrderNo.SmtRecords>>> sourceDic =  DataContainer.Smt.sortedTableByDayAndShift;
            
            Chart chart = SharedComponents.Smt.productionReportTab.chartSmtProductionReport;
            chart.Series.Clear();
            chart.Legends.Clear();
            chart.ChartAreas.Clear();

            ChartArea ar = new ChartArea();
            ar.AxisX.LabelStyle.Interval = 1;

            Series barSeries = new Series
            {
                ChartType = SeriesChartType.Column,
                BorderWidth = 1,
                BorderColor = Color.FromArgb(255, 39, 174, 96),
                Color = Color.FromArgb(150, 39, 174, 96)
            };

            foreach (var dayEntry in sourceDic)
            {
                int mstQ = 0;
                if (SharedComponents.Smt.cbSmtMst.Checked)
                {
                    mstQ = dayEntry.Value.SelectMany(s => s.Value).Where(o => o.orderInfo.clientGroup == "MST").Select(o => o.manufacturedQty).Sum();
                }
                int lgQ = 0;
                if (SharedComponents.Smt.cbSmtLg.Checked)
                {
                    lgQ = dayEntry.Value.SelectMany(s => s.Value).Where(o => o.orderInfo.clientGroup == "LG").Select(o => o.manufacturedQty).Sum();
                }
                DataPoint pt = new DataPoint();
                pt.SetValueXY(dayEntry.Key.ToString("dd-MMM"), mstQ+lgQ);
                barSeries.Points.Add(pt);
            }

            chart.ChartAreas.Add(ar);
            chart.Series.Add(barSeries);
        }
    }
}
