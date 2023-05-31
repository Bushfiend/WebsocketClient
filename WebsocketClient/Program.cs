namespace WebsocketClient
{
    internal class Program
    {

        static async Task Main(string[] args)
        {
            Console.WriteLine("Bush Websocket Client");

            var uri = new Uri("ws://localhost:9900/");
            var client = new WebSocketClient(uri);

            string eventName = "Kekw";

            try
            {
                await client.ConnectAsync();
                Console.WriteLine("Connected to the WebSocket server!");

                await client.SendMessageAsync(new SocketMessage(SocketMessage.CommandType.Subscribe, eventName));

                var sendTask = Task.Run(async () =>
                {
                    while (true)
                    {
                        Console.WriteLine("Press Enter to send a message (or 'q' to quit)...");
                        var input = Console.ReadLine();
                        if (input == "q")
                            break;
                        if (string.IsNullOrEmpty(input))
                            continue;

                        await client.SendMessageAsync(new SocketMessage(SocketMessage.CommandType.SendMessage, eventName, input));
                    }
                });

                await sendTask;

                await client.SendMessageAsync(new SocketMessage(SocketMessage.CommandType.Unsubscribe, eventName));
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