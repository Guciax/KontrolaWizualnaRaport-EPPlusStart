using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using MST.MES;
using static KontrolaWizualnaRaport.DgvImageButtonCell;

namespace KontrolaWizualnaRaport
{
    public class LotSummary
    {
        public static void FillOutOrderInfo(string orderNo, 
                                            DataGridView kittingGrid,
                                            DataGridView smtGrid, 
                                            DataGridView viGrid,
                                            DataGridView testGrid,
                                            DataGridView boxGrid)
        {

            var kittingData = MST.MES.SqlDataReaderMethods.Kitting.GetOneOrderByDataReader(orderNo);
            FillOutKittingGrid(kittingData, kittingGrid);
            
            var smtData = MST.MES.SqlDataReaderMethods.SMT.GetOneOrder(orderNo);
            FillOutSmtGrid(smtData, smtGrid, kittingData);

            var viData = MST.MES.SqlDataReaderMethods.VisualInspection.GetViForOneOrder(orderNo);
            FillOutViGrid(viData, viGrid, smtData.totalManufacturedQty);

            var testData = MST.MES.SqlDataReaderMethods.LedTest.GetTestRecordsForOneOrder(MST.MES.SqlDataReaderMethods.LedTest.TesterIdToName(),  orderNo );
            FillOutTestGrid(testData, testGrid);

            //lg potrzebny widook z nr zlecenie w tabeli box
            var boxData = MST.MES.SqlDataReaderMethods.Boxing.GetBoxingForOneOrder(orderNo);
            FillOutBoxGrid(boxData, boxGrid);
        }

        private static void FillOutBoxGrid(List<OrderStructureByOrderNo.BoxingInfo> boxData, DataGridView boxGrid)
        {
            boxGrid.Rows.Clear();
            var grouppedByBoxId = boxData.GroupBy(pcb => pcb.boxId).ToDictionary(k => k.Key, v => v.OrderBy(p=>p.boxingDate).ToList());
            boxGrid.Rows.Add("Ilość kartoów:", grouppedByBoxId.Count());
            boxGrid.Rows.Add("Ilość spakowanych wyrobów:", boxData.Count());
            boxGrid.Rows.Add();
            foreach (var boxIdEntry in grouppedByBoxId)
            {
                boxGrid.Rows.Add("Box ID:", boxIdEntry.Key);
                boxGrid.Rows.Add("Ilość:", boxIdEntry.Value.Count());
                boxGrid.Rows.Add("Start:", boxIdEntry.Value.First().boxingDate);
                boxGrid.Rows.Add("Koniec:", boxIdEntry.Value.Last().boxingDate);
                boxGrid.Rows.Add();
            }
            dgvTools.ColumnsAutoSize(boxGrid, DataGridViewAutoSizeColumnMode.AllCells);
        }

        private static void FillOutTestGrid(OrderStructureByOrderNo.TestInfo testData, DataGridView testGrid)
        {
            testGrid.Rows.Clear();
            testGrid.Rows.Add("Ilość tsterów:", testData.testerDict.Count());
            testGrid.Rows.Add();
            var dictByTester = testData.testerDict;
            foreach (var testerEntry in dictByTester)
            {
                testGrid.Rows.Add("Tester:", testerEntry.Key);
                testGrid.Rows.Add("Ilość wyrobów:", testerEntry.Value.Count());
                testGrid.Rows.Add("Ilość OK:", testerEntry.Value.Where(ser => ser.Value.pcbResultOk).Count());
                testGrid.Rows.Add("Ilość testów:", testerEntry.Value.SelectMany(ser=>ser.Value.testEntries).Count());
                testGrid.Rows.Add("Start:", testerEntry.Value.SelectMany(ser=>ser.Value.testEntries).OrderBy(p=>p.testTime).First().testTime);
                testGrid.Rows.Add("Koniec:", testerEntry.Value.SelectMany(ser=>ser.Value.testEntries).OrderBy(p=>p.testTime).Last().testTime);
                testGrid.Rows.Add();
            }
            dgvTools.ColumnsAutoSize(testGrid, DataGridViewAutoSizeColumnMode.AllCells);
        }

        private static void FillOutViGrid(OrderStructureByOrderNo.VisualInspection viData, DataGridView viGrid, double totalManufactured)
        {
            viGrid.Rows.Clear();
            viGrid.Rows.Add("Ilość NG:", viData.ngCount);
            viGrid.Rows.Add("Odpad NG:", Math.Round((double)viData.ngCount / totalManufactured * 100, 2) + "%");
            viGrid.Rows.Add("Ilość SCR:", viData.scrapCount);
            viGrid.Rows.Add("Odpad SCR:", Math.Round((double)viData.scrapCount / totalManufactured * 100, 2) + "%");
            viGrid.Rows.Add();
            viGrid.Rows.Add("Naprawionych:", viData.reworkedOkCout);
            viGrid.Rows.Add("Nieudana naprawa:", viData.reworkFailedCout);
            viGrid.Rows.Add();
            viGrid.Rows.Add("Przyczyny:");
            foreach (var reasonEntry in viData.allReasons)
            {
                viGrid.Rows.Add(reasonEntry.Key, reasonEntry.Value);
            }

            viGrid.Rows.Add("Zdjęcia wad:");
            foreach (var pcb in viData.ngScrapList)
            {
                viGrid.Rows.Add(pcb.ngSerialNo);
            }
            dgvTools.ColumnsAutoSize(viGrid, DataGridViewAutoSizeColumnMode.AllCells);
        }

        private static void FillOutSmtGrid(OrderStructureByOrderNo.SMT smtData, DataGridView smtGrid, OrderStructureByOrderNo.Kitting kittingData)
        {
            smtGrid.Rows.Clear();
            smtGrid.Rows.Add("Łączna ilość:", smtData.totalManufacturedQty);
            smtGrid.Rows.Add("Zużycie LED", smtData.ledsUsed);
            double usageByBom = kittingData.modelSpec.ledCountPerModel * smtData.totalManufacturedQty;
            string ledWaste = Math.Round(((double)smtData.ledsUsed - usageByBom) / usageByBom * 100, 2)+"%";
            smtGrid.Rows.Add("Odpad LED", ledWaste);

            smtGrid.Rows.Add();
            smtGrid.Rows.Add("Dane z linii");
            foreach (var lineEntry in smtData.smtOrders)
            {
                smtGrid.Rows.Add("Linia:", lineEntry.smtLine);
                smtGrid.Rows.Add("Start:", lineEntry.smtStartDate);
                smtGrid.Rows.Add("Koniec:", lineEntry.smtEndDate);
                smtGrid.Rows.Add("Ilość:", lineEntry.manufacturedQty);
                smtGrid.Rows.Add("Operator:", lineEntry.operatorSmt);
                smtGrid.Rows.Add("Stencil:", lineEntry.stencilId);
                smtGrid.Rows.Add();
            }
            dgvTools.ColumnsAutoSize(smtGrid, DataGridViewAutoSizeColumnMode.AllCells);
        }

        private static void FillOutKittingGrid(OrderStructureByOrderNo.Kitting kittingData, DataGridView kittingGrid)
        {
            kittingGrid.Rows.Clear();
            kittingGrid.Rows.Add("Model ID:", kittingData.modelId_12NCFormat);
            kittingGrid.Rows.Add("Model nazwa:", kittingData.ModelName);
            kittingGrid.Rows.Add("Data utworzenia:", kittingData.kittingDate);
            kittingGrid.Rows.Add("Data zakończenia:", kittingData.endDate);
            kittingGrid.Rows.Add("Ilość zamówienia:", kittingData.orderedQty);
            kittingGrid.Rows.Add("Nr. planu:", kittingData.productionPlanId);
            kittingGrid.Rows.Add();
            kittingGrid.Rows.Add("Specyfikacja");
            kittingGrid.Rows.Add("PCB / MB:", kittingData.modelSpec.pcbCountPerMB);
            kittingGrid.Rows.Add("LED:", kittingData.modelSpec.ledCountPerModel);
            kittingGrid.Rows.Add("CONN:", kittingData.modelSpec.connectorCountPerModel);
            kittingGrid.Rows.Add("RES:", kittingData.modelSpec.resistorCountPerModel);
            dgvTools.ColumnsAutoSize(kittingGrid, DataGridViewAutoSizeColumnMode.AllCells);
        }
    }
}
