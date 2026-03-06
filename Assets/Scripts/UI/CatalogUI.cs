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

                // パネル背景を追加
                SetupPanelBackground();

                // ポップアップの見た目を修正
                SetupDetailPopup();

                backButton.onClick.AddListener(() => UIManager.Instance.ShowHome());
                detailCloseButton?.onClick.AddListener(() => detailPopup?.SetActive(false));
            }
            RefreshCatalog();
            detailPopup?.SetActive(false);
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
        /// 詳細ポップアップをフルスクリーンオーバーレイとして設定
        /// </summary>
        private void SetupDetailPopup()
        {
            if (detailPopup == null) return;

            // ストレッチアンカーで全画面オーバーレイにする
            var rt = detailPopup.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            // 背景Imageを追加
            var bg = detailPopup.GetComponent<Image>();
            if (bg == null)
            {
                if (detailPopup.GetComponent<CanvasRenderer>() == null)
                    detailPopup.AddComponent<CanvasRenderer>();
                bg = detailPopup.AddComponent<Image>();
            }
            bg.color = new Color(0.08f, 0.08f, 0.15f, 0.95f);
            bg.raycastTarget = true;

            // 子要素の位置を調整
            SetRect(detailImage, 0.2f, 0.55f, 0.8f, 0.85f);
            if (detailImage != null) detailImage.preserveAspect = true;

            SetRect(detailName, 0.1f, 0.45f, 0.9f, 0.55f);
            if (detailName != null)
            {
                detailName.alignment = TextAlignmentOptions.Center;
                detailName.enableAutoSizing = true;
                detailName.fontSizeMin = 18;
                detailName.fontSizeMax = 48;
            }

            SetRect(detailRarity, 0.2f, 0.38f, 0.8f, 0.45f);
            if (detailRarity != null)
            {
                detailRarity.alignment = TextAlignmentOptions.Center;
                detailRarity.enableAutoSizing = true;
                detailRarity.fontSizeMin = 14;
                detailRarity.fontSizeMax = 36;
            }

            SetRect(detailDescription, 0.1f, 0.22f, 0.9f, 0.38f);
            if (detailDescription != null)
            {
                detailDescription.alignment = TextAlignmentOptions.Center;
                detailDescription.enableAutoSizing = true;
                detailDescription.fontSizeMin = 12;
                detailDescription.fontSizeMax = 28;
            }

            SetRect(detailCloseButton, 0.3f, 0.10f, 0.7f, 0.18f);
            var btnText = detailCloseButton?.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.enableAutoSizing = true;
                btnText.fontSizeMin = 16;
                btnText.fontSizeMax = 36;
            }
        }

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
            if (detailPopup == null) return;

            detailImage.sprite = catData.sprite;
            detailName.text = catData.catName;
            detailRarity.text = catData.rarity.ToString();
            detailDescription.text = catData.description;

            // 最前面に表示
            detailPopup.transform.SetAsLastSibling();
            detailPopup.SetActive(true);
        }
    }
}
