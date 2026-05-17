using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingSystem : MonoBehaviour
{
    [Header("Preview Materials")]
    public Material validMaterial;
    public Material invalidMaterial;
    [Header("Buildings")]
    public BuildingData[] buildings;
    public BookController bookController;
    public float snapDistance = 2f;
    public float rotationStep = 45f;

    GameObject previewObject;
    BuildingData currentBuilding;

    Renderer[] previewRenderers;

    bool canPlace = true;
    float currentRotation = 0f;

    public void SelectBuildingByUI(BuildingData building)
    {
        currentBuilding = building;
        currentRotation = 0f;

        if (previewObject != null)
            Destroy(previewObject);

        if (bookController != null)
            bookController.CloseBookFromUI();

         }
   
    void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (currentBuilding == null)
            return;

        if (previewObject == null)
            CreatePreview();

        MovePreview();
        CheckPlacement();
        HandleRotation();

        if (Input.GetMouseButtonDown(0))
            Place();
        if (Input.GetMouseButtonDown(1))
            CancelBuilding();
    }

    void CreatePreview()
    {
        previewObject = Instantiate(currentBuilding.constructionPrefab);

        previewRenderers = previewObject.GetComponentsInChildren<Renderer>();

        foreach (Collider col in previewObject.GetComponentsInChildren<Collider>())
            Destroy(col);

        foreach (Rigidbody rb in previewObject.GetComponentsInChildren<Rigidbody>())
            Destroy(rb);

        SetPreviewMaterial(validMaterial);
    }

    void MovePreview()
    {
        Ray ray = new Ray(
            Camera.main.transform.position,
            Camera.main.transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground")))
            return;

        // 🔥 ТОЧНАЯ ПОСАДКА НА ЗЕМЛЮ
        Bounds bounds = GetBounds(previewObject);

        float bottom = bounds.min.y;
        float offsetY = previewObject.transform.position.y - bottom;

        Vector3 targetPos = new Vector3(
            hit.point.x,
            hit.point.y + offsetY,
            hit.point.z
        );

        previewObject.transform.position = targetPos;

        // ===== SNAP =====
        int snapMask = LayerMask.GetMask("SnapPoint");

        Collider[] snapPoints = Physics.OverlapSphere(
            previewObject.transform.position,
            snapDistance,
            snapMask);

        Transform closest = null;
        float minDist = float.MaxValue;

        foreach (var col in snapPoints)
        {
            float dist = Vector3.Distance(
                previewObject.transform.position,
                col.transform.position);

            if (dist < minDist)
            {
                minDist = dist;
                closest = col.transform;
            }
        }

        if (closest != null)
        {
            Transform previewSnapPoints =
                previewObject.transform.Find("SnapPoints");

            if (previewSnapPoints != null)
            {
                Transform previewClosest = null;
                float minPreviewDist = float.MaxValue;

                foreach (Transform child in previewSnapPoints)
                {
                    float dist = Vector3.Distance(child.position, closest.position);

                    if (dist < minPreviewDist)
                    {
                        minPreviewDist = dist;
                        previewClosest = child;
                    }
                }

                if (previewClosest != null)
                {
                    Vector3 diff = closest.position - previewClosest.position;
                    previewObject.transform.position += diff;
                }
            }
        }
    }

    void HandleRotation()
    {
        if (previewObject == null)
            return;

        float scroll = Input.mouseScrollDelta.y;

        if (scroll > 0f)
            currentRotation += rotationStep;

        if (scroll < 0f)
            currentRotation -= rotationStep;

        previewObject.transform.rotation =
            Quaternion.Euler(0, currentRotation, 0);
    }

    Bounds GetBounds(GameObject obj)
    {
        Renderer[] rends = obj.GetComponentsInChildren<Renderer>();

        if (rends.Length == 0)
            return new Bounds(obj.transform.position, Vector3.zero);

        Bounds bounds = rends[0].bounds;

        foreach (Renderer r in rends)
            bounds.Encapsulate(r.bounds);

        return bounds;
    }

    void CheckPlacement()
    {
        canPlace = true;
        SetPreviewMaterial(validMaterial);
    }

    void SetPreviewMaterial(Material mat)
    {
        foreach (Renderer r in previewRenderers)
        {
            r.material = mat;
        }
    }

    void Place()
    {
        if (!canPlace)
            return;

        GameObject obj = Instantiate(
            currentBuilding.constructionPrefab,
            previewObject.transform.position,
            previewObject.transform.rotation);
        BuildingIdentity identity =
    obj.GetComponent<BuildingIdentity>();

        if (identity == null)
        {
            identity =
                obj.AddComponent<BuildingIdentity>();
        }

        identity.buildingID =
            currentBuilding.buildingID;

        identity.isFinished = false;
        ConstructionSite site = obj.GetComponent<ConstructionSite>();

        if (site != null)
        {
            site.data = currentBuilding;

            if (currentBuilding.resources != null)
            {
                site.resources = new List<ResourceRequirement>();

                foreach (var r in currentBuilding.resources)
                {
                    ResourceRequirement copy = new ResourceRequirement();

                    copy.type = r.type;
                    copy.requiredAmount = r.requiredAmount;
                    copy.buildPrefab = r.buildPrefab;
                    copy.dropPrefab = r.dropPrefab;
                    copy.currentAmount = 0;

                    site.resources.Add(copy);
                }
            }
        }
        else
        {
            Debug.LogError("ConstructionSite not found on prefab!");
        }

        Destroy(previewObject);
    }

    void CancelBuilding()
    {
        currentBuilding = null;

        if (previewObject != null)
            Destroy(previewObject);

        previewObject = null;

    }
}