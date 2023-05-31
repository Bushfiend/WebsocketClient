namespace WebsocketClient
{
    internal class Program
    {

        static async Task Main(string[] args)
        {
            Console.WriteLine("Bush Websocket Client");
            var client = new WebSocketClient();
            var uri = new Uri("ws://localhost:9900/");
            string eventName = "Kekw";

            try
            {
                await client.ConnectAsync(uri);
                Console.WriteLine("Connected to the WebSocket server!");

                await client.SendMessageAsync(new SocketMessage(SocketMessage.CommandType.Subscribe, eventName));

                while (true)
                {
                    Console.WriteLine("Press Enter to send a message.");
                    var input = Console.ReadLine();
                    if (input == "q")
                        break;
                    if (string.IsNullOrEmpty(input))
                        continue;

                    await client.SendMessageAsync(new SocketMessage(SocketMessage.CommandType.SendMessage, eventName, input));
                }

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