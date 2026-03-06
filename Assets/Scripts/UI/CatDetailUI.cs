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

        /// <summary>
        /// 表示する猫を設定（画面遷移前に呼ぶ）
        /// </summary>
        public static void SetTarget(OwnedCatData cat)
        {
            targetCat = cat;
        }

        private void OnEnable()
        {
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

        private void Start()
        {
            feedCheapButton.onClick.AddListener(OnFeedCheap);
            feedPremiumButton.onClick.AddListener(OnFeedPremium);
            evolveButton.onClick.AddListener(OnEvolve);
            backButton.onClick.AddListener(() => UIManager.Instance.ShowCatList());

            feedCheapCostText.text = $"{CatCollectionManager.Instance.GetCheapFoodCost()} コイン";
            feedPremiumCostText.text = $"{CatCollectionManager.Instance.GetPremiumFoodCost()} コイン";
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
                    text.text = $"{skill.skillName} {status}\n{skill.description}";
                    text.color = learned ? Color.white : Color.gray;
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
