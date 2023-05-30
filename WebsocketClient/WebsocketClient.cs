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

            // Create a task completion source to track the completion of ListenForMessages
            var listenTaskCompletionSource = new TaskCompletionSource<object>();

            cancellationToken = new CancellationTokenSource();
            var cancellationTokenToken = cancellationToken.Token;

            // Start the message receiving loop
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
                    listenTaskCompletionSource.SetResult(null); // Signal completion of ListenForMessages
                }
            }, cancellationTokenToken).ConfigureAwait(false);
        }

        private async Task ListenForMessages()
        {
            var buffer = new byte[1024];

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
                    Console.WriteLine("Received message: " + message);
                }
            }
        }

        public async Task SubscribeToEventAsync(string eventName)
        {
            var subscriptionMessage = $"SUBSCRIBE {eventName}";
            await SendMessageAsync(subscriptionMessage);
            Console.WriteLine($"Subscribed to event '{eventName}'.");
        }

        public async Task UnsubscribeFromEventAsync(string eventName)
        {
            var unsubscriptionMessage = $"UNSUBSCRIBE {eventName}";
            await SendMessageAsync(unsubscriptionMessage);
            Console.WriteLine($"Unsubscribed from event '{eventName}'.");
        }

        private async Task SendMessageAsync(string message)
        {
            if (webSocket == null)
            {
                throw new InvalidOperationException("WebSocket is not connected.");
            }

            var buffer = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task SendMessageOnEventAsync(string eventName, string message)
        {
            var payload = $"{eventName} {message}";
            await SendMessageAsync(payload);
            Console.WriteLine($"Sent message on event '{eventName}': {message}");
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
