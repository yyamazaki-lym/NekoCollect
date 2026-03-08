using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NekoCollect.Data;
using NekoCollect.Manager;

namespace NekoCollect.UI
{
    /// <summary>
    /// ホーム画面のUI制御（所持猫表示＆タッチ反応付き）
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
        [SerializeField] private Button achievementButton;

        [Header("放置コイン通知")]
        [SerializeField] private GameObject offlineCoinPopup;
        [SerializeField] private TextMeshProUGUI offlineCoinText;
        [SerializeField] private Button offlineCloseButton;

        // 所持猫表示用
        private Transform catDisplayArea;
        private List<GameObject> displayedCats = new List<GameObject>();

        // タッチ反応テキスト
        private static readonly string[] touchReactions = new string[]
        {
            "にゃ〜♪", "ゴロゴロ...", "にゃん！", "すりすり",
            "みゃー", "ぷるるる", "にゃっ♡", "zzz...",
            "ふにゃ〜", "にゃお！"
        };

        private void OnEnable()
        {
            CoinManager.Instance.OnCoinsChanged += UpdateCoinDisplay;
            CoinManager.Instance.OnIdleCoinsCollected += ShowOfflineCoins;
            UpdateCoinDisplay(CoinManager.Instance.Coins);

            // 猫表示を更新
            RefreshCatDisplay();
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
            // 背景画像を設定
            SetupPanelBackground();

            // コインアイコン設定
            SetupCoinIcon();

            // 猫表示エリアを作成
            SetupCatDisplayArea();

            clickButton.onClick.AddListener(OnClickCoin);
            gachaButton.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayTap();
                UIManager.Instance.ShowGacha();
            });
            catListButton.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayTap();
                UIManager.Instance.ShowCatList();
            });
            catalogButton.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayTap();
                UIManager.Instance.ShowCatalog();
            });
            achievementButton?.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayTap();
                UIManager.Instance.ShowAchievements();
            });
            offlineCloseButton?.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayTap();
                CloseOfflinePopup();
            });

            // ポップアップのセットアップ
            SetupOfflinePopup();
        }

        /// <summary>
        /// 猫表示エリアを生成
        /// </summary>
        private void SetupCatDisplayArea()
        {
            var areaObj = new GameObject("CatDisplayArea");
            areaObj.transform.SetParent(transform, false);
            var rt = areaObj.AddComponent<RectTransform>();
            // コインボタンの下〜ナビゲーションボタンの上のエリアに配置
            // （コインボタンはy=0.4〜0.65付近、ナビボタンはy=0.13〜0.2付近）
            // 猫は画面下部に配置してコインと重ならないようにする
            rt.anchorMin = new Vector2(0.02f, 0.02f);
            rt.anchorMax = new Vector2(0.98f, 0.40f);
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            catDisplayArea = areaObj.transform;

            // クリックボタンより背面に配置
            areaObj.transform.SetAsFirstSibling();
        }

        /// <summary>
        /// 所持猫をHome画面に表示
        /// </summary>
        private void RefreshCatDisplay()
        {
            if (catDisplayArea == null) return;

            // 既存の猫をクリア
            foreach (var cat in displayedCats)
            {
                if (cat != null) Destroy(cat);
            }
            displayedCats.Clear();

            var ownedCats = CatCollectionManager.Instance?.GetOwnedCats();
            if (ownedCats == null || ownedCats.Count == 0) return;

            // 最大5匹まで表示
            int displayCount = Mathf.Min(ownedCats.Count, 5);
            for (int i = 0; i < displayCount; i++)
            {
                var owned = ownedCats[i];
                var catData = CatCollectionManager.Instance.GetCatData(owned.catId);
                if (catData == null || catData.sprite == null) continue;

                CreateCatDisplay(catData, i, displayCount);
            }
        }

        /// <summary>
        /// 個別の猫表示を生成
        /// </summary>
        private void CreateCatDisplay(CatData catData, int index, int total)
        {
            var catObj = new GameObject($"HomeCat_{catData.catName}");
            catObj.transform.SetParent(catDisplayArea, false);

            var rt = catObj.AddComponent<RectTransform>();
            // 猫をエリア内にランダム配置（ナビボタンの手前に収まるように）
            float xRange = 0.7f;
            float x = (index / (float)Mathf.Max(total - 1, 1)) * xRange + (1f - xRange) * 0.5f;
            // Y方向はエリア上部寄りに配置（下部はナビボタン領域）
            float y = 0.45f + Random.Range(0f, 0.35f);

            // 猫サイズ（エリア内での相対サイズ）
            float catHalfW = 0.10f;
            float catHalfH = 0.20f;
            rt.anchorMin = new Vector2(x - catHalfW, y - catHalfH);
            rt.anchorMax = new Vector2(x + catHalfW, y + catHalfH);
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;

            // 猫画像
            if (catObj.GetComponent<CanvasRenderer>() == null)
                catObj.AddComponent<CanvasRenderer>();
            var image = catObj.AddComponent<Image>();
            image.sprite = catData.sprite;
            image.preserveAspect = true;
            image.raycastTarget = true;

            // タッチ反応テキスト用の子オブジェクト（非表示）
            // 猫の上部に吹き出し風に表示（横幅を広げてはみ出し対応）
            var reactionObj = new GameObject("ReactionText");
            reactionObj.transform.SetParent(catObj.transform, false);
            var reactionRt = reactionObj.AddComponent<RectTransform>();
            reactionRt.anchorMin = new Vector2(-0.5f, 1.0f);
            reactionRt.anchorMax = new Vector2(1.5f, 1.8f);
            reactionRt.sizeDelta = Vector2.zero;
            reactionRt.anchoredPosition = Vector2.zero;

            var reactionText = reactionObj.AddComponent<TextMeshProUGUI>();
            reactionText.text = "";
            reactionText.alignment = TextAlignmentOptions.Center;
            reactionText.enableAutoSizing = true;
            reactionText.fontSizeMin = 10;
            reactionText.fontSizeMax = 24;
            reactionText.color = Color.white;
            reactionText.outlineWidth = 0.3f;
            reactionText.outlineColor = Color.black;
            reactionObj.SetActive(false);

            // タッチ用ボタン
            var btn = catObj.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            var capturedData = catData;
            var capturedImage = image;
            var capturedReaction = reactionObj;
            var capturedReactionText = reactionText;
            btn.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayTap();
                StartCoroutine(PlayCatReaction(capturedImage, capturedReaction, capturedReactionText));
            });

            // ゆらゆらアニメーション開始
            StartCoroutine(IdleAnimation(catObj.transform, index));

            displayedCats.Add(catObj);
        }

        /// <summary>
        /// 猫タッチ時のリアクション演出
        /// </summary>
        private IEnumerator PlayCatReaction(Image catImage, GameObject reactionObj, TextMeshProUGUI reactionText)
        {
            // ランダムなリアクションテキスト
            reactionText.text = touchReactions[Random.Range(0, touchReactions.Length)];
            reactionObj.SetActive(true);

            var rt = catImage.GetComponent<RectTransform>();
            var reactionRt = reactionObj.GetComponent<RectTransform>();
            Vector3 originalScale = rt.localScale;

            // ぴょんと跳ねる
            float duration = 0.4f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float jumpY = Mathf.Sin(t * Mathf.PI) * 0.2f;
                float scaleX = 1f + Mathf.Sin(t * Mathf.PI * 2f) * 0.1f;
                float scaleY = 1f + Mathf.Cos(t * Mathf.PI * 2f) * 0.1f;
                rt.localScale = new Vector3(scaleX, scaleY + jumpY, 1f);

                // リアクションテキストを上に浮かせる
                float alpha = t < 0.5f ? 1f : 1f - (t - 0.5f) * 2f;
                reactionText.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }

            rt.localScale = originalScale;

            // テキストをフェードアウト
            float fadeTime = 0.3f;
            elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - (elapsed / fadeTime);
                reactionText.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }

            reactionObj.SetActive(false);
        }

        /// <summary>
        /// 猫のゆらゆらアイドルアニメーション
        /// </summary>
        private IEnumerator IdleAnimation(Transform catTransform, int offset)
        {
            float speed = 0.5f + Random.Range(0f, 0.3f);
            float amplitude = 3f + Random.Range(0f, 2f);
            float phase = offset * 1.5f;

            while (catTransform != null)
            {
                float angle = Mathf.Sin(Time.time * speed + phase) * amplitude;
                catTransform.localEulerAngles = new Vector3(0, 0, angle);
                yield return null;
            }
        }

        private void SetupOfflinePopup()
        {
            if (offlineCoinPopup == null) return;

            // Canvas直下に移動して全パネルより上に表示されるようにする
            var canvas = offlineCoinPopup.GetComponentInParent<Canvas>();
            if (canvas != null)
                offlineCoinPopup.transform.SetParent(canvas.transform);

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
            var sprite = Resources.Load<Sprite>("Backgrounds/bg_home");
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
        /// コインアイコンをクリックボタンに設定
        /// </summary>
        private void SetupCoinIcon()
        {
            var coinSprite = Resources.Load<Sprite>("UI/icon_coin");
            if (coinSprite != null && clickButton != null)
            {
                var btnImage = clickButton.GetComponent<Image>();
                if (btnImage != null)
                {
                    btnImage.sprite = coinSprite;
                    btnImage.preserveAspect = true;
                }
            }
        }

        private void OnClickCoin()
        {
            CoinManager.Instance.ClickCoin();
            AudioManager.Instance?.PlayClick();
            AchievementManager.Instance?.RecordClick();
        }

        private void UpdateCoinDisplay(long coins)
        {
            coinText.text = $"コイン: {coins:N0}";
        }

        private void ShowOfflineCoins(long amount)
        {
            if (offlineCoinPopup != null && amount > 0)
            {
                AudioManager.Instance?.PlayBonus();
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
