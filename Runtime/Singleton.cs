using UnityEngine;

namespace Conkist.Tools
{
    [DefaultExecutionOrder(-100)]
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
    
        protected static T _instance;
        public static T Instance => _instance;

        [SerializeField] protected bool _persistent = true;
    
        protected virtual void Awake (){
            if (_instance != null && _instance != this){
                Debug.LogWarning("A instance already exists. Destroying it!");
                Destroy(this.gameObject); //Or GameObject as appropriate
                return;
            }
            _instance = gameObject.GetComponent<T>();
            
            if(_persistent)
            {
                _instance.transform.SetParent(null);
                DontDestroyOnLoad(_instance.gameObject);
            }
        }
    }
}