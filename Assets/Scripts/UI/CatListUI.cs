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

        private void OnEnable()
        {
            RefreshList();
        }

        private void Start()
        {
            backButton.onClick.AddListener(() => UIManager.Instance.ShowHome());
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
            if (texts.Length > 0) texts[0].text = catData.catName;
            if (texts.Length > 1) texts[1].text = $"Lv.{owned.level}";

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
