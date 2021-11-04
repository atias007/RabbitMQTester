using Newtonsoft.Json;
using RabbitMQ.Client;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
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

            var data = await GetRabbitData(settings);
            if (data != null)
            {
                var table = new Table().AddColumns("Queue Name", "Count", "Consumers", "UnAck", "Is Alive");
                foreach (var q in data)
                {
                    table.AddRow(
                        q.name,
                        $"{q.messages:N0}",
                        GetConsumersMarkup(q.consumers.GetValueOrDefault()),
                        GetUnack(q.messages, q.messages_unacknowledged),
                        GetIsAliveMarkup(q.name, settings));
                }

                AnsiConsole.Write(table);

                ////AnsiConsole.Live(table)
                ////    .Start(ctx =>
                ////    {
                ////        ctx.Refresh();
                ////        for (int i = 0; i < data.Count(); i++)
                ////        {
                ////            var res = CheckConnection(data.ElementAt(i).name, settings);
                ////            if (res)
                ////            {
                ////                table.Rows.Update(i, 4, new Markup("[green]true[/]"));
                ////            }
                ////            else
                ////            {
                ////                table.Rows.Update(i, 4, new Markup("[red]false[/]"));
                ////            }
                ////            ctx.Refresh();
                ////        }
                ////    });
            }

            BaseUtil.WriteFooter();
        }

        public static async Task TestConnections(AppSettings settings)
        {
            BaseUtil.WriteTitle("Test Connections", "All Queues");

            var data = await GetRabbitData(settings);
            if (data != null)
            {
                foreach (var q in data)
                {
                    try
                    {
                        AnsiConsole.Write($"{q.name}... ");
                        CheckConnection(q.name, settings);
                        AnsiConsole.Markup("[green]OK[/]");
                        AnsiConsole.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.Markup("[red]Fail[/]");
                        AnsiConsole.WriteLine();
                        AnsiConsole.WriteLine();
                        AnsiConsole.WriteException(ex);
                        break;
                    }
                }
            }

            BaseUtil.WriteFooter();
        }

        private static string GetIsAliveMarkup(string queueName, AppSettings settings)
        {
            var res = CheckConnection(queueName, settings);
            if (res)
            {
                return "[green]true[/]";
            }
            else
            {
                return "[red]false[/]";
            }
        }

        private static async Task<IEnumerable<RabbitQueueDTO>> GetRabbitData(AppSettings settings)
        {
            RabbitQueueDTO[] result = null;
            var queueConfig = settings.Queue.RabbitMQ;
            HttpClientHandler handler = new();
            handler.Credentials = new NetworkCredential(queueConfig.Username, queueConfig.Password);
            using var httpClient = new HttpClient(handler);
            using var response = await httpClient.GetAsync($"http://{queueConfig.Hosts[0]}:{queueConfig.ApiPort}/api/queues");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var apiResponse = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<RabbitQueueDTO[]>(apiResponse);
            }
            else
            {
                Console.WriteLine($"Fail to connect to RabbitMQ management API. StatusCode {response.StatusCode}");
            }

            return result;
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

        private static void TestConnection(string queueName, AppSettings settings)
        {
            var config = settings.Queue.RabbitMQ;
            var factory = new ConnectionFactory
            {
                AutomaticRecoveryEnabled = true,
                Password = config.Password,
                Port = config.Port,
                UserName = config.Username,
            };

            using var connection = factory.CreateConnection(config.Hosts);
            using var channel = connection.CreateModel();
            _ = channel.BasicGet(queueName, false);
        }

        private static bool CheckConnection(string queueName, AppSettings settings)
        {
            try
            {
                TestConnection(queueName, settings);
                return true;
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
                return false;
            }
        }
    }
}