using System;
using System.Collections.Generic;

namespace WebLancerHelper
{
    internal class Program
    {
        private static TelegramBot bot;

        private static void Main(string[] args)
        {
            // TODO: Сделать запоминалку данных
            // TODO: Сделать работу через прокси
            // TODO: Сделать полное управление через кнопки
            // TODO: Сделать класс для задержок, чтобы легче настраивать было
            Log.SetupConsole();
            Console.Write("Введите логин (пример: login): ");
            string login = Console.ReadLine();
            Console.Write("Введите пароль (пример: password): ");
            string password = Console.ReadLine();
            Console.Write("Введите токен телеграм бота (пример: 0000000:XXXXXXXXXXXXXXXXX): ");
            string token = Console.ReadLine();
            Console.Write("Введите свой id в телеграмме  (пример: 0000000): ");
            long id = 0;
            long.TryParse(Console.ReadLine(), out id);
            if(id==0)
            {
                Log.ExMessage("Неверный ID");
                Console.ReadKey();
                return;
            }    
            WebLancer.API api = new WebLancer.API();
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
            Console.Write("\n\nВведите номера категорий через запятую (пример: 1,21,3) [Если оставить пустым, будет парсить все категории]: ");
            var categoriesIndices = PrepareListCategories(Console.ReadLine());
      
            Console.Clear();

            Log.ProcessMessage("Запуск...");
            api.SetLogin = login;
            api.SetPassword = password;
            if(!api.Auth())
            {
                Log.ExMessage("Неверный логин или пароль");
                Console.ReadKey();
                return;
            }
            bot = new TelegramBot(token, id, api);
            new WebLancer.ListenerMessage.Start(ListenerMessageSend, api);
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
                int num = 0;
                array[i].Trim();
                int.TryParse(array[i], out num);
                if(num!=0 && !indices.Contains(num-1))
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
