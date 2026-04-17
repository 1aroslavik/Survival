using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ResourceSlotGroup
{
    public ResourceType type;
    public List<Transform> slots = new List<Transform>();
}

public class ConstructionSite : MonoBehaviour
{
    public BuildingData data;
    public List<ResourceRequirement> resources;

    [Header("Slots per Resource")]
    public List<ResourceSlotGroup> slotGroups = new List<ResourceSlotGroup>();

    [Header("Cancel Construction")]
    public float cancelHoldTime = 1.5f;

    float cancelTimer = 0f;

    void Update()
    {
        HandleCancel();
    }

    void HandleCancel()
    {
        if (!Input.GetKey(KeyCode.G))
        {
            cancelTimer = 0;
            return;
        }

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 4f))
        {
            if (hit.transform == transform)
            {
                cancelTimer += Time.deltaTime;

                if (cancelTimer >= cancelHoldTime)
                {
                    CancelConstruction();
                }
            }
        }
    }

    void CancelConstruction()
    {
        foreach (var res in resources)
        {
            for (int i = 0; i < res.currentAmount; i++)
            {
                Vector3 spawnPos = transform.position +
                    new Vector3(Random.Range(-0.6f, 0.6f), 1f, Random.Range(-0.6f, 0.6f));

                Instantiate(res.dropPrefab, spawnPos, Random.rotation);
            }
        }

        Destroy(gameObject);
    }

    public bool AddResource(ResourceType type)
    {
        var res = resources.Find(r => r.type == type);

        if (res == null)
            return false;

        if (res.currentAmount >= res.requiredAmount)
            return false;

        var group = slotGroups.Find(g => g.type == type);

        if (group == null)
            return false;

        if (res.currentAmount >= group.slots.Count)
            return false;

        Transform slot = group.slots[res.currentAmount];

        GameObject obj = Instantiate(
            res.buildPrefab,
            slot.position,
            slot.rotation,
            transform);

        obj.transform.localScale = slot.localScale;

        if (obj.TryGetComponent(out Rigidbody rb))
            Destroy(rb);

        if (obj.TryGetComponent(out Collider col))
            Destroy(col);

        res.currentAmount++;

        if (IsComplete())
            Complete();

        return true;
    }

    bool IsComplete()
    {
        foreach (var r in resources)
        {
            if (r.currentAmount < r.requiredAmount)
                return false;
        }
        return true;
    }

    void Complete()
    {
        if (data != null && data.finishedPrefab != null)
        {
            Instantiate(
                data.finishedPrefab,
                transform.position,
                transform.rotation);
        }

        Destroy(gameObject);
    }
}