using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using JustBroadcast.Models;
using System.Text.Json;

namespace JustBroadcast.Services
{
    public class SignalRService : ISignalRService
    {
        private HubConnection? _connection;
        private readonly IConfiguration _configuration;
        private string _clientId = Guid.NewGuid().ToString();

        public event Action<CommandDto>? CommandReceived;
        public event Action<string>? ConnectionStateChanged;

        public bool IsConnected => _connection?.State == HubConnectionState.Connected;

        public SignalRService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task StartAsync(string accessToken)
        {
            if (_connection != null)
            {
                await StopAsync();
            }

            var hubUrl = _configuration["ApiSettings:SignalRHub"];
            if (string.IsNullOrEmpty(hubUrl))
            {
                Console.WriteLine("SignalR hub URL not configured");
                return;
            }

            _connection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(accessToken)!;
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
            _connection.On<object>("ReceiveCommand", (data) =>
            {
                try
                {
                    var json = JsonSerializer.Serialize(data);
                    var command = JsonSerializer.Deserialize<CommandDto>(json);
                    if (command != null)
                    {
                        CommandReceived?.Invoke(command);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing command: {ex.Message}");
                }
            });

            try
            {
                await _connection.StartAsync();
                ConnectionStateChanged?.Invoke("Connected");
                Console.WriteLine("SignalR connected successfully");

                await RegisterClient();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to SignalR: {ex.Message}");
                ConnectionStateChanged?.Invoke("Closed");
            }
        }

        private async Task RegisterClient()
        {
            if (_connection?.State == HubConnectionState.Connected)
            {
                try
                {
                    await _connection.InvokeAsync("RegisterClient", _clientId, "WebDashboard");
                    Console.WriteLine($"Registered SignalR client: {_clientId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error registering client: {ex.Message}");
                }
            }
        }

        public async Task SendRequestStatusSync(string clientId)
        {
            if (_connection?.State != HubConnectionState.Connected)
            {
                Console.WriteLine("SignalR not connected, cannot send status sync request");
                return;
            }

            try
            {
                await _connection.InvokeAsync("SendToAllExcept", clientId, new CommandDto
                {
                    command = ServiceMessages.RequestStatusSync.ToString(),
                    clientId = clientId
                });
                Console.WriteLine("Status sync request sent");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending status sync: {ex.Message}");
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
