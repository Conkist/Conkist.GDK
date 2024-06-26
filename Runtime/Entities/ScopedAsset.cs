using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Conkist.GDK
{
    /// <summary>
    /// A class for managing the lifecycle of a transient instance of a ScriptableObject.
    /// The instance is created when accessed and destroyed when disposed.
    /// </summary>
    /// <typeparam name="TAsset">The type of the ScriptableObject.</typeparam>
    [Serializable]
    public class ScopedAsset<TAsset> : IDisposable where TAsset : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The original asset to instantiate from.")]
        private TAsset _asset;

        // Holds the instantiated asset
        private TAsset _instance;

        /// <summary>
        /// Gets the instance of the asset. If the instance does not exist, it is created.
        /// </summary>
        public TAsset Asset
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.Instantiate(_asset);
                }

                return _instance;
            }
        }

        /// <summary>
        /// Disposes the instance of the asset by destroying it.
        /// </summary>
        public void Dispose()
        {
            if (_instance != null)
            {
                Object.Destroy(_instance);
                _instance = null;
            }
        }

        /// <summary>
        /// Implicitly converts a ScopedAsset instance to the underlying asset type.
        /// Provides the managed instance of the asset.
        /// </summary>
        /// <param name="scopedAsset">The ScopedAsset instance to convert.</param>
        public static implicit operator TAsset(ScopedAsset<TAsset> scopedAsset)
        {
            return scopedAsset.Asset;
        }
    }
}
