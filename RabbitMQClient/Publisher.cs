using CustomsCloud.InfrastructureCore.Queue;
using System;
using System.Threading.Tasks;

namespace RabbitMQClient
{
    public static class Publisher
    {
        public static void Run(IQueueUtil util, Config config)
        {
            BaseUtil.WriteTitle("Publisher", config.QueueName);

            var prop = new MessageProperties { Priority = 2, Persistance = true };
            Parallel.For(0, config.HighPriorityTotal, i =>
            {
                var title = $"A{i:N3} | Message with priority 2 | No TTL";
                var message = new MyMessage { Title = title, Priority = 2 };
                util.SendMessage(config.QueueName, message, prop);
            });
            Console.WriteLine($" [x] Sent {config.HighPriorityTotal} messages | Priority 2 | No TTL ");

            prop.Priority = 5;
            Parallel.For(0, config.MediumPriorityTotal, i =>
            {
                var title = $"B{i:N3} | Message with priority 5 | No TTL";
                var message = new MyMessage { Title = title, Priority = 5 };
                util.SendMessage(config.QueueName, message, prop);
            });
            Console.WriteLine($" [x] Sent {config.MediumPriorityTotal} messages | Priority 5 | No TTL ");

            prop.Priority = 1;
            prop.Expiration = TimeSpan.FromSeconds(5);
            Parallel.For(0, config.TTLTotal, i =>
            {
                var title = $"C{i:N3} | Message with priority 1 | 5sec TTL";
                var message = new MyMessage { Title = title, Priority = 1 };
                util.SendMessage(config.QueueName, message, prop);
            });
            Console.WriteLine($" [x] Sent {config.TTLTotal} messages | Priority 1 | 5sec TTL ");

            BaseUtil.WriteFooter();
        }
    }
}