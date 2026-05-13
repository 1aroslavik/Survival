using System.Collections.Generic;
using UnityEngine;

public class InventoryView : MonoBehaviour
{
    public InventoryModel model;
    public List<Transform> slotPoints = new();

    Dictionary<int, List<GameObject>> visuals = new();
    Dictionary<int, string> slotSignatures = new();

    void Start()
    {
        if (model == null)
            model = InventoryModel.Instance;

        Render();

        model.OnInventoryChanged += Render;
    }

    public void Render()
    {
        if (model == null) return;

        int notesAnchor = FindNotesAnchorSlot();

        for (int i = 0; i < slotPoints.Count; i++)
        {
            string newSig = BuildSignature(i, notesAnchor);
            string oldSig;
            slotSignatures.TryGetValue(i, out oldSig);

            if (newSig == oldSig)
                continue;

            ClearSlot(i);
            BuildSlot(i, notesAnchor);

            slotSignatures[i] = newSig;
        }
    }

    // Слот, на котором визуально складываются ВСЕ записки —
    // это самый ранний модельный слот, где сейчас лежит записка.
    int FindNotesAnchorSlot()
    {
        for (int k = 0; k < model.slots.Count && k < slotPoints.Count; k++)
        {
            var s = model.slots[k];
            if (s == null || s.isEmpty || s.data == null) continue;
            if (s.data.resourceType == ResourceType.Note) return k;
        }
        return -1;
    }

    string BuildSignature(int i, int notesAnchor)
    {
        if (i >= model.slots.Count) return "";
        var slot = model.slots[i];
        if (slot == null || slot.isEmpty || slot.data == null) return "";

        bool isNote = slot.data.resourceType == ResourceType.Note;

        // Слот-якорь — подпись зависит от ВСЕХ записок инвентаря.
        if (isNote && i == notesAnchor)
        {
            var sb = new System.Text.StringBuilder("NOTES:");
            for (int k = 0; k < model.slots.Count; k++)
            {
                var s = model.slots[k];
                if (s == null || s.isEmpty || s.data == null) continue;
                if (s.data.resourceType != ResourceType.Note) continue;
                sb.Append(k).Append('=').Append(s.data.GetInstanceID()).Append('x').Append(s.amount).Append(';');
            }
            return sb.ToString();
        }

        // Записки на не-якорных слотах визуально пустые.
        if (isNote) return "";

        return slot.data.GetInstanceID() + ":" + slot.amount;
    }

    void ClearSlot(int i)
    {
        if (!visuals.TryGetValue(i, out var list)) return;

        foreach (var obj in list)
        {
            if (obj != null)
                Destroy(obj);
        }

        visuals.Remove(i);
    }

    void BuildSlot(int i, int notesAnchor)
    {
        if (i >= model.slots.Count) return;

        var slot = model.slots[i];
        if (slot.isEmpty || slot.data == null || slot.data.inventoryPrefab == null)
            return;

        bool isNote = slot.data.resourceType == ResourceType.Note;

        // Якорный слот собирает ВСЕ записки кучкой друг на друге.
        if (isNote && i == notesAnchor)
        {
            BuildNotesPile(i);
            return;
        }

        // Записки на остальных слотах визуально пропускаем.
        if (isNote) return;

        var prefab = slot.data.inventoryPrefab;

        int visualCount = Mathf.Min(slot.amount, 5);

        float itemSize = GetItemSize(prefab);
        List<Vector3> positions = GenerateClusterPositions(visualCount, itemSize);

        var list = new List<GameObject>();

        for (int j = 0; j < visualCount; j++)
        {
            Vector3 originalScale = prefab.transform.localScale;
            Quaternion originalRotation = prefab.transform.localRotation;

            var obj = Instantiate(prefab, slotPoints[i], false);

            var itemView = obj.GetComponent<InventoryItemsView>();
            if (itemView != null)
            {
                itemView.model = model;
                itemView.slotIndex = i;
            }

            obj.transform.localPosition = positions[j];
            obj.transform.localRotation = originalRotation * GetRandomRotation();
            obj.transform.localScale = originalScale;

            RecenterToBounds(obj, slotPoints[i], positions[j]);

            list.Add(obj);
        }

        visuals[i] = list;
    }

    void BuildNotesPile(int anchorIndex)
    {
        var slotPoint = slotPoints[anchorIndex];

        var noteSlotIndices = new List<int>();
        for (int k = 0; k < model.slots.Count; k++)
        {
            var s = model.slots[k];
            if (s == null || s.isEmpty || s.data == null) continue;
            if (s.data.resourceType != ResourceType.Note) continue;
            if (s.data.inventoryPrefab == null) continue;
            noteSlotIndices.Add(k);
        }

        if (noteSlotIndices.Count == 0) return;

        int visualCount = Mathf.Min(noteSlotIndices.Count, 5);
        float itemSize = GetItemSize(model.slots[noteSlotIndices[0]].data.inventoryPrefab);
        List<Vector3> positions = GenerateClusterPositions(visualCount, itemSize);

        var list = new List<GameObject>();

        for (int j = 0; j < visualCount; j++)
        {
            int modelSlotIdx = noteSlotIndices[j];
            var data = model.slots[modelSlotIdx].data;
            var prefab = data.inventoryPrefab;

            Vector3 originalScale = prefab.transform.localScale;
            Quaternion originalRotation = prefab.transform.localRotation;

            var obj = Instantiate(prefab, slotPoint, false);

            var itemView = obj.GetComponent<InventoryItemsView>();
            if (itemView != null)
            {
                itemView.model = model;
                // Hover/E должны открывать именно ту записку, на которую навели.
                itemView.slotIndex = modelSlotIdx;
            }

            obj.transform.localPosition = positions[j];
            obj.transform.localRotation = originalRotation * Quaternion.Euler(0f, 0f, Random.Range(-10f, 10f));
            obj.transform.localScale = originalScale;

            RecenterToBounds(obj, slotPoint, positions[j]);

            list.Add(obj);
        }

        visuals[anchorIndex] = list;
    }

    // Сдвигаем объект так, чтобы центр его меша оказался в нужной точке слота
    // (а не корень трансформа, который у префаба может быть смещён).
    void RecenterToBounds(GameObject obj, Transform slotPoint, Vector3 localTarget)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers == null || renderers.Length == 0) return;

        Bounds b = renderers[0].bounds;
        for (int k = 1; k < renderers.Length; k++)
            b.Encapsulate(renderers[k].bounds);

        Vector3 worldTarget = slotPoint.TransformPoint(localTarget);
        Vector3 delta = worldTarget - b.center;
        obj.transform.position += delta;
    }

    // 🔥 Получаем "радиус" предмета
    float GetItemSize(GameObject prefab)
    {
        Renderer r = prefab.GetComponentInChildren<Renderer>();
        if (r == null) return 0.3f;

        return Mathf.Max(r.bounds.size.x, r.bounds.size.y) * 0.5f;
    }

    // 🔥 КУЧКА с учётом размера предмета
    List<Vector3> GenerateClusterPositions(int count, float itemSize)
    {
        List<Vector3> result = new List<Vector3>();

        // Один предмет — строго в центр слота, без рандомного смещения.
        if (count <= 1)
        {
            result.Add(Vector3.zero);
            return result;
        }

        float minDistance = itemSize * 1.8f;
        float radius = itemSize * (0.8f + count * 0.3f);
        int maxTries = 200;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = Vector3.zero;
            bool valid = false;

            for (int t = 0; t < maxTries; t++)
            {
                float r = Mathf.Pow(Random.value, 1.8f) * radius;
                float angle = Random.Range(0f, Mathf.PI * 2f);

                float x = Mathf.Cos(angle) * r;
                float y = Mathf.Sin(angle) * r;

                pos = new Vector3(x, i * itemSize * 0.15f, 0f);

                valid = true;

                foreach (var existing in result)
                {
                    if (Vector3.Distance(existing, pos) < minDistance)
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid) break;
            }

            result.Add(pos);
        }

        Vector3 center = Vector3.zero;
        foreach (var p in result) center += p;
        center /= result.Count;

        for (int i = 0; i < result.Count; i++)
        {
            result[i] = Vector3.Lerp(result[i], center, 0.25f);
        }

        return result;
    }

    Quaternion GetRandomRotation()
    {
        float randomY = Random.Range(-60f, 60f);
        float randomX = Random.Range(-30f, 30f);
        float randomZ = Random.Range(-15f, 15f);

        return Quaternion.Euler(randomX, randomY, randomZ);
    }
}
