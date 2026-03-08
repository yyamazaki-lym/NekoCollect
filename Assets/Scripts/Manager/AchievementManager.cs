using System;
using System.Collections.Generic;
using UnityEngine;
using NekoCollect.Data;
using NekoCollect.Save;
using NekoCollect.Util;

namespace NekoCollect.Manager
{
    /// <summary>
    /// 実績の定義
    /// </summary>
    [Serializable]
    public class AchievementDef
    {
        public string id;
        public string title;
        public string description;
        public int coinReward;
    }

    /// <summary>
    /// 実績管理マネージャー
    /// </summary>
    public class AchievementManager : SingletonMonoBehaviour<AchievementManager>
    {
        /// <summary>実績解除時に通知</summary>
        public event Action<AchievementDef> OnAchievementUnlocked;

        // 実績定義リスト
        private List<AchievementDef> allAchievements;

        protected override void Awake()
        {
            base.Awake();
            InitAchievements();
        }

        private void Start()
        {
            // イベント登録
            if (CatCollectionManager.Instance != null)
            {
                CatCollectionManager.Instance.OnCatAdded += OnCatAdded;
                CatCollectionManager.Instance.OnCatLevelUp += OnCatLevelUp;
                CatCollectionManager.Instance.OnCatEvolved += OnCatEvolved;
            }
            if (GachaManager.Instance != null)
            {
                GachaManager.Instance.OnGachaResult += OnGachaPull;
            }
        }

        private void OnDestroy()
        {
            if (CatCollectionManager.Instance != null)
            {
                CatCollectionManager.Instance.OnCatAdded -= OnCatAdded;
                CatCollectionManager.Instance.OnCatLevelUp -= OnCatLevelUp;
                CatCollectionManager.Instance.OnCatEvolved -= OnCatEvolved;
            }
            if (GachaManager.Instance != null)
            {
                GachaManager.Instance.OnGachaResult -= OnGachaPull;
            }
        }

        /// <summary>
        /// 実績を定義
        /// </summary>
        private void InitAchievements()
        {
            // ※テキストはフォントアトラスに含まれる文字のみ使用
            //  （ひらがな・カタカナ・数字・ASCII記号＋特定の漢字のみ対応）
            allAchievements = new List<AchievementDef>
            {
                // ガチャけい
                new AchievementDef { id = "first_gacha", title = "はじめてのガチャ", description = "ガチャを1かいひいた", coinReward = 100 },
                new AchievementDef { id = "gacha_10", title = "ガチャマスター", description = "ガチャを10かいひいた", coinReward = 300 },
                new AchievementDef { id = "gacha_50", title = "ガチャだいすき", description = "ガチャを50かいひいた", coinReward = 1000 },

                // コレクションけい
                new AchievementDef { id = "collect_1", title = "はじめてのねこ", description = "猫を1ひきゲット", coinReward = 50 },
                new AchievementDef { id = "collect_3", title = "ねこだいすき", description = "猫を3ひきゲット", coinReward = 200 },
                new AchievementDef { id = "collect_5", title = "ねこコレクター", description = "猫を5ひきゲット", coinReward = 500 },
                new AchievementDef { id = "collect_all", title = "コンプリート！", description = "すべての猫をゲット", coinReward = 3000 },

                // レアリティけい
                new AchievementDef { id = "get_r", title = "レアねこゲット", description = "Rいじょうの猫をゲット", coinReward = 100 },
                new AchievementDef { id = "get_sr", title = "スーパーレア！", description = "SRいじょうの猫をゲット", coinReward = 500 },
                new AchievementDef { id = "get_ssr", title = "伝説の猫", description = "SSRの猫をゲット", coinReward = 1000 },

                // そだてけい
                new AchievementDef { id = "level_5", title = "そだて上手", description = "猫をLv.5にした", coinReward = 100 },
                new AchievementDef { id = "level_10", title = "そだてマスター", description = "猫をLv.10にした", coinReward = 300 },
                new AchievementDef { id = "level_max", title = "レベルマックス", description = "猫をMAXレベルにした", coinReward = 1000 },

                // 進化けい
                new AchievementDef { id = "first_evolve", title = "はじめての進化", description = "猫を進化させた", coinReward = 500 },

                // クリックけい
                new AchievementDef { id = "click_100", title = "コインタッパー", description = "100かいタップした", coinReward = 100 },
                new AchievementDef { id = "click_1000", title = "コインマニア", description = "1000かいタップした", coinReward = 500 },
            };
        }

        /// <summary>
        /// 全実績リストを取得
        /// </summary>
        public List<AchievementDef> GetAllAchievements() => allAchievements;

        /// <summary>
        /// 実績が解除済みか
        /// </summary>
        public bool IsUnlocked(string achievementId)
        {
            return SaveManager.Instance.CurrentData.unlockedAchievements.Contains(achievementId);
        }

        /// <summary>
        /// 実績を解除
        /// </summary>
        private void Unlock(string achievementId)
        {
            if (IsUnlocked(achievementId)) return;

            var def = allAchievements.Find(a => a.id == achievementId);
            if (def == null) return;

            SaveManager.Instance.CurrentData.unlockedAchievements.Add(achievementId);

            // 報酬コイン付与
            if (def.coinReward > 0)
                CoinManager.Instance.AddCoins(def.coinReward);

            SaveManager.Instance.Save();
            OnAchievementUnlocked?.Invoke(def);

            Debug.Log($"[実績解除] {def.title}: {def.description} (+{def.coinReward}コイン)");
        }

        // --- イベントハンドラー ---

        private void OnCatAdded(CatData cat, bool isNew)
        {
            if (!isNew) return;

            // コレクション数チェック
            int count = CatCollectionManager.Instance.GetOwnedCats().Count;
            if (count >= 1) Unlock("collect_1");
            if (count >= 3) Unlock("collect_3");
            if (count >= 5) Unlock("collect_5");

            // 全猫チェック
            var allCats = CatCollectionManager.Instance.GetAllCats();
            if (count >= allCats.Length) Unlock("collect_all");

            // レアリティチェック
            if (cat.rarity >= Rarity.R) Unlock("get_r");
            if (cat.rarity >= Rarity.SR) Unlock("get_sr");
            if (cat.rarity >= Rarity.SSR) Unlock("get_ssr");
        }

        private void OnCatLevelUp(OwnedCatData owned, CatData catData)
        {
            if (owned.level >= 5) Unlock("level_5");
            if (owned.level >= 10) Unlock("level_10");
            if (owned.level >= catData.maxLevel) Unlock("level_max");
        }

        private void OnCatEvolved(CatData oldCat, CatData newCat)
        {
            Unlock("first_evolve");
        }

        private void OnGachaPull(CatData cat, bool isNew)
        {
            var data = SaveManager.Instance.CurrentData;
            data.totalGachaPulls++;

            if (data.totalGachaPulls >= 1) Unlock("first_gacha");
            if (data.totalGachaPulls >= 10) Unlock("gacha_10");
            if (data.totalGachaPulls >= 50) Unlock("gacha_50");
        }

        /// <summary>
        /// クリック時に呼ぶ（HomeUIから）
        /// </summary>
        public void RecordClick()
        {
            var data = SaveManager.Instance.CurrentData;
            data.totalClicks++;

            if (data.totalClicks >= 100) Unlock("click_100");
            if (data.totalClicks >= 1000) Unlock("click_1000");
        }

        /// <summary>
        /// 解除済み実績数を取得
        /// </summary>
        public int GetUnlockedCount()
        {
            return SaveManager.Instance.CurrentData.unlockedAchievements.Count;
        }

        /// <summary>
        /// 全実績数を取得
        /// </summary>
        public int GetTotalCount()
        {
            return allAchievements.Count;
        }
    }
}
