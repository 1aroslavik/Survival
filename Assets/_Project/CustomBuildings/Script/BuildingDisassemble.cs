using UnityEngine;

public class BuildingDisassemble : MonoBehaviour
{
    public BuildingData buildingData;
    public GameObject logPrefab;

    public float holdTime = 2f;
    float holdTimer = 0f;

    void Update()
    {
        if (!Input.GetKey(KeyCode.G))
        {
            holdTimer = 0;
            return;
        }

        Ray ray = new Ray(
            Camera.main.transform.position,
            Camera.main.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 4f))
        {
            if (hit.transform == transform)
            {
                holdTimer += Time.deltaTime;

                if (holdTimer >= holdTime)
                {
                    BreakBuilding();
                }
            }
        }
    }

    void BreakBuilding()
    {
        if (buildingData.resources == null) return;

        foreach (var res in buildingData.resources)
        {
            for (int i = 0; i < res.requiredAmount; i++)
            {
                Vector3 spawnPos = transform.position +
                    new Vector3(
                        Random.Range(-0.7f, 0.7f),
                        1f,
                        Random.Range(-0.7f, 0.7f));

                Instantiate(res.dropPrefab, spawnPos, Random.rotation);
            }
        }

        Destroy(gameObject);
    }
}