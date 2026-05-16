using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuildResourcesUI : MonoBehaviour
{
    public static BuildResourcesUI Instance;

    [Header("UI")]
    public TMP_Text resourcesText;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        UpdateResources();
    }

    void UpdateResources()
    {
        ConstructionSite[] sites =
            FindObjectsOfType<ConstructionSite>();

        Dictionary<ResourceType, int> total =
            new Dictionary<ResourceType, int>();

        foreach (ConstructionSite site in sites)
        {
            if (site == null)
                continue;

            foreach (var r in site.resources)
            {
                int left =
                    r.requiredAmount - r.currentAmount;

                if (left <= 0)
                    continue;

                if (total.ContainsKey(r.type))
                    total[r.type] += left;
                else
                    total.Add(r.type, left);
            }
        }

        if (total.Count == 0)
        {
            resourcesText.text = "";
            return;
        }

        resourcesText.text = "Resources:\n";

        foreach (var pair in total)
        {
            resourcesText.text +=
                $"{pair.Key}: {pair.Value}\n";
        }
    }
}