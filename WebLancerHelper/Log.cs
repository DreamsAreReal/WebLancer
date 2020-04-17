using System;
using System.Text;

namespace WebLancerHelper
{
    internal class Log
    {
        public static void SetupConsole()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
        }

        public static void ExMessage(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(DateTime.Now.ToShortTimeString() + " // " + text);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void ProcessMessage(string text)
        {
            Console.WriteLine(DateTime.Now.ToShortTimeString() + " // " + text);
        }

        public static void GoodMessage(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(DateTime.Now.ToShortTimeString() + " // " + text);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
