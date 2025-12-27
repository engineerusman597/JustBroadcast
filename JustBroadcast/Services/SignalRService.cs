using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using JustBroadcast.Models;
using System.Text.Json;

namespace JustBroadcast.Services
{
    public class SignalRService(IConfiguration configuration) : ISignalRService
    {
        private HubConnection? _connection;
        private string _clientId = Guid.NewGuid().ToString();

        public event Action<CommandDto>? CommandReceived;
        public event Action<string>? ConnectionStateChanged;

        public bool IsConnected => _connection?.State == HubConnectionState.Connected;

        public async Task StartAsync(string accessToken)
        {
            if (_connection != null)
            {
                await StopAsync();
            }

            var hubUrl = configuration["ApiSettings:SignalRHub"];
            if (string.IsNullOrEmpty(hubUrl))
            {
                Console.WriteLine("[SignalRService] SignalR hub URL not configured");
                return;
            }

            Console.WriteLine($"[SignalRService] Configuring SignalR connection with JWT");
            Console.WriteLine($"[SignalRService] Hub URL: {hubUrl}");
            Console.WriteLine($"[SignalRService] Access Token Length: {accessToken?.Length ?? 0}");
            Console.WriteLine($"[SignalRService] Access Token Preview: {(accessToken?.Length > 20 ? accessToken.Substring(0, 20) + "..." : accessToken)}");

            // Add token as query parameter for SignalR authentication
            var hubUrlWithToken = $"{hubUrl}?access_token={accessToken}";
            Console.WriteLine($"[SignalRService] Hub URL with token: {hubUrl}?access_token=***");

            _connection = new HubConnectionBuilder()
                .WithUrl(hubUrlWithToken, options =>
                {
                    // Set the access token for authentication
                    options.AccessTokenProvider = () => Task.FromResult(accessToken)!;

                    Console.WriteLine($"[SignalRService] JWT configured in AccessTokenProvider and query string");
                })
                .WithAutomaticReconnect()
                .Build();

            // Handle connection state changes
            _connection.Closed += async (error) =>
            {
                ConnectionStateChanged?.Invoke("Closed");
                Console.WriteLine($"SignalR connection closed: {error?.Message}");
            };

            _connection.Reconnecting += (error) =>
            {
                ConnectionStateChanged?.Invoke("Reconnecting");
                Console.WriteLine($"SignalR reconnecting: {error?.Message}");
                return Task.CompletedTask;
            };

            _connection.Reconnected += async (connectionId) =>
            {
                ConnectionStateChanged?.Invoke("Connected");
                Console.WriteLine($"SignalR reconnected: {connectionId}");
                await RegisterClient();
            };

            // Listen for commands
            _connection.On<object>("Command", (data) =>
            {
                try
                {
                    Console.WriteLine($"[SignalRService] ReceiveCommand triggered");
                    Console.WriteLine($"[SignalRService] Raw data type: {data?.GetType().Name ?? "null"}");

                    var json = JsonSerializer.Serialize(data);
                    Console.WriteLine($"[SignalRService] Serialized command JSON: {json}");

                    var command = JsonSerializer.Deserialize<CommandDto>(json);
                    if (command != null)
                    {
                        Console.WriteLine($"[SignalRService] Successfully deserialized CommandDto");
                        Console.WriteLine($"[SignalRService] Command type: {command.command}");
                        Console.WriteLine($"[SignalRService] ClientId: {command.clientId}");
                        Console.WriteLine($"[SignalRService] Invoking CommandReceived event");
                        CommandReceived?.Invoke(command);
                    }
                    else
                    {
                        Console.WriteLine($"[SignalRService] ERROR: Deserialized command is null");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SignalRService] ERROR processing command");
                    Console.WriteLine($"[SignalRService] Exception: {ex.Message}");
                    Console.WriteLine($"[SignalRService] Stack trace: {ex.StackTrace}");
                }
            });

            try
            {
                Console.WriteLine($"[SignalRService] Attempting to start connection to: {hubUrl}");
                await _connection.StartAsync();
                ConnectionStateChanged?.Invoke("Connected");
                Console.WriteLine($"[SignalRService] ✓ SignalR connected successfully");
                Console.WriteLine($"[SignalRService] Connection State: {_connection.State}");

                await RegisterClient();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalRService] ✗ Error connecting to SignalR: {ex.Message}");
                Console.WriteLine($"[SignalRService] Exception type: {ex.GetType().Name}");
                Console.WriteLine($"[SignalRService] Stack trace: {ex.StackTrace}");
                ConnectionStateChanged?.Invoke("Closed");
            }
        }

        private async Task RegisterClient()
        {
            if (_connection?.State == HubConnectionState.Connected)
            {
                try
                {
                    Console.WriteLine($"[SignalRService] Registering client: {_clientId} as WebDashboard");
                    await _connection.InvokeAsync("RegisterClient", _clientId, "WebDashboard");
                    Console.WriteLine($"[SignalRService] ✓ Client registered successfully: {_clientId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SignalRService] ✗ Error registering client: {ex.Message}");
                    Console.WriteLine($"[SignalRService] Stack trace: {ex.StackTrace}");
                }
            }
            else
            {
                Console.WriteLine($"[SignalRService] Cannot register client - connection state: {_connection?.State}");
            }
        }

        public async Task SendRequestStatusSync(string clientId)
        {
            if (_connection?.State != HubConnectionState.Connected)
            {
                Console.WriteLine("[SignalRService] SignalR not connected, cannot send status sync request");
                return;
            }

            try
            {
                await _connection.InvokeAsync("SendToAll", new CommandDto
                {
                    command = ServiceMessages.RequestStatusSync.ToString(),
                    clientId = clientId
                });
                Console.WriteLine("[SignalRService] Status sync request sent to all clients");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalRService] Error sending status sync: {ex.Message}");
            }
        }

        public async Task SendCommand(CommandDto command)
        {
            if (_connection?.State != HubConnectionState.Connected)
            {
                Console.WriteLine("[SignalRService] SignalR not connected, cannot send command");
                return;
            }

            try
            {
                Console.WriteLine($"[SignalRService] Sending command via SendToAll: {command.command}");
                await _connection.InvokeAsync("SendToAll", command);
                Console.WriteLine($"[SignalRService] ✓ Command sent successfully: {command.command}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalRService] ✗ Error sending command: {ex.Message}");
                Console.WriteLine($"[SignalRService] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<IReadOnlyCollection<ClientSession>> GetClientsSnapshot()
        {
            if (_connection?.State != HubConnectionState.Connected)
            {
                Console.WriteLine("[SignalRService] SignalR not connected, cannot get clients snapshot");
                return new List<ClientSession>();
            }

            try
            {
                Console.WriteLine("[SignalRService] Getting clients snapshot...");
                var clients = await _connection.InvokeAsync<IReadOnlyCollection<ClientSession>>("GetClientsSnapshot");
                Console.WriteLine($"[SignalRService] ✓ Got {clients?.Count ?? 0} clients from snapshot");
                return clients ?? new List<ClientSession>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalRService] ✗ Error getting clients snapshot: {ex.Message}");
                Console.WriteLine($"[SignalRService] Stack trace: {ex.StackTrace}");
                return new List<ClientSession>();
            }
        }

        public async Task StopAsync()
        {
            if (_connection != null)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
    }
}
