using UnityEngine;

public class InventoryPresenter : MonoBehaviour
{
    [Header("Inventory")]
    public Camera inventoryCamera;
    public GameObject inventoryRoot;

    [Header("HUD")]
    public GameObject statsUI;

    [Header("Extra Object")]
    public GameObject objectToShow;

    [Header("Player")]
    public MonoBehaviour playerController;

    bool isOpen = false;

    private void Start()
    {
        inventoryCamera.gameObject.SetActive(false);
        inventoryRoot.gameObject.SetActive(false);

        if (objectToShow != null)
            objectToShow.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    void ToggleInventory()
    {
        isOpen = !isOpen;

        inventoryCamera.gameObject.SetActive(isOpen);
        inventoryRoot.gameObject.SetActive(isOpen);

        // Скрываем HUD
        if (statsUI != null)
            statsUI.SetActive(!isOpen);

        // Показываем объект при открытии
        if (objectToShow != null)
            objectToShow.SetActive(isOpen);

        if (playerController != null)
            playerController.enabled = !isOpen;

        Time.timeScale = isOpen ? 0f : 1f;

        Cursor.lockState = isOpen
            ? CursorLockMode.None
            : CursorLockMode.Locked;

        Cursor.visible = isOpen;

        if (!isOpen)
        {
            if (NoteReader.Instance != null)
                NoteReader.Instance.Hide();

            if (InventoryTooltip.Instance != null)
                InventoryTooltip.Instance.Hide();
        }
    }
}