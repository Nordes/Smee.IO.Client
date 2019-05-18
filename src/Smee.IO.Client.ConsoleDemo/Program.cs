using System;
using System.Threading;
using Newtonsoft.Json;

namespace Smee.IO.Client.ConsoleDemo
{
    class Program
    {
        private static CancellationTokenSource source;

        static void Main(string[] args)
        {
            source = new CancellationTokenSource();
            var token = source.Token;

            // This could also be done automatically, but since we expect some developer to test, let's
            // keep it this way.
            Console.WriteLine("Hi there, please get a valid url on https://smee.io/new");
            Console.Write("Please enter a valid URI because no validation will be made: ");
            var smeeUri = new Uri(Console.ReadLine());
            Console.WriteLine("When you will push a key, it will stop everything!");

            var smeeCli = new SmeeClient(smeeUri);
            smeeCli.OnConnect += (sender, a) => Console.WriteLine($"Connected to Smee.io ({smeeUri})");
            smeeCli.OnDisconnect += (sender, a) => Console.WriteLine($"Disconnected from Smee.io ({smeeUri})");
            smeeCli.OnMessage += (sender, smeeEvent) =>
            {
                Console.Write("Message received: ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write(JsonConvert.SerializeObject(smeeEvent)); // This is a typed object.
                Console.ResetColor();
                Console.WriteLine();
            };
            smeeCli.OnPing += (sender, a) => Console.WriteLine("Ping from Smee");
            smeeCli.OnError += (sender, e) => Console.WriteLine("Error was raised (Disconnect/Anything else: " + e.Message);
            smeeCli.Start(token);

            Console.ReadKey(false);
            smeeCli.Stop();
            source.Cancel();
            Console.WriteLine("Finish");
        }
    }
}
