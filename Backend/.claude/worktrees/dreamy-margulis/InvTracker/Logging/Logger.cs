using Microsoft.Extensions.Logging;

namespace InvTracker.Logging;

public static partial class Log
{
    [LoggerMessage(LogLevel.Information, Message = "{Page}: cache available, and used")]
    public static partial void CacheUsed(this ILogger logger, string Page);

    [LoggerMessage(LogLevel.Information, Message = "{Page}: no cache available, now set")]
    public static partial void CacheSet(this ILogger logger, string Page);
}
