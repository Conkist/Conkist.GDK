using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Conkist.GDK.Loading
{
    public abstract class LoadEventListener : MonoBehaviour
    {
        public abstract void LoadProgress(float progress);
        public virtual void DownloadProgress(AssetsDownloadStatus status)
        {
            LoadProgress(status.PercentProgress);
        }

        public virtual UniTask GetLoadingTask(string name, LoadStatus status)
        {
            switch(status)
            {
                case LoadStatus.LoadStarted:
                    {
                        return OnLoadStarted(name);
                    }
                case LoadStatus.InterpolatedLoadProgressComplete:
                    {
                        return OnLateLoadCompleted(name);
                    }
            }
            return UniTask.Yield().ToUniTask();
        }

        protected abstract UniTask OnLoadStarted(string address);

        protected abstract UniTask OnLateLoadCompleted(string address);

    }
}
