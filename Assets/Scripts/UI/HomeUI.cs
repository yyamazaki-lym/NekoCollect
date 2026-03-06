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

            // ポップアップに背景を追加（なければ）
            if (offlineCoinPopup != null)
            {
                // CanvasRendererがなければ追加（Image表示に必要）
                if (offlineCoinPopup.GetComponent<CanvasRenderer>() == null)
                    offlineCoinPopup.AddComponent<CanvasRenderer>();

                var bg = offlineCoinPopup.GetComponent<Image>();
                if (bg == null)
                {
                    bg = offlineCoinPopup.AddComponent<Image>();
                    bg.color = new Color(0f, 0f, 0f, 0.85f);
                }
                offlineCoinPopup.SetActive(false);
            }
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
                offlineCoinText.text = $"おるすばんボーナス: +{amount:N0} コイン";

                // 子オブジェクトも確実にアクティブにする
                if (offlineCoinText != null)
                    offlineCoinText.gameObject.SetActive(true);
                if (offlineCloseButton != null)
                    offlineCloseButton.gameObject.SetActive(true);

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
