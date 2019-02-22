﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static KontrolaWizualnaRaport.CustomChecedListBoxStuff;
using static KontrolaWizualnaRaport.Form1;

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
            public static CheckBox cbSmtMst;
            public static CheckBox cbSmtLg;


            public class productionReportTab
            {
                public static CustomDataGridView dataGridViewSmtProduction;
                public static RadioButton rbModelsCount;
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

        public class VisualInspection
        {
            public class OstatnieLotyTab
            {
                public static DataGridView gridLatestLots;
            }
            public class PrzyczynyOdpaduTab
            {
                public static DateTimePicker dateTimePickerPrzyczynyOdpaduOd;
                public static DateTimePicker dateTimePickerWasteLevelBegin;
                public static CustomCheckedListBox checkedListBoxViWasteReasonsSmtLines;
                public static CustomDataGridView dataGridViewNgScrapReasons;
                public static Chart chartPrzyczynyOdpaduNg;
                public static Chart chartPrzyczynyOdpaduScrap;
                public static CheckBox checkBoxViReasonsMst;
                public static CheckBox checkBoxViReasonsLg;
            }
            public class PoziomOdpaduTab
            {
                public static CheckBox checkBoxViLevelMst;
                public static CheckBox checkBoxViLevelLg;
                public static DateTimePicker dateTimePickerWasteLevelBegin;
                public static DateTimePicker dateTimePickerWasteLevelEnd;
                public static RadioButton radioButtonDaily;
                public static RadioButton radioButtonWeekly;
                public static RadioButton radioButtonViLinesCumulated;
                public static DataGridView dataGridViewWasteLevel;
                public static Chart chartWasteLevel;
                public static CustomCheckedListBox checkedListBoxViWasteLevelSmtLines;
                public static VScrollBar vScrollBarZoomChart;
                public static CheckBox checkBoxEnableZoom;
            }

            public class Rework
            {
                public static DataGridView dataGridViewReworkDailyReport;
                public static DataGridView dataGridViewReworkByOperator;
                public static DataGridView dataGridViewServiceVsNg;
                public static Chart chartServiceVsNg;
            }

            
        }
        public class Boxing
        {
            public static CheckBox cbLg;
            public static CheckBox cbMst;
            public static DateTimePicker datetimePickerStart;
            public static DateTimePicker datetimePickerEnd;
            public static RadioButton rbModules;
            public static RadioButton rbComponentsCount;
            public static CustomDataGridView dataGridViewBoxing;
        }
    }
}
