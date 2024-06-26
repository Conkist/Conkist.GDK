namespace Conkist.GDK.Loading
{
    public interface IAssetsDownloadStatus<TDownloadStatus>
    {
        TDownloadStatus DownloadStatus { get; }
    }
}
