using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaWizualnaRaport.TabOperations.Grafik
{
    public class shiftStructure
    {
        public List<WorkerShiftStructure> workersOnThisShift { get; set; }
        public dateTools.dateShiftNo shiftInfo { get; set; }

        public int ledLgTotalHours
        {
            get
            {
                return workersOnThisShift.Select(w => w.ledLgHourrs).Sum();
            }
        }

        public int ledMstTotalHours
        {
            get
            {
                return workersOnThisShift.Select(w => w.ledMstHourrs).Sum();
            }
        }

        public int ingDriverTotalHours
        {
            get
            {
                return workersOnThisShift.Select(w => w.driverIgnHours).Sum();
            }
        }

        public int totalPplOnShift
        {
            get
            {
                return workersOnThisShift.Count();
            }
        }
    }
}
