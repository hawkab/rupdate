using System;
using xNet.Net;
using xNet.Text;
using System.Net;
using System.Linq;
using System.Text;
using xNet.Collections;

namespace Auth
{
    public class ru29V
    {
        // Method:
        public void tryConnect(string login, string password, string uid)
        {
            bool auth = false, upd = false;
            string sourcePage = "", responce = "";
            string[] raw;
            HttpRequest req = new HttpRequest();
            Random random = new Random();
            var reqParams = new RequestParams();
            int a = random.Next(4); int okCount = 0;
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
                //Console.Write("Подключение к 29ru.ru...");
                resp = req.Get("http://29.ru/passport/login.php?gm=&url=http%3A%2F%2F29.ru%2Fjob%2Fvacancy%3Fgm%3D");
                if (resp.IsOK)
                {
                    //Console.Write("Готово!"); Console.WriteLine();

                    reqParams["url"] = "http://29.ru/job/vacancy?gm=";
                    reqParams["email"] = login;
                    reqParams["password"] = password;
                    reqParams["remember"] = "0";
                    req.ConnectTimeout = 4000;
                    resp = req.Post("https://loginka.ru/auth/?url=http%3A%2F%2F29.ru%2Fjob%2Fvacancy%3Fgm%3D", reqParams);
                    reqParams.Clear();
                    try
                    {
                        sourcePage = req.Get("http://29.ru/passport/mypage.php").ToString();
                        raw = sourcePage.Substrings("<div class=\"title\" style=\"padding: 0\">	", "</div>", 0);
                        //Console.WriteLine("Пользователь: " + raw[0]);
                        if (raw.Count() > 0) { auth = true; }
                        sourcePage = req.Get("http://29.ru/job/my/vacancy/").ToString();
                        raw = sourcePage.Substrings("<tr valign=\"top\" id=\"row", "\"\tclass=\" hidden\">", 0);
                        
                        for (int i = 0; i < raw.Count(); i++)
                        {
                            req.AddHeader("X-Requested-With", "XMLHttpRequest");
                            reqParams["action"] = "edit_list";
                            reqParams["UserAction"] = "prolong4";
                            reqParams["ids_action[]"] = raw[i];
                            try
                            {
                                req.ConnectTimeout = 4000;
                                //Console.WriteLine("Обновления вакансии " + (i + 1) + " из " + raw.Count() + "... ");
                                sourcePage = req.Post("http://29.ru/job/my/vacancy/", reqParams).ToString();
                                responce += (i + 1) + "/" + raw.Count().ToString() + "-ok;"; okCount++;
                                upd = true;
                                scheduler sch = new scheduler();
                                sch.createTask(uid: uid,
                                               sit: "7",
                                               log: login,
                                               pas: password,
                                               countDays: "0",
                                               endDate: "");
                                //Console.WriteLine("Успешно!");
                            }
                            catch 
                            { responce += (i + 1) + "/" + raw.Count().ToString() + "-fail;"; /*Console.WriteLine("Ошибка обновления." + ex.Message);*/ }
                        }
                        if (okCount > 0 && okCount != raw.Count())
                        {
                            scheduler sch = new scheduler();
                            sch.createTask(uid: uid,
                                           sit: "7",
                                           log: login + " - runOnce",
                                           pas: password,
                                           countDays: "1",
                                           endDate: "",
                                           runOnce: true);
                        }
                        sourcePage = req.Get("http://29.ru/passport/mypage.php").ToString();
                        raw = sourcePage.Substrings("<a href=\"https://loginka.ru/auth/logout.php?","\" class=\"link-logout\" title=\"Выход\"><b>Выход</b></a>", 0);
                        Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " 29v-" + login + " done.");
                    }
                    catch  {/* Console.Write("Ошибка авторизации: " + ex.Message); Console.WriteLine();*/ }
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
                               sit: "7",
                               log: login + " - runOnce",
                               pas: password,
                               countDays: "1",
                               endDate: "",
                               runOnce: true);
            }
        }
    }
}