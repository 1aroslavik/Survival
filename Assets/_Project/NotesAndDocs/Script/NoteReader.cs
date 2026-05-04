 using UnityEngine;
  using UnityEngine.UI;

  public class NoteReader : MonoBehaviour
  {
      public static NoteReader Instance;

      public GameObject panel;
      public Image noteImage;

      int currentIndex;

      void Awake()
      {
          Instance = this;
          if (panel != null) panel.SetActive(false);
      }

      // Открыть с первой найденной записки
      public void Open()
      {
          if (NoteCollection.Instance == null) return;
          if (NoteCollection.Instance.collected.Count == 0)
          {
              Debug.Log("Записок пока нет");
              return;
          }

          currentIndex = NoteCollection.Instance.collected.Count - 1; // открываем последнюю найденную
          Refresh();
          panel.SetActive(true);
      }

      public void Hide() => panel.SetActive(false);

      public void Next()
      {
          var list = NoteCollection.Instance.collected;
          if (list.Count == 0) return;
          currentIndex = (currentIndex + 1) % list.Count;
          Refresh();
      }

      public void Prev()
      {
          var list = NoteCollection.Instance.collected;
          if (list.Count == 0) return;
          currentIndex = (currentIndex - 1 + list.Count) % list.Count;
          Refresh();
      }

      void Refresh()
      {
          var note = NoteCollection.Instance.collected[currentIndex];
          if (note.noteImage != null)
              noteImage.sprite = note.noteImage;
      }
  }
