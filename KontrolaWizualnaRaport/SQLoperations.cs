using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using static KontrolaWizualnaRaport.dateTools;
using static KontrolaWizualnaRaport.SMTOperations;

namespace KontrolaWizualnaRaport
{

    class SQLoperations
    {
        private readonly Form1 form;
        private readonly TextBox console;

        public SQLoperations(Form1 form, TextBox console)
        {
            this.form = form;
            this.console = console;
        }

        public static DataTable DownloadVisInspFromSQL(int daysAgo)
        {
            DateTime tillDate = System.DateTime.Now.AddDays(daysAgo * (-1));
            HashSet<string> result = new HashSet<string>();
            DataTable tabletoFill = new DataTable();

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.CommandTimeout = 60;
            command.Connection = conn;
            command.CommandText = @"SELECT Id,Data_czas,Operator,iloscDobrych,numerZlecenia,ngBrakLutowia,ngBrakDiodyLed,ngBrakResConn,ngPrzesuniecieLed,ngPrzesuniecieResConn,ngZabrudzenieLed,ngUszkodzenieMechaniczneLed,ngUszkodzenieConn,ngWadaFabrycznaDiody,ngUszkodzonePcb,ngWadaNaklejki,ngSpalonyConn,ngInne,scrapBrakLutowia,scrapBrakDiodyLed,scrapBrakResConn,scrapPrzesuniecieLed,scrapPrzesuniecieResConn,scrapZabrudzenieLed,scrapUszkodzenieMechaniczneLed,scrapUszkodzenieConn,scrapWadaFabrycznaDiody,scrapUszkodzonePcb,scrapWadaNaklejki,scrapSpalonyConn,scrapInne,ngTestElektryczny FROM tb_Kontrola_Wizualna_Karta_Pracy where Data_czas>@dataCzas AND Operator<>'test' order by Data_czas;";
            //@"SELECT Data_czas,Operator,iloscDobrych,numerZlecenia,ngBrakLutowia,ngBrakDiodyLed,ngBrakResConn,ngPrzesuniecieLed,ngPrzesuniecieResConn,ngZabrudzenieLed,ngUszkodzenieMechaniczneLed,ngUszkodzenieConn,ngWadaFabrycznaDiody,ngUszkodzonePcb,ngWadaNaklejki,ngSpalonyConn,ngInne,scrapBrakLutowia,scrapBrakDiodyLed,scrapBrakResConn,scrapPrzesuniecieLed,scrapPrzesuniecieResConn,scrapZabrudzenieLed,scrapUszkodzenieMechaniczneLed,scrapUszkodzenieConn,scrapWadaFabrycznaDiody,scrapUszkodzonePcb,scrapWadaNaklejki,scrapSpalonyConn,scrapInne,ngTestElektryczny FROM tb_Kontrola_Wizualna_Karta_Pracy WHERE Data_czas > '" + DateTime.Now.AddDays(-90).ToShortDateString() + "';";
            command.Parameters.AddWithValue("@dataCzas", tillDate);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            //try
            {
                adapter.Fill(tabletoFill);
            }
           // catch (Exception e)
            {
                //console.Text+="OP LOADER: " + e.Message + Environment.NewLine;
            }
            return tabletoFill;
        }

        public static DataTable lotTable()
        {
            DataTable result = new DataTable();

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText =
                @"SELECT Nr_Zlecenia_Produkcyjnego,Data_Poczatku_Zlecenia,NC12_wyrobu,Ilosc_wyrobu_zlecona,LiniaProdukcyjna,DataCzasWydruku,Data_Konca_Zlecenia FROM tb_Zlecenia_produkcyjne;";

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(result);

            return result;
        }

        public static Dictionary<string,string>[] LotList()
        {
            Dictionary<string, string> result1 = new Dictionary<string, string>();
            Dictionary<string, string> result2 = new Dictionary<string, string>();
            Dictionary<string, string> result3 = new Dictionary<string, string>();

            DataTable sqlTable = new DataTable();
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText =
                @"SELECT Nr_Zlecenia_Produkcyjnego,NC12_wyrobu,Ilosc_wyrobu_zlecona,LiniaProdukcyjna,DataCzasWydruku FROM tb_Zlecenia_produkcyjne order by DataCzasWydruku;";

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(sqlTable);

            foreach (DataRow row in sqlTable.Rows)
            {
                if (result1.ContainsKey(row["Nr_Zlecenia_Produkcyjnego"].ToString())) continue;
                result1.Add(row["Nr_Zlecenia_Produkcyjnego"].ToString(), row["NC12_wyrobu"].ToString().Replace("LLFML",""));
                result2.Add(row["Nr_Zlecenia_Produkcyjnego"].ToString(), row["Ilosc_wyrobu_zlecona"].ToString());
                result3.Add(row["Nr_Zlecenia_Produkcyjnego"].ToString(), row["LiniaProdukcyjna"].ToString());
            }
            Dictionary<string, string>[] result = new Dictionary<string, string>[] { result1, result2, result3 };
            return result;
        }

        public static Dictionary<DateTime, SortedDictionary<int,Dictionary<string, DataTable>>> GetBoxing(int daysAgo)
        {
            DataTable sqlTable = new DataTable();
            DateTime untilDay = DateTime.Now.Date.AddDays(daysAgo * (-1)).AddHours(6);

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText = String.Format(@"SELECT Boxing_Date,NC12_wyrobu,Wysylki_Nr FROM v_WyrobLG_opakowanie_all WHERE Boxing_Date>@until order by Boxing_Date;");
            command.Parameters.AddWithValue("@until", untilDay);

            SqlDataAdapter adapter = new SqlDataAdapter(command);

                adapter.Fill(sqlTable);

            sqlTable.Columns["Boxing_Date"].ColumnName = "Data";
            sqlTable.Columns["NC12_wyrobu"].ColumnName = "Model";

            Dictionary<DateTime, SortedDictionary<int, Dictionary<string, DataTable>>> result = new Dictionary<DateTime, SortedDictionary<int, Dictionary<string, DataTable>>>();
            foreach (DataRow row in sqlTable.Rows)
            {
                DateTime inspTime = DateTime.Parse(row["Data"].ToString());
                dateShiftNo shiftInfo = dateTools.whatDayShiftIsit(inspTime);
                string model = row["Model"].ToString().Replace("LLFML", "");

                if (!result.ContainsKey(shiftInfo.fixedDate.Date))
                {
                    result.Add(shiftInfo.fixedDate.Date, new SortedDictionary<int, Dictionary<string, DataTable>>());
                }
                if (!result[shiftInfo.fixedDate.Date].ContainsKey(shiftInfo.shift))
                {
                    result[shiftInfo.fixedDate.Date].Add(shiftInfo.shift, new Dictionary<string, DataTable>());
                }
                if (!result[shiftInfo.fixedDate.Date][shiftInfo.shift].ContainsKey(model))
                {
                    result[shiftInfo.fixedDate.Date][shiftInfo.shift].Add(model, sqlTable.Clone());

                }
                result[shiftInfo.fixedDate.Date][shiftInfo.shift][model].Rows.Add(row.ItemArray);
            }
            return result;
        }

        

        public static List<string> lotToPcbSerialNo(string lot)
        {
            
            DataTable sqlTable = new DataTable();
            List<string> result = new List<string>();

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText = String.Format(@"SELECT serial_no,wip_entity_name,result FROM MES.dbo.tb_tester_measurements WHERE wip_entity_name=@lot AND result='OK';");
            command.Parameters.AddWithValue("@lot", lot);

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            try
            {
                adapter.Fill(sqlTable);
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.ErrorCode + Environment.NewLine + ex.Message + Environment.NewLine + ex.HResult);

            }

            foreach (DataRow row in sqlTable.Rows)
            {
                result.Add(row["serial_no"].ToString());
            }

            return result;
        }

        public static DataTable GetSmtRecordsFromDb(DateTime startDate, DateTime endDate)
        {
            DataTable result = new DataTable();

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText = String.Format(@"SELECT DataCzasStart,DataCzasKoniec,LiniaSMT,OperatorSMT,NrZlecenia,Model,IloscWykonana,NGIlosc,ScrapIlosc,KoncowkiLED,StencilQR FROM MES.dbo.tb_SMT_Karta_Pracy WHERE DataCzasKoniec>=@startDate AND DataCzasKoniec<=@endDate order by [DataCzasKoniec];");
            command.Parameters.AddWithValue("@startDate", startDate);
            command.Parameters.AddWithValue("@endDate", endDate);

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(result);

            return result;
        }

        public static DataTable GetSmtLedTraceability()
        {
            DataTable result = new DataTable();

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText = String.Format(@"SELECT DataCzasKoniec,LiniaSMT,NrZlecenia,Model,KoncowkiLED FROM MES.dbo.tb_SMT_Karta_Pracy;");

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(result);

            return result;
        }

        public static DataTable GetStencilSmtRecords()
        {
            DataTable result = new DataTable();

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText = String.Format(@"SELECT IloscWykonana,StencilQR,Model,DataCzasKoniec FROM MES.dbo.tb_SMT_Karta_Pracy;");

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(result);

            return result;
        }

        public static Dictionary<string,string> lotToSmtLine(int daysAgo)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            DataTable sqlTable = new DataTable();
            DateTime untilDay = DateTime.Now.Date.AddDays(daysAgo * (-1)).AddHours(6);

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText = String.Format(@"SELECT DataCzasKoniec,LiniaSMT,NrZlecenia FROM MES.dbo.tb_SMT_Karta_Pracy WHERE DataCzasKoniec>@until order by [DataCzasKoniec];");
            command.Parameters.AddWithValue("@until", untilDay);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(sqlTable);

            foreach (DataRow row in sqlTable.Rows)
            {
                if (!result.ContainsKey(row["NrZlecenia"].ToString()))
                {
                    result.Add(row["NrZlecenia"].ToString(), row["LiniaSMT"].ToString());
                }
            }

            return result;
        }

        public static Dictionary<string, MesModels> GetMesModels()
        {
            Dictionary<string, MesModels> result = new Dictionary<string, MesModels>();

            DataTable sqlTable = new DataTable();
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText =
                @"SELECT MODEL_ID,PKG_SUM_QTY,A_PKG_QTY,B_PKG_QTY,SMT_Carrier_QTY FROM tb_MES_models;";

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(sqlTable);

            foreach (DataRow row in sqlTable.Rows)
            {
                string model = row["MODEL_ID"].ToString().Replace("LLFML","");
                if (result.ContainsKey(model)) continue;
                int ledSum = int.Parse(row["PKG_SUM_QTY"].ToString());
                int ledSumA = 0;
                int ledSumB = 0;
                if (row["A_PKG_QTY"].ToString().Trim() != "")
                {
                    ledSumA = int.Parse(row["A_PKG_QTY"].ToString());
                    ledSumB = int.Parse(row["B_PKG_QTY"].ToString());
                }
                else
                {
                    ledSumA = (int)Math.Round((double)ledSum / 2,0, MidpointRounding.ToEven);
                    ledSumB = ledSum - ledSumA;
                }
                int pcbOnCarrier = int.Parse(row["SMT_Carrier_QTY"].ToString());

                string type = "";
                if (model.Contains("22-")|| model.Contains("33-")|| model.Contains("32-")|| model.Contains("53-"))
                {
                    type = "square";
                }
                else if(model.Contains("K1-")|| model.Contains("K2-")|| model.Contains("61-")|| model.Contains("K5-"))
                {
                    type = "long";
                }
                else if (model.Contains("G1-")|| model.Contains("G2-")|| model.Contains("G5-")|| model.Contains("31-"))
                {
                    type = "short";
                }
                else
                {
                    type = "veryShort";
                }
                MesModels newModel = new MesModels(ledSum, ledSumA, ledSumB, type, pcbOnCarrier);

                result.Add(model, newModel);
            }

            return result;
        }

        public static bool UpdateSmtRecord(List<Tuple<string, string>> colValuePairList, string lotNo)
        {
            bool succes = true;
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText = "UPDATE dbo.tb_SMT_Karta_Pracy SET ";

            for (int par = 0; par < colValuePairList.Count; par++) 
            {
                if (par > 0) 
                {
                    command.CommandText += ",";
                }
                command.CommandText += colValuePairList[par].Item1 + "=@val" + par;
                command.Parameters.AddWithValue("@val" + par, colValuePairList[par].Item2);
            }

            command.CommandText += " Where NrZlecenia=@lotNo;";
            command.Parameters.AddWithValue("@lotNo", lotNo);

            try
            {
                conn.Open();
                command.ExecuteNonQuery();
            }
            catch(SqlException ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.HResult);
                succes = false;
            }
            finally
            {
                conn.Close();
            }
            return succes;
        }


        public static DataTable GetSmtRecordsForLot(string lot)
        {
            DataTable result = new DataTable();

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText = String.Format(@"SELECT DataCzasStart,DataCzasKoniec,LiniaSMT,OperatorSMT,NrZlecenia,Model,IloscWykonana,NGIlosc,ScrapIlosc,KoncowkiLED FROM MES.dbo.tb_SMT_Karta_Pracy WHERE NrZlecenia=@lot;");
            command.Parameters.AddWithValue("@lot", lot);

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(result);

            return result;
        }


        public static List<string[]> GetZlecenieString(List<string[]> ncIdPair)
        {
            DataTable tabletoFill = new DataTable();

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=ConnectToMSTDB;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText = @"SELECT ID,ZlecenieString,NC12 FROM DaneBierzaceKompAktualne_FULL where ";

            for (int i = 0; i < ncIdPair.Count; i++)
            {
                if (i > 0)
                {
                    command.CommandText += " OR ";
                }
                command.CommandText += $"(ID = @id{i} and NC12 = @nc12{i})";
                command.Parameters.AddWithValue("@id" + i, ncIdPair[i][1]);
                command.Parameters.AddWithValue("@nc12" + i, ncIdPair[i][0]);
            }
            command.CommandText += ";";


            SqlDataAdapter adapter = new SqlDataAdapter(command);
            //try
            {
                adapter.Fill(tabletoFill);
            }
            List<string[]> result = new List<string[]>();

            foreach (DataRow row in tabletoFill.Rows)
            {
                string id = row["ID"].ToString();
                string nc12 = row["NC12"].ToString();
                string lot = row["ZlecenieString"].ToString();
                result.Add(new string[] { nc12, id, lot });
            }
            return result;
        }
    }
}
