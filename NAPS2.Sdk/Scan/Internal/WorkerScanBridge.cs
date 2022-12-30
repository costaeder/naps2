﻿using System.Threading;

namespace NAPS2.Scan.Internal;

/// <summary>
/// Represents scanning in a worker process on the same machine.
/// </summary>
internal class WorkerScanBridge : IScanBridge
{
    private readonly ScanningContext _scanningContext;

    public WorkerScanBridge(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public async Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
    {
        if (_scanningContext.WorkerFactory == null)
        {
            throw new InvalidOperationException("ScanningContext.WorkerFactory must be set to scan with a worker");
        }
        using var ctx = _scanningContext.WorkerFactory.Create();
        return await ctx.Service.GetDeviceList(options);
    }

    public async Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents,
        Action<ProcessedImage, PostProcessingContext> callback)
    {
        if (_scanningContext.WorkerFactory == null)
        {
            throw new InvalidOperationException("ScanningContext.WorkerFactory must be set to scan with a worker");
        }
        using var ctx = _scanningContext.WorkerFactory.Create();
        await ctx.Service.Scan(_scanningContext, options, cancelToken, scanEvents,
            (image, tempPath) => { callback(image, new PostProcessingContext { TempPath = tempPath }); });
    }
}