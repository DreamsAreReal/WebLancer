using System;
using System.Collections.Generic;

namespace WebLancerHelper
{
    internal class Program
    {
        private static TelegramBot bot;

        private static void Main()
        {
            // TODO: Код ревью
            // TODO: Сделать работу через прокси
            // TODO: Сделать полное управление через кнопки
            Log.SetupConsole();
            Console.Write("Нужна ли авторизация? (0 - нет, 1 - да) [Если не авторизироваться, часть функционала не будет доступна]: ");
            var needAuthStr = Console.ReadLine().Trim();
            if ("1" != needAuthStr && "0" != needAuthStr)
            {
                Log.ExMessage("Неверный ответ");
                Console.ReadKey();
                return;
            }
            bool needAuth = needAuthStr == "1" ? true : false;
            string login = "";
            string password = "";
            if (needAuth)
            {
                Console.Write($"Введите логин (пример: login) [если оставить пустым, будет такое значение: {Properties.Settings.Default.Login}]: ");
                login = Console.ReadLine();
                if (login.Length == 0) login = Properties.Settings.Default.Login;
                Properties.Settings.Default.Login = login;
                Console.Write($"Введите пароль (пример: password) [если оставить пустым, будет такое значение: {Properties.Settings.Default.Password}]: ");
                password = Console.ReadLine();
                if (password.Length == 0) password = Properties.Settings.Default.Password;
                Properties.Settings.Default.Password = password;
            }
            Console.Write($"Введите токен телеграм бота (пример: 0000000:XXXXXXXXXXXXXXXXX) [если оставить пустым, будет такое значение: {Properties.Settings.Default.Token}]: ");
            string token = Console.ReadLine();
            if (token.Length == 0) token = Properties.Settings.Default.Token;
            Properties.Settings.Default.Token = token;
            Console.Write($"Введите свой id в телеграмме  (пример: 0000000) [если оставить пустым, будет такое значение: {Properties.Settings.Default.ID}]: ");
            string idString = Console.ReadLine();
            long id = 0;
            if (idString.Length == 0) id = Properties.Settings.Default.ID;
            else
            {
                if (!long.TryParse(idString, out id))
                {
                    Log.ExMessage("Неверный ID");
                    Console.ReadKey();
                    return;
                }
            }
            Properties.Settings.Default.ID = id;
            WebLancer.API api = new WebLancer.API(needAuth);
            if (!api.UpdateWorkCategory())
            {
                Console.ReadKey();
                return;
            }
            Console.Clear();
            for (int i =0; i<WebLancer.Objects.Category.Categories.Count; i++)
            {
                Console.WriteLine($"{i+1}) {WebLancer.Objects.Category.Categories[i].Name}");
            }
            Console.Write($"\n\nВведите номера категорий через запятую (пример: 1,21,3) (прошлые значения: {Properties.Settings.Default.Categories}) [Если оставить пустым, будет парсить все категории]: ");
            var categoriesString = Console.ReadLine();
            if (categoriesString.Length == 0) categoriesString = Properties.Settings.Default.Categories;
            Properties.Settings.Default.Categories = categoriesString;
            var categoriesIndices = PrepareListCategories(categoriesString);
            Properties.Settings.Default.Save();
      
            Console.Clear();

            Log.ProcessMessage("Запуск...");

            if(needAuth)
            {
                api.SetLogin = login;
                api.SetPassword = password;
                if (!api.Auth())
                {
                    Log.ExMessage("Неверный логин или пароль");
                    Console.ReadKey();
                    return;
                }
            }
            bot = new TelegramBot(token, id, api);
            if(needAuth) new WebLancer.ListenerMessage.Start(ListenerMessageSend, api);
            if (categoriesIndices.Length <= 0)
            {
                new WebLancer.ListenerTask.Start(ListenerTaskSend, api, "/jobs/?type=project");
            }
            else
            {
                foreach (var index in categoriesIndices)
                {
                    new WebLancer.ListenerTask.Start(ListenerTaskSend, api, WebLancer.Objects.Category.Categories[index].Href);
                }
            }
            bot.SendMessage("<b>Бот начал работу.</b>");
            Console.ReadKey();
        }

        private static int[] PrepareListCategories(string text)
        {
            var array = text.Split(',');
            List<int> indices = new List<int> { };
            for (int i=0; i<array.Length; i++)
            {
                int num;
                array[i] = array[i].Trim();
                if (!int.TryParse(array[i], out num)) continue;
                if(num>0 && !indices.Contains(num-1))
                {
                    indices.Add(num-1);
                }
            }
            return indices.ToArray();
        }

        private static void ListenerTaskSend(WebLancer.Objects.Task task)
        {
            string text = $"📝 <b>{task.Title}</b> 📝\n\n{task.Discription}\n\n💰 {task.Price}\n\n👩‍💻 <i>{task.Applications}</i> 👨‍💻";
            var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithUrl("Открыть проект в браузере", task.Link));
            bot.SendMessage(text, keyboard);

            Log.GoodMessage(text);

        }

        private static void ListenerMessageSend(WebLancer.ListenerMessage.Model.ListenerMessage message)
        {
            // TODO: Оформить нормально сообщения
            string text = message.name + " (" + message.nick + ")\n\n" + message.text + "\n\n" + message.time + "\n" + message.link;

            bot.SendMessage(text);

            Log.GoodMessage(text);
        }
    }
}
