using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NekoCollect.Data;
using NekoCollect.Manager;

namespace NekoCollect.UI
{
    /// <summary>
    /// ガチャ画面のUI制御
    /// </summary>
    public class GachaUI : MonoBehaviour
    {
        [Header("バナー選択")]
        [SerializeField] private Transform bannerButtonContainer;
        [SerializeField] private Button bannerButtonPrefab;

        [Header("ガチャ実行")]
        [SerializeField] private TextMeshProUGUI selectedBannerName;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button pullButton;
        [SerializeField] private Button backButton;

        [Header("結果表示")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private Image resultCatImage;
        [SerializeField] private TextMeshProUGUI resultCatName;
        [SerializeField] private TextMeshProUGUI resultRarity;
        [SerializeField] private TextMeshProUGUI resultNewLabel;
        [SerializeField] private Button resultCloseButton;

        private GachaBanner selectedBanner;
        private bool initialized;

        private void OnEnable()
        {
            if (!initialized)
            {
                initialized = true;

                // パネル背景を追加
                SetupPanelBackground();

                // ガチャ結果パネルのセットアップ
                SetupResultPanel();

                pullButton.onClick.AddListener(OnPull);
                backButton.onClick.AddListener(() => UIManager.Instance.ShowHome());
                resultCloseButton?.onClick.AddListener(() => resultPanel?.SetActive(false));
            }
            GachaManager.Instance.OnGachaResult += ShowResult;
            SetupBannerButtons();
            resultPanel?.SetActive(false);
        }

        private void OnDisable()
        {
            if (GachaManager.Instance != null)
                GachaManager.Instance.OnGachaResult -= ShowResult;
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
        /// ガチャ結果パネルを全画面オーバーレイとして設定
        /// </summary>
        private void SetupResultPanel()
        {
            if (resultPanel == null) return;

            var rt = resultPanel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            var bg = resultPanel.GetComponent<Image>();
            if (bg == null)
            {
                if (resultPanel.GetComponent<CanvasRenderer>() == null)
                    resultPanel.AddComponent<CanvasRenderer>();
                bg = resultPanel.AddComponent<Image>();
            }
            bg.color = new Color(0.05f, 0.05f, 0.12f, 0.95f);
            bg.raycastTarget = true;
        }

        private void SetupBannerButtons()
        {
            // 既存のボタンをクリア
            foreach (Transform child in bannerButtonContainer)
                Destroy(child.gameObject);

            var banners = GachaManager.Instance.GetBanners();
            foreach (var banner in banners)
            {
                var btn = Instantiate(bannerButtonPrefab, bannerButtonContainer);
                var text = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null) text.text = $"{banner.bannerName}\n({banner.cost} コイン)";

                var captured = banner;
                btn.onClick.AddListener(() => SelectBanner(captured));
            }

            // 最初のバナーを選択
            if (banners.Length > 0)
                SelectBanner(banners[0]);
        }

        private void SelectBanner(GachaBanner banner)
        {
            selectedBanner = banner;
            selectedBannerName.text = banner.bannerName;
            costText.text = $"{banner.cost} コイン";
        }

        private void OnPull()
        {
            if (selectedBanner == null) return;
            if (!GachaManager.Instance.Pull(selectedBanner))
            {
                // コイン不足の場合
                costText.text = "コインが足りません！";
            }
        }

        private void ShowResult(CatData cat, bool isNew)
        {
            if (resultPanel == null) return;

            resultCatImage.sprite = cat.sprite;
            resultCatName.text = cat.catName;
            resultRarity.text = cat.rarity.ToString();

            // レアリティに応じた色
            resultRarity.color = cat.rarity switch
            {
                Rarity.SSR => Color.yellow,
                Rarity.SR => new Color(0.8f, 0.5f, 1f),
                Rarity.R => new Color(0.3f, 0.7f, 1f),
                _ => Color.white
            };

            resultNewLabel.text = isNew ? "NEW!" : "経験値ボーナス!";
            resultNewLabel.gameObject.SetActive(true);

            // 最前面に表示
            resultPanel.transform.SetAsLastSibling();
            resultPanel.SetActive(true);
        }
    }
}
