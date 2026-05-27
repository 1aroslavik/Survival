using UnityEngine;
using System.Collections;
using TMPro;

public class FadeManager : MonoBehaviour
{
    public CanvasGroup fadeImage;
    public TextMeshProUGUI middleText;
    public TextMeshProUGUI overlayText;
    public float fadeDuration = 1.5f;
    public GameObject newspaperCanvas;
    public string mainMenuScene = "MainMenuScene";

    private Coroutine overlayCoroutine;

    void Awake()
    {
        middleText.gameObject.SetActive(false);
        overlayText.gameObject.SetActive(false);
    }

    public void FadeToBlackWithText(string text)
    {
        Debug.Log($"FadeToBlackWithText called at {Time.time}: {text}");
        if (overlayCoroutine != null)
        {
            StopCoroutine(overlayCoroutine);
            overlayCoroutine = null;
        }
        overlayText.gameObject.SetActive(false);
        StartCoroutine(DoFade(text));
    }

    public void ShowTextOverlay(string text)
    {
        Debug.Log($"ShowTextOverlay called at {Time.time}: {text}");
        if (overlayCoroutine != null) StopCoroutine(overlayCoroutine);
        overlayCoroutine = StartCoroutine(DoShowText(text));
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

    IEnumerator DoShowText(string text)
    {
        overlayText.text = text;
        overlayText.gameObject.SetActive(true);

        Color c = overlayText.color;
        overlayText.color = new Color(c.r, c.g, c.b, 1f);

        yield return new WaitForSeconds(3f);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            overlayText.color = new Color(c.r, c.g, c.b, 1f - t);
            yield return null;
        }

        overlayText.gameObject.SetActive(false);
        overlayText.color = c;
        overlayCoroutine = null;
    }

    IEnumerator DoEnding()
    {
        yield return StartCoroutine(Fade(0, 1));

        newspaperCanvas.SetActive(true);

        yield return new WaitForSeconds(6f);

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