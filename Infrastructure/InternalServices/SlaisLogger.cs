using Application.Utils.Logger;

using Microsoft.Extensions.Logging;

namespace Infrastructure.InternalServices;

public class SlaisLogger<T> : ISlaisLogger<T>
    where T : class
{
    private const string MessageTemplate = "{Message}";

    private readonly ILogger<T> _logger;

    public SlaisLogger(ILogger<T> logger)
    {
        _logger = logger;
    }

    public void LogInformation(string message)
    {
        _logger.LogInformation(MessageTemplate, message);
    }

    public void LogWarning(string message)
    {
        _logger.LogWarning(MessageTemplate, message);
    }

    public void LogDebug(string message, Exception? exception)
    {
        _logger.LogDebug(exception, MessageTemplate, message);
    }

    public void LogCritical(string message, Exception? exception)
    {
        _logger.LogCritical(exception, MessageTemplate, message);
    }

    public void LogError(string message, Exception? exception)
    {
        _logger.LogError(exception, MessageTemplate, message);
    }



}
