using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using Cysharp.Threading.Tasks;

namespace Conkist.GDK.Loading
{
    [RequireComponent(typeof(LoadingManager))]
    public class SceneManager : MonoBehaviour
    {
        protected AsyncOperation _asyncOperation;
        protected static string _sceneToLoad = "";

        [SerializeField] LoadEventListener quickLoadListener;
        [SerializeField] LoadEventListener sceneLoadListener;

        public async UniTask<SceneInstance> LoadAsync(string sceneToLoad, LoadType loadType = LoadType.FullScreen)
        {
            await LoadingEventTask(sceneToLoad, LoadStatus.LoadStarted, loadType);
            //Application.backgroundLoadingPriority = ThreadPriority.High;
            var loader = Addressables.LoadSceneAsync(sceneToLoad);

            var scene = await loader;
            await LoadingEventTask(sceneToLoad, LoadStatus.InterpolatedLoadProgressComplete, loadType);

            return scene;
        }

        public async UniTask<SceneInstance> LoadAsync(AssetReferenceScene sceneToLoad, LoadType loadType = LoadType.FullScreen)
        {
            await LoadingEventTask(sceneToLoad.AssetGUID, LoadStatus.LoadStarted, loadType);
            //Application.backgroundLoadingPriority = ThreadPriority.High;
            var loader = Addressables.LoadSceneAsync(sceneToLoad);

            var scene = await loader;
            await LoadingEventTask(sceneToLoad.AssetGUID, LoadStatus.InterpolatedLoadProgressComplete, loadType);

            return scene;
        }

        public async UniTask<SceneInstance> AddAsync(string sceneToAdd, LoadType loadType = LoadType.Hidden)
        {
            await LoadingEventTask(sceneToAdd, LoadStatus.LoadStarted, loadType);
            //Application.backgroundLoadingPriority = ThreadPriority.High;
            var loader = Addressables.LoadSceneAsync(sceneToAdd, loadMode: UnityEngine.SceneManagement.LoadSceneMode.Additive);
            var scene = await loader;
            await LoadingEventTask(sceneToAdd, LoadStatus.InterpolatedLoadProgressComplete, loadType);

            return scene;
        }

        public async UniTask<SceneInstance> AddAsync(AssetReferenceScene sceneToAdd, LoadType loadType = LoadType.Hidden)
        {
            await LoadingEventTask(sceneToAdd.AssetGUID, LoadStatus.LoadStarted, loadType);
            //Application.backgroundLoadingPriority = ThreadPriority.High;
            var loader = Addressables.LoadSceneAsync(sceneToAdd, loadMode: UnityEngine.SceneManagement.LoadSceneMode.Additive);
            var scene = await loader;
            await LoadingEventTask(sceneToAdd.AssetGUID, LoadStatus.InterpolatedLoadProgressComplete, loadType);

            return scene;
        }

        public async UniTask Remove(SceneInstance scene, bool autoRelease = true, LoadType loadType = LoadType.Hidden)
        {
            await Addressables.UnloadSceneAsync(scene, autoReleaseHandle: autoRelease);
        }

        public async UniTask Reload(LoadType loadType = LoadType.FullScreen)
        {
            await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        private UniTask LoadingEventTask(string address, LoadStatus status, LoadType type)
        {
            LoadingEvent.Trigger(address, status, type);
            switch (type)
            {
                case LoadType.Quick:
                    {
                        if(quickLoadListener != null)
                        return quickLoadListener.GetLoadingTask(address, status);
                    }
                    break;
                case LoadType.FullScreen:
                    {
                        if(sceneLoadListener != null)
                        return sceneLoadListener.GetLoadingTask(address, status);
                    }
                    break;
            }
            return UniTask.Yield().ToUniTask();
        }
    }
}
