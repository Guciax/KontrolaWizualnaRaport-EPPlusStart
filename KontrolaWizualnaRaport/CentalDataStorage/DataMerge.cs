using MST.MES;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MST.MES.OrderStructureByOrderNo;

namespace KontrolaWizualnaRaport.CentalDataStorage
{

    public class DataMerger
    {
        public static void MergeData()
        {

            foreach (var kittingEntry in DataContainer.sqlDataByProcess.Kitting)
            {
                if (!DataContainer.sqlDataByOrder.ContainsKey(kittingEntry.Key))
                {
                    DataContainer.sqlDataByOrder.Add(kittingEntry.Key, new OneOrderData());
                }
                if (DataContainer.sqlDataByOrder[kittingEntry.Key].kitting != null) continue;

                DataContainer.sqlDataByOrder[kittingEntry.Key].kitting = kittingEntry.Value;
                DataContainer.sqlDataByOrder[kittingEntry.Key].smt = new SMT();
                DataContainer.sqlDataByOrder[kittingEntry.Key].visualInspection = new VisualInspection();
                DataContainer.sqlDataByOrder[kittingEntry.Key].ledsInBoxesList = new List<BoxingInfo>();
                DataContainer.sqlDataByOrder[kittingEntry.Key].test = new TestInfo();
            }


            if (DataContainer.sqlDataByProcess.Smt != null)
            {
                foreach (var smtEntry in DataContainer.sqlDataByProcess.Smt)
                {
                    if (!DataContainer.sqlDataByProcess.Kitting.ContainsKey(smtEntry.Key)) continue;
                    if (!DataContainer.sqlDataByOrder.ContainsKey(smtEntry.Key))
                    {
                        DataContainer.sqlDataByOrder.Add(smtEntry.Key, new OneOrderData());
                    }

                    DataContainer.sqlDataByOrder[smtEntry.Key].smt = smtEntry.Value;
                }

                //ordersTimeFlow

                var grouppedByLine = DataContainer.sqlDataByProcess.Smt.SelectMany(o => o.Value.smtOrders)
                                                                       .OrderBy(o => o.smtStartDate)
                                                                       .GroupBy(o => o.smtLine)
                                                                       .ToDictionary(l => l.Key, o => o.ToList());
                foreach (var lineEntry in grouppedByLine)
                {
                    for (int i = 1; i < lineEntry.Value.Count; i++)
                    {
                        lineEntry.Value[i].previousOrder = lineEntry.Value[i - 1];
                        if (i < lineEntry.Value.Count - 1) 
                        {
                            lineEntry.Value[i].nextOrder = lineEntry.Value[i + 1];
                        }
                        
                    }
                }
            }

            if (DataContainer.sqlDataByProcess.Test != null)
            {
                foreach (var testEntry in DataContainer.sqlDataByProcess.Test)
                {
                    if (!DataContainer.sqlDataByProcess.Kitting.ContainsKey(testEntry.Key)) continue;
                    if (!DataContainer.sqlDataByOrder.ContainsKey(testEntry.Key))
                    {
                        DataContainer.sqlDataByOrder.Add(testEntry.Key, new OneOrderData());
                    }

                    DataContainer.sqlDataByOrder[testEntry.Key].test = testEntry.Value;
                }
            }

            if (DataContainer.sqlDataByProcess.VisualInspection != null)
            {
                foreach (var viEntry in DataContainer.sqlDataByProcess.VisualInspection)
                {
                    if (!DataContainer.sqlDataByProcess.Kitting.ContainsKey(viEntry.Key)) continue;
                    if (!DataContainer.sqlDataByOrder.ContainsKey(viEntry.Key))
                    {
                        DataContainer.sqlDataByOrder.Add(viEntry.Key, new OneOrderData());
                    }

                    foreach (var ng in viEntry.Value.ngScrapList)
                    {
                        if (ng.orderNo == "") continue;
                        ng.modelName = DataContainer.sqlDataByOrder[ng.orderNo].kitting.modelId_12NCFormat;
                    }
                    
                    DataContainer.sqlDataByOrder[viEntry.Key].visualInspection = viEntry.Value;
                }
            }

            if (DataContainer.sqlDataByProcess.Rework != null)
            {
                foreach (var reworkEntry in DataContainer.sqlDataByProcess.Rework)
                {
                    if (!DataContainer.sqlDataByProcess.Kitting.ContainsKey(reworkEntry.Key)) continue;
                    if (!DataContainer.sqlDataByOrder.ContainsKey(reworkEntry.Key))
                    {
                        DataContainer.sqlDataByOrder.Add(reworkEntry.Key, new OneOrderData());
                    }

                    DataContainer.sqlDataByOrder[reworkEntry.Key].rework = reworkEntry.Value;
                }
            }

            if (DataContainer.sqlDataByProcess.Boxing != null)
            {
                foreach (var boxingEntry in DataContainer.sqlDataByProcess.Boxing)
                {
                    if (!DataContainer.sqlDataByProcess.Kitting.ContainsKey(boxingEntry.Key)) continue;
                    if (!DataContainer.sqlDataByOrder.ContainsKey(boxingEntry.Key))
                    {
                        DataContainer.sqlDataByOrder.Add(boxingEntry.Key, new OneOrderData());
                    }
                    if (DataContainer.sqlDataByProcess.Kitting[boxingEntry.Key] == null) continue;
                    foreach (var pcb in boxingEntry.Value)
                    {
                        pcb.kittingInfo = DataContainer.sqlDataByProcess.Kitting[boxingEntry.Key];
                    }

                    DataContainer.sqlDataByOrder[boxingEntry.Key].ledsInBoxesList = boxingEntry.Value;
                }
            }
        }


    }
}

