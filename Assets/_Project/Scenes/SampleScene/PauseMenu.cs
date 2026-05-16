using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Windows")]
    public GameObject pauseMenu;
    public GameObject hud;
    public GameObject tutorialWindow;

    [Header("Save Warning")]
    public GameObject saveWarningPanel;

    [Header("Camera")]
    public MonoBehaviour cameraScript;

    [Header("Scenes")]
    public string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    // =========================
    // оюсгю
    // =========================

    public void PauseGame()
    {
        isPaused = true;

        Time.timeScale = 0f;

        pauseMenu.SetActive(true);

        if (hud != null)
            hud.SetActive(false);

        if (cameraScript != null)
            cameraScript.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // =========================
    // опнднкфхрэ
    // =========================

    public void ResumeGame()
    {
        isPaused = false;

        Time.timeScale = 1f;

        pauseMenu.SetActive(false);

        if (tutorialWindow != null)
            tutorialWindow.SetActive(false);

        if (saveWarningPanel != null)
            saveWarningPanel.SetActive(false);

        if (hud != null)
            hud.SetActive(true);

        if (cameraScript != null)
            cameraScript.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // =========================
    // ймнойю EXIT
    // =========================

    public void OpenSaveWarning()
    {
        pauseMenu.SetActive(false);

        if (saveWarningPanel != null)
            saveWarningPanel.SetActive(true);
    }

    // =========================
    // ймнойю YES
    // =========================

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // =========================
    // ймнойю NO
    // =========================

    public void CloseSaveWarning()
    {
        if (saveWarningPanel != null)
            saveWarningPanel.SetActive(false);

        pauseMenu.SetActive(true);
    }

    // =========================
    // насвемхе
    // =========================

    public void OpenTutorial()
    {
        pauseMenu.SetActive(false);

        if (tutorialWindow != null)
            tutorialWindow.SetActive(true);
    }

    public void CloseTutorial()
    {
        if (tutorialWindow != null)
            tutorialWindow.SetActive(false);

        pauseMenu.SetActive(true);
    }
}