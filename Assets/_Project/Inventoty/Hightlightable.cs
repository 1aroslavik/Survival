using UnityEngine;

public class Highlightable : MonoBehaviour
{
    public Material highlightMaterial;

    private Renderer[] rends;

    private Material[][] originalMats;
    private Material[][] highlightMats;

    void Start()
    {
        // 🔥 берём ВСЕ Renderer (LOD0, LOD1, LOD2)
        rends = GetComponentsInChildren<Renderer>();

        originalMats = new Material[rends.Length][];
        highlightMats = new Material[rends.Length][];

        for (int i = 0; i < rends.Length; i++)
        {
            originalMats[i] = rends[i].materials;

            highlightMats[i] = new Material[originalMats[i].Length + 1];

            for (int j = 0; j < originalMats[i].Length; j++)
                highlightMats[i][j] = originalMats[i][j];

            highlightMats[i][highlightMats[i].Length - 1] = highlightMaterial;
        }
    }

    public void Highlight(bool state)
    {
        for (int i = 0; i < rends.Length; i++)
        {
            if (state)
                rends[i].materials = highlightMats[i];
            else
                rends[i].materials = originalMats[i];
        }
    }
}