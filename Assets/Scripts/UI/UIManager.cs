using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using NekoCollect.Util;

namespace NekoCollect.UI
{
    /// <summary>
    /// 画面遷移を管理するUIマネージャー（フェードアニメーション付き）
    /// </summary>
    public class UIManager : SingletonMonoBehaviour<UIManager>
    {
        [Header("各画面のルートオブジェクト")]
        [SerializeField] private GameObject homePanel;
        [SerializeField] private GameObject gachaPanel;
        [SerializeField] private GameObject catListPanel;
        [SerializeField] private GameObject catDetailPanel;
        [SerializeField] private GameObject catalogPanel;
        [SerializeField] private GameObject achievementPanel;

        [Header("遷移設定")]
        [SerializeField] private float fadeDuration = 0.2f;

        private GameObject currentPanel;
        private bool isTransitioning;

        // フェード用オーバーレイ
        private Image fadeOverlay;

        private void Start()
        {
            // フェードオーバーレイを作成
            SetupFadeOverlay();

            // 各パネルにCanvasGroupを追加
            EnsureCanvasGroup(homePanel);
            EnsureCanvasGroup(gachaPanel);
            EnsureCanvasGroup(catListPanel);
            EnsureCanvasGroup(catDetailPanel);
            EnsureCanvasGroup(catalogPanel);
            EnsureCanvasGroup(achievementPanel);

            // 全パネルを非表示にしてからホームを表示
            HideAll();
            ShowHome();
        }

        /// <summary>
        /// フェード用の全画面オーバーレイを生成
        /// </summary>
        private void SetupFadeOverlay()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            var fadeObj = new GameObject("FadeOverlay");
            fadeObj.transform.SetParent(canvas.transform, false);

            var rt = fadeObj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;

            if (fadeObj.GetComponent<CanvasRenderer>() == null)
                fadeObj.AddComponent<CanvasRenderer>();
            fadeOverlay = fadeObj.AddComponent<Image>();
            fadeOverlay.color = new Color(0, 0, 0, 0);
            fadeOverlay.raycastTarget = false;

            // 最前面に配置
            fadeObj.transform.SetAsLastSibling();
            fadeObj.SetActive(false);
        }

        /// <summary>
        /// CanvasGroupが無ければ追加
        /// </summary>
        private void EnsureCanvasGroup(GameObject panel)
        {
            if (panel == null) return;
            if (panel.GetComponent<CanvasGroup>() == null)
                panel.AddComponent<CanvasGroup>();
        }

        public void ShowHome()
        {
            SwitchTo(homePanel);
        }

        public void ShowGacha()
        {
            SwitchTo(gachaPanel);
        }

        public void ShowCatList()
        {
            SwitchTo(catListPanel);
        }

        public void ShowCatDetail()
        {
            SwitchTo(catDetailPanel);
        }

        public void ShowCatalog()
        {
            SwitchTo(catalogPanel);
        }

        public void ShowAchievements()
        {
            SwitchTo(achievementPanel);
        }

        private void SwitchTo(GameObject panel)
        {
            if (isTransitioning) return;
            if (panel == currentPanel) return;

            // フェードオーバーレイが使えない場合は即座に切り替え
            if (fadeOverlay == null || fadeDuration <= 0f || currentPanel == null)
            {
                InstantSwitch(panel);
                return;
            }

            StartCoroutine(FadeTransition(panel));
        }

        /// <summary>
        /// 即座に画面切り替え（初回用）
        /// </summary>
        private void InstantSwitch(GameObject panel)
        {
            if (currentPanel != null)
                currentPanel.SetActive(false);
            currentPanel = panel;
            if (currentPanel != null)
            {
                currentPanel.SetActive(true);
                // CanvasGroupのアルファを1に
                var cg = currentPanel.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;
            }
        }

        /// <summary>
        /// フェード遷移コルーチン
        /// </summary>
        private IEnumerator FadeTransition(GameObject nextPanel)
        {
            isTransitioning = true;

            // フェードアウト（現在パネル → 暗転）
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.transform.SetAsLastSibling();
            fadeOverlay.raycastTarget = true;

            float elapsed = 0f;
            float halfDuration = fadeDuration * 0.5f;

            // 暗転
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                fadeOverlay.color = new Color(0, 0, 0, t);
                yield return null;
            }
            fadeOverlay.color = new Color(0, 0, 0, 1);

            // パネル切り替え
            if (currentPanel != null)
                currentPanel.SetActive(false);
            currentPanel = nextPanel;
            if (currentPanel != null)
            {
                currentPanel.SetActive(true);
                var cg = currentPanel.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;
            }

            // フェードイン（暗転 → 明転）
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                fadeOverlay.color = new Color(0, 0, 0, 1f - t);
                yield return null;
            }

            fadeOverlay.color = new Color(0, 0, 0, 0);
            fadeOverlay.raycastTarget = false;
            fadeOverlay.gameObject.SetActive(false);
            isTransitioning = false;
        }

        private void HideAll()
        {
            homePanel?.SetActive(false);
            gachaPanel?.SetActive(false);
            catListPanel?.SetActive(false);
            catDetailPanel?.SetActive(false);
            catalogPanel?.SetActive(false);
            achievementPanel?.SetActive(false);
        }
    }
}
