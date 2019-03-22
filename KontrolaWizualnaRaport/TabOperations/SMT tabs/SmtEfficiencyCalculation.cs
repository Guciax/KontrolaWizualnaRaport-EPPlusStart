using MST.MES;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaWizualnaRaport.TabOperations.SMT_tabs
{
    public class SmtEfficiencyCalculation
    {
        public static double CalculateEfficiencyForOrders(List<MST.MES.OrderStructureByOrderNo.SmtRecords> smtRecords)
        {
            double totalTime = (smtRecords.Select(r => r.smtEndDate).Max() - smtRecords.Select(r => r.smtStartDate).Min()).TotalHours;
            if (smtRecords.Count > 1)
            {
                for (int i = 1; i < smtRecords.Count; i++)
                {
                    if ((smtRecords[i].smtStartDate - smtRecords[i - 1].smtEndDate).TotalHours > 0)
                    {
                        totalTime -= (smtRecords[i].smtStartDate - smtRecords[i - 1].smtEndDate).TotalHours;
                    }
                }
            }

            double totalQuantity = smtRecords.Select(r => r.manufacturedQty).Sum();
            double efficiency = totalQuantity / totalTime;
            string modelFamily = $"{MST.MES.OrderStructureByOrderNo.GetModelFamily(smtRecords.First().orderInfo.modelId, OrderStructureByOrderNo.ModelFamilyType.SameConnAndCCT)}[{smtRecords.First().orderInfo.ledLeftovers.reelsPerRankCount}]";
            double modelNorm = DataContainer.Smt.EfficiencyNormPerModel[modelFamily];
            return efficiency / modelNorm;
        }

        public static double CalculateEfficiencyForOneOrder(MST.MES.OrderStructureByOrderNo.SmtRecords smtRecord)
        {
            double totalTime = (smtRecord.smtEndDate - smtRecord.smtStartDate).TotalHours;

            double totalQuantity = smtRecord.manufacturedQty;
            double efficiency = totalQuantity / totalTime;
            string modelFamily = $"{MST.MES.OrderStructureByOrderNo.GetModelFamily(smtRecord.orderInfo.modelId, OrderStructureByOrderNo.ModelFamilyType.SameConnAndCCT)}[{smtRecord.orderInfo.ledLeftovers.reelsPerRankCount}]";
            double modelNorm = DataContainer.Smt.EfficiencyNormPerModel[modelFamily];

            return efficiency / modelNorm;
        }


        public static Dictionary<string, double> EfficiencyOutputPerHourNormPerModel()
        {
            Dictionary<string, double> result = new Dictionary<string, double>();
            var smtRecordsGrouppedByModel = DataContainer.sqlDataByProcess.Smt.SelectMany(o => o.Value.smtOrders).GroupBy(rec => $"{rec.orderInfo.ModelFamily(OrderStructureByOrderNo.ModelFamilyType.SameConnAndCCT)}[{rec.orderInfo.ledLeftovers.reelsPerRankCount.ToString()}]").ToDictionary(model => model.Key, rec => rec.ToList());
            foreach (var modelEntry in smtRecordsGrouppedByModel)
            {
                var ordersCount = modelEntry.Value.Select(o => o.orderInfo.orderNo).Distinct().Count();
                var arrayOutputPerHour = modelEntry.Value.Where(rec => (rec.smtEndDate - rec.smtStartDate).TotalMinutes > 10 & (rec.smtEndDate - rec.smtStartDate).TotalMinutes < 90).Select(rec => Math.Round((rec.manufacturedQty) / (rec.smtEndDate - rec.smtStartDate).TotalHours, 1)).OrderBy(o => o).ToArray();
                double fast = 0;
                double slow = 0;
                double mean = 0;

                if (arrayOutputPerHour.Count() > 0)
                {
                    //fast = arrayOutputPerHour.Max();
                    //slow = arrayOutputPerHour.Min();
                    //mean = arrayOutputPerHour[(int)Math.Truncate(arrayOutputPerHour.Count() * 0.75)];
                    mean = GetMostFrequentValue(arrayOutputPerHour);
                }


                result.Add(modelEntry.Key, mean);
                Debug.WriteLine($"{modelEntry.Key};{mean * 8}");
            }

            return result;
        }

        private static double GetMostFrequentValue(double[] valArray)
        {
            if (valArray.Length ==0) return 0;
            if (valArray.Length ==1) return valArray[0];
            double step = (valArray.Max() - valArray.Min()) / 15;
            Dictionary<double, int> histogram = new Dictionary<double, int>();
            for (int i = 1; i < 16; i++)
            {
                histogram.Add(i * step, 0);
            }

            double[] stepsArray = histogram.Select(k => k.Key).ToArray();
            double prevDelta = valArray[0];
            foreach (var val in valArray)
            {
                var nearest = stepsArray.OrderBy(x => Math.Abs((long)x - val)).First();
                histogram[nearest]++;
            }

           return histogram.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
        }
    }
}

