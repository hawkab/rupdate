using System;
using xNet.Net;
using xNet.Text;
using System.Net;
using System.Linq;
using System.Text;
using xNet.Collections;

namespace Auth
{
    public class rabota
    {
        // Method:
        public void tryConnect(string login, string password, string uid)
        {
            bool auth = false, upd = false;
            string sourcePage = "", responce = "";
            string[] raw/*,rawD*/;
            HttpRequest req = new HttpRequest();
            Random random = new Random();
            var reqParams = new RequestParams();
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
                //Console.Write("Подключение к rabota.ru...");
                resp = req.Get("http://www.rabota.ru/v3_login.html");
                if (resp.IsOK)
                {
                    //Console.Write("Готово!"); Console.WriteLine();

                    reqParams["mail"] = login;
                    reqParams["password"] = password;

                    resp = req.Post("http://www.rabota.ru/v3_login.html", reqParams);
                    reqParams.Clear();
                    try
                    {
                        sourcePage = req.Get("http://www.rabota.ru/v3_resumeList.html").ToString();
                        raw = sourcePage.Substrings("class=\"login_setings\"></a>\n\t\t\t\t\t", "\t\t\t\t</div>", 0);
                        //Console.WriteLine("Пользователь: " + raw[0]);
                        if (raw.Count() > 0) { auth = true; }

                        raw = sourcePage.Substrings("onclick=\"myResumeExpires(", ");  return false;", 0);
                        if (raw.Count() < 1)
                        {
                            raw = sourcePage.Substrings("id=\"copyResume_", "\" onclick", 0);
                        }
                        
                        for (int i = 0; i < raw.Count(); i++)
                        {
                            req.AddHeader("X-Requested-With", "XMLHttpRequest");
                            reqParams["action"] = "refresh";
                            reqParams["resumeId"] = raw[i];
                            try
                            {
                                req.ConnectTimeout = 4000;
                                //Console.WriteLine("Обновления резюме " + (i + 1) + " из " + raw.Count() + "... ");
                                sourcePage = req.Post("http://arkhangelskaya.rabota.ru/v3_popupMyResumeExpires.html", reqParams).ToString();

                                //rawD = sourcePage.Substrings("class=\"login_setings\"></a>\n\t\t\t\t\t", "\t\t\t\t</div>", 0);
                                //if (rawD.Length == 0)
                                //{
                                //    responce += (i + 1) + "/" + raw.Count().ToString() + "-fail;";
                                //}
                                //else
                                //{
                                    responce += (i + 1) + "/" + raw.Count().ToString() + "-ok;"; okCount++;
                                    upd = true;
                                    scheduler sch = new scheduler();
                                    sch.createTask(uid: uid,
                                                   sit: "3",
                                                   log: login,
                                                   pas: password,
                                                   countDays: "0",
                                                   endDate: "");
                                //}
                                
                                //Console.WriteLine("Обновлено: " + sourcePage.ToString());
                            }
                            catch
                            { responce += (i + 1) + "/" + raw.Count().ToString() + "-fail;"; /*Console.WriteLine("Ошибка обновления."+ex.Message);*/ }
                        }
                        if (okCount > 0 && okCount != raw.Count())
                        {
                            scheduler sch = new scheduler();
                            sch.createTask(uid: uid,
                                           sit: "3",
                                           log: login + " - runOnce",
                                           pas: password,
                                           countDays: "1",
                                           endDate: "",
                                           runOnce: true);
                        }
                        //resp = req.Get("http://www.rabota.ru/v3_exit.html");
                        //Console.WriteLine("Процесс обновления завершён - выход из ЛК пользователя.");
                        Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " rb-" + login + " done.");
                    }
                    catch { /*Console.Write("Ошибка авторизации: "+ex.Message); Console.WriteLine();*/ }
                }
            }
            catch// (Exception ex)
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
                               sit: "3",
                               log: login + " - runOnce",
                               pas: password,
                               countDays: "1",
                               endDate: "",
                               runOnce: true);
            }
        }
    }
}

