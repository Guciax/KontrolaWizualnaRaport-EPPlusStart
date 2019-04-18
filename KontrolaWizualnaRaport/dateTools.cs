using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaWizualnaRaport
{
    public class dateTools
    {
        public static int WeekNumber(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            int week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            int result = (time.Year - 2000) * 100 + week;
            return result;
        }

        public static DateTime FirstDateOfWeekISO8601(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            // Use first Thursday in January to get first week of the year as
            // it will never be in Week 52/53
            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            // As we're adding days to a date in Week 1,
            // we need to subtract 1 in order to get the right date for week #1
            if (firstWeek == 1)
            {
                weekNum -= 1;
            }

            // Using the first Thursday as starting week ensures that we are starting in the right year
            // then we add number of weeks multiplied with days
            var result = firstThursday.AddDays(weekNum * 7);

            // Subtract 3 days from Thursday to get Monday, which is the first weekday in ISO8601
            return result.AddDays(-3);
        }

        public static int productionMonthNumber(int weekNumber, int year)
        {
            int week = weekNumber - (year - 2000) * 100;
            DateTime monday = FirstDateOfWeekISO8601(year, week);
            List<int> months = new List<int>();

            for (int i = 0; i < 5; i++)
            {
                DateTime nextDay = monday.AddDays(i);
                months.Add(nextDay.Month);
            }

            return (int)Math.Round(months.Average(), 0);
        }

        public static string productionMonthName(int weekNumber, int year)
        {

            int week = weekNumber - (year - 2000) * 100;
            DateTime monday = FirstDateOfWeekISO8601(year, week);
            DateTime wednesday = monday.AddDays(2);

            var result = wednesday.ToString("MMM");

            return result;
        }

        public static dateShiftNo GetOrderOwningShift(DateTime start, DateTime end)
        {
            //if (start.Hour == 5)
            //    ;
            var startShiftInfo = whatDayShiftIsit(start);
            var endShiftInfo = whatDayShiftIsit(end);
            if (startShiftInfo.fixedDate == endShiftInfo.fixedDate) return startShiftInfo;

            if ((endShiftInfo.fixedDate - start).TotalMinutes >= (end - endShiftInfo.fixedDate).TotalMinutes) return startShiftInfo;
            return endShiftInfo;
        }

        public class dateShiftNo
        {
            public DateTime fixedDate { get; set; }
            public int shift { get; set; }

            public override bool Equals(object obj)
            {
                dateShiftNo dateItem = obj as dateShiftNo;

                return dateItem.fixedDate == this.fixedDate;

            }

            public override int GetHashCode()
            {
                // Which is preferred?

                //return base.GetHashCode();

                return this.fixedDate.GetHashCode();
            }
        }

        

        ///<summary>
        ///<para>returns shift number and shift start date and time</para>
        ///</summary>
        public static dateShiftNo whatDayShiftIsit(DateTime inputDate)
        {
            int hourNow = inputDate.Hour;
            DateTime resultDate = new DateTime();
            int resultShift = 0;

            if (hourNow < 6)
            {
                resultDate = new DateTime(inputDate.Date.AddDays(-1).Year, inputDate.Date.AddDays(-1).Month, inputDate.Date.AddDays(-1).Day, 22, 0, 0);
                resultShift = 3;
            }

            else if (hourNow < 14)
            {
                resultDate = new DateTime(inputDate.Date.Year, inputDate.Date.Month, inputDate.Date.Day, 6, 0, 0);
                resultShift = 1;
            }

            else if (hourNow < 22)
            {
                resultDate = new DateTime(inputDate.Date.Year, inputDate.Date.Month, inputDate.Date.Day, 14, 0, 0);
                resultShift = 2;
            }

            else
            {
                resultDate = new DateTime(inputDate.Date.Year, inputDate.Date.Month, inputDate.Date.Day, 22, 0, 0);
                resultShift = 3;
            }

            dateShiftNo result = new dateShiftNo();
            result.fixedDate = resultDate;
            result.shift = resultShift;

            return result;
        }


    }
}
