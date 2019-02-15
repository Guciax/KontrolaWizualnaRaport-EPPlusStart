using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace KontrolaWizualnaRaport
{
    public class SharedComponents
    {
        public class Kitting
        {
            public static DataGridView dataGridViewKitting;
            public static DataGridView dataGridViewKittingReadyLots;
            public static DataGridView dataGridViewKittingHistory;
            public static ComboBox comboBoxKittingModels;
            public static CheckBox checkBoxKittingMst;
            public static CheckBox checkBoxKittingLg;
        }

        public class Smt
        {
            public static DateTimePicker smtStartDate;
            public static DateTimePicker smtEndDate;

            public class productionReportTab
            {
                public static DataGridView dataGridViewSmtProduction;
            }

            public class changeoversTab
            {
                public static DataGridView dataGridViewChangeOvers;
            }

            public class StencilsTab
            {
                public static DataGridView dataGridViewSmtStencilUsage;
            }

            public class ModelAnalysis
            {
                public static RadioButton radioButtonSmtShowAllModels;
                public static ComboBox comboBoxSmtModels;
            }


            public class LedWasteTab
            {
                public static DataGridView dataGridViewSmtLedDropped;
                public static DataGridView dataGridViewSmtLedWasteByModel;
                public static DataGridView dataGridViewSmtWasteTotal;
                public static DataGridView dataGridViewSmtLedWasteTotalPerLine;
                public static ComboBox comboBoxSmtLewWasteFreq;
                public static ComboBox comboBoxSmtLedWasteModels;
                public static ComboBox comboBoxSmtLedWasteLine;
                public static Chart chartLedWasteChart;
                public static List<CheckBox> smtLinesCheckBoxesList = new List<CheckBox>();

                public static Dictionary<string, bool> selectedLines
                {
                    get
                    {
                        return smtLinesCheckBoxesList.GroupBy(l => l.Text).ToDictionary(x => x.Key, chLine => chLine.First().Checked);

                    }
                }
            }
        }
    }
}
