using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace WebsocketClient
{

    public class WebSocketClient
    {
        private ClientWebSocket? webSocket;
        private CancellationTokenSource? cancellationToken;


        private Uri URI { get; set; }

        

        public WebSocketClient(Uri uri)
        {
            URI = uri;
        }

        public async Task ConnectAsync()
        {
            while (true)
            {
                webSocket = new ClientWebSocket();
                try
                {
                    await webSocket.ConnectAsync(URI, CancellationToken.None);
                }
                catch
                {
                }

                if (webSocket.State == WebSocketState.Open)
                    break;

                Console.WriteLine("Connection failed. Retrying in 5 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(5));
            }

            var listenTaskCompletionSource = new TaskCompletionSource<object>();

            cancellationToken = new CancellationTokenSource();
            var cancellationTokenToken = cancellationToken.Token;

            _ = Task.Run(async () =>
            {
                try
                {
                    await ListenForMessages();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    Console.WriteLine("Connection closed. Reconnecting...");
                    await ReconnectAsync();
                }
                finally
                {
                    listenTaskCompletionSource.SetResult(null);
                }
            }, cancellationTokenToken).ConfigureAwait(false);
        }

        private async Task ListenForMessages()
        {
            var buffer = new byte[1024];

            if (webSocket == null)
                return;

            while (true)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine("Connection closed. Reconnecting...");
                    await ReconnectAsync();
                    break;
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    var socketMessage = SocketMessage.Deserialize(message);

                    if (socketMessage.Command == SocketMessage.CommandType.SendMessage)
                        Console.WriteLine("Received message: " + socketMessage.Message);
                }          
            }
        }



        private async Task ReconnectAsync()
        {
            webSocket?.Dispose();
            await Task.Delay(TimeSpan.FromSeconds(5));

            while (true)
            {
                if (cancellationToken?.Token.IsCancellationRequested == true)
                    return;

                await ConnectAsync();

                if (webSocket?.State == WebSocketState.Open)
                {
                    Console.WriteLine("Reconnected to the WebSocket server!");
                    return;
                }
                Console.WriteLine("Reconnection failed. Retrying in 5 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        public async Task SendMessageAsync(SocketMessage message)
        {
            if (webSocket == null)
            {
                throw new InvalidOperationException("WebSocket is not connected.");
            }

            switch(message.Command)
            {
                case SocketMessage.CommandType.Subscribe:
                    Console.WriteLine($"Subscribed to event '{message.EventName}'.");
                    break;
                case SocketMessage.CommandType.Unsubscribe:
                    Console.WriteLine($"Unsubscribed to event '{message.EventName}'.");
                    break;
                case SocketMessage.CommandType.SendMessage:
                    Console.WriteLine($"Sending message to event '{message.EventName}': '{message.Message}'.");
                    break;
            }
                          
            var buffer = Encoding.UTF8.GetBytes(message.Serialize());
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
  

        public bool IsConnected()
        {
            if (webSocket == null)
                return false;
            if (webSocket.State == WebSocketState.Open)
                return true;
            else
                return false;
        }
        public void Disconnect()
        {
            cancellationToken?.Cancel();
            webSocket?.Dispose();
            webSocket = null;
            Console.WriteLine("Disconnected from the WebSocket server.");
        }
    }

}
