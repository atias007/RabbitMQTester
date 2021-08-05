using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("- Client");
            Console.WriteLine("-----------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleKeyInfo x = new('0', ConsoleKey.Escape, false, false, false);

            var factory = new ConnectionFactory() { HostName = "localhost", AutomaticRecoveryEnabled = true, NetworkRecoveryInterval = TimeSpan.FromSeconds(10) };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            var dict = new Dictionary<string, object>(new[] {
                new KeyValuePair<string, object>("x-max-priority", 10) ,
                new KeyValuePair<string, object>("x-dead-letter-exchange","dlx_pq_exchange")
                }
            );

            channel.QueueDeclare(queue: "priority_queue", durable: true, exclusive: false, autoDelete: false, arguments: dict);
            channel.BasicQos(0, 5, false);
            var consumer = new EventingBasicConsumer(channel);

            Console.WriteLine(" [*] Waiting for messages.");
            Task.Delay(1000).Wait();

            consumer.Received += (sender, ea) =>
            {
                if (startDate == null) { startDate = DateTime.Now; }
                var t = Task.Run(() =>
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonConvert.DeserializeObject<MyMessage>(json);

                    var time = (DateTime.Now.Millisecond % 10) + 1;
                    _passTime += time;
                    Console.WriteLine($" [x] {message.DateCreated:HHmmss.fff} | ({message.Title}), wait {time} sec");

                    Task.Delay(time * 1000).Wait();
                    ((EventingBasicConsumer)sender).Model.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                })
                .ContinueWith(t =>
                {
                    endDate = DateTime.Now;
                });
                AddTask(t);
            };
            channel.BasicConsume(queue: "priority_queue", autoAck: false, consumer: consumer);

            while (x.KeyChar != 'x')
            {
                x = Console.ReadKey(true);
            }

            consumer.ConsumerTags.ToList().ForEach(t => channel.BasicCancel(t));
            var y = GetRunningTasks();
            while (y > 0)
            {
                Task.Delay(100).Wait();
                Console.WriteLine($"{y}");
                Console.CursorTop -= 1;
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

            channel.Dispose();
            connection.Dispose();

            Console.ReadLine();
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
                return _waits.Where(t => t.Status == TaskStatus.Running || t.Status == TaskStatus.WaitingToRun).Count();
            }
        }
    }
}