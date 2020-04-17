using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace WebLancerHelper.WebLancer
{
    public class API
    {
        private string Login;
        private string Password;
        private HttpClient http;
        public static readonly string Domain = "https://www.weblancer.net";
        public API()
        {
            UserAgent user = UserAgent.Initilization();
            string useragent = user.GetRandom();
            if (useragent == "")
            {
                return;
            }

            http = new HttpClient();
            http.DefaultRequestHeaders.Add("user-agent", useragent);
        }

        public string SetLogin
        {
            set
            {
                Login = value;
                UserAgent user = UserAgent.Initilization();
                string useragent = user.GetRandom();
                if (useragent == "")
                {
                    return;
                }

                http = new HttpClient();
                http.DefaultRequestHeaders.Add("user-agent", useragent);
            }
        }

        public string SetPassword
        {
            set
            {
                Password = value;
                UserAgent user = UserAgent.Initilization();
                string useragent = user.GetRandom();
                if (useragent == "")
                {
                    return;
                }

                http = new HttpClient();
                http.DefaultRequestHeaders.Add("user-agent", useragent);
            }
        }

        /// <summary>
        /// Авторизация на сервисе
        /// </summary>
        /// <returns>статус авторизации</returns>
        public bool Auth()
        {
            Log.ProcessMessage("Попытка авторизации " + Login);
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                {"login", Login },
                {"password", Password },
                {"store_login", "1"}
            };
            string post = http.PostAsync(Domain + "/account/login/", new FormUrlEncodedContent(data)).Result.Content.ReadAsStringAsync().Result;
            JObject json = JsonConvert.DeserializeObject(post) as JObject;
            string err = Regex.Replace(json["error"].ToString(), "<[^>]+>", string.Empty);
            if (err == "")
            {
                Log.GoodMessage("Авторизация успешна");
                return true;
            }
            else
            {
                Log.ExMessage(err);
                return false;
            }
        }

        /// <summary>
        /// Получает все задания с странцы категории
        /// </summary>
        /// <param name="link">ссылка на страницу без домена</param>
        /// <returns>Список все заданий</returns>
        public List<Objects.Task> GetTasksFromPage(string link)
        {
            Log.ProcessMessage("Пытаемся получить список заданий со страницы " + link);
            try
            {
                string get = http.GetAsync(Domain + link).Result.Content.ReadAsStringAsync().Result;
                HtmlParser Parser = new HtmlParser();
                AngleSharp.Html.Dom.IHtmlDocument html = Parser.ParseDocument(get);
                AngleSharp.Dom.IHtmlCollection<AngleSharp.Dom.IElement> taskElements = html.QuerySelectorAll("div.row.set_href .title a");
                var tasks = new List<Objects.Task> { };
                foreach(var elem in taskElements)
                {
                    var task = GetTaskFromLink(elem.GetAttribute("href"));
                    if (task == null) continue;
                    tasks.Add(task);
                }
                Log.GoodMessage("Получили список заданий со страницы " + link);
                return tasks;
            }
            catch
            {
                Log.ExMessage("Не удалось получить список заданий со страницы " + link);
                return null;
            }
        }

        /// <summary>
        /// Получает данные задания
        /// </summary>
        /// <param name="link">ссылка на задание без домена</param>
        /// <returns>Задание</returns>
        public Objects.Task GetTaskFromLink(string link)
        {
            Log.ProcessMessage("Пытаемся получить задание " + link);
            try
            {
                if (link.Contains("vacancies")) return null;
                string get = http.GetAsync(Domain + link).Result.Content.ReadAsStringAsync().Result;
                HtmlParser Parser = new HtmlParser();
                AngleSharp.Html.Dom.IHtmlDocument html = Parser.ParseDocument(get);


                var task = new Objects.Task
                {
                    Title = html.QuerySelector(".col h1").TextContent,
                    Discription = html.QuerySelector(".text_field p").TextContent,
                    Price = html.QuerySelector(".page_header_content .title.amount") != null ? html.QuerySelector(".page_header_content .title.amount").TextContent : "Бюджет не определен",
                    TimeStamp = double.Parse(html.QuerySelector(".cols_table.no_hover .text-muted  span[data-timestamp]").GetAttribute("data-timestamp")),
                    Applications = html.QuerySelector(".block-content .title").TextContent,
                    Link = Domain + link

                };
                Log.GoodMessage("Получили задание " + link);
                return task;
            }
            catch
            {
                Log.ExMessage("Не удалось получить задание " + link);
                return null;
            }
        }

        /// <summary>
        /// Обновляет список категорий
        /// </summary>
        /// <returns>статус обновления</returns>
        public bool UpdateWorkCategory()
        {
            Log.ProcessMessage("Пытаемся обновить список категорий");
            try
            {
                string get = http.GetAsync(Domain + "/jobs/").Result.Content.ReadAsStringAsync().Result;
                HtmlParser Parser = new HtmlParser();
                AngleSharp.Html.Dom.IHtmlDocument html = Parser.ParseDocument(get);
                AngleSharp.Dom.IHtmlCollection<AngleSharp.Dom.IElement> categoriesElements = html.QuerySelectorAll(".collapse li a[data-category_id]");
                foreach (var elem in categoriesElements)
                {
                    Objects.Category.Categories.Add(new Objects.Category
                    {
                        Name = elem.TextContent,
                        Href = elem.GetAttribute("href")
                    });
                }
                Log.GoodMessage("Обновили список категорий");
                return true;
            }
            catch
            {
                Log.ExMessage("Не удалось обновить список категорий");
                return false;
            }
        }

        /// <summary>
        /// Получает список всех контактов
        /// </summary>
        /// <returns>Objects.Contact[]</returns>
        public Objects.Contact[] GetContacts()
        {
            Log.ProcessMessage("Получаем список контактов");
            if (Login == "" || Password == "") Log.ExMessage("Не назначен логин/пароль");
            try
            {
                string get = http.GetAsync(Domain + "/account/contacts/").Result.Content.ReadAsStringAsync().Result;
                if (get.Contains("Вы не авторизованы"))
                {
                    if (!Auth())
                    {
                        return null;
                    }
                    else
                    {
                        get = http.GetAsync(Domain + "/account/contacts/").Result.Content.ReadAsStringAsync().Result;
                    }
                }

                HtmlParser Parser = new HtmlParser();
                AngleSharp.Html.Dom.IHtmlDocument html = Parser.ParseDocument(get);

                List<Objects.Contact> contacts = new List<Objects.Contact> { };

                AngleSharp.Dom.IHtmlCollection<AngleSharp.Dom.IElement> links = html.QuerySelectorAll(".page_content .row .buttons a.pm_link");
                AngleSharp.Dom.IHtmlCollection<AngleSharp.Dom.IElement> names = html.QuerySelectorAll(".page_content .row .name a");
                AngleSharp.Dom.IHtmlCollection<AngleSharp.Dom.IElement> nicks = html.QuerySelectorAll(".page_content .row .nickname");
                for (int i = 0; i < links.Length; i++)
                {
                    if (links[i].GetAttribute("class").Contains("btn-success"))
                    {
                        contacts.Add(new Objects.Contact
                        {
                            Link = links[i].GetAttribute("href"),
                            Name = names[i].TextContent,
                            Nick = nicks[i].TextContent,
                            IsNew = true
                        });
                    }
                    else
                    {
                        contacts.Add(new Objects.Contact
                        {
                            Link = links[i].GetAttribute("href"),
                            Name = names[i].TextContent,
                            Nick = nicks[i].TextContent,
                            IsNew = false
                        });
                    }
                }

                Log.GoodMessage("Получили список контактов");
                return contacts.ToArray();
            }
            catch
            {
                Log.ExMessage("Не удалось получить список контактов");
                return null;
            }
        }


        /// <summary>
        /// Получить список сообщений определнного контакта
        /// </summary>
        /// <param name="nickname">nickname контакта</param>
        /// <returns>Objects.Message.MessageUnion[]</returns>
        public Objects.Message.MessageUnion[] GetMessages(string nickname)
        {
            Log.ProcessMessage("Пытаемся получить сообщения " + nickname);
            if (Login == "" || Password == "") Log.ExMessage("Не назначен логин/пароль");
            try
            {
                string get = http.GetAsync(Domain+"/account/pm/?recipient=" + nickname).Result.Content.ReadAsStringAsync().Result;
                if (get.Contains("Вы не авторизованы"))
                {
                    if (!Auth())
                    {
                        return null;
                    }
                    else
                    {
                        get = http.GetAsync(Domain+"/account/pm/?recipient=" + nickname).Result.Content.ReadAsStringAsync().Result;
                    }
                }
                string corrid = new Regex("<input type=\"hidden\" id=\"corr_id\" value=\"(.*)\"><input type=\"hidden\" id=\"item_id\"").Match(get).Groups[1].Value;
                string recip = new Regex("<input type=\"hidden\" id=\"recipient_id\" value=\"(.*)\"><div class=\"cols_table\"").Match(get).Groups[1].Value;
                string itemid = new Regex("id=\"item_id\" value=\"(.*)\"><input type=\"hidden\" id=\"recipient_id\"").Match(get).Groups[1].Value;
                Random random = new Random();
                double randomValue = random.NextDouble();

                Dictionary<string, string> data = new Dictionary<string, string>
            {
                {"message", ""},
                {"search", "" }
            };

                string post = http.PostAsync(Domain+"/ajax/?section=pm&corr_id=" + corrid + "&recip=" + recip + "&ss=0&item_id=" + itemid + "&arc=0&" + randomValue, new FormUrlEncodedContent(data)).Result.Content.ReadAsStringAsync().Result;

                Objects.Message.MessageUnion[] message = Objects.Message.Message.FromJson(post);

                Log.GoodMessage("Успешно получили список сообщений " + nickname);

                return message;
            }
            catch
            {
                Log.ExMessage("Не удалось получить список сообщений " + nickname);
                return null;
            }

        }


        /// <summary>
        /// Отправить сообщению
        /// </summary>
        /// <param name="nickname">nickname пользователя</param>
        /// <param name="text">текст сообщения</param>
        /// <returns>статус отправки</returns>
        public bool SendMessage(string nickname, string text)
        {
            Log.ProcessMessage("Пытаемся отправить сообщение " + nickname + ", текст\n" + text);
            if (Login == "" || Password == "") Log.ExMessage("Не назначен логин/пароль");
            try
            {
                string get = http.GetAsync(Domain+"/account/pm/?recipient=" + nickname).Result.Content.ReadAsStringAsync().Result;
                if (get.Contains("Вы не авторизованы"))
                {
                    if (!Auth())
                    {
                        return false;
                    }
                    else
                    {
                        get = http.GetAsync(Domain+"/account/pm/?recipient=" + nickname).Result.Content.ReadAsStringAsync().Result;
                    }
                }
                string corrid = new Regex("<input type=\"hidden\" id=\"corr_id\" value=\"(.*)\"><input type=\"hidden\" id=\"item_id\"").Match(get).Groups[1].Value;
                string recip = new Regex("<input type=\"hidden\" id=\"recipient_id\" value=\"(.*)\"><div class=\"cols_table\"").Match(get).Groups[1].Value;
                string itemid = new Regex("id=\"item_id\" value=\"(.*)\"><input type=\"hidden\" id=\"recipient_id\"").Match(get).Groups[1].Value;
                Random random = new Random();
                double randomValue = random.NextDouble();

                Dictionary<string, string> data = new Dictionary<string, string>
            {
                {"message", text},
                {"search", "" }
            };

                string post = http.PostAsync(Domain+"/ajax/?section=pm&corr_id=" + corrid + "&recip=" + recip + "&ss=0&item_id=" + itemid + "&arc=0&" + randomValue, new FormUrlEncodedContent(data)).Result.Content.ReadAsStringAsync().Result;

                Log.GoodMessage("Успешно отправлено сообщение " + nickname + ", текст\n" + text);


                return true;
            }
            catch
            {
                Log.ExMessage("Не удалось отправить сообщение " + nickname + ", текст\n" + text);
                return false;
            }
        }
    }
}
