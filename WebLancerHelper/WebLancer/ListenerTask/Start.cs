using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebLancerHelper.WebLancer.ListenerTask
{
    class Start : Listener
    {
        public Start(Action<Objects.Task> action, API api, string href)
        {
            Log.ProcessMessage("Запускаем прослушивания заданий " + href);
            Thread listner = new Thread(() =>
            {
                DateTime lastTime = DateTime.Now;
                while (true)
                {
                    try
                    {
                        var tasks = api.GetTasksFromPage(href);
                        for(int i = tasks.Count-1; i>=0; i--)
                        {
                            DateTime taskTime = UnixTimeStampToDateTime(tasks[i].TimeStamp);
                            if (lastTime < taskTime)
                            {
                                action?.Invoke(tasks[i]);
                                lastTime = taskTime;
                                Log.GoodMessage("Задание передано на отправку");
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Log.ExMessage("Ошибка при прослушивании: \n\n" + ex.ToString());
                    }
                    Thread.Sleep(TimeSpan.FromMinutes(5));
                }
            })
            {
                IsBackground = true
            };
            listner.Start();
        }
    }
}
