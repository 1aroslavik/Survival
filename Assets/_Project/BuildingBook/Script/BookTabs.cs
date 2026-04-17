using UnityEngine;

public class BookTabs : MonoBehaviour
{
    public GameObject fireContent;
    public GameObject wallContent;
    public GameObject stairsContent;
    public GameObject otherContent;

    void Start()
    {
        // по умолчанию можно открыть что-то одно
        OpenFire();
    }

    void DisableAll()
    {
        fireContent.SetActive(false);
        wallContent.SetActive(false);
        stairsContent.SetActive(false);
        otherContent.SetActive(false);
    }

    public void OpenFire()
    {
        DisableAll();
        fireContent.SetActive(true);
    }

    public void OpenWall()
    {
        DisableAll();
        wallContent.SetActive(true);
    }

    public void OpenStairs()
    {
        DisableAll();
        stairsContent.SetActive(true);
    }

    public void OpenOther()
    {
        DisableAll();
        otherContent.SetActive(true);
    }
}