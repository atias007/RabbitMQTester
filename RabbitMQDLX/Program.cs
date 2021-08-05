using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQDLX
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("- DLX");
            Console.WriteLine("-----------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleKeyInfo x = new('0', ConsoleKey.Escape, false, false, false);

            var factory = new ConnectionFactory() { HostName = "localhost", AutomaticRecoveryEnabled = true, NetworkRecoveryInterval = TimeSpan.FromSeconds(10) };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "priority_queue_dlx", durable: true, exclusive: false, autoDelete: false, arguments: null);
            channel.BasicQos(0, 5, false);
            var consumer = new EventingBasicConsumer(channel);

            Console.WriteLine(" [*] Waiting for messages.");

            consumer.Received += (sender, ea) =>
            {
                var t = Task.Run(() =>
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonConvert.DeserializeObject<MyMessage>(json);

                    var time = (DateTime.Now.Millisecond % 10) + 1;

                    Console.WriteLine($" [x] {message.DateCreated:HHmmss.fff} | ({message.Title}), wait {time} sec");

                    Thread.Sleep(time * 1000);
                    ((EventingBasicConsumer)sender).Model.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                });
            };
            channel.BasicConsume(queue: "priority_queue_dlx", autoAck: false, consumer: consumer);

            while (x.KeyChar != 'x')
            {
                x = Console.ReadKey();
                consumer.ConsumerTags.ToList().ForEach(t => channel.BasicCancel(t));
            }

            Console.WriteLine("Bye bye");
            channel.Dispose();
            connection.Dispose();
        }
    }
}