using Serilog.Events;
using Serilog.Core;
using Microsoft.AspNetCore.SignalR;

public class SignalRSink : ILogEventSink, IDisposable
{
    private readonly IFormatProvider _formatProvider;
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRSink(IHubContext<NotificationHub> hubContext, IFormatProvider formatProvider)
    {
        _hubContext = hubContext;
        _formatProvider = formatProvider;
    }

    public void Emit(LogEvent logEvent)
    {
        // Format the log message
        var message = logEvent.RenderMessage(_formatProvider);

        // Send the message to all connected SignalR clients
        Task.Run(() =>
            _hubContext.Clients.All.SendAsync(NotificationHub.RECEIVE_LOG_EVENT, logEvent.Level.ToString(), message, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
        );
    }

    public void Dispose()
    {
        // Cleanup resources if necessary
    }
}
