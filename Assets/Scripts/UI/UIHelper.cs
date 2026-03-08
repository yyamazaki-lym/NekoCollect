using UnityEngine;
using UnityEngine.UI;
using TMPro;
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

        // フォント初期化済みフラグ
        private static bool fontInitialized;

        /// <summary>
        /// ゲームで使用する全ての日本語文字をフォントアトラスに事前登録する。
        /// 起動時に一度呼ぶことでWebGLでの□□表示を防ぐ。
        /// </summary>
        public static void EnsureFontCharacters()
        {
            if (fontInitialized) return;
            fontInitialized = true;

            // ゲーム内で使用する全ての日本語文字
            string requiredChars =
                // 基本ひらがな・カタカナ（既存アトラスにある）
                // 実績関連
                "実績達成解除済未了報酬獲得" +
                // 実績タイトル・説明
                "初回引匹集全完伝説的中毒好者入手発見超越" +
                // 育成関連
                "育上最大限度近" +
                // UI共通
                "画面表示戻閉開確認情詳細数現在" +
                // ホーム画面
                "配置更新結果" +
                // その他ゲーム内テキスト
                "猫一覧図鑑進化安高級放置" +
                "経験値習得新規" +
                // ASCII記号（アトラスに不足の可能性）
                "/:()%+-.,!?0123456789 " +
                // 実績タイトル・説明文
                "はじめてのガチャを回引いたマスター" +
                "ねこコレクターコンプリート" +
                "レアスーパー" +
                "猫をLv.5にした10最大レベル" +
                "タッパーマニアタップした100回1000回" +
                "以上の猫を入手した" +
                // 猫タッチ反応
                "にゃ〜♪ゴロ...ん！すりみゃーぷるにゃっ♡zzz...ふにゃおぉ" +
                // コイン表示
                "コイン" +
                // ポップアップ
                "おるすばんボーナス" +
                // フルワイド記号
                "：／＋％（）「」";

            // デフォルトフォントに文字を事前登録
            var defaultFont = TMP_Settings.defaultFontAsset;
            if (defaultFont != null)
            {
                defaultFont.TryAddCharacters(requiredChars);
            }

            // フォールバックフォントにも登録
            if (TMP_Settings.fallbackFontAssets != null)
            {
                foreach (var fallback in TMP_Settings.fallbackFontAssets)
                {
                    if (fallback != null)
                    {
                        fallback.TryAddCharacters(requiredChars);
                    }
                }
            }
        }

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
