using System;

namespace Conkist.GDK.Loading
{
    public interface ITrackableProgress<out T> : IDisposable
    {
        void TrackProgress(IProgress<T> progress);
    }
}
