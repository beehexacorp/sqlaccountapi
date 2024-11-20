using Microsoft.AspNetCore.SignalR;

public class NotificationHub : Hub
{
    public const string RECEIVE_LOG_EVENT = "ReceiveLog";
    // public async Task SendLog(string logLevel, string message)
    // {
    //     await Clients.All.SendAsync("ReceiveLog", logLevel, message, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    // }
}