using KontrolaWizualnaRaport.CentalDataStorage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport.TabOperations.Grafik
{
    class PplOnShftLoader
    {
        public static Dictionary<DateTime, DayStructure> loadPeopleOnShifts()
        {
            Dictionary<DateTime, DayStructure> result = new Dictionary<DateTime, DayStructure>();
            Dictionary<string, int> months = new Dictionary<string, int>
            {
                {"STYCZEŃ",1 },
                {"LUTY",2 },
                {"MARZEC",3 },
                {"KWIECIEŃ",4 },
                {"MAJ",5 },
                {"CZERWIEC",6 },
                {"LIPIEC",7 },
                {"SIERPIEŃ",8 },
                {"WRZESIEŃ",9 },
                {"PAŹDZIERNIK",10 },
                {"LISTOPAD",11 },
                {"GRUDZIEŃ",12 }
            };


            foreach (var yearEntry in GlobalParameters.GrafikPerYearPath)
            {
                if (!File.Exists(yearEntry.Value)) { continue; }

                var fs = new FileStream(yearEntry.Value, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var pck = new OfficeOpenXml.ExcelPackage();
                try
                {
                    pck = new OfficeOpenXml.ExcelPackage(fs);
                }
                catch (Exception e) { MessageBox.Show(e.Message); }

                if (pck.Workbook.Worksheets.Count != 0)
                {
                    foreach (OfficeOpenXml.ExcelWorksheet worksheet in pck.Workbook.Worksheets)
                    {
                        if (worksheet.Dimension == null) continue;
                        if (worksheet.Cells["T1"].Value==null) { continue; }
                        if (!months.ContainsKey(worksheet.Cells["T1"].Value.ToString().ToUpper())) {
                            continue; }
                        int monthNumber = months[worksheet.Cells["T1"].Value.ToString().ToUpper()];
                        Debug.WriteLine(monthNumber);
                        int colIndex = 20;
                        while (worksheet.Cells[2, colIndex].Value is DateTime)
                        {
                            string dateString = worksheet.Cells[2, colIndex].Value.ToString();
                            DateTime day = (DateTime)worksheet.Cells[2, colIndex].Value;

                            float lgHoursPl = float.Parse(worksheet.Cells[4, colIndex].Value.ToString());
                            float mstHoursPl = float.Parse(worksheet.Cells[5, colIndex].Value.ToString());
                            float driverHoursPl = float.Parse(worksheet.Cells[6, colIndex].Value.ToString());
                            float otherPl = float.Parse(worksheet.Cells[7, colIndex].Value.ToString());

                            float lgHoursUkr = float.Parse(worksheet.Cells[8, colIndex].Value.ToString());
                            float mstHoursUkr = float.Parse(worksheet.Cells[9, colIndex].Value.ToString());
                            float driverHoursUkr = float.Parse(worksheet.Cells[10, colIndex].Value.ToString());
                            float otherUkr = float.Parse(worksheet.Cells[11, colIndex].Value.ToString());

                            DayStructure newDay = new DayStructure
                            {
                                lgLedHoursPl = lgHoursPl,
                                mstLedHoursPl = mstHoursPl,
                                driverIgnLedHoursPl = driverHoursPl,
                                otherPl = otherPl,
                                lgLedHoursUkr = lgHoursUkr,
                                mstLedHoursUkr = mstHoursUkr,
                                driverIgnLedHoursUkr = driverHoursUkr,
                                otherUkr = otherUkr
                            };

                            result.Add(day, newDay);
                            colIndex += 2;
                        }

                    }
                }
            }

            return result;
        }
    }
}
