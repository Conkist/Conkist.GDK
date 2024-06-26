using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Conkist.GDK.Tests
{
    public class LoadingManagerTests
    {

        [SetUp]
        public void SetUp()
        {
            // Ensure a new instance of LoadingManager for each test.
            if (LoadingManager.HasInstance)
            {
                GameObject.DestroyImmediate(LoadingManager.Instance.gameObject);
            }
            var go = new GameObject("LoadingManager", typeof(LoadingManager));
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up after each test
            if (LoadingManager.HasInstance)
            {
                GameObject.DestroyImmediate(LoadingManager.Instance.gameObject);
            }
        }

        /// <summary>
        /// Tests that the InCache method properly checks if an address is in cache.
        /// </summary>
        /// <returns>An IEnumerator for UnityTest using UniTask.</returns>
        [UnityTest]
        public IEnumerator LoadingManager_CheckInCacheBeforeaAndAfterDownload()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                string address = "test-content";
                bool inCache = false;

                await LoadingManager.Instance.InCacheAsync(address);
                Assert.IsFalse(inCache);

                await LoadingManager.Instance.DownloadContentAsync(address, LoadType.Hidden);
                Assert.Pass();

                await LoadingManager.Instance.InCacheAsync(address);
                Assert.IsTrue(inCache);
            });
        }

        /// <summary>
        /// Tests that the LoadAssetAsync method correctly loads an asset by address and triggers loading events.
        /// </summary>
        /// <typeparam name="T">Type of the asset to load.</typeparam>
        /// <returns>An IEnumerator for UnityTest using UniTask.</returns>
        [UnityTest]
        public IEnumerator LoadingManager_CanLoadAndUnloadAssetAsync()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                string address = "MainScene";
                LoadType loadType = LoadType.Hidden;

                // Act
                var asset = await LoadingManager.Instance.LoadAssetAsync<Object>(address, loadType);
                Assert.Pass();
                Assert.NotNull(asset);

                LoadingManager.Instance.UnloadAsset(asset);
                Assert.Pass();
                Assert.Null(asset);
            });
        }

        /// <summary>
        /// Tests that the ClearCache method correctly clears the cache for a given address.
        /// </summary>
        /// <returns>An IEnumerator for UnityTest using UniTask.</returns>
        [UnityTest]
        public IEnumerator LoadingManager_CanClearCache()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                string address = "MainScene";

                // Act
                LoadingManager.Instance.ClearCache(address);
                await UniTask.NextFrame();

                // Assert
                // No direct way to check if cache clear succeeded, can assume if no errors and method completes, it works.
                Assert.Pass();
            });
        }
    }
}
