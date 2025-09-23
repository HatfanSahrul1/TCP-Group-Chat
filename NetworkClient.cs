using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;

namespace tcp_group_chat
{
    public class ChatMessage
    {
        public string Type { get; set; }          // "msg", "join", "leave", "pm", "sys"
        public string From { get; set; }
        public string To { get; set; }
        public string Text { get; set; }
        public long Ts { get; set; }             // timestamp
    }

    public class NetworkClient
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private string _username;
        private bool _isConnected = false;
        private CancellationTokenSource _cancellationTokenSource;

        // Events untuk komunikasi dengan UI
        public event EventHandler<ChatMessage> MessageReceived;
        public event EventHandler<string> ConnectionStateChanged;
        public event EventHandler<string> ErrorOccurred;

        public bool IsConnected => _isConnected && _client?.Connected == true;

        public async Task<bool> ConnectAsync(string serverAddress, int port, string username)
        {
            try
            {
                _username = username;
                _client = new TcpClient();
                _cancellationTokenSource = new CancellationTokenSource();

                await _client.ConnectAsync(serverAddress, port);
                _stream = _client.GetStream();
                _isConnected = true;

                ConnectionStateChanged?.Invoke(this, "Connecting...");

                // Mulai listening untuk pesan dari server
                _ = Task.Run(() => ListenForMessages(_cancellationTokenSource.Token));

                // Kirim join message
                var joinMessage = new ChatMessage
                {
                    Type = "join",
                    From = username,
                    Text = $"{username} joined the chat",
                    Ts = GetTimestamp()
                };

                await SendMessageAsync(joinMessage);
                ConnectionStateChanged?.Invoke(this, "Connected");
                return true;
            }
            catch (Exception ex)
            {
                _isConnected = false;
                ErrorOccurred?.Invoke(this, $"Connection failed: {ex.Message}");
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (_isConnected)
                {
                    // Kirim leave message
                    var leaveMessage = new ChatMessage
                    {
                        Type = "leave",
                        From = _username,
                        Text = $"{_username} left the chat",
                        Ts = GetTimestamp()
                    };

                    await SendMessageAsync(leaveMessage);
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Error during disconnect: {ex.Message}");
            }
            finally
            {
                _isConnected = false;
                _cancellationTokenSource?.Cancel();
                _stream?.Close();
                _client?.Close();
                ConnectionStateChanged?.Invoke(this, "Disconnected");
            }
        }

        public async Task SendGroupMessageAsync(string message)
        {
            if (!_isConnected) return;

            var chatMessage = new ChatMessage
            {
                Type = "msg",
                From = _username,
                Text = message,
                Ts = GetTimestamp()
            };

            await SendMessageAsync(chatMessage);
        }

        public async Task SendPrivateMessageAsync(string targetUser, string message)
        {
            if (!_isConnected) return;

            var chatMessage = new ChatMessage
            {
                Type = "pm",
                From = _username,
                To = targetUser,
                Text = message,
                Ts = GetTimestamp()
            };

            await SendMessageAsync(chatMessage);
        }

        private async Task SendMessageAsync(ChatMessage message)
        {
            try
            {
                if (!_isConnected || _stream == null) return;

                string json = JsonSerializer.Serialize(message);
                byte[] messageBytes = Encoding.UTF8.GetBytes(json);
                byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);
                
                // Send length first, then message
                await _stream.WriteAsync(lengthBytes, 0, lengthBytes.Length);
                await _stream.WriteAsync(messageBytes, 0, messageBytes.Length);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Error sending message: {ex.Message}");
            }
        }

        private async Task<ChatMessage> ReceiveMessageAsync()
        {
            try
            {
                if (!_isConnected || _stream == null) return null;

                // Read message length first (4 bytes)
                byte[] lengthBytes = new byte[4];
                int bytesRead = 0;
                while (bytesRead < 4)
                {
                    int read = await _stream.ReadAsync(lengthBytes, bytesRead, 4 - bytesRead);
                    if (read == 0) return null; // Connection closed
                    bytesRead += read;
                }

                int messageLength = BitConverter.ToInt32(lengthBytes, 0);
                if (messageLength <= 0 || messageLength > 65536) // Sanity check
                {
                    ErrorOccurred?.Invoke(this, "Invalid message length received");
                    return null;
                }

                // Read the actual message
                byte[] messageBytes = new byte[messageLength];
                bytesRead = 0;
                while (bytesRead < messageLength)
                {
                    int read = await _stream.ReadAsync(messageBytes, bytesRead, messageLength - bytesRead);
                    if (read == 0) return null; // Connection closed
                    bytesRead += read;
                }

                string jsonData = Encoding.UTF8.GetString(messageBytes);
                return JsonSerializer.Deserialize<ChatMessage>(jsonData);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Error receiving message: {ex.Message}");
                return null;
            }
        }

        private async Task ListenForMessages(CancellationToken cancellationToken)
        {
            try
            {
                while (_isConnected && !cancellationToken.IsCancellationRequested)
                {
                    var message = await ReceiveMessageAsync();
                    if (message == null)
                    {
                        // Connection closed or error
                        break;
                    }

                    // Invoke pada UI thread
                    MessageReceived?.Invoke(this, message);
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                ErrorOccurred?.Invoke(this, $"Connection lost: {ex.Message}");
            }
            finally
            {
                if (_isConnected)
                {
                    _isConnected = false;
                    ConnectionStateChanged?.Invoke(this, "Disconnected");
                }
            }
        }

        private static long GetTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public void Dispose()
        {
            _ = Task.Run(async () => await DisconnectAsync());
        }
    }
}