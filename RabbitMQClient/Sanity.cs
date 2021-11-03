using Newtonsoft.Json;
using RabbitMQ.Client;
using Spectre.Console;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RabbitMQClient
{
    public static class Sanity
    {
        public static async Task Run(AppSettings settings)
        {
            BaseUtil.WriteTitle("Sanity", "All Queues");
            var queueConfig = settings.Queue.RabbitMQ;
            HttpClientHandler handler = new();
            handler.Credentials = new NetworkCredential(queueConfig.Username, queueConfig.Password);
            using var httpClient = new HttpClient(handler);
            using var response = await httpClient.GetAsync($"http://{queueConfig.Hosts[0]}:{queueConfig.ApiPort}/api/queues");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var apiResponse = await response.Content.ReadAsStringAsync();
                var qs = JsonConvert.DeserializeObject<RabbitQueueDTO[]>(apiResponse);
                var table = new Table().AddColumns("Queue Name", "Count", "Consumers", "UnAck", "Is Alive");
                foreach (var q in qs)
                {
                    table.AddRow(q.name, $"{q.messages:N0}", GetConsumersMarkup(q.consumers.GetValueOrDefault()), GetUnack(q.messages, q.messages_unacknowledged));
                }

                AnsiConsole.Live(table)
                    .Start(ctx =>
                    {
                        ctx.Refresh();
                        for (int i = 0; i < qs.Length; i++)
                        {
                            var res = CheckConnection(qs[i].name, queueConfig);
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

            BaseUtil.WriteFooter();
        }

        private static string GetConsumersMarkup(int consumers)
        {
            if (consumers == 0)
            {
                return $"[red]{consumers:N0}[/]";
            }
            else
            {
                return $"{consumers:N0}";
            }
        }

        private static string GetUnack(int messages, int unack)
        {
            if (messages > 0 && unack == 0)
            {
                return $"[red]{unack:N0}[/]";
            }
            else
            {
                return $"{unack:N0}";
            }
        }

        private static bool CheckConnection(string queueName, RabbitMQ config)
        {
            var factory = new ConnectionFactory
            {
                AutomaticRecoveryEnabled = true,
                Password = config.Username,
                Port = config.Port,
                UserName = config.Password,
            };

            try
            {
                using (var connection = factory.CreateConnection(config.Hosts))
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