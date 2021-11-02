using CustomsCloud.InfrastructureCore.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQClient
{
    public static class Listener
    {
        private static readonly List<Task> _waits = new();
        private static readonly object Locker = new();
        private static DateTime? startDate;
        private static DateTime endDate;
        private static int _passTime = 0;
        private static readonly List<int> _priority = new();

        public static void Run(IQueueUtil util, Config config)
        {
            BaseUtil.WriteTitle("Listener", config.QueueName);

            ConsoleKeyInfo x = new('0', ConsoleKey.Escape, false, false, false);

            var count = util.GetCount(config.QueueName);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{count} messages in queue");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine();

            util.Receive(queueName: config.QueueName, maxHandleItems: 5, responseAction: Handler);

            Console.WriteLine(" [*] Waiting for messages.");
            Console.WriteLine("     --> press x key to stop listen");

            while (x.KeyChar != 'x')
            {
                x = Console.ReadKey(true);
            }

            util.CancelConsume(config.QueueName);
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

            BaseUtil.WriteFooter();
        }

        private static void Handler(MessageEventArgs args)
        {
            var t = Task.Run(() =>
            {
                if (startDate == null) { startDate = DateTime.Now; }
                var message = JsonConvert.DeserializeObject<MyMessage>(args.Body);
                lock (Locker)
                {
                    _priority.Add(message.Priority);
                }

                var time = new Random().Next(1, 3);
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