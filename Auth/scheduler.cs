using System;
using queryToMySQL;
using Microsoft.Win32.TaskScheduler;

namespace Auth
{
    public class scheduler
    {
        public void createTask(string uid, string sit, string log, string pas, string countDays, string endDate, bool runOnce=false, long leftBefore=0)
         {
                using (TaskService ts = new TaskService())
                {
                    TaskDefinition td = ts.NewTask(); queryToDB q = new queryToDB();
                
                    //td.RegistrationInfo.Description = "\twww.resumeupdater.ru\nОбновление всех резюме аккаунта.";
                    td.Principal.UserId = string.Concat(Environment.UserDomainName, "\\", Environment.UserName);
                    td.Principal.LogonType = TaskLogonType.S4U; 
                    td.Settings.MultipleInstances = TaskInstancesPolicy.Parallel;
                                                            
                    DailyTrigger daily = new DailyTrigger();
                    daily.StartBoundary = DateTime.Now;

                    if (endDate.Length != 0 && (countDays != "0" || countDays != "1"))
                    {
                        daily.EndBoundary = DateTime.Today.AddDays(Convert.ToDouble(countDays) + 1);
                        q.executeQuery("UPDATE `resumeupdater` SET `end_boundary`=(now() + interval (`count_day`+1) day) WHERE `id`=" + uid);
                        q.executeQuery("UPDATE `resumeupdater` SET `site_password`=0 WHERE `id`=" + uid);
                    }
             
                    daily.DaysInterval = 1;
                    td.Actions.Add(new ExecAction(Environment.GetCommandLineArgs()[0].Replace(".vshost",""),
                            "-id " + uid +
                            " -sd " + sit +
                            " -ln " + log +
                            " -pd " + pas, null));
                    try {ts.RootFolder.CreateFolder("resumeUpdater");} catch { }
                    try
                    {
                        string sNameSite = "";
                        switch (sit)
                        {
                            case "1": sNameSite = "hh"; if (leftBefore > 0) { daily.Repetition.Interval = TimeSpan.FromSeconds(leftBefore); } else {daily.Repetition.Interval = TimeSpan.FromSeconds(14420);} break;
                            case "2": sNameSite = "sj"; if (leftBefore > 0) { daily.Repetition.Interval = TimeSpan.FromSeconds(leftBefore); } else {daily.Repetition.Interval = TimeSpan.FromSeconds(3620);} break;
                            case "3": sNameSite = "rb"; if (leftBefore > 0) { daily.Repetition.Interval = TimeSpan.FromSeconds(leftBefore); } else {daily.Repetition.Interval = TimeSpan.FromSeconds(3620);} break;
                            case "4": sNameSite = "jb"; if (leftBefore > 0) { daily.Repetition.Interval = TimeSpan.FromSeconds(leftBefore); } break;
                            case "5": sNameSite = "zp"; if (leftBefore > 0) { daily.Repetition.Interval = TimeSpan.FromSeconds(leftBefore); } break;
                            case "6": sNameSite = "9R"; if (leftBefore > 0) { daily.Repetition.Interval = TimeSpan.FromSeconds(leftBefore); } break;
                            case "7": sNameSite = "9V"; if (leftBefore > 0) { daily.Repetition.Interval = TimeSpan.FromSeconds(leftBefore); } break;
                            case "8": sNameSite = "hhV"; if (leftBefore > 0) { daily.Repetition.Interval = TimeSpan.FromSeconds(leftBefore); } break;
                        }

                        if (runOnce) { td.Triggers.Add(new TimeTrigger() { StartBoundary = DateTime.Now.AddMinutes(5) }); } else { td.Triggers.Add(daily); }

                        if (countDays == "0")
                        {
                            using (TaskService tc = new TaskService())
                            {
                                Microsoft.Win32.TaskScheduler.Task task = tc.GetTask(@"resumeUpdater\" + sNameSite + " - " + log);
                                TaskDefinition tt = task.Definition;
                                foreach (Microsoft.Win32.TaskScheduler.Trigger trigger in task.Definition.Triggers)
                                {
                                    trigger.StartBoundary = DateTime.Now;
                                    switch (sit)
                                    {
                                        case "1": if (leftBefore > 0) { trigger.Repetition.Interval = TimeSpan.FromSeconds(leftBefore); } else { trigger.Repetition.Interval = TimeSpan.FromSeconds(14420); } break;
                                        case "2": if (leftBefore > 0) { trigger.Repetition.Interval = TimeSpan.FromSeconds(leftBefore); } else { trigger.Repetition.Interval = TimeSpan.FromSeconds(3620); } break;
                                        case "3": if (leftBefore > 0) { trigger.Repetition.Interval = TimeSpan.FromSeconds(leftBefore); } else { trigger.Repetition.Interval = TimeSpan.FromSeconds(3620); } break;
                                        case "4": if (leftBefore > 0) { trigger.Repetition.Interval = TimeSpan.FromSeconds(leftBefore); } break;
                                        case "5": if (leftBefore > 0) { trigger.Repetition.Interval = TimeSpan.FromSeconds(leftBefore); } break;
                                        case "6": if (leftBefore > 0) { trigger.Repetition.Interval = TimeSpan.FromSeconds(leftBefore); } break;
                                        case "7": if (leftBefore > 0) { trigger.Repetition.Interval = TimeSpan.FromSeconds(leftBefore); } break;
                                    }
                                }
                                tc.RootFolder.RegisterTaskDefinition(@"resumeUpdater\" + sNameSite + " - " + log, tt);
                            }
                        }
                        else { ts.RootFolder.RegisterTaskDefinition(@"resumeUpdater\" + sNameSite + " - " + log, td); }
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                }
         }
    }
}