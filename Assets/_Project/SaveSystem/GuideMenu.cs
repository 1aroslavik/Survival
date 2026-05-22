using UnityEngine;

public class GuidePanelUI : MonoBehaviour
{
    [Header("Pages")]
    public GameObject gameButtons;
    public GameObject inventoryButtons;
    public GameObject buildingBookButtons;

    [Header("Menus")]
    public GameObject guidePanel;
    public GameObject pauseMenu;

    private void Start()
    {
        OpenGamePage();
    }

    // GAME PAGE
    public void OpenGamePage()
    {
        CloseAllPages();
        gameButtons.SetActive(true);
    }

    // INVENTORY PAGE
    public void OpenInventoryPage()
    {
        CloseAllPages();
        inventoryButtons.SetActive(true);
    }

    // BUILDING BOOK PAGE
    public void OpenBuildingBookPage()
    {
        CloseAllPages();
        buildingBookButtons.SetActive(true);
    }

    // BACK BUTTON
    public void BackToPauseMenu()
    {
        guidePanel.SetActive(false);
        pauseMenu.SetActive(true);

        CloseAllPages();
    }

    void CloseAllPages()
    {
        gameButtons.SetActive(false);
        inventoryButtons.SetActive(false);
        buildingBookButtons.SetActive(false);
    }
}