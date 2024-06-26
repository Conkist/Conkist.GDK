using System;
using UnityEngine.Scripting;
using UnityEngine.U2D;

namespace Conkist.GDK.Loading
{
    public class SpriteAtlasProvider : ISpriteAtlasProvider
    {
        private readonly IAssetsReferenceLoader<SpriteAtlas> _spriteAtlasLoader;
        private readonly IAssetReferenceProvider<SpriteAtlas, string> _spriteAtlasAssetReferenceProvider;

        [RequiredMember]
        public SpriteAtlasProvider(IAssetsReferenceLoader<UnityEngine.U2D.SpriteAtlas> spriteAtlasLoader,
            IAssetReferenceProvider<SpriteAtlas, string> spriteAtlasAssetReferenceProvider)
        {
            _spriteAtlasLoader = spriteAtlasLoader;
            _spriteAtlasAssetReferenceProvider = spriteAtlasAssetReferenceProvider;
        }

        public void SubscribeToAtlasManagerRequests()
        {
            SpriteAtlasManager.atlasRequested += OnAtlasRequested;
        }

        public void UnsubscribeFromAtlasManagerRequests()
        {
            SpriteAtlasManager.atlasRequested -= OnAtlasRequested;
        }

        public void UnloadSpriteAtlases()
        {
            _spriteAtlasLoader.UnloadAllAssets();
        }

        private async void OnAtlasRequested(string atlasName, Action<UnityEngine.U2D.SpriteAtlas> callback)
        {
            var spriteAtlas =
                await _spriteAtlasLoader.LoadAssetAsync(_spriteAtlasAssetReferenceProvider.GetByKey(atlasName));

            callback?.Invoke(spriteAtlas);
        }
    }
}
