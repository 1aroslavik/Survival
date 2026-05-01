using UnityEngine;

public class ChestInteract : MonoBehaviour
{
    [Header("Lid")]
    public Transform lid;
    public float openAngle = -110f;
    public float speed = 4f;

    [Header("Loot")]
    public GameObject[] possibleItems;
    public Transform[] spawnPoints;

    [Header("UI")]
    public GameObject openHint;

    [Header("Player")]
    public Camera playerCamera;
    public float interactDistance = 3f;

    private bool isOpen;
    private bool spawned;

    private Quaternion closedRot;
    private Quaternion openRot;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        closedRot = lid.localRotation;
        openRot = Quaternion.Euler(openAngle, 0f, 0f);

        if (openHint != null)
            openHint.SetActive(false);
    }

    void Update()
    {
        bool isLooking = CheckRaycast();

        // ХИНТ
        if (openHint != null)
            openHint.SetActive(isLooking && !isOpen);

        // ОТКРЫТИЕ
        if (isLooking && Input.GetKeyDown(KeyCode.E))
        {
            OpenChest();
        }

        // АНИМАЦИЯ
        Quaternion target = isOpen ? openRot : closedRot;

        lid.localRotation = Quaternion.Lerp(
            lid.localRotation,
            target,
            Time.deltaTime * speed);
    }

    bool CheckRaycast()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            return hit.collider.GetComponentInParent<ChestInteract>() != null;
        }

        return false;
    }

    public void OpenChest()
    {
        if (isOpen) return;

        isOpen = true;

        if (!spawned)
        {
            SpawnLoot();
            spawned = true;
        }
    }

    void SpawnLoot()
    {
        foreach (var point in spawnPoints)
        {
            if (possibleItems.Length == 0) return;

            int randomIndex = Random.Range(0, possibleItems.Length);

            GameObject item = Instantiate(
                possibleItems[randomIndex],
                point.position,
                point.rotation);

            SetLayerRecursively(item, LayerMask.NameToLayer("PostProcessing"));
        }
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}