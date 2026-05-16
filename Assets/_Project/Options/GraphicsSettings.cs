using UnityEngine;
using TMPro;

public class GraphicsSettings : MonoBehaviour
{
    public TMP_Dropdown graphicsDropdown;

    void Start()
    {
        int quality =
            PlayerPrefs.GetInt(
                "GraphicsQuality",
                2
            );

        graphicsDropdown.value =
            quality;

        graphicsDropdown.RefreshShownValue();

        QualitySettings.SetQualityLevel(
            quality
        );

        graphicsDropdown.onValueChanged
            .AddListener(SetQuality);
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(
            index,
            true
        );

        PlayerPrefs.SetInt(
            "GraphicsQuality",
            index
        );

        PlayerPrefs.Save();
    }
}