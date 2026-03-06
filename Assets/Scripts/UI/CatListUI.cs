using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NekoCollect.Data;
using NekoCollect.Manager;
using NekoCollect.Save;

namespace NekoCollect.UI
{
    /// <summary>
    /// 猫一覧画面のUI制御
    /// </summary>
    public class CatListUI : MonoBehaviour
    {
        [Header("UI参照")]
        [SerializeField] private Transform catGridContainer;
        [SerializeField] private GameObject catCardPrefab;
        [SerializeField] private Button backButton;

        private bool initialized;

        private void OnEnable()
        {
            if (!initialized)
            {
                initialized = true;

                // パネル背景を追加
                SetupPanelBackground();

                backButton.onClick.AddListener(() => UIManager.Instance.ShowHome());
            }
            RefreshList();
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

        private void RefreshList()
        {
            // 既存のカードをクリア
            foreach (Transform child in catGridContainer)
                Destroy(child.gameObject);

            var ownedCats = CatCollectionManager.Instance.GetOwnedCats();
            foreach (var owned in ownedCats)
            {
                var catData = CatCollectionManager.Instance.GetCatData(owned.catId);
                if (catData == null) continue;

                var card = Instantiate(catCardPrefab, catGridContainer);
                SetupCatCard(card, owned, catData);
            }
        }

        private void SetupCatCard(GameObject card, OwnedCatData owned, CatData catData)
        {
            // カード内のUIを設定
            var image = card.GetComponentInChildren<Image>();
            if (image != null) image.sprite = catData.sprite;

            var texts = card.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0)
            {
                texts[0].text = catData.catName;
                texts[0].enableAutoSizing = true;
                texts[0].fontSizeMin = 10;
                texts[0].fontSizeMax = 24;
            }
            if (texts.Length > 1)
            {
                texts[1].text = $"Lv.{owned.level}";
                texts[1].enableAutoSizing = true;
                texts[1].fontSizeMin = 10;
                texts[1].fontSizeMax = 20;
            }

            // タップで詳細画面を開く
            var button = card.GetComponent<Button>();
            if (button == null) button = card.AddComponent<Button>();

            var capturedOwned = owned;
            button.onClick.AddListener(() =>
            {
                CatDetailUI.SetTarget(capturedOwned);
                UIManager.Instance.ShowCatDetail();
            });
        }
    }
}
