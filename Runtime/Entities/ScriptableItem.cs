using UnityEngine;

namespace Conkist.Tools
{
    public abstract class ScriptableItem : ScriptableObject
    {
        public string Id => name;
        [SerializeField] string nameKey;
        [SerializeField] string imagePath;
        [TextArea(5,8)]
        [SerializeField] string descriptionKey;
    }
}
