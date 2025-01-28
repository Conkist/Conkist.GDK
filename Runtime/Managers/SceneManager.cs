using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using Cysharp.Threading.Tasks;

namespace Conkist.GDK
{
    public class SceneManager : ScriptableObject
    {

        private static SceneInstance activeScene;

        public static async UniTask<SceneInstance> LoadSceneAsync(string sceneToLoad, LoadType loadType = LoadType.FullScreen)
        {
            if(LoadingManager.IsLoading){
                Debug.LogWarning("Manager is currently loading something");
                return activeScene;
            }
            LoadingManager.StartupLoading(sceneToLoad, loadType);

            //Application.backgroundLoadingPriority = ThreadPriority.High;
            var loader = Addressables.LoadSceneAsync(sceneToLoad);
            loader.Completed += (op) => {
                activeScene = op.Result;
                LoadingManager.ChangeLoadingState(LoadingStates.LoadProgressComplete);
                LoadingManager.ChangeLoadingState(LoadingStates.DestinationSceneActivation);
            };
            var scene = await loader;

            LoadingManager.ChangeLoadingState(LoadingStates.ExitFade);
            LoadingManager._isLoading = false;

            return scene;
        }

        public static async UniTask<SceneInstance> LoadSceneAsync(AssetReference sceneToLoad, LoadType loadType = LoadType.FullScreen)
        {
            if(LoadingManager.IsLoading){
                Debug.LogWarning("Manager is currently loading something");
                return activeScene;
            }
            LoadingManager.StartupLoading(sceneToLoad.AssetGUID, loadType);

            //Application.backgroundLoadingPriority = ThreadPriority.High;
            var loader = Addressables.LoadSceneAsync(sceneToLoad);
            loader.Completed += (op) => {
                activeScene = op.Result;
                LoadingManager.ChangeLoadingState(LoadingStates.LoadProgressComplete);
                LoadingManager.ChangeLoadingState(LoadingStates.DestinationSceneActivation);
            };
            var scene = await loader;

            LoadingManager.ChangeLoadingState(LoadingStates.ExitFade);
            LoadingManager._isLoading = false;

            return scene;
        }

        public static async UniTask<SceneInstance> AddSceneAsync(string sceneToAdd, LoadType loadType = LoadType.Hidden, bool forceMultiload = false, bool activate = true)
        {
            if(forceMultiload)
            {
                var forceload = Addressables.LoadSceneAsync(sceneToAdd, loadMode: UnityEngine.SceneManagement.LoadSceneMode.Additive);
                forceload.Completed += (op) => OnCompleteAdditiveLoadOp(op.Result, forceMultiload: true, activate);
                return await forceload;
            }
            if(LoadingManager.IsLoading){
                Debug.LogWarning("Manager is currently loading something");
                return activeScene;
            }
            LoadingManager.StartupLoading(sceneToAdd, loadType);

            //Application.backgroundLoadingPriority = ThreadPriority.High;
            var loader = Addressables.LoadSceneAsync(sceneToAdd, loadMode: UnityEngine.SceneManagement.LoadSceneMode.Additive);
            loader.Completed += (op) => {
                LoadingManager.ChangeLoadingState(LoadingStates.LoadProgressComplete);
                OnCompleteAdditiveLoadOp(op.Result, forceMultiload: true, activate);
            };
            var scene = await loader;

            LoadingManager.ChangeLoadingState(LoadingStates.ExitFade);
            LoadingManager._isLoading = false;

            return scene;
        }

        public static async UniTask<SceneInstance> AddSceneAsync(AssetReference sceneToAdd, LoadType loadType = LoadType.Hidden, bool forceMultiload = false, bool activate = true)
        {
            if(forceMultiload)
            {
                var forceload = Addressables.LoadSceneAsync(sceneToAdd, loadMode: UnityEngine.SceneManagement.LoadSceneMode.Additive);
                forceload.Completed += (op) => OnCompleteAdditiveLoadOp(op.Result, forceMultiload, activate);
                return await forceload;
            }
            if(LoadingManager.IsLoading){
                Debug.LogWarning("Manager is currently loading something");
                return activeScene;
            }
            Debug.Log("Add Scene from Reference");
            LoadingManager.StartupLoading(sceneToAdd.AssetGUID, loadType);

            //Application.backgroundLoadingPriority = ThreadPriority.High;
            var loader = Addressables.LoadSceneAsync(sceneToAdd, loadMode: UnityEngine.SceneManagement.LoadSceneMode.Additive);
            loader.Completed += (op) => {
                LoadingManager.ChangeLoadingState(LoadingStates.LoadProgressComplete);
                OnCompleteAdditiveLoadOp(op.Result, forceMultiload, activate);
            };
            var scene = await loader;

            LoadingManager.ChangeLoadingState(LoadingStates.ExitFade);
            LoadingManager._isLoading = false;

            return scene;
        }

        private static void OnCompleteAdditiveLoadOp(SceneInstance scene, bool forceMultiload, bool activate)
        {
            if(activate) UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene.Scene);
            activeScene = scene;
            if(!forceMultiload) LoadingManager.ChangeLoadingState(LoadingStates.DestinationSceneActivation);
        }

        public static bool IsSceneLoaded(string address)
        {
            return UnityEngine.SceneManagement.SceneManager.GetSceneByName(address).isLoaded;
        }

        public static async UniTask ReloadScene(LoadType loadType = LoadType.FullScreen)
        {
            LoadingManager._isLoading = true;
            LoadingManager.ChangeLoadType(loadType);
            LoadingManager.ChangeLoadingState(LoadingStates.LoadStarted);

            var task = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
                .ToUniTask(Progress.Create<float>(LoadingManager.LoadProgress));
            await task;

            LoadingManager.ChangeLoadingState(LoadingStates.LoadProgressComplete);
            LoadingManager._isLoading = false;
            LoadingEvents.ReloadSceneEvent.Trigger();
        }

        public static async UniTask ReloadApplication(){
            LoadingManager._isLoading = true;
            LoadingManager.ChangeLoadType(LoadType.Hidden);
            LoadingManager.ChangeLoadingState(LoadingStates.LoadStarted);

            var task = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(0)
                .ToUniTask(Progress.Create<float>(LoadingManager.LoadProgress));
            await task;

            LoadingManager.ChangeLoadingState(LoadingStates.LoadProgressComplete);
            LoadingManager._isLoading = false;
            LoadingEvents.ReloadSceneEvent.Trigger();
        }

        /// <summary>
        /// Basic remove operation for additive scenes
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="autoRelease"></param>
        /// <param name="loadType"></param>
        /// <returns></returns>
        public static async UniTask RemoveScene(SceneInstance scene, bool autoRelease = true, LoadType loadType = LoadType.Hidden)
        {
            await Addressables.UnloadSceneAsync(scene, autoReleaseHandle: autoRelease);
        }

        public static void CreateScene(string v)
        {
            Debug.Log("Creating Scene: " + v);
        }
    }
}
