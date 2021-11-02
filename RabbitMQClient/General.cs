using CustomsCloud.InfrastructureCore.Queue;
using System;

namespace RabbitMQClient
{
    public static class General
    {
        public static void Purge(IQueueUtil util, Config config)
        {
            BaseUtil.WriteTitle("Purge", config.QueueName);

            WriteCount(util, config);
            util.Purge(config.QueueName);

            BaseUtil.WriteFooter();
        }

        public static void Count(IQueueUtil util, Config config)
        {
            BaseUtil.WriteTitle("Count", config.QueueName);
            WriteCount(util, config);
            BaseUtil.WriteFooter();
        }

        private static void WriteCount(IQueueUtil util, Config config)
        {
            var count = util.GetCount(config.QueueName);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{count} messages in queue");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}