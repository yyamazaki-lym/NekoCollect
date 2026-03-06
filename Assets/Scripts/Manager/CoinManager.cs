using System;
using UnityEngine;
using NekoCollect.Util;

namespace NekoCollect.Manager
{
    /// <summary>
    /// コインの管理（クリック獲得・放置獲得）
    /// </summary>
    public class CoinManager : SingletonMonoBehaviour<CoinManager>
    {
        /// <summary>コイン残高が変化したときに通知</summary>
        public event Action<long> OnCoinsChanged;

        /// <summary>放置コインを受け取ったときに通知（獲得量）</summary>
        public event Action<long> OnIdleCoinsCollected;

        [Header("設定")]
        [SerializeField] private int baseClickCoin = 1;
        [SerializeField] private float idleCoinInterval = 1f;
        /// <summary>放置コインの最大蓄積時間（秒）</summary>
        [SerializeField] private float maxIdleSeconds = 28800f; // 8時間

        private float idleTimer;

        public long Coins => SaveManager.Instance.CurrentData.coins;

        /// <summary>
        /// ゲーム起動時の放置コインを計算して付与
        /// </summary>
        public void CollectOfflineCoins()
        {
            var saveData = SaveManager.Instance.CurrentData;
            var lastLogin = saveData.LastLoginTime;
            var elapsed = (DateTime.Now - lastLogin).TotalSeconds;
            elapsed = Math.Min(elapsed, maxIdleSeconds);

            if (elapsed > 1)
            {
                float totalFriendliness = CatCollectionManager.Instance.GetTotalFriendliness();
                float idleCoinRate = GetIdleCoinRate(totalFriendliness);
                long offlineCoins = (long)(elapsed * idleCoinRate);

                if (offlineCoins > 0)
                {
                    AddCoins(offlineCoins);
                    OnIdleCoinsCollected?.Invoke(offlineCoins);
                }
            }
        }

        private void Update()
        {
            // リアルタイム放置コイン
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleCoinInterval)
            {
                idleTimer -= idleCoinInterval;
                float totalFriendliness = CatCollectionManager.Instance.GetTotalFriendliness();
                float rate = GetIdleCoinRate(totalFriendliness);
                if (rate > 0)
                {
                    AddCoins((long)rate);
                }
            }
        }

        /// <summary>
        /// クリックでコインを獲得
        /// </summary>
        public void ClickCoin()
        {
            int bonus = CatCollectionManager.Instance.GetClickCoinBonus();
            AddCoins(baseClickCoin + bonus);
        }

        /// <summary>
        /// コインを加算
        /// </summary>
        public void AddCoins(long amount)
        {
            SaveManager.Instance.CurrentData.coins += amount;
            OnCoinsChanged?.Invoke(Coins);
        }

        /// <summary>
        /// コインを消費（足りなければfalse）
        /// </summary>
        public bool SpendCoins(long amount)
        {
            if (Coins < amount) return false;
            SaveManager.Instance.CurrentData.coins -= amount;
            OnCoinsChanged?.Invoke(Coins);
            return true;
        }

        /// <summary>
        /// なつき度に基づく毎秒コイン獲得率を計算
        /// </summary>
        private float GetIdleCoinRate(float totalFriendliness)
        {
            // なつき度10ごとに毎秒1コイン + スキルボーナス
            float baseRate = totalFriendliness / 10f;
            float bonusPercent = CatCollectionManager.Instance.GetIdleCoinBonusPercent();
            return baseRate * (1f + bonusPercent / 100f);
        }
    }
}
