using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Settings")]
    public float interactDistance = 3f;
    public Camera playerCamera;

    [Header("References")]
    public InventoryModel inventory;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            // 🔹 Сушилка
            Dryer dryer = hit.collider.GetComponent<Dryer>();

            if (dryer != null)
            {
                dryer.TryAddMeat(inventory);
                return;
            }

            
        }
    }
}