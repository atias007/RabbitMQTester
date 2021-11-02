using System.Threading.Tasks;

namespace RabbitMQScom
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            await Tester.Run();
        }
    }
}