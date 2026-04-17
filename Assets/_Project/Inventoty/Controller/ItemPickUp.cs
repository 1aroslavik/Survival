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

    // 🔥 ОДНА СТРОКА ДЛЯ ВСЕХ СООБЩЕНИЙ
    public TMP_Text infoText;

    [Header("Settings")]
    public float pickupDistance = 3f;
    public KeyCode pickupKey = KeyCode.E;

    WorldItem currentItem;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (inventory == null)
            inventory = FindObjectOfType<InventoryModel>();

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
            // 🔹 сначала стройка
            if (logPickup != null && logPickup.TryAddToConstruction())
                return;

            // 🔹 потом подбор
            if (currentItem != null)
            {
                TryPickUp();
            }
        }
    }

    void CheckForItem()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance, pickupLayer))
        {
            WorldItem item = hit.collider.GetComponentInParent<WorldItem>();

            if (item != null)
            {
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
        if (currentItem == null || inventory == null)
            return;

        if (currentItem.data == null || currentItem.amount <= 0)
            return;

        // 🔥 ПРОВЕРКА НА ПОЛНЫЙ ИНВЕНТАРЬ
        if (inventory.IsFull())
        {
            ShowMessage("INVENTORY IS FULL", 2f, Color.red);
            return;
        }

        var data = currentItem.data;
        int amount = currentItem.amount;

        bool added = inventory.TryAdd(data, amount);

        Debug.Log($"TryAdd: {added} | Item: {data.name}");

        if (!added)
        {
            ShowMessage("INVENTORY IS FULL", 2f, Color.red);
            return;
        }

        // ✅ УСПЕШНО
        Destroy(currentItem.gameObject);

        if (inventoryView != null)
            inventoryView.Render();

        // 🔥 МОЖНО ПОКАЗАТЬ ДРУГОЕ СООБЩЕНИЕ
        ShowMessage("Picked up: " + data.name, 1.5f, Color.green);
    }

    // 🔥 УНИВЕРСАЛЬНАЯ ФУНКЦИЯ
    void ShowMessage(string message, float time, Color color)
    {
        if (infoText == null)
            return;

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