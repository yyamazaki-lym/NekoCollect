using System;
using System.Collections.Generic;

namespace NekoCollect.Save
{
    /// <summary>
    /// 所持猫の個別データ
    /// </summary>
    [Serializable]
    public class OwnedCatData
    {
        /// <summary>CatDataのcatIdと対応</summary>
        public string catId;
        public int level = 1;
        public int currentExp;
        /// <summary>習得済みスキルのインデックス</summary>
        public List<int> learnedSkillIndices = new List<int>();
    }

    /// <summary>
    /// ゲーム全体のセーブデータ（JSON化してPlayerPrefsに保存）
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public List<OwnedCatData> ownedCats = new List<OwnedCatData>();
        public long coins;
        /// <summary>最終ログイン時間（DateTime.ToBinary()形式）</summary>
        public long lastLoginTimeBinary;

        /// <summary>
        /// 最終ログイン時間をDateTimeで取得
        /// </summary>
        public DateTime LastLoginTime
        {
            get => DateTime.FromBinary(lastLoginTimeBinary);
            set => lastLoginTimeBinary = value.ToBinary();
        }
    }
}
