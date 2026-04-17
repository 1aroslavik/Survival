using UnityEngine;

[System.Serializable]
public class CraftIngredient
{
    public ItemData item;
    public int amount;
}

[CreateAssetMenu(menuName = "Crafting/Recipe")]
public class CraftRecipe : ScriptableObject
{
    public CraftIngredient[] ingredients;

    public ItemData result;
    public int resultAmount = 1;
}