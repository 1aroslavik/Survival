using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject settingsPanel;
    public void LoadSavedGame(string sceneName)
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        SceneManager.LoadScene(sceneName);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        SaveSystem.Instance.LoadGame();
    }

    public void NewGame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    public void OpenSettings()
    {
        mainPanel.SetActive(false);

        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);

        mainPanel.SetActive(true);
    }

    // ================= EXIT =================

    public void ExitGame()
    {
        Application.Quit();

        Debug.Log("GAME CLOSED");
    }
}