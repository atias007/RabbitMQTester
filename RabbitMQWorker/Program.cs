using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace RabbitMQPublisher
{
    internal class Program
    {
        // https://blog.devgenius.io/rabbitmq-with-docker-on-windows-in-30-minutes-172e88bb0808

        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            //setup our DI
            var serviceProvider = new ServiceCollection()
                .AddLogging(c => c.AddConsole())
                .AddSingleton(typeof(IQueueUtil), typeof(RabbitQueueUtil))
                .AddSingleton<IConfiguration>(config)
                .BuildServiceProvider();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("- Publisher");
            Console.WriteLine("-----------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;

            var util = serviceProvider.GetService(typeof(IQueueUtil)) as IQueueUtil;

            var prop = new MessageProperties { Priority = 2, Persistance = true };
            Parallel.For(0, 50, i =>
            {
                var title = $"A{i:N3} | Message with priority 2 | No TTL";
                var message = new MyMessage { Title = title, Priority = 2 };
                util.SendMessage("TestQueue", message, prop);
            });
            Console.WriteLine(" [x] Sent 50 messages | Priority 2 | No TTL ");

            prop.Priority = 5;
            Parallel.For(0, 10, i =>
            {
                var title = $"B{i:N3} | Message with priority 5 | No TTL";
                var message = new MyMessage { Title = title, Priority = 5 };
                util.SendMessage("TestQueue", message, prop);
            });
            Console.WriteLine(" [x] Sent 10 messages | Priority 5 | No TTL ");

            prop.Priority = 1;
            prop.Expiration = TimeSpan.FromSeconds(5);
            Parallel.For(0, 3, i =>
            {
                var title = $"C{i:N3} | Message with priority 1 | 5sec TTL";
                var message = new MyMessage { Title = title, Priority = 1 };
                util.SendMessage("TestQueue", message, prop);
            });
            Console.WriteLine(" [x] Sent 3 messages | Priority 1 | 5sec TTL ");

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}