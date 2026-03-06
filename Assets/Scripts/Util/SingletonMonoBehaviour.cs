using UnityEngine;

namespace NekoCollect.Util
{
    /// <summary>
    /// シングルトンMonoBehaviourの基底クラス
    /// </summary>
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this as T;
        }
    }
}
