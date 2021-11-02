﻿using CustomsCloud.InfrastructureCore.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQScom;
using System;

namespace RabbitMQClient
{
    // https://blog.devgenius.io/rabbitmq-with-docker-on-windows-in-30-minutes-172e88bb0808
    internal class Program
    {
        private static IQueueUtil _queueUtil;
        private static Config _config = new Config();

        private static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            config.GetSection("Config").Bind(_config);

            //setup our DI
            var serviceProvider = new ServiceCollection()
                .AddLogging(c => c.AddConsole())
                .AddSingleton(typeof(IQueueUtil), typeof(RabbitQueueUtil))
                .AddSingleton<IConfiguration>(config)
                .BuildServiceProvider();

            _queueUtil = serviceProvider.GetService(typeof(IQueueUtil)) as IQueueUtil;

            bool showMenu = true;
            while (showMenu)
            {
                showMenu = MainMenu();
            }
        }

        private static bool MainMenu()
        {
            Console.Clear();
            Console.ResetColor();
            Console.WriteLine("-----------------------");
            Console.WriteLine("1) Sanity");
            Console.WriteLine("2) Publish messages");
            Console.WriteLine("3) Purge queue");
            Console.WriteLine("4) Consume Messages");
            Console.WriteLine("5) Count Messages");
            Console.WriteLine("-----------------------");
            Console.WriteLine("9) Exit");
            Console.Write("\r\nSelect an option: ");

            switch (Console.ReadLine())
            {
                case "1":
                    Sanity.Run();
                    return true;

                case "2":
                    Publisher.Run(_queueUtil, _config);
                    return true;

                case "3":
                    General.Purge(_queueUtil, _config);
                    return true;

                case "4":
                    Listener.Run(_queueUtil, _config);
                    return true;

                case "5":
                    General.Count(_queueUtil, _config);
                    return true;

                case "9":
                    return false;

                default:
                    return true;
            }
        }
    }
}