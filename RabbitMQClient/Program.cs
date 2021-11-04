using CustomsCloud.InfrastructureCore.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System;

namespace RabbitMQClient
{
    // https://blog.devgenius.io/rabbitmq-with-docker-on-windows-in-30-minutes-172e88bb0808
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var tuple = Start();
                bool showMenu = true;
                while (showMenu)
                {
                    try
                    {
                        showMenu = MainMenu(tuple);
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.WriteException(ex);
                        BaseUtil.WriteFooter();
                    }
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
            }

            BaseUtil.WriteFooter();
        }

        private static (IQueueUtil, AppSettings) Start()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var appSettings = new AppSettings();
            config.Bind(appSettings);

            //setup our DI
            var serviceProvider = new ServiceCollection()
                .AddLogging(c => c.AddConsole())
                .AddSingleton(typeof(IQueueUtil), typeof(RabbitQueueUtil))
                .AddSingleton<IConfiguration>(config)
                .BuildServiceProvider();

            var queueUtil = serviceProvider.GetService(typeof(IQueueUtil)) as IQueueUtil;
            return new(queueUtil, appSettings);
        }

        private static bool MainMenu((IQueueUtil, AppSettings) tuple)
        {
            var util = tuple.Item1;
            var settings = tuple.Item2;

            Console.Clear();
            Console.ResetColor();
            Console.WriteLine("-----------------------");
            Console.WriteLine("1) Sanity");
            Console.WriteLine("2) Test Connections");
            Console.WriteLine("3) Publish messages");
            Console.WriteLine("4) Purge queue");
            Console.WriteLine("5) Consume Messages");
            Console.WriteLine("6) Count Messages");
            Console.WriteLine("-----------------------");
            Console.WriteLine("9) Exit");
            Console.Write("\r\nSelect an option: ");

            switch (Console.ReadLine())
            {
                case "1":
                    Sanity.Run(settings).Wait();
                    return true;

                case "2":
                    Sanity.TestConnections(settings).Wait();
                    return true;

                case "3":
                    Publisher.Run(util, settings.Config);
                    return true;

                case "4":
                    General.Purge(util, settings.Config);
                    return true;

                case "5":
                    Listener.Run(util, settings.Config);
                    return true;

                case "6":
                    General.Count(util, settings.Config);
                    return true;

                case "9":
                    return false;

                default:
                    return true;
            }
        }
    }
}