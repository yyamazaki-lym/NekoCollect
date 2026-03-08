using UnityEngine;
using UnityEngine.UI;
using NekoCollect.Data;

namespace NekoCollect.UI
{
    /// <summary>
    /// UI共通ヘルパー
    /// </summary>
    public static class UIHelper
    {
        // レアリティ別カード枠スプライト（キャッシュ）
        private static Sprite frameCommon;
        private static Sprite frameRare;
        private static Sprite frameSR;
        private static Sprite frameUR;
        private static bool framesLoaded;

        /// <summary>
        /// レアリティに応じたカード枠スプライトを取得
        /// </summary>
        public static Sprite GetRarityFrame(Rarity rarity)
        {
            if (!framesLoaded)
            {
                frameCommon = Resources.Load<Sprite>("UI/card_frame_common");
                frameRare = Resources.Load<Sprite>("UI/card_frame_rare");
                frameSR = Resources.Load<Sprite>("UI/card_frame_sr");
                frameUR = Resources.Load<Sprite>("UI/card_frame_ur");
                framesLoaded = true;
            }

            return rarity switch
            {
                Rarity.SSR => frameUR,
                Rarity.SR => frameSR,
                Rarity.R => frameRare,
                _ => frameCommon
            };
        }

        /// <summary>
        /// レアリティに応じた色を取得
        /// </summary>
        public static Color GetRarityColor(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.SSR => new Color(1f, 0.85f, 0f),       // 金色
                Rarity.SR => new Color(0.8f, 0.5f, 1f),       // 紫
                Rarity.R => new Color(0.3f, 0.7f, 1f),        // 青
                _ => new Color(0.7f, 0.7f, 0.7f)              // グレー
            };
        }

        /// <summary>
        /// カードにレアリティ枠を適用
        /// </summary>
        public static void ApplyRarityFrame(GameObject card, Rarity rarity)
        {
            // カードの背景Imageにフレーム設定
            var frameSprite = GetRarityFrame(rarity);
            if (frameSprite == null) return;

            // "Frame"子オブジェクトを探すか作成
            var frameTf = card.transform.Find("Frame");
            Image frameImage;
            if (frameTf == null)
            {
                var frameObj = new GameObject("Frame");
                frameObj.transform.SetParent(card.transform, false);
                var rt = frameObj.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                rt.anchoredPosition = Vector2.zero;
                if (frameObj.GetComponent<CanvasRenderer>() == null)
                    frameObj.AddComponent<CanvasRenderer>();
                frameImage = frameObj.AddComponent<Image>();
                frameImage.raycastTarget = false;
                // 最前面に
                frameObj.transform.SetAsLastSibling();
            }
            else
            {
                frameImage = frameTf.GetComponent<Image>();
            }

            if (frameImage != null)
            {
                frameImage.sprite = frameSprite;
                frameImage.type = Image.Type.Simple;
                frameImage.preserveAspect = false;
                frameImage.color = Color.white;
            }
        }
    }
}
