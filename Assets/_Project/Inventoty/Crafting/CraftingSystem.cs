using System.Collections.Generic;
using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem Instance;

    public List<CraftRecipe> recipes;

    void Awake()
    {
        Instance = this;
    }

    public CraftRecipe currentRecipe;

    public void CheckRecipes(List<ItemData> items)
    {
        currentRecipe = null;

        Debug.Log("Items in craft area:");
        foreach (var i in items)
            Debug.Log(" - " + i.name);

        foreach (var recipe in recipes)
        {
            Debug.Log("Checking recipe: " + recipe.name);

            if (MatchRecipe(recipe, items))
            {
                Debug.Log("Recipe found!");

                currentRecipe = recipe;

                // 🔥 показываем ТЕКСТ вместо предмета
                CraftUI.Instance.ShowRecipe(recipe);

                return;
            }
        }

        Debug.Log("No recipe found");

        CraftUI.Instance.HideAll();
    }

    bool MatchRecipe(CraftRecipe recipe, List<ItemData> items)
    {
        // 🔥 считаем предметы в крафте
        Dictionary<ItemData, int> itemCounts = new();

        foreach (var item in items)
        {
            if (!itemCounts.ContainsKey(item))
                itemCounts[item] = 0;

            itemCounts[item]++;
        }

        // 🔥 считаем предметы в рецепте
        Dictionary<ItemData, int> recipeCounts = new();

        foreach (var ing in recipe.ingredients)
        {
            if (!recipeCounts.ContainsKey(ing.item))
                recipeCounts[ing.item] = 0;

            recipeCounts[ing.item] += ing.amount;
        }

        // 🔥 1. проверка: одинаковое количество типов предметов
        if (itemCounts.Count != recipeCounts.Count)
            return false;

        // 🔥 2. проверка: точное совпадение количества
        foreach (var pair in recipeCounts)
        {
            if (!itemCounts.ContainsKey(pair.Key))
                return false;

            if (itemCounts[pair.Key] != pair.Value)
                return false;
        }

        return true;
    }

    public void Craft()
    {
        if (currentRecipe == null) return;

        var inventory = FindObjectOfType<InventoryModel>();

        inventory.TryAdd(currentRecipe.result, currentRecipe.resultAmount);

        CraftArea.Instance.Clear();

        FindObjectOfType<InventoryView>().Render();

        // 🔥 скрываем текст и кнопку
        CraftUI.Instance.HideAll();

        Debug.Log("CRAFT PRESSED");

        currentRecipe = null;
    }
}