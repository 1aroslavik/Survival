  using UnityEngine;

  public class StartingItems : MonoBehaviour
  {
      public ItemData journal;

      void Start()
      {
          if (journal != null && InventoryModel.Instance != null)
              InventoryModel.Instance.TryAdd(journal, 1);
      }
  }
