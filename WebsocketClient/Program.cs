namespace WebsocketClient
{
    internal class Program
    {
        private static string[] helpString = { "'/sub eventname' to subscribe to an event", };
        
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
                        Console.WriteLine("Enter some text to send a message or type '/help' to get a list of commands.");
                        Console.Write("Input:");
                        var input = Console.ReadLine();
                        switch(input)
                        {
                            case "/help":
                                Array.ForEach(helpString, str => { Console.WriteLine(str); });
                                break;
                            case "q":
                                goto quit;
                            case "":
                                break;
                            default:
                                await client.SendMessageAsync(new SocketMessage(SocketMessage.CommandType.SendMessage, eventName, input));
                                break;

                        }
                    }
                    quit:;
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