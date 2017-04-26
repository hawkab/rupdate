using System;
using xNet.Net;
using xNet.Text;
using System.Net;
using System.Linq;
using System.Text;
using xNet.Collections;

namespace Auth
{
    public class hhV
    {
        // Method:
        public void tryConnect(string login, string password, string uid)
        {
            bool auth = false, upd = false;
            string token = "", sourcePage = "", responce = "";
            string[] row, rawD, rawC;
            HttpRequest req = new HttpRequest();
            var reqParams = new RequestParams();
            DateTime hasBeenUpdated, now, canBeUpdated; TimeSpan span;
            long leftBefore = 0, leftBef = 0, maxLeft = 0;
            req.Referer = "http://arkhangelsk.hh.ru/logon.do?backUrl=http%3A%2F%2Farkhangelsk.hh.ru%2F";
            Random random = new Random();
            req.ConnectTimeout = 4000;
            int a = random.Next(4), okCount = 0;
            switch (a)
            {
                case 0: req.UserAgent = HttpHelper.ChromeUserAgent(); break;
                case 1: req.UserAgent = HttpHelper.IEUserAgent(); break;
                case 2: req.UserAgent = HttpHelper.OperaUserAgent(); break;
                case 3: req.UserAgent = HttpHelper.FirefoxUserAgent(); break;
            }
            HttpResponse resp;
            req.Cookies = new CookieDictionary();
            try
            {
                //Console.Write("Подключение к hh.ru...");
                resp = req.Get("http://arkhangelsk.hh.ru/logon.do");
                if (resp.IsOK)
                {
                    //Console.Write("Готово!"); Console.WriteLine();
                    if (resp.Cookies.ContainsKey("_xsrf"))
                    {
                        resp.Cookies.TryGetValue("_xsrf", out token);
                        reqParams["username"] = login;
                        reqParams["password"] = password;
                        reqParams["action"] = "Войти в личный кабинет";
                        reqParams["_xsrf"] = token;

                        resp = req.Post("http://arkhangelsk.hh.ru/logon.do", reqParams);
                        reqParams.Clear();
                        try
                        {
                            sourcePage = req.Get("http://arkhangelsk.hh.ru/employer/vacancies").ToString();
                            row = sourcePage.Substrings("<li class=\"usermenu-item m-item_username HH-ApplicantSettingsFio-TopItem\">", "</li>", 0);
                            
                            //Console.WriteLine("Пользователь: "+row[0]);
                            if (row.Count() > 0) { auth = true; }

                            row = sourcePage.Substrings("<a href=\"/vacancy/", "\" class=\"prosper-link HH-LoadVacancies-Vacancy", 0);
                            rawD = sourcePage.Substrings("<div class=\"myvacancies-detail\"><div class=\"\">", "<div>", 0);
                            rawC = sourcePage.Substrings("<div class=\"metallic-label metallic-label_free\">", "</div>", 0);
                            //rawD = rawD.Where(w => w != rawD[row.Length-1]).ToArray();
                            for (int i = 0; i < rawD.Length; i++)
                            {
                                rawD[i] = (Convert.ToDateTime(rawD[i].Replace("&nbsp;", " "))).ToString();
                                hasBeenUpdated = Convert.ToDateTime(rawD[i]);
                                now = DateTime.Now;
                                span =  hasBeenUpdated-now;
                                rawD[i] = (Convert.ToInt32(span.TotalSeconds)).ToString();
                            }

                            for (int i = 0; i < row.Count(); i++)
                            {

                                if (!((Convert.ToInt32(rawD[i]) - 259200) > 2290409 && rawC[i].Contains("Бесплатная")))
                                {
                                    //req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                                    req.AddHeader("X-Requested-With", "XMLHttpRequest");
                                    req.AddHeader("X-Xsrftoken", token);
                                    reqParams["id"] = row[i];

                                    req.Referer = "http://arkhangelsk.hh.ru/employer/vacancies";
                                    try
                                    {
                                        //Console.WriteLine("Обновление резюме " + (i + 1) + " из " + row.Count() + "... ");
                                        req.ConnectTimeout = 4000;
                                        resp = req.Post("http://arkhangelsk.hh.ru/employer/vacancy/update?id=" + row[i], reqParams);
                                        //Console.WriteLine("Успешно!");
                                        responce += (i + 1) + "/" + row.Count().ToString() + "-ok;"; okCount++;
                                        upd = true;
                                        scheduler sch = new scheduler();
                                        sch.createTask(uid: uid,
                                                       sit: "8",
                                                       log: login,
                                                       pas: password,
                                                       countDays: "0",
                                                       endDate: "");
                                    }
                                    catch
                                    {
                                        responce += (i + 1) + "/" + row.Count().ToString() + "-fail;";/*Console.WriteLine("Ошибка обновления: "+ex.Message );*/
                                        scheduler sch = new scheduler();
                                        sch.createTask(uid: uid,
                                                       sit: "8",
                                                       log: login + " - runOnce",
                                                       pas: password,
                                                       countDays: "1",
                                                       endDate: "",
                                                       runOnce: true);
                                    }
                                }
                                else
                                {
                                    if (maxLeft < Convert.ToInt32(rawD[i]))
                                    {
                                        maxLeft = Convert.ToInt32(rawD[i]);
                                        hasBeenUpdated = DateTime.Now.AddSeconds(-maxLeft);
                                        canBeUpdated = hasBeenUpdated.AddSeconds(14400);
                                        span = canBeUpdated - DateTime.Now;
                                        leftBef = Convert.ToInt32(span.TotalSeconds);
                                    }

                                }
                            }
                            if (okCount >= 0 && okCount != row.Count())
                            {
                                scheduler sch1 = new scheduler();
                                sch1.createTask(uid: uid,
                                               sit: "8",
                                               log: login,
                                               pas: password,
                                               countDays: "0",
                                               endDate: "",
                                               leftBefore: leftBef + 15);
                            }
                            reqParams.Clear();
                            reqParams["_xsrf"] = token;
                            resp = req.Post("http://arkhangelsk.hh.ru/logoff.do", reqParams);
                            Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " hhV-" + login + " done.");
                        }
                        catch (Exception ex) {Console.Write("Ошибка авторизации: "+ ex.Message ); Console.WriteLine(); }
                    }
                    else { }
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
                               sit: "1",
                               log: login + " - runOnce",
                               pas: password,
                               countDays: "1",
                               endDate: "",
                               runOnce: true);
            }
        }
    }
}
