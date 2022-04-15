using System;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CryptoExchange.Net.UnitTests.TestImplementations;

public class TestStringLogger : ILogger
{
    private readonly StringBuilder _builder = new();

    public IDisposable BeginScope<TState>(TState state)
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
        Func<TState, Exception, string> formatter)
    {
        _builder.AppendLine(formatter(state, exception));
    }

    public string GetLogs()
    {
        return _builder.ToString();
    }
}