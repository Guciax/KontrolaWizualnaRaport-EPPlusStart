using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static KontrolaWizualnaRaport.Form1;

namespace KontrolaWizualnaRaport.TabOperations.Grafik
{
    public class ShowData
    {
        public static void FilloutGrid()
        {
            CustomDataGridView grid = SharedComponents.Grafik.grid;
            DateTime dateStart = SharedComponents.Grafik.dateStart.Value;
            DateTime dateEnd = SharedComponents.Grafik.dateEnd.Value;
            Dictionary<string, List<DayStructure>> reorderByDateKey = new Dictionary<string, List<DayStructure>>();
            grid.Columns.Clear();
            grid.Rows.Clear();
            grid.Columns.Add("Data", "Data");
            grid.Columns.Add("c1", "DRIV/IGN-PL");
            grid.Columns.Add("c2", "DRIV/IGN-UKR");
            grid.Columns.Add("c3", "LED MST-PL");
            grid.Columns.Add("c4", "LED MST-UKR");
            grid.Columns.Add("c5", "LED LG-PL");
            grid.Columns.Add("c6", "Nadgodziny");
            grid.Columns.Add("c7", "LED LG-UKR");
            grid.Columns.Add("c8", "Nadgodziny");

            foreach (var dayEntry in DataContainer.peopleOnShifts)
            {
                if (dayEntry.Key < dateStart) continue;
                if (dayEntry.Key > dateEnd) continue;
                string dateKey = DateKey(dayEntry.Key);
                if (!reorderByDateKey.ContainsKey(dateKey))
                {
                    reorderByDateKey.Add(dateKey, new List<DayStructure>());
                }
                reorderByDateKey[dateKey].Add(dayEntry.Value);
            }

            foreach (var dateKey in reorderByDateKey)
            {
                grid.Rows.Add(dateKey.Key,
                    dateKey.Value.Select(o => o.driverIgnLedHoursPl).Sum(),
                    dateKey.Value.Select(o => o.driverIgnLedHoursUkr).Sum(),
                    dateKey.Value.Select(o => o.mstLedHoursPl).Sum(),
                    dateKey.Value.Select(o => o.mstLedHoursUkr).Sum(),
                              dateKey.Value.Select(o => o.lgLedHoursPl).Sum(),
                              0,
                              dateKey.Value.Select(o => o.lgLedHoursUkr).Sum(),
                              0
                              );
            }

            dgvTools.ColumnsAutoSize(grid, DataGridViewAutoSizeColumnMode.AllCells);
        }

        private static string DateKey(DateTime day)
        {
            if (SharedComponents.Grafik.daily.Checked) return day.ToString("dd-MMM-yyyy");
            if (SharedComponents.Grafik.weekly.Checked) return  dateTools.WeekNumber(day).ToString();
            if (SharedComponents.Grafik.monthly.Checked) return day.ToString("MMM");
            return day.ToString("yyyy");
        }
    }
}
