using System;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace WebLancerHelper
{
    public class TelegramBot
    {
        private readonly TelegramBotClient client;
        private readonly long id;
        private readonly WebLancer.API api;
        public TelegramBot(string token, long id, WebLancer.API api)
        {
            this.api = api;
            this.id = id;
            client = new TelegramBotClient(token);
            client.OnMessage += BotOnMessageReceived;
            client.StartReceiving();
            Log.GoodMessage("ID телеграм бота: " + client.BotId);

        }

        private void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            if (messageEventArgs.Message.Type != Telegram.Bot.Types.Enums.MessageType.Text)
            {
                return;
            }

            if (messageEventArgs.Message.Chat.Id != id)
            {
                return;
            }

            if (messageEventArgs.Message.Text.Contains("/messages"))
            {
                string[] data = messageEventArgs.Message.Text.Split(' ');
                if (data.Length == 2)
                {
                    WebLancer.Objects.Message.MessageUnion[] messages = api.GetMessages(data[1]);
                    if(messages == null)
                    {
                        SendMessage("Вы не авторизованы");
                        return;
                    }

                    if (messages.Length!=0)
                    {
                        if (messages[0].MessageClassArray == null)
                        {
                            SendMessage("Ошибка, нет сообщений");
                            return;
                        }
                        string text = "";
                        for (int i = 0; i < messages[0].MessageClassArray.Length; i++)
                        {
                            text += "<b>" + messages[0].MessageClassArray[i].Login + "</b>: <i>" + messages[0].MessageClassArray[i].Message + "</i>\n";
                        }
                        SendMessage(text);
                    }
                    else
                    {
                        SendMessage("Ошибка, нет сообщений");
                        return;
                    }
                }
                else
                {
                    SendMessage("Не верный формат: /messages [nickname]");
                    return;
                }
            }

            if (messageEventArgs.Message.Text.Contains("/send"))
            {
                string[] data = messageEventArgs.Message.Text.Split(' ');
                if (data.Length == 3)
                {
                    if (api.SendMessage(data[1], data[2]))
                    {
                        SendMessage("Успешно отправили сообщение юзеру: " + data[1]);
                    }
                    else
                    {
                        SendMessage("Не удалось отправить сообщение юзеру: " + data[1]);
                    }
                }
                else
                {
                    SendMessage("Не верный формат: /send [nickname] [text]");
                    return;
                }
            }
        }

        public void SendMessage(string text)
        {
            try
            { 
                client.SendTextMessageAsync(id, StripHtmlTagsUsingRegex(text), Telegram.Bot.Types.Enums.ParseMode.Html, true).Wait();
            }
            catch
            {
                Log.ExMessage("Не удалось отправить сообщение: \n" + text);
            }
        }
        public void SendMessage(string text, InlineKeyboardMarkup keyboard)
        {
            try
            {
                client.SendTextMessageAsync(id, StripHtmlTagsUsingRegex(text), Telegram.Bot.Types.Enums.ParseMode.Html, true, false, replyMarkup: keyboard).Wait();
            }
            catch
            {
                Log.ExMessage("Не удалось отправить сообщение: \n" + text);
            }
        }

        static string StripHtmlTagsUsingRegex(string inputString)
        {
            return Regex.Replace(inputString, @"<br \/>|<br >|<br>|<br\/>", String.Empty);
        }
    }
}
