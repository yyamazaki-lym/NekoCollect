using UnityEngine;
using NekoCollect.Util;

namespace NekoCollect.UI
{
    /// <summary>
    /// 画面遷移を管理するUIマネージャー
    /// </summary>
    public class UIManager : SingletonMonoBehaviour<UIManager>
    {
        [Header("各画面のルートオブジェクト")]
        [SerializeField] private GameObject homePanel;
        [SerializeField] private GameObject gachaPanel;
        [SerializeField] private GameObject catListPanel;
        [SerializeField] private GameObject catDetailPanel;
        [SerializeField] private GameObject catalogPanel;

        private GameObject currentPanel;

        private void Start()
        {
            // 全パネルを非表示にしてからホームを表示
            HideAll();
            ShowHome();
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

        private void SwitchTo(GameObject panel)
        {
            if (currentPanel != null)
                currentPanel.SetActive(false);
            currentPanel = panel;
            if (currentPanel != null)
                currentPanel.SetActive(true);
        }

        private void HideAll()
        {
            homePanel?.SetActive(false);
            gachaPanel?.SetActive(false);
            catListPanel?.SetActive(false);
            catDetailPanel?.SetActive(false);
            catalogPanel?.SetActive(false);
        }
    }
}
