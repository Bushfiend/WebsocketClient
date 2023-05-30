namespace WebsocketClient
{
    internal class Program
    {
        const string eventName = "Event1";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Bush Websocket CLient");
            var client = new WebSocketClient();
            var uri = new Uri("ws://205.234.34.56:9900/");

            try
            {
                await client.ConnectAsync(uri);
                Console.WriteLine("Connected to the WebSocket server!");

                await client.SubscribeToEventAsync(eventName);

                while (true)
                {
                    Console.WriteLine("Press Enter to send a message (or 'q' to quit)... no tolower() here lol");
                    var input = Console.ReadLine();
                    if (input == "q")
                        break;
                    if (string.IsNullOrEmpty(input))
                        continue;

                    await client.SendMessageOnEventAsync(eventName, input);
                }

                await client.UnsubscribeFromEventAsync(eventName);
                client.Disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }
    }
}