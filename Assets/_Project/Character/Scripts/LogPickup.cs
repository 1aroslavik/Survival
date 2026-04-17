using UnityEngine;

public class LogPickup : MonoBehaviour
{
    public float pickupDistance = 3f;

    public HandsController hands;
    public GameObject carryHandsPrefab;
    public WeaponEquipment weaponEquipment;

    public InventoryModel inventory;

    private GameObject worldLog;
    private GameObject carriedLog;

    Transform currentHoldPoint;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 🔥 сначала стройка
            if (TryAddToConstruction())
                return;

            // дальше старая логика
            if (carriedLog == null)
                TryPickup();
            else
                DropLog();
        }
    }

    void TryPickup()
    {
        Ray ray = new Ray(Camera.main.transform.position,
                          Camera.main.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance))
        {
            if (hit.collider.CompareTag("Log"))
            {
                if (hit.collider.GetComponentInParent<ConstructionSite>() != null)
                    return;

                worldLog = hit.collider.gameObject;

                if (weaponEquipment != null)
                    weaponEquipment.Unequip();

                hands.SetCarry(carryHandsPrefab);

                foreach (Transform t in hands.GetComponentsInChildren<Transform>())
                {
                    if (t.name == "HoldPoint")
                    {
                        currentHoldPoint = t;
                        break;
                    }
                }

                carriedLog = Instantiate(worldLog,
                    currentHoldPoint.position,
                    currentHoldPoint.rotation);

                carriedLog.transform.SetParent(currentHoldPoint);
                carriedLog.transform.localPosition = Vector3.zero;
                carriedLog.transform.localRotation = Quaternion.identity;

                Destroy(carriedLog.GetComponent<Rigidbody>());
                Destroy(carriedLog.GetComponent<Collider>());

                worldLog.SetActive(false);
            }
        }
    }

    public bool TryAddToConstruction()
    {
        Ray ray = new Ray(Camera.main.transform.position,
                          Camera.main.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance))
        {
            ConstructionSite site =
                hit.collider.GetComponentInParent<ConstructionSite>();

            if (site == null)
                return false;

            // 🪵 БРЕВНО
            if (carriedLog != null)
            {
                bool added = site.AddResource(ResourceType.Log);

                if (added)
                {
                    Destroy(carriedLog);
                    carriedLog = null;

                    if (worldLog != null)
                        Destroy(worldLog);

                    worldLog = null;

                    if (hands != null)
                        hands.ClearHands();

                    return true;
                }

                return false;
            }

            // 🪨 ПАЛКИ / КАМНИ
            if (inventory != null)
            {
                foreach (var r in site.resources)
                {
                    // 🔥 проверяем есть ли ресурс
                    if (!inventory.TryRemoveOne(r.type))
                        continue;

                    bool added = site.AddResource(r.type);

                    if (added)
                        return true;
                }
            }
        }

        return false;
    }

    void DropLog()
    {
        Destroy(carriedLog);

        if (worldLog != null)
        {
            worldLog.transform.position =
                Camera.main.transform.position + Camera.main.transform.forward * 1.5f;

            worldLog.SetActive(true);
        }

        if (hands != null)
            hands.ClearHands();

        carriedLog = null;
        worldLog = null;
    }
}