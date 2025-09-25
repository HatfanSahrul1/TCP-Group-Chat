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

    public class NetworkClient : IDisposable
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private string _username;
        private bool _isConnected = false;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private bool _disposed = false;

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

                // Set timeout untuk connection
                _client.ReceiveTimeout = 30000; // 30 seconds
                _client.SendTimeout = 10000;    // 10 seconds

                ConnectionStateChanged?.Invoke(this, "Connecting...");

                // Connect with timeout
                var connectTask = _client.ConnectAsync(serverAddress, port);
                var timeoutTask = Task.Delay(10000); // 10 second timeout
                
                var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException("Connection timeout");
                }

                _stream = _client.GetStream();
                _isConnected = true;

                // Start listening in background thread (don't await)
                _ = Task.Run(async () => await ListenForMessages(_cancellationTokenSource.Token))
                    .ConfigureAwait(false);

                // Send join message
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
            if (_disposed) return;
            
            try
            {
                if (_isConnected && _client?.Connected == true)
                {
                    // Send leave message with timeout
                    var leaveMessage = new ChatMessage
                    {
                        Type = "leave",
                        From = _username,
                        Text = $"{_username} left the chat",
                        Ts = GetTimestamp()
                    };

                    var cts = new CancellationTokenSource(5000); // 5 second timeout
                    try
                    {
                        await SendMessageAsync(leaveMessage, cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        // Timeout - continue with disconnect
                    }
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
                
                try
                {
                    _stream?.Close();
                    _client?.Close();
                }
                catch { }
                
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

        private async Task SendMessageAsync(ChatMessage message, CancellationToken cancellationToken = default)
        {
            if (!_isConnected || _stream == null || _disposed) return;

            await _sendLock.WaitAsync(cancellationToken);
            try
            {
                string json = JsonSerializer.Serialize(message);
                byte[] messageBytes = Encoding.UTF8.GetBytes(json);
                byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);
                
                // Send length first, then message with timeout
                await _stream.WriteAsync(lengthBytes, 0, lengthBytes.Length, cancellationToken);
                await _stream.WriteAsync(messageBytes, 0, messageBytes.Length, cancellationToken);
                await _stream.FlushAsync(cancellationToken);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                ErrorOccurred?.Invoke(this, $"Error sending message: {ex.Message}");
            }
            finally
            {
                _sendLock.Release();
            }
        }

        private async Task<ChatMessage> ReceiveMessageAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_isConnected || _stream == null || _disposed) return null;

                // Read message length first (4 bytes) with timeout
                byte[] lengthBytes = new byte[4];
                int bytesRead = 0;
                while (bytesRead < 4 && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        int read = await _stream.ReadAsync(lengthBytes, bytesRead, 4 - bytesRead, cancellationToken);
                        if (read == 0) return null; // Connection closed
                        bytesRead += read;
                    }
                    catch (Exception ex) when (ex is System.IO.IOException || ex is SocketException)
                    {
                        // Connection lost
                        _isConnected = false;
                        return null;
                    }
                }

                int messageLength = BitConverter.ToInt32(lengthBytes, 0);
                if (messageLength <= 0 || messageLength > 65536) // Sanity check
                {
                    ErrorOccurred?.Invoke(this, "Invalid message length received");
                    return null;
                }

                // Read the actual message with timeout
                byte[] messageBytes = new byte[messageLength];
                bytesRead = 0;
                while (bytesRead < messageLength && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        int read = await _stream.ReadAsync(messageBytes, bytesRead, messageLength - bytesRead, cancellationToken);
                        if (read == 0) return null; // Connection closed
                        bytesRead += read;
                    }
                    catch (Exception ex) when (ex is System.IO.IOException || ex is SocketException)
                    {
                        // Connection lost
                        _isConnected = false;
                        return null;
                    }
                }

                string jsonData = Encoding.UTF8.GetString(messageBytes);
                return JsonSerializer.Deserialize<ChatMessage>(jsonData);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                ErrorOccurred?.Invoke(this, $"Error receiving message: {ex.Message}");
                return null;
            }
        }

        private async Task ListenForMessages(CancellationToken cancellationToken)
        {
            try
            {
                while (_isConnected && !cancellationToken.IsCancellationRequested && !_disposed)
                {
                    var message = await ReceiveMessageAsync(cancellationToken);
                    if (message == null)
                    {
                        // Connection closed or error - exit gracefully
                        _isConnected = false;
                        break;
                    }

                    // Post to UI thread without blocking
                    _ = Task.Run(() => MessageReceived?.Invoke(this, message));
                    
                    // Small delay to prevent CPU spinning
                    await Task.Delay(10, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancelling - don't report as error
            }
            catch (Exception ex) when (ex is System.IO.IOException || ex is SocketException)
            {
                // Network connection lost - handle gracefully
                _isConnected = false;
            }
            catch (Exception ex)
            {
                // Unexpected error
                _isConnected = false;
                ErrorOccurred?.Invoke(this, $"Unexpected error: {ex.Message}");
            }
            finally
            {
                if (!_disposed)
                {
                    _isConnected = false;
                    _ = Task.Run(() => ConnectionStateChanged?.Invoke(this, "Disconnected"));
                }
            }
        }

        private static long GetTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            
            _ = Task.Run(async () => 
            {
                try
                {
                    await DisconnectAsync();
                }
                finally
                {
                    _sendLock?.Dispose();
                    _cancellationTokenSource?.Dispose();
                }
            });
        }
    }
}