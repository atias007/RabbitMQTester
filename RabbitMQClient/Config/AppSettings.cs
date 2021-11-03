using System.Collections.Generic;

namespace RabbitMQClient
{
    public class RabbitMQ
    {
        public List<string> Hosts { get; set; }
        public int Port { get; set; }
        public int ApiPort { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class Queue
    {
        public RabbitMQ RabbitMQ { get; set; }
    }

    public class Config
    {
        public string QueueName { get; set; }
        public int HighPriorityTotal { get; set; }
        public int MediumPriorityTotal { get; set; }
        public int TTLTotal { get; set; }
    }

    public class AppSettings
    {
        public Queue Queue { get; set; }
        public Config Config { get; set; }
    }
}