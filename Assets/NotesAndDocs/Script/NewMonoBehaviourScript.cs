  using System.Collections.Generic;
  using UnityEngine;

  public class NoteCollection : MonoBehaviour
  {
      public static NoteCollection Instance;

      public List<ItemData> collected = new();

      void Awake()
      {
          if (Instance != null && Instance != this) { Destroy(gameObject); return; }
          Instance = this;
      }

      public void Add(ItemData note)
      {
          if (note == null) return;
          if (collected.Contains(note)) return;
          collected.Add(note);
          Debug.Log($"📜 Записка добавлена: {note.itemName}. Всего: {collected.Count}");
      }
  }
