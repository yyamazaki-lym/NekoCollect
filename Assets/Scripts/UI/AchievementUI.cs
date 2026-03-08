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
        [SerializeField] private GameObject achievementItemPrefab;
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

                backButton.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayTap();
                    UIManager.Instance.ShowHome();
                });
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

        private void RefreshList()
        {
            if (achievementListContainer == null) return;

            // 既存のアイテムをクリア
            foreach (Transform child in achievementListContainer)
                Destroy(child.gameObject);

            var achievements = AchievementManager.Instance.GetAllAchievements();
            int unlocked = 0;

            foreach (var ach in achievements)
            {
                bool isUnlocked = AchievementManager.Instance.IsUnlocked(ach.id);
                if (isUnlocked) unlocked++;

                var item = Instantiate(achievementItemPrefab, achievementListContainer);
                SetupAchievementItem(item, ach, isUnlocked);
            }

            // 進捗表示
            if (progressText != null)
            {
                progressText.text = $"実績: {unlocked} / {achievements.Count} ({100f * unlocked / achievements.Count:F0}%)";
            }
        }

        private void SetupAchievementItem(GameObject item, AchievementDef ach, bool unlocked)
        {
            var texts = item.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0)
            {
                texts[0].text = unlocked ? ach.title : "？？？";
                texts[0].enableAutoSizing = true;
                texts[0].fontSizeMin = 12;
                texts[0].fontSizeMax = 24;
                texts[0].alignment = TextAlignmentOptions.Left;
                texts[0].color = unlocked ? new Color(1f, 0.9f, 0.3f) : Color.gray;
            }
            if (texts.Length > 1)
            {
                texts[1].text = unlocked ? $"{ach.description} (+{ach.coinReward}コイン)" : "???";
                texts[1].enableAutoSizing = true;
                texts[1].fontSizeMin = 10;
                texts[1].fontSizeMax = 18;
                texts[1].alignment = TextAlignmentOptions.Left;
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
