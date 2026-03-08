using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NekoCollect.Data;
using NekoCollect.Manager;

namespace NekoCollect.UI
{
    /// <summary>
    /// ガチャ画面のUI制御（演出アニメーション付き）
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

        // 演出用オブジェクト（コードで生成）
        private GameObject animOverlay;
        private Image capsuleImage;
        private Image flashImage;
        private Image[] sparkleImages;

        private GachaBanner selectedBanner;
        private bool initialized;
        private bool isAnimating;

        // ガチャ結果の一時保持
        private CatData pendingCat;
        private bool pendingIsNew;

        private void OnEnable()
        {
            if (!initialized)
            {
                initialized = true;

                // パネル背景を追加
                SetupPanelBackground();

                // ガチャ結果パネルのセットアップ
                SetupResultPanel();

                // ガチャ演出オーバーレイの作成
                SetupAnimationOverlay();

                pullButton.onClick.AddListener(OnPull);
                backButton.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayTap();
                    UIManager.Instance.ShowHome();
                });
                resultCloseButton?.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayTap();
                    resultPanel?.SetActive(false);
                });
            }
            GachaManager.Instance.OnGachaResult += OnGachaResult;
            SetupBannerButtons();
            resultPanel?.SetActive(false);
            animOverlay?.SetActive(false);
        }

        private void OnDisable()
        {
            if (GachaManager.Instance != null)
                GachaManager.Instance.OnGachaResult -= OnGachaResult;
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
            var sprite = Resources.Load<Sprite>("Backgrounds/bg_gacha");
            if (sprite != null)
            {
                bg.sprite = sprite;
                bg.type = Image.Type.Simple;
                bg.preserveAspect = false;
                bg.color = Color.white;
            }
            else
            {
                bg.color = new Color(0.12f, 0.12f, 0.18f, 1f);
            }
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

        /// <summary>
        /// ガチャ演出用オーバーレイを生成
        /// </summary>
        private void SetupAnimationOverlay()
        {
            // 全画面オーバーレイ
            animOverlay = new GameObject("GachaAnimOverlay");
            animOverlay.transform.SetParent(transform, false);
            var overlayRt = animOverlay.AddComponent<RectTransform>();
            overlayRt.anchorMin = Vector2.zero;
            overlayRt.anchorMax = Vector2.one;
            overlayRt.sizeDelta = Vector2.zero;
            overlayRt.anchoredPosition = Vector2.zero;

            // 背景（暗転）
            if (animOverlay.GetComponent<CanvasRenderer>() == null)
                animOverlay.AddComponent<CanvasRenderer>();
            var overlayBg = animOverlay.AddComponent<Image>();
            overlayBg.color = new Color(0f, 0f, 0f, 0.9f);
            overlayBg.raycastTarget = true;

            // カプセル画像
            var capsuleObj = new GameObject("Capsule");
            capsuleObj.transform.SetParent(animOverlay.transform, false);
            var capsuleRt = capsuleObj.AddComponent<RectTransform>();
            capsuleRt.anchorMin = new Vector2(0.3f, 0.35f);
            capsuleRt.anchorMax = new Vector2(0.7f, 0.65f);
            capsuleRt.sizeDelta = Vector2.zero;
            capsuleRt.anchoredPosition = Vector2.zero;
            if (capsuleObj.GetComponent<CanvasRenderer>() == null)
                capsuleObj.AddComponent<CanvasRenderer>();
            capsuleImage = capsuleObj.AddComponent<Image>();
            var capsuleSprite = Resources.Load<Sprite>("UI/gacha_capsule");
            if (capsuleSprite != null)
            {
                capsuleImage.sprite = capsuleSprite;
                capsuleImage.preserveAspect = true;
            }
            else
            {
                capsuleImage.color = new Color(0.9f, 0.3f, 0.3f);
            }

            // フラッシュエフェクト
            var flashObj = new GameObject("Flash");
            flashObj.transform.SetParent(animOverlay.transform, false);
            var flashRt = flashObj.AddComponent<RectTransform>();
            flashRt.anchorMin = Vector2.zero;
            flashRt.anchorMax = Vector2.one;
            flashRt.sizeDelta = Vector2.zero;
            flashRt.anchoredPosition = Vector2.zero;
            if (flashObj.GetComponent<CanvasRenderer>() == null)
                flashObj.AddComponent<CanvasRenderer>();
            flashImage = flashObj.AddComponent<Image>();
            flashImage.color = new Color(1f, 1f, 1f, 0f);
            flashImage.raycastTarget = false;

            // きらきらエフェクト（4つの星）
            sparkleImages = new Image[6];
            for (int i = 0; i < sparkleImages.Length; i++)
            {
                var sparkle = new GameObject($"Sparkle_{i}");
                sparkle.transform.SetParent(animOverlay.transform, false);
                var sRt = sparkle.AddComponent<RectTransform>();
                sRt.sizeDelta = new Vector2(30, 30);
                sRt.anchorMin = new Vector2(0.5f, 0.5f);
                sRt.anchorMax = new Vector2(0.5f, 0.5f);
                sRt.anchoredPosition = Vector2.zero;
                if (sparkle.GetComponent<CanvasRenderer>() == null)
                    sparkle.AddComponent<CanvasRenderer>();
                sparkleImages[i] = sparkle.AddComponent<Image>();
                sparkleImages[i].color = new Color(1f, 1f, 0.5f, 0f);
                sparkleImages[i].raycastTarget = false;
            }

            animOverlay.SetActive(false);
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
            if (selectedBanner == null || isAnimating) return;
            AudioManager.Instance?.PlayGachaRoll();
            if (!GachaManager.Instance.Pull(selectedBanner))
            {
                // コイン不足の場合
                costText.text = "コインが足りません！";
            }
        }

        /// <summary>
        /// ガチャ結果を受信（演出開始）
        /// </summary>
        private void OnGachaResult(CatData cat, bool isNew)
        {
            pendingCat = cat;
            pendingIsNew = isNew;
            StartCoroutine(PlayGachaAnimation());
        }

        /// <summary>
        /// ガチャ演出コルーチン
        /// </summary>
        private IEnumerator PlayGachaAnimation()
        {
            isAnimating = true;
            pullButton.interactable = false;

            // カプセルの色をレアリティに応じて設定
            Color rarityColor = UIHelper.GetRarityColor(pendingCat.rarity);
            bool isHighRarity = pendingCat.rarity >= Rarity.SR;

            // オーバーレイ表示
            animOverlay.SetActive(true);
            animOverlay.transform.SetAsLastSibling();

            // カプセル初期状態
            var capsuleRt = capsuleImage.GetComponent<RectTransform>();
            capsuleRt.localScale = Vector3.one;
            capsuleRt.localEulerAngles = Vector3.zero;
            capsuleImage.color = Color.white;
            flashImage.color = new Color(1f, 1f, 1f, 0f);
            foreach (var s in sparkleImages) s.color = new Color(1f, 1f, 0.5f, 0f);

            // フェーズ1: カプセル落下（上からバウンド）
            float dropDuration = 0.4f;
            float elapsed = 0f;
            Vector2 startPos = new Vector2(0, 300);
            Vector2 endPos = Vector2.zero;
            while (elapsed < dropDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dropDuration;
                // バウンス曲線
                float bounce = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 2f)) * (1f - t) * 0.3f;
                float y = Mathf.Lerp(startPos.y, endPos.y, t) + bounce * 100f;
                capsuleRt.anchoredPosition = new Vector2(0, y);
                yield return null;
            }
            capsuleRt.anchoredPosition = endPos;

            // フェーズ2: カプセルが揺れる
            float shakeDuration = isHighRarity ? 1.2f : 0.6f;
            elapsed = 0f;
            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float intensity = elapsed / shakeDuration;
                float angle = Mathf.Sin(elapsed * 20f * (1f + intensity)) * 15f * (1f + intensity);
                capsuleRt.localEulerAngles = new Vector3(0, 0, angle);

                // 高レアリティの場合は光り始める
                if (isHighRarity)
                {
                    float glow = intensity * 0.3f;
                    capsuleImage.color = Color.Lerp(Color.white, rarityColor, glow);
                }
                yield return null;
            }

            // フェーズ3: フラッシュ
            capsuleRt.localEulerAngles = Vector3.zero;
            float flashDuration = 0.3f;
            elapsed = 0f;

            // 高レアリティは色付きフラッシュ
            Color flashColor = isHighRarity ? new Color(rarityColor.r, rarityColor.g, rarityColor.b, 1f) : Color.white;
            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flashDuration;
                flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 1f - t);
                capsuleRt.localScale = Vector3.one * (1f + t * 0.5f);
                capsuleImage.color = new Color(1f, 1f, 1f, 1f - t);
                yield return null;
            }

            // フェーズ4: きらきらエフェクト（高レアリティのみ）
            if (isHighRarity)
            {
                float sparkleDuration = 0.5f;
                elapsed = 0f;
                while (elapsed < sparkleDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / sparkleDuration;
                    for (int i = 0; i < sparkleImages.Length; i++)
                    {
                        float angle = (360f / sparkleImages.Length) * i + t * 180f;
                        float rad = angle * Mathf.Deg2Rad;
                        float radius = 80f + t * 120f;
                        var sRt = sparkleImages[i].GetComponent<RectTransform>();
                        sRt.anchoredPosition = new Vector2(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius);
                        sRt.localScale = Vector3.one * (1f - t) * 1.5f;
                        sRt.localEulerAngles = new Vector3(0, 0, t * 360f);
                        sparkleImages[i].color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, (1f - t) * 0.8f);
                    }
                    yield return null;
                }
            }

            // 演出終了、結果パネル表示
            animOverlay.SetActive(false);
            ShowResult(pendingCat, pendingIsNew);

            isAnimating = false;
            pullButton.interactable = true;
        }

        private void ShowResult(CatData cat, bool isNew)
        {
            if (resultPanel == null) return;

            resultCatImage.sprite = cat.sprite;
            resultCatName.text = cat.catName;
            resultRarity.text = cat.rarity.ToString();

            // レアリティに応じた色
            resultRarity.color = UIHelper.GetRarityColor(cat.rarity);
            resultCatName.color = UIHelper.GetRarityColor(cat.rarity);

            // レアリティに応じたSE
            if (cat.rarity >= Rarity.SR)
                AudioManager.Instance?.PlayGachaRare();
            else
                AudioManager.Instance?.PlayGachaResult();

            resultNewLabel.text = isNew ? "NEW!" : "経験値ボーナス!";
            resultNewLabel.gameObject.SetActive(true);

            // 最前面に表示
            resultPanel.transform.SetAsLastSibling();
            resultPanel.SetActive(true);
        }
    }
}
