using UnityEngine;
using UnityEngine.UI;

public class FullscreenSettings : MonoBehaviour
{
    public Toggle fullscreenToggle;

    void Start()
    {
        fullscreenToggle.isOn =
            Screen.fullScreen;

        fullscreenToggle.onValueChanged
            .AddListener(SetFullscreen);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen =
            isFullscreen;
    }
}