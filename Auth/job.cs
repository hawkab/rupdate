using System;
using xNet.Net;
using xNet.Text;
using System.Net;
using System.Linq;
using System.Text;
using xNet.Collections;

namespace Auth
{
    public class job
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
            int a = random.Next(4), okCount = 0;
            long leftBefore=0, leftBef=0,maxLeft=0;
            DateTime hasBeenUpdated, now, canBeUpdated; TimeSpan span;
            switch (a)
            {
                case 1: req.UserAgent = HttpHelper.ChromeUserAgent(); break;
                case 2: req.UserAgent = HttpHelper.IEUserAgent();break;
                case 3: req.UserAgent = HttpHelper.OperaUserAgent(); break;
                case 4: req.UserAgent = HttpHelper.FirefoxUserAgent(); break;
            }
            HttpResponse resp;
            req.Cookies = new CookieDictionary();
            try
            {
                //Console.Write("Подключение к job.ru...");
                resp = req.Get("http://www.job.ru/seeker/login/");
                if (resp.IsOK)
                {
                    //Console.Write("Готово!"); Console.WriteLine();
                    reqParams["__EVENTTARGET"] = "ctl00$cpH$btLogin";
                    reqParams["ctl00$cpH$tbEmail"] = login;
                    reqParams["ctl00$cpH$tbPassword"] = password;

                    resp = req.Post("http://www.job.ru/seeker/login/", reqParams);
                    reqParams.Clear();
                        try
                        {
                            sourcePage = req.Get("http://www.job.ru/seeker/user/cv/").ToString();
                            raw = sourcePage.Substrings("profile\u002F\">", "</a>", 0);
                            //Console.WriteLine("Пользователь: " + raw[0]);
                            if (raw.Count() > 0) { auth = true; }
                            raw = sourcePage.Substrings("href='/seeker/user/cv/preview/?cv=", "\'>", 0);
                            rawD = sourcePage.Substrings("_cvModer_lastUpdate\">", "</span>", 0);
                            for (int i = 0; i < raw.Count(); i++)
                            {
                                if (rawD.Count() > 0) 
                                {
                                    hasBeenUpdated = Convert.ToDateTime(rawD[i]);
                                    now = DateTime.Now;
                                    span = now - hasBeenUpdated;
                                    leftBefore = Convert.ToInt32 (span.TotalSeconds);
                                }
                                if (leftBefore > 86410)
                                {
                                    reqParams.Clear();
                                    reqParams["ahhc"] = "TmeJobs.Web.Seeker.AjaxHandlers";
                                    reqParams["ahhm"] = "UpdateCv";
                                    reqParams["ahass"] = "TmeJobs.Web";
                                    reqParams["ahargs"] = "[" + raw[i] + "]";
                                    try
                                    {
                                        //Console.WriteLine("Обновления резюме " + (i + 1) + " из " + raw.Count() + "... ");
                                        req.ConnectTimeout = 4000;
                                        sourcePage = req.Post("http://www.job.ru/seeker/user/cv/ajax.ashx", reqParams).ToString();
                                        //Console.WriteLine("Обновлено: "+sourcePage.ToString());

                                            hasBeenUpdated = Convert.ToDateTime(sourcePage.ToString());
                                            now = DateTime.Now;
                                            span = now - hasBeenUpdated;
                                            leftBefore = Convert.ToInt32(span.TotalSeconds);

                                        if (leftBefore < 20)
                                        {
                                            responce += (i + 1) + "/" + raw.Count().ToString() + "-ok;"; okCount++;
                                            upd = true;
                                            scheduler sch = new scheduler();
                                            sch.createTask(uid: uid,
                                                           sit: "4",
                                                           log: login,
                                                           pas: password,
                                                           countDays: "0",
                                                           endDate: "");
                                        }
                                    }
                                    catch
                                    { /*Console.WriteLine("Ошибка обновления:" +ex.Message );*/ }
                                }else
                                {
                                    responce += (i + 1) + "/" + raw.Count().ToString() + "-fail;";
                                    if (maxLeft < leftBefore) 
                                    {
                                        maxLeft = leftBefore;
                                        hasBeenUpdated = Convert.ToDateTime(rawD[i]);
                                        canBeUpdated = hasBeenUpdated.AddSeconds(86400);
                                        span = canBeUpdated - DateTime.Now;
                                        leftBef = Convert.ToInt32(span.TotalSeconds);
                                    }
                                }
                            }
                            if (okCount >= 0 && okCount != raw.Count())
                            {
                                scheduler sch = new scheduler();
                                sch.createTask(uid: uid,
                                               sit: "4",
                                               log: login,
                                               pas: password,
                                               countDays: "0",
                                               endDate: "",
                                               leftBefore: leftBef+15);
                            }
                            reqParams.Clear();
                            reqParams["__EVENTTARGET"] = "ctl00$ctl10$lbLogout";
                            resp = req.Post("http://arhangelsk.job.ru/seeker/user/cv/", reqParams);
                            //Console.WriteLine("Процесс обновления завершён - выход из ЛК пользователя.");
                            Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " jb-" + login + " done.");
                        }
                        catch (Exception ex) { Console.Write("Ошибка авторизации: Неверный логин/пароль."+ex.Message ); Console.WriteLine(); }
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
                               sit: "4",
                               log: login + " - runOnce",
                               pas: password,
                               countDays: "1",
                               endDate: "",
                               runOnce: true);
            }
        }
    }
}

