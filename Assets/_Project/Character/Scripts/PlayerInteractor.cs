using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public float interactDistance = 3f;
    public Camera playerCamera;
    public InventoryModel inventory;

    void Update()
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
            // 🔥 сначала мясо (если оно есть)
            DryingMeat meat = hit.collider.GetComponentInParent<DryingMeat>();

            if (meat != null)
            {
                Destroy(meat.gameObject);
                return;
            }

            // 🔥 потом сушилка
            Dryer dryer = hit.collider.GetComponentInParent<Dryer>();

            if (dryer != null)
            {
                dryer.TryAddMeat(inventory);
                return;
            }
        }
    }
}