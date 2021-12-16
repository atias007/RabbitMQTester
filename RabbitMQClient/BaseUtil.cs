using System;

namespace RabbitMQClient
{
    public static class BaseUtil
    {
        public static void WriteTitle(string module, string queueName)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine($"- {module}   ({queueName})");
            Console.WriteLine("-----------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;
        }

        // xxxxxxxxxxxxxxxxx
        public static void WriteFooter()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(" Press [enter] to exit.");
            Console.ResetColor();
            Console.ReadLine();
        }
    }
}