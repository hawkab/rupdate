using System;
using xNet.Net;
using xNet.Text;
using System.Net;
using System.Linq;
using System.Text;
using xNet.Collections;

namespace Auth
{
    public class zarplata
    {
        // Method:
        public void tryConnect(string login, string password, string uid)
        {
            bool auth = false, upd = false;
            string data = "", sourcePage = "", responce = "";
            string[] raw;
            HttpRequest req = new HttpRequest();
            Random random = new Random();
            int a = random.Next(4); int okCount = 0;
            switch (a)
            {
                case 1: req.UserAgent = HttpHelper.ChromeUserAgent(); break;
                case 2: req.UserAgent = HttpHelper.IEUserAgent(); break;
                case 3: req.UserAgent = HttpHelper.OperaUserAgent(); break;
                case 4: req.UserAgent = HttpHelper.FirefoxUserAgent(); break;
            }
            HttpResponse resp;
            req.Cookies = new CookieDictionary();
            try
            {
                //Console.Write("Подключение к zarplata.ru...");
                resp = req.Get("http://www.zarplata.ru/user/login/");
                if (resp.IsOK)
                {
                    //Console.Write("Готово!"); Console.WriteLine();
                    req.AddHeader("X-Requested-With","XMLHttpRequest");


                    data = ("{\"email\":\"" + login + "\",\"password\":\"" + password + "\",\"groupId\":null,\"rememberMe\":false,\"zarplataSubscription\":null}");
                    resp = req.Post("http://www.zarplata.ru/webservices/user.asmx/LoginZarplata", data, "application/json; charset=UTF-8");
                    try
                    {
                        sourcePage = req.Get("http://www.zarplata.ru/user/applicant/resume/").ToString();
                        raw = sourcePage.Substrings("userInfo.userName = '", "'; ", 0);
                        //Console.WriteLine("Пользователь: " + raw[0]);
                        if (raw.Count() > 0) { auth = true; }
                        sourcePage = req.Get("http://www.zarplata.ru/webapi/resume/resumes/").ToString();
                        raw = sourcePage.Substrings("{\"Id\":", ",\"ProfessionName\"", 0);

                        for (int i = 0; i < raw.Count(); i++)
                        {
                            try
                            {
                                //Console.WriteLine("Обновления резюме " + (i + 1) + " из " + raw.Count() + "... ");
                                req.ConnectTimeout = 4000;
                                data = ("{\"typeAnnouncement\":\"resume\",\"ids\":\"" + raw[i] + "\",\"action\":\"repub\",\"stopCallback\":false,\"firmDepartmentId\":0}");
                                sourcePage = req.Post("http://www.zarplata.ru/user/Ajax.asmx/AnnouncementsDoAction", data, "application/json; charset=utf-8").ToString();
                                //Console.WriteLine("Успешно!");
                                responce += (i + 1) + "/" + raw.Count().ToString() + "-ok;"; okCount++;
                                upd = true;
                                scheduler sch = new scheduler();
                                sch.createTask(uid: uid,
                                               sit: "5",
                                               log: login,
                                               pas: password,
                                               countDays: "0",
                                               endDate: "");
                            }
                            catch
                            { responce += (i + 1) + "/" + raw.Count().ToString() + "-fail;";/*Console.WriteLine("Ошибка обновления.");*/ }
                        }
                        if (okCount > 0 && okCount != raw.Count())
                        {
                            scheduler sch = new scheduler();
                            sch.createTask(uid: uid,
                                           sit: "5",
                                           log: login + " - runOnce",
                                           pas: password,
                                           countDays: "1",
                                           endDate: "",
                                           runOnce: true);
                        }
                        //resp = req.Get("http://www.zarplata.ru/logout.ashx");
                        //Console.WriteLine("Процесс обновления завершён - выход из ЛК пользователя.");
                        Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " zp-" + login + " done.");
                    }
                    catch { /*Console.Write("Ошибка авторизации: " +ex.Message); Console.WriteLine(); */}
                }
            }
            catch //(Exception ex)
            {
                //Console.WriteLine("Error: " + ex.Message);
            }
           queryToMySQL.queryToDB q = new queryToMySQL.queryToDB();
            q.executeQuery("INSERT INTO `resumeup_db`.`history_update` " +
                            "(`id`,`resume_id`,`date_time`,`auth`,`updated`,`responce`) VALUES (null, '" +
                            uid + "', now()" +
                            ",'" + Convert.ToInt32(auth).ToString() +
                            "','" + Convert.ToInt32(upd).ToString() +
                            "','" + responce + "')");
            if (!auth)
            {
                scheduler sch = new scheduler();
                sch.createTask(uid: uid,
                               sit: "5",
                               log: login + " - runOnce",
                               pas: password,
                               countDays: "1",
                               endDate: "",
                               runOnce: true);
            }
        }
    }
}