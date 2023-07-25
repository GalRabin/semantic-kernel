// Copyright (c) Microsoft. All rights reserved.

using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

namespace ConsoleAppWithInstrumentation;

public sealed class LoggingMeterListener : IDisposable
{
    private readonly ILogger _logger;
    private readonly MeterListener _meterListener;
    private readonly string _scenario = string.Empty;
    private readonly string _tenantID = string.Empty;

    public LoggingMeterListener(ILogger logger, string scenario, string tenantId)
    {
        this._logger = logger;
        this._meterListener = new MeterListener();
        this._scenario = scenario;
        this._tenantID = tenantId;
    }

    public void Start()
    {
        // Subscribe to an instrument
        this._meterListener.InstrumentPublished = (instrument, meterListener) =>
        {
            meterListener.EnableMeasurementEvents(instrument);
        };

        // Callback for double measurements
        this._meterListener.SetMeasurementEventCallback<double>(this.OnMeasurementRecorded);

        // Callback for long measurements
        this._meterListener.SetMeasurementEventCallback<long>(this.OnMeasurementRecorded);

        // Callback for int measurements
        this._meterListener.SetMeasurementEventCallback<int>(this.OnMeasurementRecorded);

        // Start the MeterListener
        this._meterListener.Start();
    }

    private void OnMeasurementRecorded<T>(
        Instrument instrument,
        T measurement,
        ReadOnlySpan<KeyValuePair<string, object?>> tags,
        object? state)
    {
        var updatedTags = new List<KeyValuePair<string, object?>>(tags.ToArray());
        updatedTags.Add(new KeyValuePair<string, object?>("TenantId", this._tenantID));
        updatedTags.Add(new KeyValuePair<string, object?>("Scenario", this._scenario));

        var tagsString = string.Join(", ", updatedTags.Select(tag => $"{tag.Key}={tag.Value}"));
        var formattedTags = $"[{tagsString}]";

        this._logger?.LogInformation("[METRIC]{0} {1} {2}", formattedTags, instrument.Name, measurement?.ToString());
    }

    public void Dispose()
    {
        this._meterListener?.Dispose();
    }
}
