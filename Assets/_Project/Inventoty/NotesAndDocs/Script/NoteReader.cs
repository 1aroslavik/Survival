using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-100)]
public class NoteReader : MonoBehaviour
{
    public static NoteReader Instance;

    public GameObject panel;
    public Image noteImage;

    [Header("Buttons (можно не назначать — найдутся по имени)")]
    public Button nextButton;
    public Button prevButton;
    public Button closeButton;

    int currentIndex;

    public bool IsOpen => panel != null && panel.activeSelf;

    void Awake()
    {
        Instance = this;
        Debug.Log("[NoteReader] Awake — Instance установлен");

        AutoFindButtons();
        WireButtons();

        if (noteImage != null) noteImage.raycastTarget = false;

        if (panel != null) panel.SetActive(false);
    }

    void OnEnable()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        if (!IsOpen) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            Prev();
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            Next();

        if (Input.GetMouseButtonDown(0))
            HandleManualMouseClick();
    }

    void HandleManualMouseClick()
    {
        Vector2 mouse = Input.mousePosition;
        if (IsMouseOverButton(nextButton, mouse)) { Debug.Log("[NoteReader] Next via manual click"); Next(); return; }
        if (IsMouseOverButton(prevButton, mouse)) { Debug.Log("[NoteReader] Prev via manual click"); Prev(); return; }
        if (IsMouseOverButton(closeButton, mouse)) { Debug.Log("[NoteReader] Close via manual click"); Hide(); return; }
    }

    bool IsMouseOverButton(Button btn, Vector2 screenPos)
    {
        if (btn == null) return false;
        var rect = btn.GetComponent<RectTransform>();
        if (rect == null) return false;

        var canvas = btn.GetComponentInParent<Canvas>();
        Camera cam = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = canvas.worldCamera;

        return RectTransformUtility.RectangleContainsScreenPoint(rect, screenPos, cam);
    }

    void AutoFindButtons()
    {
        if (panel == null) return;

        if (nextButton == null)
        {
            var t = panel.transform.Find("NextButton");
            if (t != null) nextButton = t.GetComponent<Button>();
        }
        if (prevButton == null)
        {
            var t = panel.transform.Find("PrevButton");
            if (t != null) prevButton = t.GetComponent<Button>();
        }
        if (closeButton == null)
        {
            var t = panel.transform.Find("CloseButton");
            if (t != null) closeButton = t.GetComponent<Button>();
        }

        Debug.Log($"[NoteReader] Кнопки: next={nextButton != null}, prev={prevButton != null}, close={closeButton != null}");
    }

    void WireButtons()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() => { Debug.Log("[NoteReader] Next clicked"); Next(); });
        }
        if (prevButton != null)
        {
            prevButton.onClick.RemoveAllListeners();
            prevButton.onClick.AddListener(() => { Debug.Log("[NoteReader] Prev clicked"); Prev(); });
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => { Debug.Log("[NoteReader] Close clicked"); Hide(); });
        }
    }

    public void Open()
    {
        if (NoteCollection.Instance == null) return;
        if (NoteCollection.Instance.collected.Count == 0)
        {
            Debug.Log("Записок пока нет");
            return;
        }

        currentIndex = NoteCollection.Instance.collected.Count - 1;
        Refresh();
        if (panel != null) panel.SetActive(true);
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void OpenForItem(ItemData note)
    {
        Debug.Log($"[NoteReader] OpenForItem вызван для: {(note != null ? note.itemName : "null")}");

        if (note == null) return;
        NoteCollection.GetOrCreate();
        if (panel == null) { Debug.LogWarning("[NoteReader] panel не назначен"); return; }
        if (noteImage == null) { Debug.LogWarning("[NoteReader] noteImage не назначен"); return; }
        if (note.noteImage == null)
        {
            Debug.LogWarning($"[NoteReader] У ItemData '{note.itemName}' не заполнено Note Image");
            return;
        }

        var list = NoteCollection.Instance.collected;
        int idx = list.IndexOf(note);
        if (idx < 0)
        {
            NoteCollection.Instance.Add(note);
            idx = list.IndexOf(note);
        }
        if (idx < 0) return;

        currentIndex = idx;
        Refresh();
        panel.SetActive(true);
    }

    public void Next()
    {
        if (NoteCollection.Instance == null) return;
        var list = NoteCollection.Instance.collected;
        if (list.Count == 0) return;
        currentIndex = (currentIndex + 1) % list.Count;
        Refresh();
    }

    public void Prev()
    {
        if (NoteCollection.Instance == null) return;
        var list = NoteCollection.Instance.collected;
        if (list.Count == 0) return;
        currentIndex = (currentIndex - 1 + list.Count) % list.Count;
        Refresh();
    }

    void Refresh()
    {
        if (NoteCollection.Instance == null || noteImage == null) return;
        var note = NoteCollection.Instance.collected[currentIndex];
        if (note != null && note.noteImage != null)
        {
            noteImage.sprite = note.noteImage;
            Debug.Log($"[NoteReader] Refresh idx={currentIndex} ItemData='{note.name}' itemName='{note.itemName}' sprite='{note.noteImage.name}'");
        }
    }
}
