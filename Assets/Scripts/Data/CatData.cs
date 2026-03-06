using System;
using UnityEngine;

namespace NekoCollect.Data
{
    /// <summary>
    /// 猫の基礎ステータス
    /// </summary>
    [Serializable]
    public struct CatStats
    {
        /// <summary>なつき度（放置コインに影響）</summary>
        public float friendliness;
        /// <summary>元気（レベルアップ速度に影響）</summary>
        public float energy;
        /// <summary>かわいさ（図鑑用）</summary>
        public float cuteness;
    }

    /// <summary>
    /// 猫のマスターデータ
    /// </summary>
    [CreateAssetMenu(fileName = "NewCat", menuName = "NekoCollect/CatData")]
    public class CatData : ScriptableObject
    {
        [Header("基本情報")]
        public string catId;
        public string catName;
        public Rarity rarity;
        [TextArea] public string description;

        [Header("見た目")]
        public Sprite sprite;

        [Header("ステータス")]
        public int maxLevel = 20;
        public CatStats baseStats;
        /// <summary>レベルアップ時のステータス上昇率</summary>
        public float statsGrowthRate = 0.1f;

        [Header("スキル")]
        public SkillData[] skills;

        [Header("進化")]
        /// <summary>進化先（nullなら進化なし）</summary>
        public CatData evolutionTarget;
        /// <summary>進化に必要なレベル</summary>
        public int evolutionLevel;

        /// <summary>
        /// 指定レベルでのステータスを計算
        /// </summary>
        public CatStats GetStatsAtLevel(int level)
        {
            float multiplier = 1f + statsGrowthRate * (level - 1);
            return new CatStats
            {
                friendliness = baseStats.friendliness * multiplier,
                energy = baseStats.energy * multiplier,
                cuteness = baseStats.cuteness * multiplier
            };
        }

        /// <summary>
        /// 指定レベルに必要な累計経験値
        /// </summary>
        public int GetExpForLevel(int level)
        {
            // レベル^2 * 10 の簡易曲線
            return level * level * 10;
        }
    }
}
