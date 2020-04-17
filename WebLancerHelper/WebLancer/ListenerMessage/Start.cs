using System;
using System.Threading;

namespace WebLancerHelper.WebLancer.ListenerMessage
{
    internal class Start : Listener
    {

        public Start(Action<Model.ListenerMessage> action, API api)
        {
            Log.ProcessMessage("Запускаем прослушивания сообщений");

            Thread listner = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        Objects.Contact[] contacts = api.GetContacts();
                        foreach (Objects.Contact contact in contacts)
                        {
                            if (contact.IsNew == true)
                            {
                                Log.ProcessMessage("Получили новое сообщение от " + contact.Nick);
                                Objects.Message.MessageUnion[] data = api.GetMessages(contact.Nick);
                                string text = "";
                                string time = "";
                                for (int i = data[0].MessageClassArray.Length - 1; i >= 0; i--)
                                {
                                    if (data[0].MessageClassArray[i].Login == contact.Nick)
                                    {
                                        text = data[0].MessageClassArray[i].Message;
                                        time = data[0].MessageClassArray[i].Time + " " + UnixTimeStampToDateTime(data[0].MessageClassArray[i].AddedTime).ToShortTimeString();
                                    }
                                }
                                Model.ListenerMessage senderMessage = new Model.ListenerMessage
                                {
                                    name = contact.Name,
                                    nick = contact.Nick,
                                    link = API.Domain + contact.Link,
                                    text = text,
                                    time = time
                                };
                                action.Invoke(senderMessage);
                                Log.GoodMessage("Сообщение передано на отправку");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.ExMessage("Ошибка при прослушивании: \n\n" + ex.ToString());
                    }
                    Thread.Sleep(TimeSpan.FromMinutes(3));
                }
            })
            {
                IsBackground = true
            };
            listner.Start();
        }

       


    }
}
