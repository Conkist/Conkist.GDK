using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;
using Cysharp.Threading.Tasks;
using Conkist.GDK.Loading;

namespace Conkist.GDK
{
    /// <summary>
    /// Represents a loading event with address, status, and type.
    /// </summary>
    public struct LoadingEvent
    {
        static LoadingEvent ev;

        public string Address;
        public LoadStatus Status;
        public LoadType Type;

        public LoadingEvent(string address, LoadStatus status, LoadType type)
        {
            Address = address;
            Status = status;
            Type = type;
        }

        /// <summary>
        /// Triggers a loading event.
        /// </summary>
        /// <param name="address">The address related to the event.</param>
        /// <param name="status">The load status.</param>
        /// <param name="type">The type of load.</param>
        public static void Trigger(string address, LoadStatus status, LoadType type)
        {
            ev.Address = address;
            ev.Status = status;
            ev.Type = type;

            EventManager.TriggerEvent(ev);
        }
    }

    /// <summary>
    /// Enum representing different loading status stages.
    /// </summary>
    public enum LoadStatus
    {
        LoadStarted,
        BeforeEntryFade, EntryFade, AfterEntryFade,
        UnloadOriginScene, LoadDestinationScene, LoadProgressComplete, InterpolatedLoadProgressComplete, DestinationSceneActivation,
        BeforeExitFade, ExitFade,
        UnloadSceneLoader, LoadTransitionComplete
    }

    /// <summary>
    /// Enum representing different types of loads.
    /// </summary>
    public enum LoadType
    {
        Hidden, Quick, FullScreen
    }

    /// <summary>
    /// Manages loading operations and broadcasts loading events using LoadingEvent.
    /// </summary>
    public class LoadingManager : Singleton<LoadingManager>
    {
        [SerializeField] SceneManager _sceneManager;
        public static SceneManager Scene => Instance._sceneManager;


        //TODO VOLTAR DAQUI, VER VARIAVEIS PRIVADAS DE ACOMPANHAMENTO DO LOADING MANAGER!!!!!

        public bool IsLoading { get; private set; }
        public bool IsDownloading { get; private set; }
        public LoadStatus LoadStatus { get; private set; }
        public LoadType LoadType { get; private set; }

        [Header("Bindings")]
        [SerializeField] LoadEventListener quickLoadListener;
        [SerializeField] LoadEventListener downloadListener;

        [Header("Events")]
        [SerializeField] UnityEvent onLoadSetup;
        [SerializeField] UnityEvent onLoadComplete;

        /// <summary>
        /// Initializes the scene manager if not already set.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            if (Scene == null) _sceneManager = GetComponent<SceneManager>();
        }

        /// <summary>
        /// Checks asynchronously if a given address is in the cache, invoking a callback with the result.
        /// </summary>
        /// <param name="address">The address to check.</param>
        /// <param name="callback">Callback indicating if the address is in cache.</param> 
        public void InCache(string address, UnityAction<bool> callback)
        {
            try
            {
                var download = Addressables.GetDownloadSizeAsync(address);
                download.Completed += (op) =>
                {
                    callback?.Invoke(op.IsDone && op.Result == 0);
                };
            }catch(InvalidKeyException ex)
            {
                Debug.LogWarning("The address:" + address + " is not listed in the addressable groups. Try to make a new build if does.");
            }
        }

        /// <summary>
        /// Asynchronously checks if a given address is in the cache.
        /// </summary>
        /// <param name="address">The address to check.</param>
        /// <returns>True if in cache, otherwise false.</returns>
        public async UniTask<bool> InCacheAsync(string address)
        {
            try
            {
                var getSizeAsyncOp = Addressables.GetDownloadSizeAsync(address).Task.AsUniTask();
                long result = await getSizeAsyncOp;
                return result > 0;
            }
            catch (InvalidKeyException ex)
            {
                Debug.LogWarning("The address:" + address + " is not listed in the addressable groups. Try to make a new build if does.");
                return false;
            }
        }

        /// <summary>
        /// Asynchronously downloads content from the given address, showing progress and triggering load events.
        /// </summary>
        /// <param name="address">The address to download content from.</param>
        /// <param name="loadType">The type of load.</param>
        public async UniTask DownloadContentAsync(string address, LoadType loadType = LoadType.FullScreen)
        {
            IsDownloading = true;
            await LoadingEventTask(address, LoadStatus.LoadStarted, loadType);
            long size = await Addressables.GetDownloadSizeAsync(address);

            if (size > 0)
            {
                await UniTask.NextFrame();

                var download = Addressables.DownloadDependenciesAsync(address)
                    .ToUniTask(Progress.Create<float>(LoadProgress));
                await download;

                if (!download.Status.IsCompleted())
                {
                    download = Addressables.DownloadDependenciesAsync(address)
                    .ToUniTask(Progress.Create<float>(LoadProgress));
                    await download;
                }

                await LoadingEventTask(address, LoadStatus.InterpolatedLoadProgressComplete, loadType);
                IsDownloading = false;
                await UniTask.Delay(300, DelayType.Realtime);
                Addressables.Release(address);
            }
        }

        /// <summary>
        /// Asynchronously downloads content from multiple asset labels, showing progress and triggering load events.
        /// </summary>
        /// <param name="assetLabels">Asset labels representing the content to download.</param>
        public async UniTask DownloadContentAsync(params AssetLabelReference[] assetLabels)
        {
            IsDownloading = true;
            await LoadingEventTask(assetLabels.ToString(), LoadStatus.LoadStarted, LoadType.FullScreen);
            var downloadPack = new AssetLabelsDownloadPack(assetLabels);

            downloadPack.TrackProgress(Progress.Create<AssetsDownloadStatus>(DownloadProgress));
            var result = await downloadPack.StartDownloadAsync();

            if (!result.IsSuccess)
            {
                result = await downloadPack.StartDownloadAsync();
            }

            await LoadingEventTask(assetLabels.ToString(), LoadStatus.InterpolatedLoadProgressComplete, LoadType.FullScreen);
            IsDownloading = false;
            await UniTask.Delay(300, DelayType.Realtime);
            downloadPack.Dispose();
        }

        /// <summary>
        /// Asynchronously loads an asset from the given address and triggers load events.
        /// </summary>
        /// <typeparam name="T">Type of the asset to load.</typeparam>
        /// <param name="address">The address to load the asset from.</param>
        /// <param name="loadType">The type of load.</param>
        /// <returns>The loaded asset.</returns>
        public async UniTask<T> LoadAssetAsync<T>(string address, LoadType loadType = LoadType.Hidden) where T : Object
        {
            IsLoading = true;
            await LoadingEventTask(address, LoadStatus.LoadStarted, loadType);
            var locations = await Addressables.LoadResourceLocationsAsync(address, typeof(T));
            if (locations.Count == 0) return null;

            var asset = await Addressables.LoadAssetAsync<T>(address);
            IsLoading = false;
            await LoadingEventTask(address, LoadStatus.InterpolatedLoadProgressComplete, loadType);
            return asset;
        }

        /// <summary>
        /// Asynchronously loads an asset using an asset reference and triggers load events.
        /// </summary>
        /// <typeparam name="T">Type of the asset to load.</typeparam>
        /// <param name="assetReference">The asset reference.</param>
        /// <param name="loadType">The type of load.</param>
        /// <returns>The loaded asset.</returns>
        public async UniTask<T> LoadAssetAsync<T>(AssetReferenceT<T> assetReference, LoadType loadType = LoadType.Hidden) where T : Object
        {
            IsLoading = true;
            await LoadingEventTask(assetReference.AssetGUID, LoadStatus.LoadStarted, loadType);
            IAssetsReferenceLoader<T> loader = new AssetsReferenceLoader<T>();
            await loader.PreloadAssetAsync(assetReference);

            if (loader.TryGetAsset(assetReference, out T result))
            {
                await LoadingEventTask(assetReference.AssetGUID, LoadStatus.InterpolatedLoadProgressComplete, loadType);
                IsLoading = false;
                return result;
            }
            else
            {
                IsLoading = false;
                Debug.LogWarning("No asset loaded");
            }
            return null;
        }

        /// <summary>
        /// Preloads an asset asynchronously.
        /// </summary>
        /// <typeparam name="T">Type of the asset.</typeparam>
        /// <param name="assetReference">The asset reference.</param>
        /// <returns>A UniTask representing the async operation.</returns>
        public UniTask GetAssetPreloader<T>(AssetReferenceT<T> assetReference) where T : Object
        {
            IsLoading = true;
            var loader = new AssetsReferenceLoader<T>().PreloadAssetAsync(assetReference);
            IsLoading = false;
            return loader;
        }

        /// <summary>
        /// Unloads an asset identified by its address.
        /// </summary>
        /// <param name="address">The address of the asset to unload.</param>
        public void UnloadAsset(string address)
        {
            Addressables.Release(address);
        }

        /// <summary>
        /// Unloads a given asset.
        /// </summary>
        /// <typeparam name="T">Type of the asset.</typeparam>
        /// <param name="asset">The asset to unload.</param>
        public void UnloadAsset<T>(T asset)
        {
            Addressables.Release(asset);
        }

        /// <summary>
        /// Unloads an instantiated game object.
        /// </summary>
        /// <param name="instance">The game object instance to unload.</param>
        public void UnloadInstance(GameObject instance)
        {
            Addressables.ReleaseInstance(instance);
        }

        /// <summary>
        /// Clears the cache for a given address.
        /// </summary>
        /// <param name="address">The address to clear the cache for.</param>
        public void ClearCache(string address)
        {
            Addressables.ClearDependencyCacheAsync(address);
        }

        /// <summary>
        /// Fires a loading event and returns a task based on the load type.
        /// </summary>
        /// <param name="address">The address related to the event.</param>
        /// <param name="status">The load status.</param>
        /// <param name="type">The type of load.</param>
        /// <returns>A UniTask representing the event task.</returns>
        private UniTask LoadingEventTask(string address, LoadStatus status, LoadType type)
        {
            LoadStatus = status;
            LoadType = type;
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
                        if(downloadListener != null)
                        return downloadListener.GetLoadingTask(address, status);
                    }
                    break;
            }
            return UniTask.Yield().ToUniTask();
        }

        private void LoadProgress(float progress)
        {
            downloadListener.LoadProgress(progress);
        }

        private void DownloadProgress(AssetsDownloadStatus status)
        {
            downloadListener.DownloadProgress(status);
        }
    }
}