using System;
using UnityEngine;
using NekoCollect.Data;
using NekoCollect.Util;

namespace NekoCollect.Manager
{
    /// <summary>
    /// ガチャの抽選ロジック
    /// </summary>
    public class GachaManager : SingletonMonoBehaviour<GachaManager>
    {
        /// <summary>ガチャ結果の通知（猫データ, 新規かどうか）</summary>
        public event Action<CatData, bool> OnGachaResult;

        [Header("ガチャバナー")]
        [SerializeField] private GachaBanner[] banners;

        /// <summary>
        /// 利用可能なバナー一覧を取得
        /// </summary>
        public GachaBanner[] GetBanners() => banners;

        /// <summary>
        /// ガチャを1回引く
        /// </summary>
        public bool Pull(GachaBanner banner)
        {
            // コイン消費
            if (!CoinManager.Instance.SpendCoins(banner.cost))
                return false;

            // 重み付きランダム抽選
            CatData result = DrawFromBanner(banner);
            if (result == null) return false;

            // 猫を追加
            bool isNew = !CatCollectionManager.Instance.HasCat(result.catId);
            CatCollectionManager.Instance.AddCat(result);

            OnGachaResult?.Invoke(result, isNew);
            SaveManager.Instance.Save();
            return true;
        }

        /// <summary>
        /// 重み付きランダムで猫を抽選
        /// </summary>
        private CatData DrawFromBanner(GachaBanner banner)
        {
            float totalWeight = banner.TotalWeight;
            if (totalWeight <= 0f) return null;

            float roll = UnityEngine.Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var entry in banner.entries)
            {
                cumulative += entry.weight;
                if (roll <= cumulative)
                    return entry.cat;
            }

            // フォールバック（浮動小数点誤差対策）
            return banner.entries[banner.entries.Length - 1].cat;
        }
    }
}
