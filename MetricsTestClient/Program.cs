using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;

Console.WriteLine("SignalR Metrics Test Client");
Console.WriteLine("============================");

// Configuration
var hubUrl = "http://178.222.112.105:5016/hubs/broadcast"; // Update with your SignalR hub URL
var accessToken = "YOUR_ACCESS_TOKEN_HERE"; // Get this from your browser's localStorage after logging in

// Create SignalR connection
var connection = new HubConnectionBuilder()
    .WithUrl(hubUrl, options =>
    {
        options.AccessTokenProvider = () => Task.FromResult(accessToken)!;
    })
    .WithAutomaticReconnect()
    .Build();

// Connect
try
{
    await connection.StartAsync();
    Console.WriteLine("✓ Connected to SignalR hub");

    // Register as a playout server client
    var clientId = Guid.NewGuid().ToString();
    await connection.InvokeAsync("RegisterClient", clientId, "PlayoutServer");
    Console.WriteLine($"✓ Registered as PlayoutServer with ID: {clientId}");

    Console.WriteLine("\nSending metrics every second. Press Ctrl+C to stop.\n");

    var random = new Random();
    var iteration = 0;

    while (true)
    {
        iteration++;

        // Generate random metrics
        var cpuPercent = random.Next(40, 80);
        var gpuPercent = random.Next(30, 70);
        var ramPercent = random.Next(50, 85);

        // Create metrics command
        var metricsCommand = new
        {
            command = "Metrics",
            clientId = clientId,
            group = "PlayoutServer",
            channelId = "",
            playoutId = "",
            userid = "",
            data = new
            {
                cpuPercent = cpuPercent,
                gpuPercent = gpuPercent,
                ramPercent = ramPercent
            }
        };

        // Send to all clients
        await connection.InvokeAsync("SendToAll", metricsCommand);

        Console.WriteLine($"[{iteration}] Sent: CPU={cpuPercent}%, GPU={gpuPercent}%, RAM={ramPercent}%");

        // Wait 1 second
        await Task.Delay(1000);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Error: {ex.Message}");
    Console.WriteLine($"Stack: {ex.StackTrace}");
}
finally
{
    await connection.StopAsync();
    await connection.DisposeAsync();
}
