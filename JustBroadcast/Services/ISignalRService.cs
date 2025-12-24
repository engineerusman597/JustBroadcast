using JustBroadcast.Models;

namespace JustBroadcast.Services
{
    public interface ISignalRService
    {
        event Action<CommandDto>? CommandReceived;
        event Action<string>? ConnectionStateChanged;

        Task StartAsync(string accessToken);
        Task StopAsync();
        Task SendRequestStatusSync(string clientId);
        Task SendCommand(CommandDto command);
        bool IsConnected { get; }
    }
}
