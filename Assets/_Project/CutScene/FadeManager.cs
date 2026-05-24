using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class FadeManager : MonoBehaviour
{
    public CanvasGroup fadeImage;
    public TextMeshProUGUI middleText;
    public float fadeDuration = 1.5f;

    public void FadeToBlackWithText(string text)
    {
        StartCoroutine(DoFade(text));
    }

    IEnumerator DoFade(string text)
    {
        // Темнеем
        yield return StartCoroutine(Fade(0, 1));
        
        // Показываем текст
        middleText.text = text;
        middleText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        
        // Прячем текст и светлеем
        middleText.gameObject.SetActive(false);
        yield return StartCoroutine(Fade(1, 0));
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