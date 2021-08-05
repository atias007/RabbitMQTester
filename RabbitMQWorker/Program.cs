using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RabbitMQWorker
{
    internal class Program
    {
        // https://blog.devgenius.io/rabbitmq-with-docker-on-windows-in-30-minutes-172e88bb0808
        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var dict = new Dictionary<string, object>(new[] { new KeyValuePair<string, object>("x-max-priority", 10) });
                channel.QueueDeclare(queue: "priority_queue", durable: true, exclusive: false, autoDelete: false, arguments: dict);

                //Console.Write("Type a message: ");
                //var message = Console.ReadLine();

                for (int i = 0; i < 100; i++)
                {
                    var message = $"({i + 1}) Message with priority 1";
                    var body = Encoding.UTF8.GetBytes(message);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.Priority = 1;

                    channel.BasicPublish(exchange: "",
                                         routingKey: "priority_queue",
                                         basicProperties: properties,
                                         body: body);

                    Console.WriteLine(" [x] Sent {0}", message);
                }

                var message1 = $"({41}) Message with priority 5";
                var body1 = Encoding.UTF8.GetBytes(message1);

                var properties1 = channel.CreateBasicProperties();
                properties1.Persistent = true;
                properties1.Priority = 5;

                channel.BasicPublish(exchange: "",
                                     routingKey: "priority_queue",
                                     basicProperties: properties1,
                                     body: body1);
                Console.WriteLine(" [x] Sent {0}", message1);

                channel.BasicPublish(exchange: "",
                                     routingKey: "priority_queue",
                                     basicProperties: properties1,
                                     body: body1);

                Console.WriteLine(" [x] Sent {0}", message1);
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}