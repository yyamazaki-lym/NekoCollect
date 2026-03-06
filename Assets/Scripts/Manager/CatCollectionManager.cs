using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NekoCollect.Data;
using NekoCollect.Save;
using NekoCollect.Util;

namespace NekoCollect.Manager
{
    /// <summary>
    /// 猫のコレクション管理、育成、進化
    /// </summary>
    public class CatCollectionManager : SingletonMonoBehaviour<CatCollectionManager>
    {
        /// <summary>猫が追加されたときに通知</summary>
        public event Action<CatData, bool> OnCatAdded; // (猫データ, 新規かどうか)
        /// <summary>猫がレベルアップしたときに通知</summary>
        public event Action<OwnedCatData, CatData> OnCatLevelUp;
        /// <summary>猫が進化したときに通知</summary>
        public event Action<CatData, CatData> OnCatEvolved; // (進化前, 進化後)

        [Header("全猫マスターデータ")]
        [SerializeField] private CatData[] allCats;

        [Header("育成設定")]
        [SerializeField] private int cheapFoodCost = 10;
        [SerializeField] private int cheapFoodExp = 5;
        [SerializeField] private int premiumFoodCost = 50;
        [SerializeField] private int premiumFoodExp = 30;
        /// <summary>被り猫の経験値ボーナス</summary>
        [SerializeField] private int duplicateExpBonus = 20;

        private Dictionary<string, CatData> catMasterMap;

        protected override void Awake()
        {
            base.Awake();
            catMasterMap = new Dictionary<string, CatData>();
            foreach (var cat in allCats)
            {
                catMasterMap[cat.catId] = cat;
            }
        }

        /// <summary>
        /// IDから猫マスターデータを取得
        /// </summary>
        public CatData GetCatData(string catId)
        {
            catMasterMap.TryGetValue(catId, out var data);
            return data;
        }

        /// <summary>
        /// 全猫マスターデータを取得（図鑑用）
        /// </summary>
        public CatData[] GetAllCats() => allCats;

        /// <summary>
        /// 所持猫リストを取得
        /// </summary>
        public List<OwnedCatData> GetOwnedCats() => SaveManager.Instance.CurrentData.ownedCats;

        /// <summary>
        /// 猫を所持しているか
        /// </summary>
        public bool HasCat(string catId)
        {
            return GetOwnedCats().Any(c => c.catId == catId);
        }

        /// <summary>
        /// 猫を追加（ガチャ結果）
        /// </summary>
        public void AddCat(CatData catData)
        {
            var owned = GetOwnedCats().Find(c => c.catId == catData.catId);
            if (owned != null)
            {
                // 被り: 経験値ボーナスを付与
                AddExp(owned, catData, duplicateExpBonus);
                OnCatAdded?.Invoke(catData, false);
            }
            else
            {
                // 新規追加
                var newCat = new OwnedCatData { catId = catData.catId };
                GetOwnedCats().Add(newCat);
                OnCatAdded?.Invoke(catData, true);
            }
            SaveManager.Instance.Save();
        }

        /// <summary>
        /// 安いエサを与える
        /// </summary>
        public bool FeedCheap(OwnedCatData cat)
        {
            if (!CoinManager.Instance.SpendCoins(cheapFoodCost)) return false;
            var catData = GetCatData(cat.catId);
            AddExp(cat, catData, cheapFoodExp);
            SaveManager.Instance.Save();
            return true;
        }

        /// <summary>
        /// 高級エサを与える
        /// </summary>
        public bool FeedPremium(OwnedCatData cat)
        {
            if (!CoinManager.Instance.SpendCoins(premiumFoodCost)) return false;
            var catData = GetCatData(cat.catId);
            AddExp(cat, catData, premiumFoodExp);
            SaveManager.Instance.Save();
            return true;
        }

        /// <summary>
        /// 経験値を加算し、レベルアップを処理
        /// </summary>
        private void AddExp(OwnedCatData cat, CatData catData, int amount)
        {
            // 経験値ボーナス（スキル）
            float expBonusPercent = GetExpBonusPercent(cat);
            amount = (int)(amount * (1f + expBonusPercent / 100f));

            cat.currentExp += amount;

            // レベルアップ判定
            while (cat.level < catData.maxLevel)
            {
                int needed = catData.GetExpForLevel(cat.level + 1);
                if (cat.currentExp >= needed)
                {
                    cat.currentExp -= needed;
                    cat.level++;
                    CheckSkillLearning(cat, catData);
                    OnCatLevelUp?.Invoke(cat, catData);
                }
                else break;
            }
        }

        /// <summary>
        /// レベルに応じたスキル習得チェック
        /// </summary>
        private void CheckSkillLearning(OwnedCatData cat, CatData catData)
        {
            for (int i = 0; i < catData.skills.Length; i++)
            {
                if (cat.level >= catData.skills[i].requiredLevel
                    && !cat.learnedSkillIndices.Contains(i))
                {
                    cat.learnedSkillIndices.Add(i);
                }
            }
        }

        /// <summary>
        /// 進化可能か判定
        /// </summary>
        public bool CanEvolve(OwnedCatData cat)
        {
            var catData = GetCatData(cat.catId);
            return catData.evolutionTarget != null && cat.level >= catData.evolutionLevel;
        }

        /// <summary>
        /// 猫を進化させる
        /// </summary>
        public bool Evolve(OwnedCatData cat)
        {
            if (!CanEvolve(cat)) return false;

            var oldCatData = GetCatData(cat.catId);
            var newCatData = oldCatData.evolutionTarget;

            // 進化先に変更
            cat.catId = newCatData.catId;
            cat.level = 1;
            cat.currentExp = 0;
            cat.learnedSkillIndices.Clear();

            // 進化先が未所持扱いなら新規フラグ不要（既にownedCatsに入っている）
            OnCatEvolved?.Invoke(oldCatData, newCatData);
            SaveManager.Instance.Save();
            return true;
        }

        /// <summary>
        /// 所持猫全体のなつき度合計を計算
        /// </summary>
        public float GetTotalFriendliness()
        {
            float total = 0f;
            foreach (var cat in GetOwnedCats())
            {
                var catData = GetCatData(cat.catId);
                if (catData != null)
                {
                    total += catData.GetStatsAtLevel(cat.level).friendliness;
                }
            }
            return total;
        }

        /// <summary>
        /// スキルによるクリックコインボーナス合計
        /// </summary>
        public int GetClickCoinBonus()
        {
            int bonus = 0;
            foreach (var cat in GetOwnedCats())
            {
                var catData = GetCatData(cat.catId);
                if (catData == null) continue;
                foreach (int idx in cat.learnedSkillIndices)
                {
                    if (idx < catData.skills.Length &&
                        catData.skills[idx].effectType == SkillEffectType.ClickCoinBonus)
                    {
                        bonus += (int)catData.skills[idx].effectValue;
                    }
                }
            }
            return bonus;
        }

        /// <summary>
        /// スキルによる放置コインボーナス%合計
        /// </summary>
        public float GetIdleCoinBonusPercent()
        {
            float bonus = 0f;
            foreach (var cat in GetOwnedCats())
            {
                var catData = GetCatData(cat.catId);
                if (catData == null) continue;
                foreach (int idx in cat.learnedSkillIndices)
                {
                    if (idx < catData.skills.Length &&
                        catData.skills[idx].effectType == SkillEffectType.IdleCoinBonus)
                    {
                        bonus += catData.skills[idx].effectValue;
                    }
                }
            }
            return bonus;
        }

        /// <summary>
        /// 指定猫の経験値ボーナス%
        /// </summary>
        private float GetExpBonusPercent(OwnedCatData cat)
        {
            var catData = GetCatData(cat.catId);
            if (catData == null) return 0f;
            float bonus = 0f;
            foreach (int idx in cat.learnedSkillIndices)
            {
                if (idx < catData.skills.Length &&
                    catData.skills[idx].effectType == SkillEffectType.ExpBonus)
                {
                    bonus += catData.skills[idx].effectValue;
                }
            }
            return bonus;
        }

        /// <summary>
        /// エサのコストを取得
        /// </summary>
        public int GetCheapFoodCost() => cheapFoodCost;
        public int GetPremiumFoodCost() => premiumFoodCost;
    }
}
