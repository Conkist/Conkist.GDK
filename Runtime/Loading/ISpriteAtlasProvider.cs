namespace Conkist.GDK.Loading
{
    public interface ISpriteAtlasProvider
    {
        void SubscribeToAtlasManagerRequests();
        void UnsubscribeFromAtlasManagerRequests();
        void UnloadSpriteAtlases();
    }
}
