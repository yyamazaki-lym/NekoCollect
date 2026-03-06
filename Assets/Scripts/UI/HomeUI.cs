using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NekoCollect.Manager;

namespace NekoCollect.UI
{
    /// <summary>
    /// ホーム画面のUI制御
    /// </summary>
    public class HomeUI : MonoBehaviour
    {
        [Header("UI参照")]
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private TextMeshProUGUI idleRateText;
        [SerializeField] private Button clickButton;
        [SerializeField] private Button gachaButton;
        [SerializeField] private Button catListButton;
        [SerializeField] private Button catalogButton;

        [Header("放置コイン通知")]
        [SerializeField] private GameObject offlineCoinPopup;
        [SerializeField] private TextMeshProUGUI offlineCoinText;
        [SerializeField] private Button offlineCloseButton;

        private void OnEnable()
        {
            CoinManager.Instance.OnCoinsChanged += UpdateCoinDisplay;
            CoinManager.Instance.OnIdleCoinsCollected += ShowOfflineCoins;
            UpdateCoinDisplay(CoinManager.Instance.Coins);
        }

        private void OnDisable()
        {
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.OnCoinsChanged -= UpdateCoinDisplay;
                CoinManager.Instance.OnIdleCoinsCollected -= ShowOfflineCoins;
            }
        }

        private void Start()
        {
            clickButton.onClick.AddListener(OnClickCoin);
            gachaButton.onClick.AddListener(() => UIManager.Instance.ShowGacha());
            catListButton.onClick.AddListener(() => UIManager.Instance.ShowCatList());
            catalogButton.onClick.AddListener(() => UIManager.Instance.ShowCatalog());
            offlineCloseButton?.onClick.AddListener(CloseOfflinePopup);

            // ポップアップのセットアップ
            SetupOfflinePopup();
        }

        private void SetupOfflinePopup()
        {
            if (offlineCoinPopup == null) return;

            // ストレッチアンカーで全画面オーバーレイにする
            var rt = offlineCoinPopup.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            // 背景Imageでレイキャストをブロック（後ろの要素を操作不可に）
            var bg = offlineCoinPopup.GetComponent<Image>();
            if (bg == null)
            {
                if (offlineCoinPopup.GetComponent<CanvasRenderer>() == null)
                    offlineCoinPopup.AddComponent<CanvasRenderer>();
                bg = offlineCoinPopup.AddComponent<Image>();
            }
            bg.color = new Color(0f, 0f, 0f, 0.85f);
            bg.raycastTarget = true;

            // テキストの位置を中央上に
            if (offlineCoinText != null)
            {
                var textRt = offlineCoinText.GetComponent<RectTransform>();
                textRt.anchorMin = new Vector2(0.1f, 0.45f);
                textRt.anchorMax = new Vector2(0.9f, 0.65f);
                textRt.anchoredPosition = Vector2.zero;
                textRt.sizeDelta = Vector2.zero;
                offlineCoinText.alignment = TextAlignmentOptions.Center;
                offlineCoinText.enableAutoSizing = true;
                offlineCoinText.fontSizeMin = 18;
                offlineCoinText.fontSizeMax = 60;
            }

            // OKボタンの位置とサイズを調整
            if (offlineCloseButton != null)
            {
                var btnRt = offlineCloseButton.GetComponent<RectTransform>();
                btnRt.anchorMin = new Vector2(0.3f, 0.25f);
                btnRt.anchorMax = new Vector2(0.7f, 0.35f);
                btnRt.anchoredPosition = Vector2.zero;
                btnRt.sizeDelta = Vector2.zero;

                // ボタンのテキストを大きく
                var btnText = offlineCloseButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.enableAutoSizing = true;
                    btnText.fontSizeMin = 18;
                    btnText.fontSizeMax = 48;
                }
            }

            offlineCoinPopup.SetActive(false);
        }

        private void OnClickCoin()
        {
            CoinManager.Instance.ClickCoin();
        }

        private void UpdateCoinDisplay(long coins)
        {
            coinText.text = $"コイン: {coins:N0}";
        }

        private void ShowOfflineCoins(long amount)
        {
            if (offlineCoinPopup != null && amount > 0)
            {
                offlineCoinText.text = $"おるすばんボーナス\n+{amount:N0} コイン";

                // 子オブジェクトも確実にアクティブにする
                if (offlineCoinText != null)
                    offlineCoinText.gameObject.SetActive(true);
                if (offlineCloseButton != null)
                    offlineCloseButton.gameObject.SetActive(true);

                // 最前面に表示
                offlineCoinPopup.transform.SetAsLastSibling();
                offlineCoinPopup.SetActive(true);
            }
        }

        /// <summary>
        /// 放置コイン通知を閉じる（ボタンから呼ぶ）
        /// </summary>
        public void CloseOfflinePopup()
        {
            if (offlineCoinPopup != null)
                offlineCoinPopup.SetActive(false);
        }
    }
}
