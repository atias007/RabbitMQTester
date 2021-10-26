using CustomsCloud.InfrastructureCore.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQClient
{
    // https://blog.devgenius.io/rabbitmq-with-docker-on-windows-in-30-minutes-172e88bb0808
    internal class Program
    {
        private static readonly List<Task> _waits = new();
        private static readonly object Locker = new();
        private static DateTime? startDate;
        private static DateTime endDate;
        private static int _passTime = 0;
        private static readonly List<int> _priority = new();

        private static void Handler(MessageEventArgs args)
        {
            var t = Task.Run(() =>
            {
                if (startDate == null) { startDate = DateTime.Now; }
                var message = JsonConvert.DeserializeObject<MyMessage>(args.Body);
                _priority.Add(message.Priority);

                var time = new Random().Next(1, 10);
                _passTime += time;
                Console.WriteLine($" [x] {message.DateCreated:HH:mm:ss} | ({message.Title}), wait {time} sec");

                Task.Delay(time * 1000).Wait();
                args.Acknowledge();
            })
            .ContinueWith(t =>
            {
                endDate = DateTime.Now;
            });
            AddTask(t);
        }

        private static void Main(string[] args)
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

            var util = serviceProvider.GetService(typeof(IQueueUtil)) as IQueueUtil;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("- Client");
            Console.WriteLine("-----------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleKeyInfo x = new('0', ConsoleKey.Escape, false, false, false);

            var count = util.GetCount("TestQueue");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{count} messages in queue");
            Console.ForegroundColor = ConsoleColor.White;

            if (count != 63)
            {
                util.Purge("TestQueue");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Purge Queue --> run publisher");
                Console.ForegroundColor = ConsoleColor.White;
            }

            Console.WriteLine();

            util.Receive(queueName: "TestQueue", maxHandleItems: 5, responseAction: Handler);

            Console.WriteLine(" [*] Waiting for messages.");

            while (x.KeyChar != 'x')
            {
                x = Console.ReadKey(true);
            }

            util.CancelConsume("TestQueue");
            Console.WriteLine();
            Console.WriteLine();

            var y = GetRunningTasks();
            while (y > 0)
            {
                Task.Delay(100).Wait();
                Console.CursorTop -= 1;
                Console.WriteLine($"wait for {y} tasks to complete...");
                y = GetRunningTasks();
            }

            Console.WriteLine("Bye bye");

            var total = endDate.Subtract(startDate.GetValueOrDefault()).TotalSeconds;
            Console.WriteLine($"Elpased seconds: {_passTime:N0}");
            Console.WriteLine($"Actual seconds: {total:N0}");

            var ratio = _passTime * 1.0 / total;
            if (ratio < 5.5)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.WriteLine($"Time rate: {ratio:N2}");

            var counter = _priority.Take(10).Count(a => a == 5);
            if (counter == 10)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.WriteLine($"Priority high counter: {counter}");

            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();

            //await host.RunAsync();
        }

        private static void AddTask(Task t)
        {
            lock (Locker)
            {
                _waits.Add(t);
                _waits.RemoveAll(t => t.Status == TaskStatus.RanToCompletion);
            }
        }

        private static int GetRunningTasks()
        {
            lock (Locker)
            {
                return _waits.Where(t => t.Status != TaskStatus.RanToCompletion).Count();
            }
        }
    }
}