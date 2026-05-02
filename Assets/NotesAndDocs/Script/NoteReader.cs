  using UnityEngine;
  using UnityEngine.UI;

  public class NoteReader : MonoBehaviour
  {
      public static NoteReader Instance;

      public GameObject panel;
      public Image noteImage;

      void Awake()
      {
          Instance = this;
          if (panel != null) panel.SetActive(false);
      }

      public void Show(ItemData item)
      {
          if (item.noteImage == null)
          {
              Debug.LogWarning($"У {item.itemName} не задана картинка записки");
              return;
          }

          noteImage.sprite = item.noteImage;
          panel.SetActive(true);
      }

      public void Hide()
      {
          panel.SetActive(false);
      }
  }
