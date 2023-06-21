﻿using NAPS2.ImportExport;

namespace NAPS2.Scan.Batch;

public class BatchSettings
{
    public string? ProfileDisplayName { get; set; }

    public string? BatchScanNumber { get; set; }

    public BatchScanType ScanType { get; set; }

    public int ScanCount { get; set; }

    public double ScanIntervalSeconds { get; set; }

    public BatchOutputType OutputType { get; set;  }

    public SaveSeparator SaveSeparator { get; set; }

    public string? SavePath { get; set; }
}