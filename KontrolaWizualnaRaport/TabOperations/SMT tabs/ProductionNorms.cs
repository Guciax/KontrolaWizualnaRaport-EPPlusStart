using MST.MES;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MST.MES.ModelInfo;

namespace KontrolaWizualnaRaport.TabOperations.SMT_tabs
{
    public class ProductionNorms
    {
        public static void ShowProductionNorm(string modelId, string smtLine, DataGridView grid)
        {
            ModelInfo.ModelSpecification modelSpec = null;
            if (!DataContainer.mesModels.TryGetValue(modelId, out modelSpec))
                return;
            var dtModels = DataContainer.DevTools.devToolsDb.Where(m => m.nc12 == modelId+"00");
            var pcbDimensions = MST.MES.Data_structures.DevTools.DevToolsModelsOperations.GetMPcbimensions(dtModels.First());


            var eff = SmtEfficiencyCalculation.NewWay.CalculateModelNormPerHour(modelId, smtLine);
            grid.Rows.Clear();

            grid.Rows.Add("Specyfikacja");
            dgvTools.SetRowColor(grid.Rows[grid.Rows.Count - 1], Color.LightSteelBlue);
            grid.Rows.Add("12NC:", $"{modelId}");
            grid.Rows.Add("Nazwa:", $"{eff.modelSpec.modelName}");
            grid.Rows.Add("Ilość LED:", $"{eff.modelSpec.ledCountPerModel}");
            grid.Rows.Add("Ilość PCB/MB:", $"{eff.modelSpec.pcbCountPerMB}");
            grid.Rows.Add("Ilość Conn:", $"{eff.modelSpec.connectorCountLgMstCalculated}");
            grid.Rows.Add("Ilość PCB/MB:", $"{eff.modelSpec.pcbCountPerMB}");
            grid.Rows.Add("Wymiary MB:", $"{eff.mbDimensionsLWmm.Item1}x{eff.mbDimensionsLWmm.Item2}mm");
            grid.Rows.Add("Wymiary PCB:", $"{pcbDimensions.Item1}x{pcbDimensions.Item2}mm");

            grid.Rows.Add("Norma SMT");
            dgvTools.SetRowColor(grid.Rows[grid.Rows.Count - 1], Color.LightSteelBlue);
            grid.Rows.Add("Montaż LED", $"{eff.siplaceCT} sek");
            grid.Rows.Add("Montaż CONN", $"{eff.connCT} sek");
            grid.Rows.Add("Reflow", $"{eff.reflowCT} sek");
            grid.Rows.Add("Wydajność godz.", $"{eff.outputPerHour} szt");
            grid.Rows.Add("Wydajność zm.", $"{eff.outputPerHour * 8} szt");
            if (dtModels.Count() == 0)
                return;
            grid.Rows.Add("Norma Test");
            dgvTools.SetRowColor(grid.Rows[grid.Rows.Count - 1], Color.LightSteelBlue);
            var normPerHour = GetTestOutputPerHour(dtModels.First());

            grid.Rows.Add("Wydajność godz", $"{normPerHour} szt.");
            grid.Rows.Add("Wydajność zm.", $"{normPerHour * 8} szt.");

            grid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            grid.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private static int GetTestOutputPerHour(MST.MES.Data_structures.DevToolsModelStructure model)
        {
            var pcbDimensions = MST.MES.Data_structures.DevTools.DevToolsModelsOperations.GetMPcbimensions(model);

            if (model.name.ToUpper().StartsWith("LIN"))
            {
                if (pcbDimensions.Item1 < 615) return 120;
                return 80;
            }
            if (model.name.ToUpper().StartsWith("REC"))
            {
                return 120;
            }
            if (model.name.ToUpper().StartsWith("RD"))
            {
                if (pcbDimensions.Item1 < 355) return 120;
                if (pcbDimensions.Item1 < 450) return 80;
                return 60;
            }
            if (model.name.ToUpper().StartsWith("HEX"))
            {
                return 200;
            }
            if (pcbDimensions.Item1 > 450 || pcbDimensions.Item2 > 450) return 60;

            return 120;
        }


    }
}
