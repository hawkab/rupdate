using System;
using xNet.Net;
using xNet.Text;
using System.Net;
using System.Linq;
using System.Text;
using xNet.Collections;

namespace Auth
{
    public class superjob
    {
        // Method:
        public void tryConnect(string login, string password, string uid)
        {
            bool auth = false, upd = false;
            string sourcePage = "", responce = "";
            string[] raw, rawD;
            HttpRequest req = new HttpRequest();
            var reqParams = new RequestParams();
            Random random = new Random();
            int a = random.Next(4), okCount = 0, awMax = 0;
            switch (a)
            {
                case 1: req.UserAgent = HttpHelper.ChromeUserAgent(); break;
                case 2: req.UserAgent = HttpHelper.IEUserAgent(); break;
                case 3: req.UserAgent = HttpHelper.OperaUserAgent();break;
                case 4: req.UserAgent = HttpHelper.FirefoxUserAgent();break;
            }
            HttpResponse resp;
            req.Cookies = new CookieDictionary();
            try
            {
                //Console.Write("Подключение к superjob.ru...");
                resp = req.Get("http://www.superjob.ru/user/login");
                if (resp.IsOK)
                {
                    //Console.Write("Готово!"); Console.WriteLine();
                    reqParams["returnUrl"] = "http://www.superjob.ru/";
                    reqParams["LoginForm[login]"] = login;
                    reqParams["LoginForm[password]"] = password;

                    resp = req.Post("http://www.superjob.ru/user/login", reqParams);
                    try
                    {
                        sourcePage = req.Get("http://www.superjob.ru/user/resume").ToString();
                        raw = sourcePage.Substrings("RegUserUserName_text h_border_dotted\">", "</span>", 0);
                        //Console.WriteLine("Пользователь: " + raw[0]);
                        if (raw.Count() > 0) { auth = true; }
                        raw = sourcePage.Substrings("http://www.superjob.ru/resume/update_datepub.html?id=", "\"", 0);

                        for (int i = 0; i < raw.Count(); i++)
                        {
                            try
                            {
                                //Console.WriteLine("Обновления резюме " + (i + 1) + " из " + raw.Count() + "... ");
                                req.ConnectTimeout = 4000;
                                sourcePage = req.Get("http://www.superjob.ru/resume/update_datepub.html?id="+raw[i]).ToString();
                                rawD = sourcePage.Substrings("data-time-to-refresh=\"", "\"\n", 0);//sourcePage.Substrings("<td class=\"m_colomn2\">", "</td>", 0);
                                if (awMax < Convert.ToInt32(rawD[0])) { awMax = Convert.ToInt32(rawD[0]); }
                                if (Convert.ToInt32(rawD[0]) >= 3540)
                                {
                                    responce += (i + 1) + "/" + raw.Count().ToString() + "-ok;"; okCount++;
                                    upd = true;
                                }
                                //Console.WriteLine(rawD[0]);
                            }
                            catch
                            { responce += (i + 1) + "/" + raw.Count().ToString() + "-fail;";/*Console.WriteLine("Ошибка обновления.");*/ }
                        }
                        if (okCount >= 0 && okCount != raw.Count())
                        {
                            scheduler sch = new scheduler();
                            sch.createTask(uid: uid,
                                                   sit: "2",
                                                   log: login,
                                                   pas: password,
                                                   countDays: "0",
                                                   endDate: "",
                                                   leftBefore: awMax+15);
                        } else
                        {
                            scheduler sch = new scheduler();
                            sch.createTask(uid: uid,
                                           sit: "2",
                                           log: login,
                                           pas: password,
                                           countDays: "0",
                                           endDate: "");
                        }

                        //Console.WriteLine("Процесс обновления завершён - выход из ЛК пользователя.");
                        Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " sj-" + login + " done.");
                        //req.MaximumAutomaticRedirections = 7;
                        //resp = req.Get("http://www.superjob.ru/user/logout");
                    }
                    catch { /*Console.Write("Ошибка авторизации: "+ex.Message); Console.WriteLine();*/ }
                }
            }
            catch
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
                               sit: "2",
                               log: login + " - runOnce",
                               pas: password,
                               countDays: "1",
                               endDate: "",
                               runOnce: true);
            }
        }
    }
}

