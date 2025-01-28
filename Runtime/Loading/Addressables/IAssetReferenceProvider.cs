using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Conkist.GDK.Loading
{
    //TODO as generic key-value SO in separate module?
    public interface IAssetReferenceProvider<TAsset, in TKey>
        where TAsset : Object
    {
        AssetReferenceT<TAsset> GetByKey(TKey key);
    }
}
