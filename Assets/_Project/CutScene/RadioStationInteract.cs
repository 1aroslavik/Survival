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
    [TextArea] public string notReadyMessage = "Im not done yet. ";

    Collider myCollider;
    Highlightable highlight;
    bool playerLooking;
    Coroutine messageRoutine;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        myCollider = GetComponent<Collider>();
        if (myCollider == null) myCollider = GetComponentInChildren<Collider>();
        highlight = GetComponentInChildren<Highlightable>();

        if (playerCamera == null)
            Debug.LogError($"[RadioStationInteract] {name}: Player Camera не назначена и Camera.main = null (нет камеры с тегом MainCamera).");
        if (myCollider == null)
            Debug.LogError($"[RadioStationInteract] {name}: на объекте нет Collider — добавь BoxCollider или MeshCollider.");
        if (highlight == null)
            Debug.LogWarning($"[RadioStationInteract] {name}: нет компонента Highlightable — подсветки не будет.");

        if (interactHint != null)
        {
            // включаем все дочерние объекты hint (текст внутри должен быть активен)
            foreach (Transform child in interactHint.transform)
                child.gameObject.SetActive(true);
            interactHint.SetActive(false);
        }
        // messageText гасим только если он НЕ внутри interactHint
        if (messageText != null && (interactHint == null || !messageText.transform.IsChildOf(interactHint.transform)))
            messageText.gameObject.SetActive(false);

        Debug.Log($"[RadioStationInteract] START на объекте '{name}'. Camera={playerCamera}, Collider={myCollider}, Highlight={highlight}");
    }

    void Update()
    {
        bool looking = IsPlayerLookingAtMe();

        // ФОРС: каждый кадр выставляем нужное состояние, чтобы никто другой не перебил
        if (interactHint != null && interactHint.activeSelf != looking)
        {
            interactHint.SetActive(looking);
            Debug.Log($"[RadioStationInteract] {name}: interactHint.SetActive({looking}). hint='{interactHint.name}' activeInHierarchy={interactHint.activeInHierarchy}");
        }

        if (looking != playerLooking)
        {
            playerLooking = looking;
            if (highlight != null) highlight.Highlight(looking);
            Debug.Log($"[RadioStationInteract] {name}: playerLooking -> {looking}");
        }

        if (playerLooking && Input.GetKeyDown(interactKey))
            TryInteract();
    }

    bool IsPlayerLookingAtMe()
    {
        if (playerCamera == null) return false;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red);

        RaycastHit[] hits = Physics.RaycastAll(ray, interactDistance, ~0, QueryTriggerInteraction.Collide);
        string log = $"[{name}] hits ({hits.Length}): ";
        foreach (var h in hits)
            log += $"'{h.collider.name}'({h.distance:F2}m), ";
        Debug.Log(log);

        foreach (var h in hits)
        {
            if (h.collider.transform == transform) return true;
            if (h.collider.transform.IsChildOf(transform)) return true;
            if (transform.IsChildOf(h.collider.transform)) return true;
            var found = h.collider.GetComponentInParent<RadioStationInteract>();
            if (found == this) return true;
        }
        return false;
    }

    void TryInteract()
    {
        int count = NoteCollection.Instance != null ? NoteCollection.Instance.collected.Count : 0;

        if (count < requiredNotes)
        {
            int left = requiredNotes - count;
            ShowMessage($"{notReadyMessage} (notes left: {left})");
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
