using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class FadeManager : MonoBehaviour
{
    public CanvasGroup fadeImage;
    public TextMeshProUGUI middleText;
    public float fadeDuration = 1.5f;
    public GameObject newspaperCanvas;
    public string mainMenuScene = "MainMenuScene";

    public void FadeToBlackWithText(string text)
    {
        StartCoroutine(DoFade(text));
    }

    public void ShowNewspaperAndEnd()
    {
        StartCoroutine(DoEnding());
    }

    IEnumerator DoFade(string text)
    {
        yield return StartCoroutine(Fade(0, 1));
        
        middleText.text = text;
        middleText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        
        middleText.gameObject.SetActive(false);
        yield return StartCoroutine(Fade(1, 0));
    }

IEnumerator DoEnding()
{
    // Темнеем
    yield return StartCoroutine(Fade(0, 1));
    
    // Включаем газету пока экран чёрный
    newspaperCanvas.SetActive(true);
    
    // Газета висит
    yield return new WaitForSeconds(6f);
    
    // Темнеем и уходим
    yield return StartCoroutine(Fade(0, 1));
    UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
}
    IEnumerator Fade(float from, float to)
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeImage.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        fadeImage.alpha = to;
    }
}