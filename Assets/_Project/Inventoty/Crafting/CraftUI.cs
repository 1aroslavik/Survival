using UnityEngine;
using TMPro;

public class CraftUI : MonoBehaviour
{
    public static CraftUI Instance;

    public GameObject craftButton;
    public TMP_Text craftText;

    void Awake()
    {
        Instance = this;

        craftButton.SetActive(false);
        craftText.gameObject.SetActive(false);
    }

    public void ShowRecipe(CraftRecipe recipe)
    {
        craftText.text = recipe.result.itemName;

        craftText.gameObject.SetActive(true);
        craftButton.SetActive(true);
    }

    public void HideAll()
    {
        craftText.gameObject.SetActive(false);
        craftButton.SetActive(false);
    }
}