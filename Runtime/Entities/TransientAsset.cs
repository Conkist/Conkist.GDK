using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Conkist.GDK
{
    /// <summary>
    /// A class for handling transient assets of a specific type.
    /// When an asset is requested, a new instance of the asset is created.
    /// </summary>
    /// <typeparam name="TAsset">The type of the ScriptableObject.</typeparam>
    [Serializable]
    public class TransientAsset<TAsset> where TAsset : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The original asset to instantiate from.")]
        private TAsset _asset;

        /// <summary>
        /// Gets a new instance of the asset.
        /// </summary>
        public TAsset Asset => Object.Instantiate(_asset);

        /// <summary>
        /// Implicitly converts a TransientAsset instance to the underlying asset type.
        /// Provides a new instance of the asset.
        /// </summary>
        /// <param name="scopedAsset">The TransientAsset instance to convert.</param>
        public static implicit operator TAsset(TransientAsset<TAsset> scopedAsset)
        {
            return scopedAsset.Asset;
        }
    }
}

