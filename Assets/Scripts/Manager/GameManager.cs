using UnityEngine;
using NekoCollect.Util;

namespace NekoCollect.Manager
{
    /// <summary>
    /// ゲーム全体の統括マネージャー
    /// </summary>
    public class GameManager : SingletonMonoBehaviour<GameManager>
    {
        protected override void Awake()
        {
            base.Awake();
            // シーン遷移しても破棄されないようにする
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // 放置コインの回収
            CoinManager.Instance.CollectOfflineCoins();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveManager.Instance.Save();
            }
        }

        private void OnApplicationQuit()
        {
            SaveManager.Instance.Save();
        }
    }
}
