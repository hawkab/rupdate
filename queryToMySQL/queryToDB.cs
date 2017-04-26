using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using System.Diagnostics;

namespace queryToMySQL
{
    public class queryToDB
    {
        public Tuple<DataTable, string, DataSet> executeQuery(string queryString)
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet("amount");
            string lInf = "";
        
            MySqlConnectionStringBuilder mysqlCSB;
            mysqlCSB = new MySqlConnectionStringBuilder();
            mysqlCSB.Server = "resumeup.mysql.ukraine.com.ua";
            mysqlCSB.Database = "resumeup_db";
            mysqlCSB.UserID = "resumeup_w1";
            mysqlCSB.Password = "i0r4x99g";//zx892r0h = ka8l963k = 81pn40uu = 8x3n62sz

            using (MySqlConnection con = new MySqlConnection())
            {
                con.ConnectionString = mysqlCSB.ConnectionString;
                MySqlCommand com = new MySqlCommand(queryString, con);
                try
                {
                    con.Open();
                    lInf = "Подключено к серверу бд. Версия: " + con.ServerVersion.ToString() + ".";

                    using (MySqlDataReader dr = com.ExecuteReader())
                    {
                        if (dr.HasRows) { dt.Load(dr); }
                    }
                }
                catch (Exception ex)
                {
                    lInf = "Ошибка подключения: " + ex.Message;
                    try
                    {
                        if (System.IO.File.Exists("data.xml") && queryString.ToLower().Contains("select"))
                        {
                            ds.ReadXml("data.xml");
                            lInf += " Последяя сессия с БД загружена.";
                            dt = ds.Tables [0];
                        }
                    }
                    catch (Exception exw) {Console.Write(exw.Message); }
                }
            }
            var result = new Tuple<DataTable, string, DataSet>(dt, lInf, ds);
            if (queryString.ToLower().Contains ("select") && lInf.Contains("Подключено"))
            {
                try
                {
                    if (System.IO.File.Exists("data.xml"))
                    {
                        DateTime saveNow = DateTime.Now;/* Копирование старого файла, в случае наличия. */
                        System.IO.File.Copy("data.xml", "data-" + saveNow.ToString("yyyy-MM-dd(HH.mm.ss)") + ".xml");
                        string sourceName = Environment.CurrentDirectory.ToString() + @"\data-" + saveNow.ToString("yyyy-MM-dd(HH.mm.ss)") + ".xml";
                        string targetName = Environment.CurrentDirectory.ToString() + @"\data.7z";
                        /* Архивирование предыдущего источника. */
                        System.Diagnostics.Process p = new System.Diagnostics.Process();
                        p.StartInfo.FileName = @"C:\Program Files\7-Zip\7z.exe";
                        p.StartInfo.Arguments  = "a \"" + targetName + "\" \"" + sourceName + "\" -mx=9";
                        p.Start();
                        /* Удаление ранее созданных источников данных. */
                        Process.Start("cmd.exe", string.Format("/c del /q data-*).xml"));
                    }
                    dt.WriteXml("data.xml");
                }
                catch { }
            }
            return result;
            }
        }
}