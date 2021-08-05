using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQPublisher
{
    internal class Program
    {
        // https://blog.devgenius.io/rabbitmq-with-docker-on-windows-in-30-minutes-172e88bb0808
        public static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("- Publisher");
            Console.WriteLine("-----------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;

            var factory = new ConnectionFactory { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var dict = new Dictionary<string, object>(new[] {
                    new KeyValuePair<string, object>("x-max-priority", 10) ,
                    new KeyValuePair<string, object>("x-dead-letter-exchange","dlx_pq_exchange")
                    }
                );

                channel.QueueDeclare(queue: "priority_queue", durable: true, exclusive: false, autoDelete: false, arguments: dict);

                var i = 0;
                for (i = 0; i < 100; i++)
                {
                    var title = $"{i} | Message with priority 2 | No TTL";
                    var message = new MyMessage { Title = title };
                    var json = JsonConvert.SerializeObject(message);
                    var body = Encoding.UTF8.GetBytes(json);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.Priority = 2;

                    channel.BasicPublish(exchange: "",
                                         routingKey: "priority_queue",
                                         basicProperties: properties,
                                         body: body);

                    Console.WriteLine(" [x] Sent {0}", title);
                }

                var t = 0;
                for (t = 0; t < 20; t++)
                {
                    var title = $"{i + t} | Message with priority 5 | No TTL";
                    var message = new MyMessage { Title = title };
                    var json = JsonConvert.SerializeObject(message);
                    var body = Encoding.UTF8.GetBytes(json);

                    var properties1 = channel.CreateBasicProperties();
                    properties1.Persistent = true;
                    properties1.Priority = 5;

                    channel.BasicPublish(exchange: "",
                                         routingKey: "priority_queue",
                                         basicProperties: properties1,
                                         body: body);

                    Console.WriteLine(" [x] Sent {0}", title);
                }

                for (int u = 0; u < 3; u++)
                {
                    var title = $"{t + u} | Message with priority 1 | 60sec TTL";
                    var message = new MyMessage { Title = title };
                    var json = JsonConvert.SerializeObject(message);
                    var body = Encoding.UTF8.GetBytes(json);

                    var properties1 = channel.CreateBasicProperties();
                    properties1.Persistent = true;
                    properties1.Priority = 1;
                    properties1.Expiration = "60000";

                    channel.BasicPublish(exchange: "",
                                         routingKey: "priority_queue",
                                         basicProperties: properties1,
                                         body: body);

                    Console.WriteLine(" [x] Sent {0}", title);
                }

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}