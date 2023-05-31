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

        public async Task ConnectAsync(Uri uri)
        {
            webSocket = new ClientWebSocket();
            await webSocket.ConnectAsync(uri, CancellationToken.None);
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
                    Console.WriteLine("Error in ListenForMessages: " + ex.Message);
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

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    var socketMessage = SocketMessage.Deserialize(message);

                    if(socketMessage.Command == SocketMessage.CommandType.SendMessage)
                        Console.WriteLine("Received message: " + socketMessage.Message);
                }
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
  
        public void Disconnect()
        {
            cancellationToken?.Cancel();
            webSocket?.Dispose();
            webSocket = null;
            Console.WriteLine("Disconnected from the WebSocket server.");
        }
    }

}
