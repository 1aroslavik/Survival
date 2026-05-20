using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    [Header("Документы (по порядку)")]
    public Sprite[] documentSprites;

    [Header("UI")]
    public Image documentImage;
    public CanvasGroup documentCanvasGroup;
    public CanvasGroup blackOverlay;
    public CanvasGroup skipPrompt; // перетащи сюда CanvasGroup с текстом "Нажмите пробел"

    [Header("Настройки")]
    public float fadeDuration = 0.8f;
    public float crashFadeDuration = 0.3f;
    public float readDuration = 4f;
    public float timeBetweenDocs = 1f;
    public float skipPromptFadeDuration = 1.5f; // как долго появляется надпись

    [Header("Тряска камеры")]
    public CameraShake cameraShake;

    [Header("Следующая сцена")]
    public string nextSceneName = "SampleScene";

    private Coroutine docsCoroutine;

    void Start()
    {
        documentCanvasGroup.alpha = 0f;
        blackOverlay.alpha = 0f;

        if (skipPrompt != null)
        {
            skipPrompt.alpha = 0f;
            StartCoroutine(FadeWithDuration(skipPrompt, 0f, 1f, skipPromptFadeDuration));
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    public void StartDocuments()
    {
        docsCoroutine = StartCoroutine(ShowAllDocuments());
    }

    public void StopDocuments()
    {
        if (docsCoroutine != null)
            StopCoroutine(docsCoroutine);

        StartCoroutine(Fade(documentCanvasGroup, documentCanvasGroup.alpha, 0f));
    }

    public void TriggerCrash()
    {
        if (cameraShake != null)
            cameraShake.TriggerCrashShake();

        StartCoroutine(CrashSequence());
    }

    IEnumerator ShowAllDocuments()
    {
        foreach (Sprite doc in documentSprites)
        {
            documentImage.sprite = doc;
            yield return Fade(documentCanvasGroup, 0f, 1f);
            yield return new WaitForSeconds(readDuration);
            yield return Fade(documentCanvasGroup, 1f, 0f);
            yield return new WaitForSeconds(timeBetweenDocs);
        }
    }

    IEnumerator CrashSequence()
    {
        yield return FadeWithDuration(blackOverlay, 0f, 1f, crashFadeDuration);
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator Fade(CanvasGroup group, float from, float to)
    {
        yield return FadeWithDuration(group, from, to, fadeDuration);
    }

    IEnumerator FadeWithDuration(CanvasGroup group, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        group.alpha = to;
    }
}