using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport.TabOperations.ViTab
{
    public class ViResultVsReworkResult
    {
        public static void FilloutOperatorGrid()
        {
            DataGridView grid = SharedComponents.VisualInspection.KontrolaWizualnaAnalizaOcen.dataGridViewViResultVeReworkResult;

            var ngGrouppedByOperator = DataContainer.sqlDataByProcess.VisualInspection.SelectMany(o => o.Value.ngScrapList)
                                                                                      .GroupBy(ng => ng.viOperator)
                                                                                      .ToDictionary(op => op.Key, ng => ng.ToList());

            foreach (var operatorEntry in ngGrouppedByOperator)
            {
                var totalNg = operatorEntry.Value.Where(ng=>ng.typeNgScrap=="ng").Count();
                 
            }
        }
    }
}
