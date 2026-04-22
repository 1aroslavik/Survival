using UnityEngine;
using TMPro;
using System.Collections;

public class ItemPickUp : MonoBehaviour
{
    [Header("References")]
    public InventoryModel inventory;
    public InventoryView inventoryView;
    public Camera playerCamera;
    public LayerMask pickupLayer;
    Highlightable currentHighlight;

    [Header("UI")]
    public GameObject pickupHint;
    public LogPickup logPickup;
    public TMP_Text infoText;

    [Header("Settings")]
    public float pickupDistance = 3f;
    public KeyCode pickupKey = KeyCode.E;

    WorldItem currentItem;

    void Start()
    {
        Debug.Log("START ItemPickUp");

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            Debug.Log("Camera auto assigned: " + playerCamera);
        }

        if (inventory == null)
        {
            inventory = FindObjectOfType<InventoryModel>();
            Debug.Log("Inventory found: " + inventory);
        }

        if (pickupHint != null)
            pickupHint.SetActive(false);

        if (infoText != null)
            infoText.gameObject.SetActive(false);
    }

    void Update()
    {
        CheckForItem();

        if (Input.GetKeyDown(pickupKey))
        {
            Debug.Log("KEY PRESSED");

            if (logPickup != null && logPickup.TryAddToConstruction())
            {
                Debug.Log("Added to construction");
                return;
            }

            if (currentItem != null)
            {
                Debug.Log("Trying pickup...");
                TryPickUp();
            }
            else
            {
                Debug.Log("NO CURRENT ITEM");
            }
        }
    }

    void CheckForItem()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance, pickupLayer))
        {
            Debug.Log("RAY HIT: " + hit.collider.name);

            WorldItem item = hit.collider.GetComponentInParent<WorldItem>();

            if (item != null)
            {
                Debug.Log("FOUND ITEM: " + item.name);

                Highlightable newHighlight = item.GetComponentInChildren<Highlightable>();

                if (newHighlight != currentHighlight)
                {
                    if (currentHighlight != null)
                        currentHighlight.Highlight(false);

                    if (newHighlight != null)
                        newHighlight.Highlight(true);

                    currentHighlight = newHighlight;
                }

                currentItem = item;

                if (pickupHint != null)
                    pickupHint.SetActive(true);

                return;
            }
        }

        if (currentHighlight != null)
        {
            currentHighlight.Highlight(false);
            currentHighlight = null;
        }

        currentItem = null;

        if (pickupHint != null)
            pickupHint.SetActive(false);
    }

    void TryPickUp()
    {
        Debug.Log("=== TRY PICKUP START ===");

        if (currentItem == null)
        {
            Debug.Log("❌ currentItem NULL");
            return;
        }

        Debug.Log("Item object: " + currentItem.name);

        if (currentItem.data == null)
        {
            Debug.Log("❌ DATA IS NULL");
            return;
        }

        Debug.Log("Item DATA: " + currentItem.data.name);
        Debug.Log("Amount: " + currentItem.amount);

        if (inventory == null)
        {
            Debug.Log("❌ INVENTORY NULL");
            return;
        }

        Debug.Log("Inventory ID: " + inventory.GetInstanceID());

        if (inventory.IsFull())
        {
            Debug.Log("❌ INVENTORY FULL");
            ShowMessage("INVENTORY IS FULL", 2f, Color.red);
            return;
        }

        var data = currentItem.data;
        int amount = currentItem.amount;

        bool added = inventory.TryAdd(data, amount);

        Debug.Log($"TryAdd RESULT: {added} | ITEM: {data.name}");

        if (!added)
        {
            ShowMessage("INVENTORY IS FULL", 2f, Color.red);
            return;
        }

        Debug.Log("✅ ITEM ADDED SUCCESS");

        Destroy(currentItem.gameObject);

        if (inventoryView != null)
        {
            inventoryView.Render();
            Debug.Log("Inventory UI updated");
        }

        ShowMessage("Picked up: " + data.name, 1.5f, Color.green);

        Debug.Log("=== TRY PICKUP END ===");
    }

    void ShowMessage(string message, float time, Color color)
    {
        if (infoText == null)
        {
            Debug.Log("No infoText assigned");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(ShowMessageRoutine(message, time, color));
    }

    IEnumerator ShowMessageRoutine(string message, float time, Color color)
    {
        infoText.text = message;
        infoText.color = color;
        infoText.gameObject.SetActive(true);

        yield return new WaitForSeconds(time);

        infoText.gameObject.SetActive(false);
    }
}