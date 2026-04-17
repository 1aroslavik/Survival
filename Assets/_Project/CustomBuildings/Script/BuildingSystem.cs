using System.Collections.Generic;
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

    // 🔥 ВЫЗЫВАЕТСЯ ИЗ UI (кнопок книги)
    public void SelectBuildingByUI(BuildingData building)
    {
        currentBuilding = building;

        currentRotation = 0f;

        if (previewObject != null)
            Destroy(previewObject);

        // 🔥 ЗАКРЫВАЕМ КНИГУ
        if (bookController != null)
            bookController.CloseBookFromUI();
    }

    void Update()
    {
        // ❗ не строим если курсор на UI
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

        if (Input.GetKeyDown(KeyCode.Escape))
            CancelBuilding();
    }

    void CreatePreview()
    {
        previewObject = Instantiate(currentBuilding.constructionPrefab);

        previewRenderers = previewObject.GetComponentsInChildren<Renderer>();

        // удаляем физику
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

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
            return;

        previewObject.transform.position = hit.point;

        // 🔥 SNAP SYSTEM
        int snapMask = LayerMask.GetMask("SnapPoint");

        Collider[] snapPoints =
            Physics.OverlapSphere(previewObject.transform.position,
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

            if (previewSnapPoints == null)
                return;

            Transform previewClosest = null;
            float minPreviewDist = float.MaxValue;

            foreach (Transform child in previewSnapPoints)
            {
                float dist = Vector3.Distance(
                    child.position,
                    closest.position);

                if (dist < minPreviewDist)
                {
                    minPreviewDist = dist;
                    previewClosest = child;
                }
            }

            if (previewClosest != null)
            {
                Vector3 offset =
                    previewObject.transform.position -
                    previewClosest.position;

                previewObject.transform.position =
                    closest.position + offset;
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

    void CheckPlacement()
    {
        int mask = LayerMask.GetMask("Default", "Building");

        Collider[] hits = Physics.OverlapBox(
            previewObject.transform.position,
            previewObject.transform.localScale / 2f,
            previewObject.transform.rotation,
            mask);

        canPlace = hits.Length == 0;

        SetPreviewMaterial(canPlace ? validMaterial : invalidMaterial);
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

        // создаём объект стройки
        GameObject obj = Instantiate(
            currentBuilding.constructionPrefab,
            previewObject.transform.position,
            previewObject.transform.rotation);

        // получаем ConstructionSite
        ConstructionSite site = obj.GetComponent<ConstructionSite>();

        if (site != null)
        {
            // 🔥 ОБЯЗАТЕЛЬНО передаём BuildingData
            site.data = currentBuilding;

            // 🔥 копируем ресурсы
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

        // удаляем превью
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