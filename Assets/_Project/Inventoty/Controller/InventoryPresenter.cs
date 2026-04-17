using UnityEngine;

public class InventoryPresenter : MonoBehaviour
{
    [Header("Inventory")]
    public Camera inventoryCamera;
    public GameObject inventoryRoot;

    [Header("Player")]
    public MonoBehaviour playerController;

    bool isOpen = false;

    private void Start()
    {
        inventoryCamera.gameObject.SetActive(false);
        inventoryRoot.gameObject.SetActive(false);
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

        if(playerController != null)
            playerController.enabled = !isOpen;

        Time.timeScale = isOpen ? 0f : 1f;

        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;

        Cursor.visible = isOpen;
    }

}
