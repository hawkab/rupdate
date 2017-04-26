using Auth;
using System;
using System.Linq;
using System.Data;
using System.Text;
using queryToMySQL;
using System.Threading;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace rupdate
{
    class Program
    {
        static void Main(string[] args)
        {
            //for (int i = 1; i < 5; i++) 
            //{/* Определение hardwareId, проверка его наличия на сервере */
            //    if (!HWID.checkIt())
            //    {
            //        Thread.Sleep(5 * 1000);
            //    }
            //    else { i = 5; }
            //} if (!HWID.checkIt()) { Environment.Exit(0); }*/
            if (args.Count() > 0)
            {
                /* Обработка командной аргументов строки. */
                int i = 0, usId = 0, seId = 0;
                string slogin = "", spassword = "";
                foreach (string line in args)
                {
                    if (i > 0)
                    {
                        try
                        {
                            switch (args[i - 1])
                            {
                                case "-id": usId = Convert.ToInt32(line); // - Идентификатор резюме в бд на сайте;
                                    break;
                                case "-sd": seId = Convert.ToInt32(line); // - Сайт;
                                    break;
                                case "-ln": slogin = line;                // - Логин;
                                    break;
                                case "-pd": spassword = line;             // - Пароль;
                                    break;
                            }
                        }
                        catch
                        { }
                    }
                    i++;
                }
                goWork(usId.ToString(), seId.ToString(), slogin, spassword);
            }
            else
            {
                /* Подключение к бд на сервере, получение списка актуальных пользователей */
                DataTable dt = new DataTable();DataSet ds = new DataSet("amount");
                string lInf = ""; int thWatch = 0;
                var result = new Tuple<DataTable, string, DataSet>(dt, lInf, ds);
                queryToDB q = new queryToDB(); 
                result = q.executeQuery("SELECT * FROM  `listOfActiveUser`");
                Console.WriteLine(result.Item2.ToString());
                if (result.Item2.Contains("одключен") | result.Item2.Contains("агружен"))
                {
                    for (int i = 0; i < result.Item1.Rows.Count; i++)
                    {
                        Console.Write(result.Item1.Rows[i][0].ToString() + " ");
                        Console.Write(result.Item1.Rows[i][1].ToString() + " ");
                        Console.Write(result.Item1.Rows[i][2].ToString() + " ");
                        Console.Write(result.Item1.Rows[i][3].ToString() + " ");

                        if (result.Item1.Rows[i][3].ToString() != "0" && result.Item1.Rows[i][3].ToString().Length > 5)
                        {


                            /* Создание задачи в планировщике vista, 7, 8 */
                            string revise = result.Item1.Rows[i][5].ToString();
                            if (revise.Length == 0) { revise = result.Item1.Rows[i][4].ToString(); }  
                            scheduler sch = new scheduler(); 
                            sch.createTask(uid: result.Item1.Rows[i][0].ToString(),
                                           sit: result.Item1.Rows[i][1].ToString(),
                                           log: result.Item1.Rows[i][2].ToString(),
                                           pas: result.Item1.Rows[i][3].ToString(),
                                           countDays: result.Item1.Rows[i][4].ToString(),
                                           endDate: result.Item1.Rows[i][5].ToString());
                        }
                            /* Запуск отдельного потока на обновление каждой записи */
                            thWatch++;
                            if (thWatch == 6) { Thread.Sleep(10000); thWatch = 0; } else { Thread.Sleep(100); }
                            new System.Threading.Thread(delegate()
                            {
                                goWork(
                                    result.Item1.Rows[i - 1][0].ToString(),
                                    result.Item1.Rows[i - 1][1].ToString(),
                                    result.Item1.Rows[i - 1][2].ToString(),
                                    result.Item1.Rows[i - 1][3].ToString());
                            }).Start();
                        
                    }
                }
            }
            
        }
        static void goWork(string uid, string sit, string log, string pas)
        {/* Процедура запуска обновления записи */
            hh HH = new hh();           job JB = new job();             superjob sj = new superjob();
            rabota rb = new rabota();   zarplata zp = new zarplata();   ru29 R29 = new ru29();
            ru29V R29V = new ru29V();   hhV HHV = new hhV();
            if (pas.Length > 4)
            {
                switch (sit)
                {
                    case "1": HH.tryConnect(log, pas, uid); break;
                    case "2": sj.tryConnect(log, pas, uid); break;
                    case "3": rb.tryConnect(log, pas, uid); break;
                    case "4": JB.tryConnect(log, pas, uid); break;
                    case "5": zp.tryConnect(log, pas, uid); break;
                    case "6": R29.tryConnect(log, pas, uid); break;
                    case "7": R29V.tryConnect(log, pas, uid); break;
                    case "8": HHV.tryConnect(log, pas, uid); break;
                }
            }
            
        }
    }
}