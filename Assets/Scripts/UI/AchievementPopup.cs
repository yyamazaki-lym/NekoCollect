using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NekoCollect.Manager;

namespace NekoCollect.UI
{
    /// <summary>
    /// 実績解除時のトースト通知
    /// </summary>
    public class AchievementPopup : MonoBehaviour
    {
        private GameObject popupObj;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI descText;
        private CanvasGroup canvasGroup;

        private void Start()
        {
            SetupPopup();

            if (AchievementManager.Instance != null)
                AchievementManager.Instance.OnAchievementUnlocked += ShowPopup;
        }

        private void OnDestroy()
        {
            if (AchievementManager.Instance != null)
                AchievementManager.Instance.OnAchievementUnlocked -= ShowPopup;
        }

        /// <summary>
        /// トースト通知用UIを生成
        /// </summary>
        private void SetupPopup()
        {
            // Canvas直下に配置
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            popupObj = new GameObject("AchievementPopup");
            popupObj.transform.SetParent(canvas.transform, false);

            var rt = popupObj.AddComponent<RectTransform>();
            // 画面上部にバナー表示
            rt.anchorMin = new Vector2(0.05f, 0.85f);
            rt.anchorMax = new Vector2(0.95f, 0.95f);
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;

            // 背景
            if (popupObj.GetComponent<CanvasRenderer>() == null)
                popupObj.AddComponent<CanvasRenderer>();
            var bg = popupObj.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.1f, 0.25f, 0.95f);
            bg.raycastTarget = false;

            // CanvasGroupでフェード制御
            canvasGroup = popupObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            // タイトルテキスト
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(popupObj.transform, false);
            var titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.05f, 0.5f);
            titleRt.anchorMax = new Vector2(0.95f, 0.95f);
            titleRt.sizeDelta = Vector2.zero;
            titleRt.anchoredPosition = Vector2.zero;
            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "";
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = 12;
            titleText.fontSizeMax = 28;
            titleText.color = new Color(1f, 0.9f, 0.3f);

            // 説明テキスト
            var descObj = new GameObject("Description");
            descObj.transform.SetParent(popupObj.transform, false);
            var descRt = descObj.AddComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0.05f, 0.05f);
            descRt.anchorMax = new Vector2(0.95f, 0.5f);
            descRt.sizeDelta = Vector2.zero;
            descRt.anchoredPosition = Vector2.zero;
            descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = "";
            descText.alignment = TextAlignmentOptions.Center;
            descText.enableAutoSizing = true;
            descText.fontSizeMin = 10;
            descText.fontSizeMax = 20;
            descText.color = Color.white;

            popupObj.SetActive(false);
        }

        private void ShowPopup(AchievementDef achievement)
        {
            if (popupObj == null) return;
            StopAllCoroutines();
            StartCoroutine(ShowPopupCoroutine(achievement));
        }

        private IEnumerator ShowPopupCoroutine(AchievementDef achievement)
        {
            titleText.text = $"🏆 {achievement.title}";
            descText.text = $"{achievement.description} (+{achievement.coinReward}コイン)";

            popupObj.SetActive(true);
            popupObj.transform.SetAsLastSibling();

            AudioManager.Instance?.PlayBonus();

            // フェードイン
            float fadeIn = 0.3f;
            float elapsed = 0f;
            while (elapsed < fadeIn)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeIn);
                yield return null;
            }
            canvasGroup.alpha = 1f;

            // 表示維持
            yield return new WaitForSeconds(2.5f);

            // フェードアウト
            float fadeOut = 0.5f;
            elapsed = 0f;
            while (elapsed < fadeOut)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeOut);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            popupObj.SetActive(false);
        }
    }
}
