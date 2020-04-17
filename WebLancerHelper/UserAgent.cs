using System;
using System.IO;

namespace WebLancerHelper
{
    internal class UserAgent
    {
        private readonly string[] userAgents = { };
        private static UserAgent userAgent = null;

        public UserAgent()
        {
            if (File.Exists("useragent.txt"))
            {
                userAgents = File.ReadAllLines("useragent.txt");
            }
            else
            {
                Log.ExMessage("Не найден файл useragent.txt");
            }
        }

        public static UserAgent Initilization()
        {
            if (userAgent != null)
            {
                return userAgent;
            }
            else
            {
                userAgent = new UserAgent();
                return userAgent;
            }
        }

        public string GetRandom()
        {
            if (userAgents.Length > 0)
            {
                Random random = new Random();
                return userAgents[random.Next(0, userAgents.Length - 1)];
            }
            else
            {
                Log.ExMessage("В списке нет user-agent's");
                return "";
            }
        }


    }
}
