// Copyright (c) Microsoft. All rights reserved.

namespace ConsoleAppWithInstrumentation;
using Microsoft.Extensions.Logging;
// using OpenTelemetry.Trace;
using System;

internal sealed class LoggerDecorator : ILogger
{
    private readonly ILogger _logger;
    private readonly string _corellationID;

    public LoggerDecorator(ILogger logger, string correlationID)
    {
        this._logger = logger;
        this._corellationID = correlationID;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        // You could add custom logic here...
        return this._logger.IsEnabled(logLevel);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        // You could add custom logic here...
        // For example, you might want to add additional information to the log message:

        // message = $"[CorellationID={this._corellationID}][TraceID={Tracer.CurrentSpan.Context.TraceId}][SpanID={Tracer.CurrentSpan.Context.SpanId}] {message}";
        var message = $"[CorellationID={this._corellationID}] {formatter(state, exception)}";

        this._logger.Log(logLevel, eventId, state, exception, (s, e) => message);
    }
}
