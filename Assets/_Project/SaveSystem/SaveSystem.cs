using UnityEngine;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;

    string savePath;

    private void Awake()
    {
        Instance = this;

        savePath = Application.persistentDataPath + "/save.json";
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveGame();
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            LoadGame();
        }
    }
    public void SaveGame()
    {
        SaveData data = new SaveData();

        // PLAYER

        GameObject player = GameObject.FindWithTag("Player");

        PlayerStats stats = player.GetComponent<PlayerStats>();

        // POSITION

        data.posX = player.transform.position.x;
        data.posY = player.transform.position.y;
        data.posZ = player.transform.position.z;

        // STATS

        data.health = stats.health;
        data.hunger = stats.hunger;
        data.thirst = stats.thirst;
        data.stamina = stats.stamina;
        data.radiation = stats.radiation;

        // INVENTORY

        foreach (var slot in InventoryModel.Instance.slots)
        {
            if (slot.isEmpty)
                continue;

            InventoryItemSave itemSave =
                new InventoryItemSave();

            itemSave.itemID = slot.data.itemID;
            itemSave.amount = slot.amount;

            data.inventory.Add(itemSave);
        }
        // BUILDINGS

        BuildingIdentity[] buildings =
            FindObjectsOfType<BuildingIdentity>();

        foreach (var building in buildings)
        {
            BuildingSave save =
                new BuildingSave();

            save.buildingID =
                building.buildingID;

            save.isFinished =
                building.isFinished;

            save.posX =
                building.transform.position.x;

            save.posY =
                building.transform.position.y;

            save.posZ =
                building.transform.position.z;

            save.rotX =
                building.transform.eulerAngles.x;

            save.rotY =
                building.transform.eulerAngles.y;

            save.rotZ =
                building.transform.eulerAngles.z;

            data.buildings.Add(save);
        }
        // JSON

        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(savePath, json);

        Debug.Log("GAME SAVED");
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("NO SAVE FILE");

            return;
        }

        string json = File.ReadAllText(savePath);

        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // PLAYER

        GameObject player = GameObject.FindWithTag("Player");

        PlayerStats stats = player.GetComponent<PlayerStats>();

        // POSITION

        player.transform.position = new Vector3(
            data.posX,
            data.posY,
            data.posZ
        );

        // STATS

        stats.health = data.health;
        stats.hunger = data.hunger;
        stats.thirst = data.thirst;
        stats.stamina = data.stamina;
        stats.radiation = data.radiation;

        // CLEAR INVENTORY

        InventoryModel.Instance.ClearInventory();

        // LOAD INVENTORY

        foreach (var itemSave in data.inventory)
        {
            ItemData item =
                ItemDatabase.Instance.GetByID(
                    itemSave.itemID
                );

            if (item == null)
            {
                Debug.LogWarning(
                    "ITEM NOT FOUND: " +
                    itemSave.itemID
                );

                continue;
            }

            InventoryModel.Instance.TryAdd(
                item,
                itemSave.amount
            );
        }
        // DELETE OLD BUILDINGS

        BuildingIdentity[] oldBuildings =
            FindObjectsOfType<BuildingIdentity>();

        foreach (var old in oldBuildings)
        {
            Destroy(old.gameObject);
        }

        // LOAD BUILDINGS

        foreach (var save in data.buildings)
        {
            BuildingData building =
                BuildingDatabase.Instance.GetByID(
                    save.buildingID
                );

            if (building == null)
                continue;

            GameObject prefab =
                save.isFinished
                ? building.finishedPrefab
                : building.constructionPrefab;

            GameObject obj = Instantiate(
                prefab,
                new Vector3(
                    save.posX,
                    save.posY,
                    save.posZ
                ),
                Quaternion.Euler(
                    save.rotX,
                    save.rotY,
                    save.rotZ
                )
            );

            BuildingIdentity identity =
                obj.GetComponent<BuildingIdentity>();

            if (identity == null)
            {
                identity =
                    obj.AddComponent<BuildingIdentity>();
            }

            identity.buildingID =
                save.buildingID;

            identity.isFinished =
                save.isFinished;
        }
        Debug.Log("GAME LOADED");
    }
}