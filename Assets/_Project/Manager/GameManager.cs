using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

   // public AnimalSpawner animalSpawner;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            //animalSpawner = GetComponent<AnimalSpawner>();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}