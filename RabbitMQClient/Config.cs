namespace RabbitMQClient
{
    public class Config
    {
        public string QueueName { get; set; }

        public int HighPriorityTotal { get; set; }

        public int MediumPriorityTotal { get; set; }

        public int TTLTotal { get; set; }
    }
}