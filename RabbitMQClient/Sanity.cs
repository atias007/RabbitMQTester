using CustomsCloud.InfrastructureCore.Queue;
using RabbitMQScom;
using System;
using System.Threading.Tasks;

namespace RabbitMQClient
{
    public static class Sanity
    {
        public static void Run()
        {
            BaseUtil.WriteTitle("Sanity", null);

            Tester.Run().Wait();

            BaseUtil.WriteFooter();
        }
    }
}