using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class RadioStationInteract : MonoBehaviour
{
    [Header("Условие")]
    public int requiredNotes = 7;

    [Header("Сцена концовки")]
    public string endingSceneName = "EndingCutscene";

    [Header("Взаимодействие")]
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    public Camera playerCamera;

    [Header("UI")]
    [Tooltip("GameObject с подсказкой 'Нажмите E' — появляется при наведении")]
    public GameObject interactHint;
    [Tooltip("TMP_Text для сообщения 'Im not done yet'")]
    public TMP_Text messageText;
    public float messageDuration = 2.5f;
    [TextArea] public string notReadyMessage = "Im not done yet. I need to find all the notes.";

    [Header("Дополнительные объекты для подсветки")]
    [Tooltip("Сюда можно перетащить другие Highlightable (например, Radio), чтобы они светились вместе с этим объектом")]
    public Highlightable[] extraHighlights;

    [Header("Overlay-объекты (включаются при наведении)")]
    [Tooltip("GameObject'ы, которые включаются при наведении и выключаются при потере фокуса. Например, копия меша с HighlightMat.")]
    public GameObject[] highlightOverlays;

    Collider myCollider;
    Highlightable highlight;
    bool playerLooking;
    Coroutine messageRoutine;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        myCollider = GetComponent<Collider>();
        highlight = GetComponentInChildren<Highlightable>();

        if (interactHint != null) interactHint.SetActive(false);
        if (messageText != null) messageText.gameObject.SetActive(false);
    }

    void Update()
    {
        bool looking = IsPlayerLookingAtMe();

        if (looking != playerLooking)
        {
            playerLooking = looking;
            if (interactHint != null) interactHint.SetActive(looking);
            if (highlight != null) highlight.Highlight(looking);
            if (extraHighlights != null)
            {
                foreach (var h in extraHighlights)
                    if (h != null) h.Highlight(looking);
            }
            if (highlightOverlays != null)
            {
                foreach (var go in highlightOverlays)
                    if (go != null) go.SetActive(looking);
            }
        }

        if (playerLooking && Input.GetKeyDown(interactKey))
            TryInteract();
    }

    bool IsPlayerLookingAtMe()
    {
        if (playerCamera == null || myCollider == null) return false;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            return hit.collider == myCollider || hit.collider.transform.IsChildOf(transform);

        return false;
    }

    void TryInteract()
    {
        int count = NoteCollection.Instance != null ? NoteCollection.Instance.collected.Count : 0;

        if (count < requiredNotes)
        {
            int left = requiredNotes - count;
            ShowMessage($"{notReadyMessage} (left: {left})");
            return;
        }

        SceneManager.LoadScene(endingSceneName);
    }

    void ShowMessage(string text)
    {
        if (messageText == null) return;
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = StartCoroutine(MessageRoutine(text));
    }

    IEnumerator MessageRoutine(string text)
    {
        messageText.text = text;
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(messageDuration);
        messageText.gameObject.SetActive(false);
    }
}
