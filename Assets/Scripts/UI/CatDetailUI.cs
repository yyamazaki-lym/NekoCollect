using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NekoCollect.Data;
using NekoCollect.Manager;
using NekoCollect.Save;

namespace NekoCollect.UI
{
    /// <summary>
    /// 猫詳細/育成画面のUI制御
    /// </summary>
    public class CatDetailUI : MonoBehaviour
    {
        [Header("猫情報")]
        [SerializeField] private Image catImage;
        [SerializeField] private TextMeshProUGUI catNameText;
        [SerializeField] private TextMeshProUGUI rarityText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Slider expBar;
        [SerializeField] private TextMeshProUGUI expText;

        [Header("ステータス")]
        [SerializeField] private TextMeshProUGUI friendlinessText;
        [SerializeField] private TextMeshProUGUI energyText;
        [SerializeField] private TextMeshProUGUI cutenessText;

        [Header("スキル")]
        [SerializeField] private Transform skillListContainer;
        [SerializeField] private GameObject skillItemPrefab;

        [Header("育成ボタン")]
        [SerializeField] private Button feedCheapButton;
        [SerializeField] private TextMeshProUGUI feedCheapCostText;
        [SerializeField] private Button feedPremiumButton;
        [SerializeField] private TextMeshProUGUI feedPremiumCostText;

        [Header("進化")]
        [SerializeField] private Button evolveButton;
        [SerializeField] private TextMeshProUGUI evolveInfoText;

        [Header("ナビゲーション")]
        [SerializeField] private Button backButton;

        private static OwnedCatData targetCat;
        private bool initialized;

        /// <summary>
        /// 表示する猫を設定（画面遷移前に呼ぶ）
        /// </summary>
        public static void SetTarget(OwnedCatData cat)
        {
            targetCat = cat;
        }

        private void OnEnable()
        {
            if (!initialized)
            {
                initialized = true;

                // パネルに背景を追加
                SetupPanelBackground();

                // レイアウト修正
                AdjustLayout();

                feedCheapButton.onClick.AddListener(OnFeedCheap);
                feedPremiumButton.onClick.AddListener(OnFeedPremium);
                evolveButton.onClick.AddListener(OnEvolve);
                backButton.onClick.AddListener(() => UIManager.Instance.ShowCatList());
                feedCheapCostText.text = $"{CatCollectionManager.Instance.GetCheapFoodCost()} コイン";
                feedPremiumCostText.text = $"{CatCollectionManager.Instance.GetPremiumFoodCost()} コイン";
            }

            if (targetCat != null)
                RefreshDisplay();

            CatCollectionManager.Instance.OnCatLevelUp += OnLevelUp;
            CatCollectionManager.Instance.OnCatEvolved += OnEvolved;
        }

        private void OnDisable()
        {
            if (CatCollectionManager.Instance != null)
            {
                CatCollectionManager.Instance.OnCatLevelUp -= OnLevelUp;
                CatCollectionManager.Instance.OnCatEvolved -= OnEvolved;
            }
        }

        /// <summary>
        /// パネルに背景色を追加
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
            bg.color = new Color(0.12f, 0.12f, 0.18f, 1f);
            bg.raycastTarget = true;
        }

        /// <summary>
        /// UI要素の位置を再配置（重なり防止・十分な余白確保）
        /// </summary>
        private void AdjustLayout()
        {
            // 猫画像を上部に（小さめにして下のスペースを確保）
            SetRect(catImage, 0.30f, 0.82f, 0.70f, 0.97f);
            catImage.preserveAspect = true;

            // 名前（大きめ）
            SetRect(catNameText, 0.05f, 0.74f, 0.95f, 0.82f);
            SetAutoSizeWithMarginReset(catNameText, 18, 40);

            // レアリティ（左半分）とレベル（右半分）を同じ行に
            SetRect(rarityText, 0.05f, 0.67f, 0.48f, 0.74f);
            SetAutoSizeWithMarginReset(rarityText, 12, 28);

            SetRect(levelText, 0.52f, 0.67f, 0.95f, 0.74f);
            SetAutoSizeWithMarginReset(levelText, 12, 28);

            // 経験値バー
            SetRect(expBar, 0.1f, 0.63f, 0.9f, 0.67f);

            // 経験値テキスト
            SetRect(expText, 0.2f, 0.58f, 0.8f, 0.63f);
            SetAutoSizeWithMarginReset(expText, 12, 24);

            // ステータス3種（各8%の高さで十分な余白を確保）
            SetRect(friendlinessText, 0.05f, 0.49f, 0.95f, 0.57f);
            SetAutoSizeWithMarginReset(friendlinessText, 12, 28);

            SetRect(energyText, 0.05f, 0.41f, 0.95f, 0.49f);
            SetAutoSizeWithMarginReset(energyText, 12, 28);

            SetRect(cutenessText, 0.05f, 0.33f, 0.95f, 0.41f);
            SetAutoSizeWithMarginReset(cutenessText, 12, 28);

            // スキルリスト
            SetRect(skillListContainer, 0.05f, 0.22f, 0.95f, 0.32f);

            // 餌ボタン2つ（横並び）
            SetRect(feedCheapButton, 0.05f, 0.13f, 0.48f, 0.21f);
            SetRect(feedPremiumButton, 0.52f, 0.13f, 0.95f, 0.21f);

            // 進化ボタン
            SetRect(evolveButton, 0.15f, 0.06f, 0.85f, 0.12f);

            // 戻るボタン
            SetRect(backButton, 0.3f, 0.01f, 0.7f, 0.055f);

            // 餌ボタンのテキストをAutoSizeに
            SetAutoSize(feedCheapCostText, 12, 24);
            SetAutoSize(feedPremiumCostText, 12, 24);
            if (evolveInfoText != null)
                SetAutoSize(evolveInfoText, 12, 24);
        }

        /// <summary>
        /// RectTransformをアンカー比率で設定するヘルパー
        /// </summary>
        private void SetRect(Component comp, float xMin, float yMin, float xMax, float yMax)
        {
            if (comp == null) return;
            var rt = comp.GetComponent<RectTransform>();
            if (rt == null) return;
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
        }

        /// <summary>
        /// AutoSizeを設定し、TMPのマージンをリセットして余計なパディングを除去
        /// </summary>
        private void SetAutoSizeWithMarginReset(TextMeshProUGUI text, float min, float max)
        {
            if (text == null) return;
            text.enableAutoSizing = true;
            text.fontSizeMin = min;
            text.fontSizeMax = max;
            text.alignment = TextAlignmentOptions.Center;
            // TMPのマージンをゼロにしてテキスト表示領域を最大化
            text.margin = Vector4.zero;
            // 余分なパディングを無効化
            text.extraPadding = false;
        }

        private void SetAutoSize(TextMeshProUGUI text, float min, float max)
        {
            if (text == null) return;
            text.enableAutoSizing = true;
            text.fontSizeMin = min;
            text.fontSizeMax = max;
            text.alignment = TextAlignmentOptions.Center;
        }

        private void RefreshDisplay()
        {
            if (targetCat == null) return;

            var catData = CatCollectionManager.Instance.GetCatData(targetCat.catId);
            if (catData == null) return;

            // 基本情報
            catImage.sprite = catData.sprite;
            catNameText.text = catData.catName;
            rarityText.text = catData.rarity.ToString();
            levelText.text = $"Lv. {targetCat.level} / {catData.maxLevel}";

            // 経験値バー
            if (targetCat.level < catData.maxLevel)
            {
                int needed = catData.GetExpForLevel(targetCat.level + 1);
                expBar.value = (float)targetCat.currentExp / needed;
                expText.text = $"{targetCat.currentExp} / {needed}";
            }
            else
            {
                expBar.value = 1f;
                expText.text = "MAX";
            }

            // ステータス
            var stats = catData.GetStatsAtLevel(targetCat.level);
            friendlinessText.text = $"なつき度: {stats.friendliness:F1}";
            energyText.text = $"元気: {stats.energy:F1}";
            cutenessText.text = $"かわいさ: {stats.cuteness:F1}";

            // スキル表示
            RefreshSkills(catData);

            // 進化ボタン
            UpdateEvolveButton(catData);

            // レベルMAXならエサボタンを無効化
            bool isMaxLevel = targetCat.level >= catData.maxLevel;
            feedCheapButton.interactable = !isMaxLevel;
            feedPremiumButton.interactable = !isMaxLevel;
        }

        private void RefreshSkills(CatData catData)
        {
            foreach (Transform child in skillListContainer)
                Destroy(child.gameObject);

            for (int i = 0; i < catData.skills.Length; i++)
            {
                var skill = catData.skills[i];
                var item = Instantiate(skillItemPrefab, skillListContainer);
                var text = item.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    bool learned = targetCat.learnedSkillIndices.Contains(i);
                    string status = learned ? "" : $"(Lv.{skill.requiredLevel}で習得)";
                    text.text = $"{skill.skillName} {status}";
                    text.color = learned ? Color.white : Color.gray;
                    text.enableAutoSizing = true;
                    text.fontSizeMin = 10;
                    text.fontSizeMax = 22;
                }
            }
        }

        private void UpdateEvolveButton(CatData catData)
        {
            if (catData.evolutionTarget != null)
            {
                evolveButton.gameObject.SetActive(true);
                bool canEvolve = CatCollectionManager.Instance.CanEvolve(targetCat);
                evolveButton.interactable = canEvolve;
                evolveInfoText.text = canEvolve
                    ? $"→ {catData.evolutionTarget.catName} に進化！"
                    : $"Lv.{catData.evolutionLevel}で進化可能";
            }
            else
            {
                evolveButton.gameObject.SetActive(false);
            }
        }

        private void OnFeedCheap()
        {
            CatCollectionManager.Instance.FeedCheap(targetCat);
            RefreshDisplay();
        }

        private void OnFeedPremium()
        {
            CatCollectionManager.Instance.FeedPremium(targetCat);
            RefreshDisplay();
        }

        private void OnEvolve()
        {
            CatCollectionManager.Instance.Evolve(targetCat);
        }

        private void OnLevelUp(OwnedCatData cat, CatData catData)
        {
            if (cat == targetCat)
                RefreshDisplay();
        }

        private void OnEvolved(CatData oldCat, CatData newCat)
        {
            RefreshDisplay();
        }
    }
}
