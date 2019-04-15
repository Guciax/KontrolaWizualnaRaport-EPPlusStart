using MST.MES;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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


        public static Dictionary<string, SortedDictionary<double, int>> EfficiencyHistogramPerModel(OrderStructureByOrderNo.ModelFamilyType familyOption)
        {
            Dictionary<string, SortedDictionary<double, int>> result = new Dictionary<string, SortedDictionary<double, int>>();
            MST.MES.OrderStructureByOrderNo.SmtRecords previousOrder = null;

            Dictionary<string, List<double>> outputPerModel = new Dictionary<string, List<double>>();
            foreach (var dayEntry in DataContainer.Smt.sortedTableByDayAndShift)
            {
                foreach (var shiftEntry in dayEntry.Value)
                {
                    foreach (var order in shiftEntry.Value.OrderBy(o=>o.smtEndDate))
                    {
                        if (previousOrder == null)
                        {
                            previousOrder = order;
                            continue;
                        }
                        if ((order.smtEndDate - previousOrder.smtEndDate).TotalMinutes > 60)
                        {
                            previousOrder = order;
                            continue;
                        }

                        string model = $"{order.orderInfo.ModelFamily(familyOption)} [{order.orderInfo.ledLeftovers.reelsPerRankCount.ToString()}]";
                        double duration = (order.smtEndDate - previousOrder.smtEndDate).TotalHours;
                        if (duration < 0.15 || duration > 1 ) continue;
                        double outPutPerHour = order.manufacturedQty / duration;
                        if (!outputPerModel.ContainsKey(model)) {
                            outputPerModel.Add(model, new List<double>());
                        }

                        outputPerModel[model].Add(outPutPerHour);
                    }
                }
            }

            foreach (var modelEntry in outputPerModel)
            {
                result.Add(modelEntry.Key, MakeSteppedHistogram(modelEntry.Value.ToArray(), 15));
            }

            return result;
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
                //Debug.WriteLine($"{modelEntry.Key};{mean * 8}");
            }

            return result;
        }

        public static SortedDictionary<double, int> MakeSteppedHistogram(double[] inputValues, int numberOfSteps)
        {
            SortedDictionary<double, int> histogram = new SortedDictionary<double, int>();
            if (inputValues.Length == 0) return new SortedDictionary<double, int>();
            if (inputValues.Length == 1) return new SortedDictionary<double, int>() { { inputValues[0], 1 } };

            double step = (inputValues.Max() - inputValues.Min()) / numberOfSteps;
            for (int i = 1; i < 16; i++)
            {
                histogram.Add(i * step, 0);
            }

            double[] stepsArray = histogram.Select(k => k.Key).ToArray();
            foreach (var val in inputValues)
            {
                var nearest = stepsArray.OrderBy(x => Math.Abs(x - val)).First();
                histogram[nearest]++;
            }
            return histogram;
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

        public class NewWay
        {
            public class SmtLineConfiguration
            {
                public string lineName;
                public int pcbReflowSpeed;
                public int carrierReflowSpeed;
                public int siplaceCph;
                public int printerCt;
                public int connCph;
            }

            public class SmtEfficiencyNormStruct
            {
                public double siplaceCT { get; set; }
                public double connCT { get; set; }
                public double reflowCT { get; set; }
                public double outputPerHour { get; set; }
                public ModelInfo.ModelSpecification modelSpec { get; set; }
                public Tuple<double, double> mbDimensionsLWmm { get; set; }
            }

            public static SmtEfficiencyNormStruct CalculateModelNormPerHour(string modelId, string smtLine, int siplaceHeads2or4=2)
            {
                ModelInfo.ModelSpecification modelSpec;
                Tuple<double, double>  mbDimensionsLWmm = new Tuple<double, double>(0, 0);
                if (!DataContainer.mesModels.TryGetValue(modelId, out modelSpec)) {
                    
                    return null;
                        }

                Dictionary<string, SmtLineConfiguration> lineConfiguration = new Dictionary<string, SmtLineConfiguration>
                {
                    {"SMT1",new SmtLineConfiguration(){lineName="SMT1", pcbReflowSpeed=90, carrierReflowSpeed=90, siplaceCph=20000, printerCt = 30, connCph =  10000} },
                    {"SMT2",new SmtLineConfiguration(){lineName="SMT2", pcbReflowSpeed=130, carrierReflowSpeed=110, siplaceCph=20000, printerCt = 30, connCph =  10000 } },
                    {"SMT3",new SmtLineConfiguration(){lineName="SMT3", pcbReflowSpeed=95, carrierReflowSpeed=90, siplaceCph=20000, printerCt = 30, connCph =  10000 } },
                    {"SMT4",new SmtLineConfiguration(){lineName="SMT4", pcbReflowSpeed=130, carrierReflowSpeed=0, siplaceCph=20000, printerCt = 40, connCph =  10000 } },
                    {"SMT5",new SmtLineConfiguration(){lineName="SMT5", pcbReflowSpeed=140, carrierReflowSpeed=105, siplaceCph=20000, printerCt = 30, connCph =  1800 } },
                    {"SMT6",new SmtLineConfiguration(){lineName="SMT6", pcbReflowSpeed=140, carrierReflowSpeed=110, siplaceCph=20000, printerCt = 30, connCph =  1800 } },
                    {"SMT7",new SmtLineConfiguration(){lineName="SMT7", pcbReflowSpeed=90, carrierReflowSpeed=90, siplaceCph=15000, printerCt = 25, connCph =  1800 } },
                    {"SMT8",new SmtLineConfiguration(){lineName="SMT8", pcbReflowSpeed=90, carrierReflowSpeed=90, siplaceCph=15000, printerCt = 25, connCph =  1800 } },
                };

                
                var dtModels = DataContainer.DevTools.devToolsDb.Where(rec => rec.nc12 == modelId + "00");
                double reflowCT = 0;
                double siplaceCph = 0;
                double connCT = 0;
                int pcbLoadingUnloading = 15;

                if (dtModels.Count() > 0)
                {//MST
                    mbDimensionsLWmm = GetMbDimensions(dtModels.First()); //need smth better than first
                    reflowCT = (double)(mbDimensionsLWmm.Item1 / 10) / (double)lineConfiguration[smtLine].pcbReflowSpeed * 60 + pcbLoadingUnloading;
                    siplaceCph = 50 * modelSpec.ledCountPerModel * modelSpec.pcbCountPerMB + 10000;
                    connCT = modelSpec.pcbCountPerMB * modelSpec.connectorCountPerModel * (3600 / (double)lineConfiguration[smtLine].connCph)+ pcbLoadingUnloading;
                }
                else
                {//LG
                    mbDimensionsLWmm = GetLgModelMbDimension(modelId);
                    reflowCT = (double)(mbDimensionsLWmm.Item1 / 10) / (double)lineConfiguration[smtLine].carrierReflowSpeed * 60 + pcbLoadingUnloading;
                    siplaceCph = 50 * modelSpec.ledCountPerModel * modelSpec.pcbCountPerMB + 10000;
                    double connQty = GetLgModelConnQty(modelId);
                    connCT = modelSpec.pcbCountPerMB * connQty * (3600 / (double)lineConfiguration[smtLine].connCph) + pcbLoadingUnloading;
                }

                if (siplaceCph < 15000) siplaceCph = 15000;
                if (siplaceCph > lineConfiguration[smtLine].siplaceCph) siplaceCph = lineConfiguration[smtLine].siplaceCph;

                double siplaceCT = modelSpec.ledCountPerModel * modelSpec.pcbCountPerMB / (siplaceCph / 3600)+ pcbLoadingUnloading;
                double lineCT = Math.Max(
                                        Math.Max(lineConfiguration[smtLine].printerCt, connCT),
                                        Math.Max(siplaceCT, reflowCT));

                Debug.WriteLine($"{modelId};{modelSpec.pcbCountPerMB};{modelSpec.ledCountPerModel};{siplaceCT};{connCT};{reflowCT}");
                return new SmtEfficiencyNormStruct()
                {
                    connCT = Math.Ceiling(connCT),
                    outputPerHour = Math.Ceiling(3600 / lineCT * modelSpec.pcbCountPerMB),
                    reflowCT = Math.Ceiling(reflowCT),
                    siplaceCT = Math.Ceiling(siplaceCT),
                    modelSpec = modelSpec,
                    mbDimensionsLWmm = mbDimensionsLWmm
                };
            }

            private static double GetLgModelConnQty(string modelId)
            {
                if (modelId.StartsWith("K2") || modelId.StartsWith("G2"))
                {
                    int connMark = int.Parse(modelId[8].ToString());
                    if (connMark % 2 == 0) return 4;
                }
                return 2;
            }

            private static Tuple<double, double>GetLgModelMbDimension(string model)
            {

                if (model.StartsWith("33")) return new Tuple<double, double>(270, 270);
                if (model.StartsWith("22")) return new Tuple<double, double>(250, 250);
                if (model.StartsWith("32")) return new Tuple<double, double>(272, 230);
                return new Tuple<double, double>(600, 350);
            }

            private static Tuple<double, double> GetMbDimensions(MST.MES.Data_structures.DevToolsModelStructure dtModel00)
            {
                MST.MES.Data_structures.DevToolsModelStructure model = new MST.MES.Data_structures.DevToolsModelStructure();
                bool success = false;
                foreach (var subComponent in dtModel00.children)
                {
                    if (subComponent.nc12.StartsWith("6010616")) //SMD Assy
                    {
                        foreach (var component in subComponent.children)
                        {
                            if (component.name.StartsWith("PCB") & component.children.Count == 0)
                            {
                                model = component;
                                success = true;
                                break;
                            }
                            if (component.nc12.StartsWith("4010440"))
                            {
                                foreach (var pcb in component.children)
                                {
                                    if (pcb.name.StartsWith("MB"))
                                    {
                                        model = pcb;
                                        success = true;
                                        break;
                                    }
                                }
                            }
                            if (success) break;
                        }
                    }
                    if (success) break;
                }

                Tuple<double, double> result = new Tuple<double, double>(0, 0);
                if (success)
                {
                    try
                    {
                        string[] size = model.atributes["L x W"].ToUpper().Split('X');
                        string l = size[0];
                        string w = size[1];
                        result = new Tuple<double, double>(double.Parse(l, CultureInfo.InvariantCulture), double.Parse(w, CultureInfo.InvariantCulture));
                    }
                    catch(Exception ex)
                    {

                        return new Tuple<double, double>(-1, -1);
                    }
                }

                return result;
            }

            public static void ShowOperatorEfficiency(string operatorName, DateTime startDate, DateTime endDate, DataGridView grid)
            {
                grid.Rows.Clear();
                var smtRecords = DataContainer.sqlDataByOrder.SelectMany(o => o.Value.smt.smtOrders)
                                                             .Where(o => o.operatorSmt == operatorName)
                                                             .Where(o => o.smtStartDate.Date >= startDate & o.smtEndDate.Date <= endDate)
                                                             .Where(o => o.manufacturedQty > 0)
                                                             .GroupBy(o => o.smtStartDate.Date)
                                                             .ToDictionary(d => d.Key, o => o.ToList());

                foreach (var dayEntry in smtRecords)
                {
                    var grouppedByModel = dayEntry.Value.OrderBy(o=>o.smtEndDate).GroupBy(o => o.orderInfo.modelId);
                    grid.Rows.Add(dayEntry.Key.Date.ToString("dd-MM-yyyy"));
                    dgvTools.SetRowColor(grid.Rows[grid.Rows.Count - 1], Color.LightSteelBlue);
                    int dayRow = grid.Rows.Count - 1;
                    List<Tuple<double, double>> effDuration = new List<Tuple<double, double>>();
                    foreach (var modelEntry in grouppedByModel)
                    {
                        var duration = (modelEntry.Select(o => o.smtEndDate).Max() - modelEntry.Select(o => o.smtStartDate).Min()).TotalHours;
                        if (duration > 0.5) duration -= 0.5;
                        var totalQuantity = modelEntry.Select(o => o.manufacturedQty).Sum();
                        var norm = CalculateModelNormPerHour(modelEntry.Key, modelEntry.First().smtLine);
                        var eff = Math.Round(totalQuantity / duration / norm.outputPerHour * 100, 0);
                        effDuration.Add(new Tuple<double, double>(eff, duration));
                        string modelName = modelEntry.Key[2] == '-' ? modelEntry.Key : modelEntry.Key.Insert(4, " ").Insert(8, " ");
                        var dur = modelEntry.Select(o => o.smtEndDate).Max().Subtract(modelEntry.Select(o => o.smtStartDate).Min()).ToString((@"hh\:mm"));//.TotalMinutes;
                        //var hours = Math.Floor(dur / 60);
                        //var minutes = dur - hours * 60;

                        grid.Rows.Add(modelName,
                                      modelEntry.Select(o => o.smtStartDate).Min().ToShortTimeString(),
                                      modelEntry.Select(o => o.smtEndDate).Max().ToShortTimeString(),
                                      $"{dur}",
                                      totalQuantity,
                                      modelEntry.First().smtLine,
                                      $"{norm.outputPerHour} szt/h",
                                      eff + " %");
                    }
                    grid.Rows[dayRow].Cells[grid.Columns.Count - 1].Value = Math.Round(WeightedAverage(effDuration), 0) + " %";
                }
                dgvTools.ColumnsAutoSize(grid, DataGridViewAutoSizeColumnMode.AllCells);

            }

            private static double WeightedAverage(List<Tuple<double, double>> valueWeigth)
            {
                var sumOfWeigth = valueWeigth.Select(v => v.Item2).Sum();
                var sumWeightedValues = valueWeigth.Select(v => v.Item1 * v.Item2).Sum();
                return sumWeightedValues / sumOfWeigth;
            }
        }
    }
}

