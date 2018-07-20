﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
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
            command.Connection = conn;
            command.CommandText = @"SELECT Id,Data_czas,Operator,iloscDobrych,numerZlecenia,ngBrakLutowia,ngBrakDiodyLed,ngBrakResConn,ngPrzesuniecieLed,ngPrzesuniecieResConn,ngZabrudzenieLed,ngUszkodzenieMechaniczneLed,ngUszkodzenieConn,ngWadaFabrycznaDiody,ngUszkodzonePcb,ngWadaNaklejki,ngSpalonyConn,ngInne,scrapBrakLutowia,scrapBrakDiodyLed,scrapBrakResConn,scrapPrzesuniecieLed,scrapPrzesuniecieResConn,scrapZabrudzenieLed,scrapUszkodzenieMechaniczneLed,scrapUszkodzenieConn,scrapWadaFabrycznaDiody,scrapUszkodzonePcb,scrapWadaNaklejki,scrapSpalonyConn,scrapInne,ngTestElektryczny FROM tb_Kontrola_Wizualna_Karta_Pracy where Data_czas>@dataCzas;";
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

        public static Dictionary<DateTime, SortedDictionary<int, Dictionary<string, Dictionary<string, DataTable>>>> GetTestMeasurements (int daysAgo)
        {
            Dictionary<DateTime, SortedDictionary<int, Dictionary<string, Dictionary<string, DataTable>>>> result = new Dictionary<DateTime, SortedDictionary<int, Dictionary<string, Dictionary<string, DataTable>>>>();
            DataTable sqlTable = new DataTable();
            string untilDay = DateTime.Now.Date.AddDays(daysAgo * (-1)).AddHours(6).ToString("yyyy-MM-dd") ;

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText = String.Format(@"SELECT serial_no,inspection_time,wip_entity_name,tester_id FROM MES.dbo.tb_tester_measurements WHERE inspection_time>@until and tester_id<>'0' order by inspection_time;");
            command.Parameters.AddWithValue("@until", untilDay);
            Debug.WriteLine("allala");
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            try
            {
                adapter.Fill(sqlTable);
            }
            catch(SqlException ex)
            {
                MessageBox.Show(ex.ErrorCode+Environment.NewLine+ ex.Message + Environment.NewLine + ex.HResult);
                return result;
            }

            sqlTable.Columns["inspection_time"].ColumnName = "Data";
            sqlTable.Columns["tester_id"].ColumnName = "Tester";
            sqlTable.Columns["serial_no"].ColumnName = "PCB";
            sqlTable.Columns["wip_entity_name"].ColumnName = "LOT";

            
            foreach (DataRow row in sqlTable.Rows)
            {
                string lineID = row["Tester"].ToString();
                string testerID = TestOperations.testerIdToName(lineID);

                if (testerID == "") continue;

                DateTime inspTime = DateTime.Parse(row["Data"].ToString());
                dateShiftNo shiftInfo = dateTools. whatDayShiftIsit(inspTime);
                string lot = row["LOT"].ToString();
                
                if (!result.ContainsKey(shiftInfo.fixedDate.Date))
                {
                    result.Add(shiftInfo.fixedDate.Date, new SortedDictionary<int, Dictionary<string, Dictionary<string, DataTable>>>());
                }
                if (!result[shiftInfo.fixedDate.Date].ContainsKey(shiftInfo.shift))
                {
                    result[shiftInfo.fixedDate.Date].Add(shiftInfo.shift, new Dictionary<string, Dictionary<string, DataTable>>());
                }
                if (!result[shiftInfo.fixedDate.Date][shiftInfo.shift].ContainsKey(testerID))
                {
                    result[shiftInfo.fixedDate.Date][shiftInfo.shift].Add(testerID, new Dictionary<string, DataTable>());
                }
                if (!result[shiftInfo.fixedDate.Date][shiftInfo.shift][testerID].ContainsKey(lot))
                {
                    result[shiftInfo.fixedDate.Date][shiftInfo.shift][testerID].Add(lot, sqlTable.Clone());
                }
                result[shiftInfo.fixedDate.Date][shiftInfo.shift][testerID][lot].Rows.Add(row.ItemArray);
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

        public static DataTable GetStencilSmtRecords()
        {
            DataTable result = new DataTable();

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText = String.Format(@"SELECT IloscWykonana,StencilQR,Model FROM MES.dbo.tb_SMT_Karta_Pracy;");

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
                int ledSumA = int.Parse(row["A_PKG_QTY"].ToString());
                int ledSumB = int.Parse(row["B_PKG_QTY"].ToString());
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

        public static DataTable GetKittingInfoForLot(string lot)
        {
            DataTable result = new DataTable();

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText =
                @"SELECT Nr_Zlecenia_Produkcyjnego,NC12_wyrobu,Ilosc_wyrobu_zlecona,DataCzasWydruku,Data_Konca_Zlecenia,RankA,RankB,Ilosc_wyr_dobrego,Ilosc_wyr_do_poprawy,Ilosc_wyr_na_zlom,Numer_Klienta FROM tb_Zlecenia_produkcyjne where Nr_Zlecenia_Produkcyjnego=@lot;";
            command.Parameters.AddWithValue("@lot" , lot);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(result);

            return result;
        }

        public static DataTable GetLedRework()
        {
            DataTable result = new DataTable();

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText = String.Format(@"SELECT Data,Operator,Model,SerialNo,OpisNaprawy,NaprawianyKomponent,WynikNaprawy FROM tb_NaprawaLED_Karta_Pracy order by Data;");

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(result);

            return result;
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

        public static DataTable GetVisInspForLotL(string lot)
        {

            DataTable tabletoFill = new DataTable();

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText = @"SELECT Id,Data_czas,Operator,iloscDobrych,numerZlecenia,ngBrakLutowia,ngBrakDiodyLed,ngBrakResConn,ngPrzesuniecieLed,ngPrzesuniecieResConn,ngZabrudzenieLed,ngUszkodzenieMechaniczneLed,ngUszkodzenieConn,ngWadaFabrycznaDiody,ngUszkodzonePcb,ngWadaNaklejki,ngSpalonyConn,ngInne,scrapBrakLutowia,scrapBrakDiodyLed,scrapBrakResConn,scrapPrzesuniecieLed,scrapPrzesuniecieResConn,scrapZabrudzenieLed,scrapUszkodzenieMechaniczneLed,scrapUszkodzenieConn,scrapWadaFabrycznaDiody,scrapUszkodzonePcb,scrapWadaNaklejki,scrapSpalonyConn,scrapInne,ngTestElektryczny FROM tb_Kontrola_Wizualna_Karta_Pracy where numerZlecenia=@lot;";
            //@"SELECT Data_czas,Operator,iloscDobrych,numerZlecenia,ngBrakLutowia,ngBrakDiodyLed,ngBrakResConn,ngPrzesuniecieLed,ngPrzesuniecieResConn,ngZabrudzenieLed,ngUszkodzenieMechaniczneLed,ngUszkodzenieConn,ngWadaFabrycznaDiody,ngUszkodzonePcb,ngWadaNaklejki,ngSpalonyConn,ngInne,scrapBrakLutowia,scrapBrakDiodyLed,scrapBrakResConn,scrapPrzesuniecieLed,scrapPrzesuniecieResConn,scrapZabrudzenieLed,scrapUszkodzenieMechaniczneLed,scrapUszkodzenieConn,scrapWadaFabrycznaDiody,scrapUszkodzonePcb,scrapWadaNaklejki,scrapSpalonyConn,scrapInne,ngTestElektryczny FROM tb_Kontrola_Wizualna_Karta_Pracy WHERE Data_czas > '" + DateTime.Now.AddDays(-90).ToShortDateString() + "';";
            command.Parameters.AddWithValue("@lot", lot);
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

        public static Dictionary<string, Dictionary<string, DataTable>> GetTestMeasurementsForLot(string lot)
        {
            DataTable sqlTable = new DataTable();

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText = String.Format(@"SELECT serial_no,inspection_time,tester_id,wip_entity_id,wip_entity_name,program_id,result,ng_type,lm,lm_w,sdcm,cri,cct,v,i,w,x,y,r9,bin,lx,retest,module_num,lm1_gain,x1_offset,y1_offset,vf1_offset,cri1_offset,cct1_offset,lm1_master,x1_master,y1_master,vf1_master,cri1_master,cct1_master,hi_pot,light_on,optical,result_int FROM MES.dbo.tb_tester_measurements WHERE wip_entity_name=@lot order by inspection_time DESC;");
            command.Parameters.AddWithValue("@lot", lot);
            
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            try
            {
                adapter.Fill(sqlTable);
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.HResult);
            }

            Dictionary<string, Dictionary<string, DataTable>> testerPcbMeasurements = new Dictionary<string, Dictionary<string, DataTable>>();
            foreach (DataRow row in sqlTable.Rows)
            {
                string tester = TestOperations.testerIdToName(row["tester_id"].ToString());
                if (tester == "") continue;
                string pcb = row["serial_no"].ToString();


                if (!testerPcbMeasurements.ContainsKey(tester))
                {
                    testerPcbMeasurements.Add(tester, new Dictionary<string, DataTable>());
                }
                if (!testerPcbMeasurements[tester].ContainsKey(pcb))
                {
                    testerPcbMeasurements[tester].Add(pcb, sqlTable.Clone());
                    testerPcbMeasurements[tester][pcb].Rows.Add(row.ItemArray);
                }
            }
           

            return testerPcbMeasurements;
        }

        public static Dictionary<string, List<string>> GetBoxingInfo(List<string> inputPcbs)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            DataTable sqlTable = new DataTable();
            
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText = String.Format(@"SELECT serial_no,Box_LOT_NO,Boxing_Date FROM tb_WyrobLG_opakowanie WHERE ");
            for (int i = 0; i < inputPcbs.Count; i++) 
            {
                if (i > 0) 
                {
                    command.CommandText += " OR ";
                }
                command.CommandText += "serial_no=@pcb" + i;
                command.Parameters.AddWithValue("@pcb" + i, inputPcbs[i]);
            }
            command.CommandText += ";";

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(sqlTable);

            foreach (DataRow row in sqlTable.Rows)
            {
                string boxIdDate = row["Box_LOT_NO"].ToString() + " " + row["Boxing_Date"].ToString();
                inputPcbs.Remove(row["serial_no"].ToString());

                if(!result.ContainsKey(boxIdDate))
                {
                    result.Add(boxIdDate, new List<string>());
                }
                result[boxIdDate].Add(row["serial_no"].ToString());
            }

            //result.Add("Matching", new List<string>());
            //foreach (var pcb in inputPcbs)
            //{
            //    result["Matching"].Add(pcb);
            //}
             return result;
        }
    }
}
