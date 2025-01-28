using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Conkist.GDK.Loading
{
    public interface IAssetsReferenceLoader<TAsset> : IAssetsLoader<AssetReferenceT<TAsset>, TAsset>
        where TAsset : Object
    {
    }
}
