using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace KontrolaWizualnaRaport.TabOperations.Test
{
    public class TestStatisticsCharts
    {

        public static void DrawTestStatisticsChart()
        {
            string modelName = SharedComponents.Test.Charts.cbTestStatisticsModel.Text.Trim();
            if (modelName == "") return;
            
            string param1Text = SharedComponents.Test.Charts.cbTestStatisticsParam1.Text.Trim();
            if (param1Text != "")
            {
                testChartParamIndex paramIndex1 = testChartParamIndex.first;
                testPerameter param1;
                Enum.TryParse(param1Text, out param1);
                TestStatisticsCharts.DrawChart(paramIndex1, param1, modelName);
            }
            else
            {
                SharedComponents.Test.Charts.chartTestStatistics.Series["param1Series"].Points.Clear();
            }

                    string param2Text = SharedComponents.Test.Charts.cbTestStatisticsParam2.Text.Trim();
            if (param2Text != "")
            {
                testChartParamIndex paramIndex2 = testChartParamIndex.second;
                testPerameter param2;
                Enum.TryParse(param2Text, out param2);
                TestStatisticsCharts.DrawChart(paramIndex2, param2, modelName);
            }
            else
            {
                SharedComponents.Test.Charts.chartTestStatistics.Series["param2Series"].Points.Clear();
            }


        }
        public static void PrepareComponents()
        {
            SharedComponents.Test.Charts.cbTestStatisticsModel.Items.Clear();
            var orderNumberArray = DataContainer.sqlDataByOrder.Where(o=>o.Value.test != null).Select(o => o.Value.test.orderNo).ToArray();
            var models = DataContainer.sqlDataByOrder.Where(o => orderNumberArray.Contains(o.Value.kitting.orderNo)).Select(o => o.Value.kitting.modelId_12NCFormat).Distinct().OrderBy(m=>m).ToArray();
            SharedComponents.Test.Charts.cbTestStatisticsModel.Items.AddRange(models);

            SharedComponents.Test.Charts.cbTestStatisticsParam1.Items.Clear();
            SharedComponents.Test.Charts.cbTestStatisticsParam2.Items.Clear();
            foreach (testPerameter param in (testPerameter[])Enum.GetValues(typeof(testPerameter)))
            {
                SharedComponents.Test.Charts.cbTestStatisticsParam1.Items.Add(param);
                SharedComponents.Test.Charts.cbTestStatisticsParam2.Items.Add(param);
            }
            SetUpChart(SharedComponents.Test.Charts.chartTestStatistics);
            

        }

        public static void LoadTestParametersBasedOnModel(string modelName)
        {
            SharedComponents.Test.Charts.chartTestStatistics.Series["param1Series"].Points.Clear();
            SharedComponents.Test.Charts.chartTestStatistics.Series["param2Series"].Points.Clear();

            var ordersArray = DataContainer.sqlDataByProcess.Kitting.Where(o => o.Value.modelId_12NCFormat == modelName).Select(o => o.Value.orderNo).ToArray();
            var filteredByOrder = DataContainer.sqlDataByProcess.Test.Where(o => ordersArray.Contains(o.Key)).SelectMany(o => o.Value.pcbDict).Select(pcb => pcb.Value.testEntries.First()).OrderBy(rec => rec.testTime).ToList();
            string testerName = SharedComponents.Test.Charts.cbTestStatisticsTester.Text.Trim();
            if (testerName != "") 
            {
                filteredByOrder.RemoveAll(rec => rec.testerName!=testerName);
            }

            List<string> props = new List<string>();

            if (filteredByOrder.Where(rec => rec.cct >= 0).Any()) {
                props.Add("cct");
            }
            if (filteredByOrder.Where(rec => rec.cri >= 0).Any()) {
                props.Add("cri");
            }
            if (filteredByOrder.Where(rec => rec.cX >= 0).Any()) {
                props.Add("cX");
            }
            if (filteredByOrder.Where(rec => rec.cY >= 0).Any()) {
                props.Add("cY");
            }
            if (filteredByOrder.Where(rec => rec.i >= 0).Any()) {
                props.Add("i");
            }
            if (filteredByOrder.Where(rec => rec.lm >= 0).Any()) {
                props.Add("lm");
            }
            if (filteredByOrder.Where(rec => rec.lmW >= 0).Any()) {
                props.Add("lmW");
            }
            if (filteredByOrder.Where(rec => rec.sdcm >= 0).Any()) {
                props.Add("sdcm");
            }
            if (filteredByOrder.Where(rec => rec.v >= 0).Any()) {
                props.Add("v");
            }
            if (filteredByOrder.Where(rec => rec.w >= 0).Any()) {
                props.Add("w");
            }

            SharedComponents.Test.Charts.cbTestStatisticsParam1.Items.Clear();
            SharedComponents.Test.Charts.cbTestStatisticsParam1.Items.Add("");
            SharedComponents.Test.Charts.cbTestStatisticsParam1.Items.AddRange(props.ToArray());
            SharedComponents.Test.Charts.cbTestStatisticsParam2.Items.Clear();
            SharedComponents.Test.Charts.cbTestStatisticsParam2.Items.Add("");
            SharedComponents.Test.Charts.cbTestStatisticsParam2.Items.AddRange(props.ToArray());

            var testers = filteredByOrder.Select(rec => rec.testerName).Distinct().ToArray();
            SharedComponents.Test.Charts.cbTestStatisticsTester.Items.Clear();
            SharedComponents.Test.Charts.cbTestStatisticsTester.Items.Add("");
            SharedComponents.Test.Charts.cbTestStatisticsTester.Items.AddRange(testers);

            SetUpChart(SharedComponents.Test.Charts.chartTestStatistics);
        }

        public static void LoadTestParametersBasedOnTester(string modelName, string testerName)
        {
            SharedComponents.Test.Charts.chartTestStatistics.Series["param1Series"].Points.Clear();
            SharedComponents.Test.Charts.chartTestStatistics.Series["param2Series"].Points.Clear();

            var ordersArray = DataContainer.sqlDataByProcess.Kitting.Where(o => o.Value.modelId_12NCFormat == modelName).Select(o => o.Value.orderNo).ToArray();
            var filteredByOrder = DataContainer.sqlDataByProcess.Test.Where(o => ordersArray.Contains(o.Key)).SelectMany(o => o.Value.pcbDict).Select(pcb => pcb.Value.testEntries.First()).OrderBy(rec => rec.testTime).ToList();

            if (testerName != "")
            {
                filteredByOrder.RemoveAll(rec => rec.testerName != testerName);
            }

            List<string> props = new List<string>();

            if (filteredByOrder.Where(rec => rec.cct >= 0).Any())
            {
                props.Add("cct");
            }
            if (filteredByOrder.Where(rec => rec.cri >= 0).Any())
            {
                props.Add("cri");
            }
            if (filteredByOrder.Where(rec => rec.cX >= 0).Any())
            {
                props.Add("cX");
            }
            if (filteredByOrder.Where(rec => rec.cY >= 0).Any())
            {
                props.Add("cY");
            }
            if (filteredByOrder.Where(rec => rec.i >= 0).Any())
            {
                props.Add("i");
            }
            if (filteredByOrder.Where(rec => rec.lm >= 0).Any())
            {
                props.Add("lm");
            }
            if (filteredByOrder.Where(rec => rec.lmW >= 0).Any())
            {
                props.Add("lmW");
            }
            if (filteredByOrder.Where(rec => rec.sdcm >= 0).Any())
            {
                props.Add("sdcm");
            }
            if (filteredByOrder.Where(rec => rec.v >= 0).Any())
            {
                props.Add("v");
            }
            if (filteredByOrder.Where(rec => rec.w >= 0).Any())
            {
                props.Add("w");
            }

            SharedComponents.Test.Charts.cbTestStatisticsParam1.Items.Clear();
            SharedComponents.Test.Charts.cbTestStatisticsParam1.Items.Add("");
            SharedComponents.Test.Charts.cbTestStatisticsParam1.Items.AddRange(props.ToArray());
            
            SharedComponents.Test.Charts.cbTestStatisticsParam2.Items.Clear();
            SharedComponents.Test.Charts.cbTestStatisticsParam2.Items.Add("");
            SharedComponents.Test.Charts.cbTestStatisticsParam2.Items.AddRange(props.ToArray());

        }

        public enum testPerameter
        {
            v,
            w,
            i,
            lm,
            lmW,
            cct,
            cri,
            cX,
            cY,
            sdcm, //all double
            
        }

        public enum testChartParamIndex
        {
            first=1,
            second=2
        }

        public static void SetUpChart(Chart chart)
        {
            chart.Series.Clear();
            chart.ChartAreas.Clear();

            Series param1Series = new Series()
            {
                ChartType = SeriesChartType.Line,
                Name = "param1Series",
                Color = System.Drawing.Color.FromArgb(255, 41, 128, 185),
                YAxisType = AxisType.Primary,
                BorderWidth = 2,
                MarkerSize = 20,
                MarkerStyle = MarkerStyle.Circle,
                ToolTip = "#VAL{0.0}"
            };

            Series param2Series = new Series()
            {
                ChartType = SeriesChartType.Line,
                Name = "param2Series",
                Color = System.Drawing.Color.FromArgb(255, 192, 57, 43),
                YAxisType = AxisType.Secondary,
                BorderWidth = 2,
                MarkerSize = 20,
                MarkerStyle = MarkerStyle.Circle,
                ToolTip = "#VAL{0.0}"
            };

            ChartArea ar = new ChartArea();
            ar.AxisX.Interval = 10;
            //ar.AxisY.LabelStyle.Format = "{#.0}";
            //ar.AxisY2.LabelStyle.Format = "{#.0}";
            ar.AxisX.LabelStyle.Font = new System.Drawing.Font("Arial", 8);
            ar.AxisX.LabelStyle.Angle = 90;
            ar.AxisX.IsMarginVisible = false;
            ar.Position = new ElementPosition(0, 0, 100, 100);
            //ar.AxisX.MajorGrid.LineColor = System.Drawing.Color.Gray;
            ar.AxisX.LineColor = System.Drawing.Color.Gray;

            ar.AxisY.MajorGrid.LineColor = System.Drawing.Color.Gray;
            ar.AxisY.MinorGrid.Enabled = true;
            ar.AxisY.MinorGrid.LineColor = System.Drawing.Color.LightGray;
            ar.AxisY.LabelStyle.ForeColor = System.Drawing.Color.FromArgb(255, 41, 128, 185);
            ar.AxisY.LineColor = System.Drawing.Color.FromArgb(255, 41, 128, 185);

            ar.AxisY2.MajorGrid.LineColor = System.Drawing.Color.Gray;
            ar.AxisY2.MinorGrid.Enabled = true;
            ar.AxisY2.MinorGrid.LineColor = System.Drawing.Color.LightGray;
            ar.AxisY2.LabelStyle.ForeColor = System.Drawing.Color.FromArgb(255, 192, 57, 43);
            ar.AxisY.LineColor = System.Drawing.Color.FromArgb(255, 192, 57, 43);

            chart.Series.Add(param1Series);
            chart.Series.Add(param2Series);
            chart.ChartAreas.Add(ar);

            chart.Legends[0].Position.Auto = false;
            chart.Legends[0].Position.X = 5;
            chart.Legends[0].Position.Y = 0;
            chart.Legends[0].Position.Width = 10;
            chart.Legends[0].Position.Height = 3;
        }

        public static void DrawChart(testChartParamIndex paramIndex, testPerameter paramToDraw, string modelName)
        {
            var ordersArray = DataContainer.sqlDataByProcess.Kitting.Where(o => o.Value.modelId_12NCFormat == modelName).Select(o => o.Value.orderNo).ToArray();
            var filtredByOrder = DataContainer.sqlDataByProcess.Test.Where(o => ordersArray.Contains(o.Key)).SelectMany(o => o.Value.pcbDict).Select(pcb => pcb.Value.testEntries.First()).OrderBy(rec => rec.testTime).ToList();
            if (!SharedComponents.Test.Charts.chkBoxTestStatisticsOK.Checked)
            {
                filtredByOrder.RemoveAll(rec=>rec.testResultOk);
            }
            if (!SharedComponents.Test.Charts.chkBoxTestStatisticsNG.Checked)
            {
                filtredByOrder.RemoveAll(rec => !rec.testResultOk);
            }

            string selectedTester = SharedComponents.Test.Charts.cbTestStatisticsTester.Text;
            if (selectedTester.Trim() != "")
            {
                filtredByOrder.RemoveAll(rec => rec.testerName!=selectedTester);
            }

            Chart chart = SharedComponents.Test.Charts.chartTestStatistics;
            
            Series se;
            if (paramIndex == testChartParamIndex.first)
            {
                se = chart.Series["param1Series"];
                se.LegendText =paramToDraw.ToString() ;
                //chart.ChartAreas[0].AxisY.Title = $"wyniki pomiaru parametru: {paramToDraw.ToString()}";
            }
            else if(paramIndex == testChartParamIndex.second)
            {
                se = chart.Series["param2Series"];
                se.LegendText = paramToDraw.ToString();
                //chart.ChartAreas[0].AxisY2.Title = $"wyniki pomiaru parametru: {paramToDraw.ToString()}";
            }
            else
            {
                return;
            }

            se.Points.Clear();
            DataTable tagTable = new DataTable();
            tagTable.Columns.Add("serial");
            tagTable.Columns.Add("testDate");
            tagTable.Columns.Add("orderNo");
            tagTable.Columns.Add("pomiar");

            foreach (var testRecord in filtredByOrder)
            {
                FieldInfo[] recordFields = testRecord.GetType().GetFields();
                var field = recordFields.Where(f => f.Name == paramToDraw.ToString()).First();
                double fieldValue = (double)field.GetValue(testRecord);
                DateTime testDate = testRecord.testTime;

                DataPoint pt = new DataPoint();
                pt.MarkerSize = 5;
                pt.SetValueXY(testDate.ToString("dd-MM HH:mm"), fieldValue);

                se.Points.Add(pt);

                tagTable.Rows.Add(testRecord.serialNo, testRecord.testTime, testRecord.orderNo, fieldValue);
            }

            se.Tag = tagTable;

            if (chart.Series[0].Points.Count > 0)
            {
                var y1Min = chart.Series[0].Points.SelectMany(pt => pt.YValues).Min();
                var y1Max = chart.Series[0].Points.SelectMany(pt => pt.YValues).Max();
                chart.ChartAreas[0].AxisY.Minimum = y1Min * 0.999;
                chart.ChartAreas[0].AxisY.Maximum = y1Max * 1.001;
                chart.ChartAreas[0].AxisY.Interval = (chart.ChartAreas[0].AxisY.Maximum - chart.ChartAreas[0].AxisY.Minimum) / 10;
                chart.ChartAreas[0].AxisY.MinorGrid.Interval = (chart.ChartAreas[0].AxisY.Maximum - chart.ChartAreas[0].AxisY.Minimum) / 20;
                if (y1Max - y1Min < 1)
                {
                    chart.ChartAreas[0].AxisY.LabelStyle.Format = "{#.00}";
                }else if (y1Max - y1Min < 10)
                {
                    chart.ChartAreas[0].AxisY.LabelStyle.Format = "{#.0}";
                }
                else 
                {
                    chart.ChartAreas[0].AxisY.LabelStyle.Format = "{#}";
                }
            }

            if (chart.Series[1].Points.Count > 0)
            {
                var y2Min = chart.Series[1].Points.SelectMany(pt => pt.YValues).Min();
                var y2Max = chart.Series[1].Points.SelectMany(pt => pt.YValues).Max();
                chart.ChartAreas[0].AxisY2.Minimum = y2Min * 0.999;
                chart.ChartAreas[0].AxisY2.Maximum = y2Max * 1.001;
                chart.ChartAreas[0].AxisY2.Interval = (chart.ChartAreas[0].AxisY2.Maximum - chart.ChartAreas[0].AxisY2.Minimum) / 10;
                chart.ChartAreas[0].AxisY.MinorGrid.Interval = (chart.ChartAreas[0].AxisY.Maximum - chart.ChartAreas[0].AxisY.Minimum) / 20;
                if (y2Max - y2Min < 0.1)
                {
                    chart.ChartAreas[0].AxisY2.LabelStyle.Format = "{#.000}";
                }
                else if (y2Max - y2Min < 1)
                {
                    chart.ChartAreas[0].AxisY2.LabelStyle.Format = "{#.00}";
                }
                else if (y2Max - y2Min < 10)
                {
                    chart.ChartAreas[0].AxisY2.LabelStyle.Format = "{#.0}";
                }
                else
                {
                    chart.ChartAreas[0].AxisY2.LabelStyle.Format = "{#}";
                }
            }
            var xPointsCount = chart.Series[0].Points.Count();
            if (xPointsCount > 0)
            {
                chart.ChartAreas[0].AxisX.Interval = xPointsCount / 20;
            }
            
            

        }
    }
}
