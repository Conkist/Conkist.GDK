using System;

namespace Conkist.GDK.Loading
{
    public struct AssetsDownloadStatus
    {
        public long DownloadSizeBytes;
        public long DownloadedBytes;
        public float PercentProgress;
        public bool IsDownloaded =>
            DownloadSizeBytes == 0 || DownloadOperationStatus == DownloadOperationStatus.Succeeded;

        public DownloadOperationStatus DownloadOperationStatus;

        public Exception FailureException;
    }
}
