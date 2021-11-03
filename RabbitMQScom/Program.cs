using Spectre.Console;
using System.Threading.Tasks;

namespace RabbitMQScom
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {
                await Tester.Run();
            }
            catch (System.Exception ex)
            {
                AnsiConsole.WriteException(ex);
            }
        }
    }
}