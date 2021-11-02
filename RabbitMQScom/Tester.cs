using Newtonsoft.Json;
using RabbitMQ.Client;
using Spectre.Console;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RabbitMQScom
{
    public static class Tester
    {
        public static async Task Run()
        {
            HttpClientHandler handler = new();
            handler.Credentials = new NetworkCredential("guest", "guest");
            using var httpClient = new HttpClient(handler);
            using var response = await httpClient.GetAsync("http://localhost:15672/api/queues");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var apiResponse = await response.Content.ReadAsStringAsync();
                var qs = JsonConvert.DeserializeObject<RabbitQueueDTO[]>(apiResponse);
                var table = new Table().AddColumns("Queue Name", "Count", "UnAck", "Consumers", "Is Alive");
                foreach (var q in qs)
                {
                    table.AddRow(q.name, $"{q.messages:N0}", $"{q.consumers:N0}", $"{q.messages_unacknowledged:N0}");
                }

                AnsiConsole.Live(table)
                    .Start(ctx =>
                    {
                        ctx.Refresh();
                        for (int i = 0; i < qs.Length; i++)
                        {
                            var res = CheckConnection(qs[i].name);
                            if (res)
                            {
                                table.Rows.Update(i, 4, new Markup("[green]true[/]"));
                            }
                            else
                            {
                                table.Rows.Update(i, 4, new Markup("[red]false[/]"));
                            }
                            ctx.Refresh();
                        }
                    });

                //AnsiConsole.Write(table);
            }
            else
            {
                Console.WriteLine($"Fail to connect to RabbitMQ management API. StatusCode {response.StatusCode}");
            }
        }

        private static bool CheckConnection(string queueName)
        {
            var factory = new ConnectionFactory
            {
                HostName = "127.0.0.1",
                AutomaticRecoveryEnabled = true,
                Password = "guest",
                Port = 5672,
                UserName = "guest",
            };

            try
            {
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    _ = channel.BasicGet(queueName, false);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}