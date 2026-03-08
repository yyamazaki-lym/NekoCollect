using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NekoCollect.Manager;

namespace NekoCollect.UI
{
    /// <summary>
    /// 実績画面のUI制御
    /// </summary>
    public class AchievementUI : MonoBehaviour
    {
        [Header("UI参照")]
        [SerializeField] private Transform achievementListContainer;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Button backButton;

        private bool initialized;

        private void OnEnable()
        {
            if (!initialized)
            {
                initialized = true;

                // パネル背景を追加
                SetupPanelBackground();

                // リストコンテナにレイアウトコンポーネントを確保
                EnsureListLayout();

                if (backButton != null)
                {
                    backButton.onClick.AddListener(() =>
                    {
                        AudioManager.Instance?.PlayTap();
                        UIManager.Instance.ShowHome();
                    });
                }
            }
            RefreshList();
        }

        /// <summary>
        /// パネルに背景画像を設定
        /// </summary>
        private void SetupPanelBackground()
        {
            var bg = GetComponent<Image>();
            if (bg == null)
            {
                if (GetComponent<CanvasRenderer>() == null)
                    gameObject.AddComponent<CanvasRenderer>();
                bg = gameObject.AddComponent<Image>();
            }
            // 実績画面は図鑑と同じ背景を流用
            var sprite = Resources.Load<Sprite>("Backgrounds/bg_catalog");
            if (sprite != null)
            {
                bg.sprite = sprite;
                bg.type = Image.Type.Simple;
                bg.preserveAspect = false;
                bg.color = Color.white;
            }
            else
            {
                bg.color = new Color(0.10f, 0.08f, 0.18f, 1f);
            }
            bg.raycastTarget = true;
        }

        /// <summary>
        /// リストコンテナにVerticalLayoutGroupとContentSizeFitterを確保
        /// </summary>
        private void EnsureListLayout()
        {
            if (achievementListContainer == null) return;

            var containerObj = achievementListContainer.gameObject;

            // VerticalLayoutGroupがなければ追加
            var vlg = containerObj.GetComponent<VerticalLayoutGroup>();
            if (vlg == null)
            {
                vlg = containerObj.AddComponent<VerticalLayoutGroup>();
            }
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.spacing = 10f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // ContentSizeFitterがなければ追加（スクロール用）
            var csf = containerObj.GetComponent<ContentSizeFitter>();
            if (csf == null)
            {
                csf = containerObj.AddComponent<ContentSizeFitter>();
            }
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void RefreshList()
        {
            if (achievementListContainer == null) return;

            // AchievementManagerが初期化されていない場合はスキップ
            if (AchievementManager.Instance == null) return;

            // 既存のアイテムをクリア
            foreach (Transform child in achievementListContainer)
                Destroy(child.gameObject);

            var achievements = AchievementManager.Instance.GetAllAchievements();
            if (achievements == null) return;

            int unlocked = 0;

            foreach (var ach in achievements)
            {
                bool isUnlocked = AchievementManager.Instance.IsUnlocked(ach.id);
                if (isUnlocked) unlocked++;

                // プログラムでアイテムを生成（Prefab不要）
                var item = CreateAchievementItem(achievementListContainer);
                SetupAchievementItem(item, ach, isUnlocked);
            }

            // 進捗表示
            if (progressText != null && achievements.Count > 0)
            {
                progressText.text = $"クリア: {unlocked}／{achievements.Count}（{100f * unlocked / achievements.Count:F0}％）";
            }
        }

        /// <summary>
        /// 実績アイテムをプログラムで生成
        /// </summary>
        private GameObject CreateAchievementItem(Transform parent)
        {
            var item = new GameObject("AchievementItem");
            item.transform.SetParent(parent, false);

            // RectTransform設定
            var rt = item.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, 80f);

            // 背景
            if (item.GetComponent<CanvasRenderer>() == null)
                item.AddComponent<CanvasRenderer>();
            var bg = item.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f, 0.5f);
            bg.raycastTarget = false;

            // タイトルテキスト
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(item.transform, false);
            var titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.03f, 0.5f);
            titleRt.anchorMax = new Vector2(0.97f, 0.95f);
            titleRt.sizeDelta = Vector2.zero;
            titleRt.anchoredPosition = Vector2.zero;
            var titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "";
            titleTmp.alignment = TextAlignmentOptions.Left;
            titleTmp.enableAutoSizing = true;
            titleTmp.fontSizeMin = 12;
            titleTmp.fontSizeMax = 24;

            // 説明テキスト
            var descObj = new GameObject("Description");
            descObj.transform.SetParent(item.transform, false);
            var descRt = descObj.AddComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0.03f, 0.05f);
            descRt.anchorMax = new Vector2(0.97f, 0.5f);
            descRt.sizeDelta = Vector2.zero;
            descRt.anchoredPosition = Vector2.zero;
            var descTmp = descObj.AddComponent<TextMeshProUGUI>();
            descTmp.text = "";
            descTmp.alignment = TextAlignmentOptions.Left;
            descTmp.enableAutoSizing = true;
            descTmp.fontSizeMin = 10;
            descTmp.fontSizeMax = 18;

            return item;
        }

        private void SetupAchievementItem(GameObject item, AchievementDef ach, bool unlocked)
        {
            var texts = item.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0)
            {
                texts[0].text = unlocked ? ach.title : "？？？";
                texts[0].color = unlocked ? new Color(1f, 0.9f, 0.3f) : Color.gray;
            }
            if (texts.Length > 1)
            {
                texts[1].text = unlocked ? $"{ach.description}（+{ach.coinReward}コイン）" : "???";
                texts[1].color = unlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            }

            // 解除済みは背景を明るく
            var bg = item.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = unlocked
                    ? new Color(0.2f, 0.2f, 0.3f, 0.8f)
                    : new Color(0.1f, 0.1f, 0.15f, 0.5f);
            }
        }
    }
}
