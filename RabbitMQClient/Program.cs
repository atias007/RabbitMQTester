using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQClient
{
    // https://blog.devgenius.io/rabbitmq-with-docker-on-windows-in-30-minutes-172e88bb0808
    internal class Program
    {
        private static List<Task> _waits = new List<Task>();
        private static readonly object Locker = new object();

        private static void Main(string[] args)
        {
            ConsoleKeyInfo x = new('0', ConsoleKey.Escape, false, false, false);

            var factory = new ConnectionFactory() { HostName = "localhost", AutomaticRecoveryEnabled = true, NetworkRecoveryInterval = TimeSpan.FromSeconds(10) };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            var dict = new Dictionary<string, object>(new[] { new KeyValuePair<string, object>("x-max-priority", 10) });
            channel.QueueDeclare(queue: "priority_queue", durable: true, exclusive: false, autoDelete: false, arguments: dict);
            channel.BasicQos(0, 5, false);
            var consumer = new EventingBasicConsumer(channel);

            Console.WriteLine(" [*] Waiting for messages.");

            consumer.Received += (sender, ea) =>
            {
                var t = Task.Run(() =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var time = (DateTime.Now.Millisecond % 10) + 1;
                    Console.WriteLine(" [x] Received {0}, wait {1} sec", message, time);
                    Thread.Sleep(time * 1000);
                    ((EventingBasicConsumer)sender).Model.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                });
                AddTask(t);
            };
            channel.BasicConsume(queue: "priority_queue", autoAck: false, consumer: consumer);

            while (x.KeyChar != 'x')
            {
                x = Console.ReadKey();
                consumer.ConsumerTags.ToList().ForEach(t => channel.BasicCancel(t));
            }

            var y = GetRunningTasks();
            while (y > 0)
            {
                Thread.Sleep(200);
                Console.WriteLine($"{y}");
                y = GetRunningTasks();
            }

            Console.WriteLine("Bye bye");
            channel.Dispose();
            connection.Dispose();
            //Console.ReadLine();
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