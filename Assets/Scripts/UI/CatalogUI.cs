using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NekoCollect.Data;
using NekoCollect.Manager;

namespace NekoCollect.UI
{
    /// <summary>
    /// 図鑑画面のUI制御
    /// </summary>
    public class CatalogUI : MonoBehaviour
    {
        [Header("UI参照")]
        [SerializeField] private Transform catalogGridContainer;
        [SerializeField] private GameObject catalogCardPrefab;
        [SerializeField] private TextMeshProUGUI completionText;
        [SerializeField] private Button backButton;

        [Header("詳細ポップアップ")]
        [SerializeField] private GameObject detailPopup;
        [SerializeField] private Image detailImage;
        [SerializeField] private TextMeshProUGUI detailName;
        [SerializeField] private TextMeshProUGUI detailRarity;
        [SerializeField] private TextMeshProUGUI detailDescription;
        [SerializeField] private Button detailCloseButton;

        private bool initialized;

        private void OnEnable()
        {
            if (!initialized)
            {
                initialized = true;
                backButton.onClick.AddListener(() => UIManager.Instance.ShowHome());
                detailCloseButton?.onClick.AddListener(() => detailPopup?.SetActive(false));
            }
            RefreshCatalog();
            detailPopup?.SetActive(false);
        }

        private void RefreshCatalog()
        {
            // 既存のカードをクリア
            foreach (Transform child in catalogGridContainer)
                Destroy(child.gameObject);

            var allCats = CatCollectionManager.Instance.GetAllCats();
            int ownedCount = 0;

            foreach (var catData in allCats)
            {
                bool owned = CatCollectionManager.Instance.HasCat(catData.catId);
                if (owned) ownedCount++;

                var card = Instantiate(catalogCardPrefab, catalogGridContainer);
                SetupCatalogCard(card, catData, owned);
            }

            // コンプリート率表示
            completionText.text = $"図鑑: {ownedCount} / {allCats.Length} ({100f * ownedCount / allCats.Length:F0}%)";
        }

        private void SetupCatalogCard(GameObject card, CatData catData, bool owned)
        {
            var image = card.GetComponentInChildren<Image>();
            var text = card.GetComponentInChildren<TextMeshProUGUI>();

            if (owned)
            {
                if (image != null) image.sprite = catData.sprite;
                if (text != null) text.text = catData.catName;

                // タップで詳細ポップアップ
                var button = card.GetComponent<Button>();
                if (button == null) button = card.AddComponent<Button>();
                var captured = catData;
                button.onClick.AddListener(() => ShowDetail(captured));
            }
            else
            {
                // 未取得: シルエット表示
                if (image != null)
                {
                    image.sprite = catData.sprite;
                    image.color = Color.black;
                }
                if (text != null) text.text = "？？？";
            }
        }

        private void ShowDetail(CatData catData)
        {
            detailPopup?.SetActive(true);
            detailImage.sprite = catData.sprite;
            detailName.text = catData.catName;
            detailRarity.text = catData.rarity.ToString();
            detailDescription.text = catData.description;
        }
    }
}
