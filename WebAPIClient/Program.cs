using System;

namespace WebAPIClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var client = new System.Net.Http.HttpClient();
            Console.WriteLine("Hello World!");
            var proxy = new MyNameSpace.MyClassName("https://localhost:5001", client);
            var result = proxy.IsAliveAsync().Result;
        }
    }
}