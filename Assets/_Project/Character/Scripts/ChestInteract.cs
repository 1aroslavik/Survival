using TMPro;
using UnityEngine;

public class ChestInteract : MonoBehaviour
{
    [Header("Lid")]
    public Transform lid;
    public float openAngle = -110f;
    public float speed = 4f;

    [Header("Loot")]
    public GameObject[] possibleItems;     // префабы предметов
    public Transform[] spawnPoints;        // точки спавна

    [Header("UI")]
    public GameObject openHint;
    private bool playerInside;
    private bool isOpen;
    private bool spawned;

    private Quaternion closedRot;
    private Quaternion openRot;

    void Start()
    {
        closedRot = lid.localRotation;
        openRot = Quaternion.Euler(openAngle, 0f, 0f);

        if (openHint != null)
            openHint.SetActive(false);
    }

    void Update()
    {
        if (playerInside && Input.GetKeyDown(KeyCode.E))
        {
            if (!isOpen)
            {
                isOpen = true;

                // скрываем подсказку
                if (openHint != null)
                    openHint.SetActive(false);

                if (!spawned)
                {
                    SpawnLoot();
                    spawned = true;
                }
            }
        }

        Quaternion target = isOpen ? openRot : closedRot;

        lid.localRotation = Quaternion.Lerp(
            lid.localRotation,
            target,
            Time.deltaTime * speed);
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

            // выставляем слой всем детям
            SetLayerRecursively(item, LayerMask.NameToLayer("PostProcessing"));
        }
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;

            if (!isOpen && openHint != null)
                openHint.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;

            if (openHint != null)
                openHint.SetActive(false);
        }
    }
}