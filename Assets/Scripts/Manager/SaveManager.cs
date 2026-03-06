using UnityEngine;
using NekoCollect.Save;
using NekoCollect.Util;

namespace NekoCollect.Manager
{
    /// <summary>
    /// PlayerPrefsへのセーブ/ロードを管理
    /// </summary>
    public class SaveManager : SingletonMonoBehaviour<SaveManager>
    {
        private const string SAVE_KEY = "NekoCollect_SaveData";

        public SaveData CurrentData { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Load();
        }

        /// <summary>
        /// セーブデータをPlayerPrefsから読み込む
        /// </summary>
        public void Load()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                CurrentData = JsonUtility.FromJson<SaveData>(json);
            }
            else
            {
                CurrentData = new SaveData();
                CurrentData.LastLoginTime = System.DateTime.Now;
            }
        }

        /// <summary>
        /// 現在のデータをPlayerPrefsに保存
        /// </summary>
        public void Save()
        {
            CurrentData.LastLoginTime = System.DateTime.Now;
            string json = JsonUtility.ToJson(CurrentData);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// セーブデータを初期化（デバッグ用）
        /// </summary>
        public void ResetData()
        {
            CurrentData = new SaveData();
            CurrentData.LastLoginTime = System.DateTime.Now;
            Save();
        }
    }
}
