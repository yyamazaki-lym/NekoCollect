using System;
using UnityEngine;

namespace NekoCollect.Data
{
    /// <summary>
    /// ガチャの排出エントリ
    /// </summary>
    [Serializable]
    public struct GachaEntry
    {
        public CatData cat;
        /// <summary>排出重み（確率は全エントリの重み合計に対する比率）</summary>
        public float weight;
    }

    /// <summary>
    /// ガチャバナーのデータ
    /// </summary>
    [CreateAssetMenu(fileName = "NewGachaBanner", menuName = "NekoCollect/GachaBanner")]
    public class GachaBanner : ScriptableObject
    {
        [Header("基本情報")]
        public string bannerName;
        [TextArea] public string description;

        [Header("コスト")]
        /// <summary>1回のガチャに必要なコイン</summary>
        public int cost = 100;

        [Header("排出テーブル")]
        public GachaEntry[] entries;

        /// <summary>
        /// 全エントリの重み合計
        /// </summary>
        public float TotalWeight
        {
            get
            {
                float total = 0f;
                foreach (var entry in entries)
                    total += entry.weight;
                return total;
            }
        }
    }
}
