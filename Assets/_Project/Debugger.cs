using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

public class Debugger : MonoBehaviour
{
    private Image img;
    private Sprite lastSprite;

    void Awake()
    {
        img = GetComponent<Image>();
        lastSprite = img.sprite;
    }

    void OnEnable()
    {
        Check("OnEnable");
    }

    void Start()
    {
        Check("Start");
    }

    void Update()
    {
        Check("Update");
    }

    void Check(string phase)
    {
        if (img.sprite != lastSprite)
        {
            lastSprite = img.sprite;

            StackTrace stackTrace = new StackTrace(true);

            UnityEngine.Debug.Log(
                $"SPRITE CHANGED → {img.sprite.name}\nPHASE: {phase}\nSTACK:\n{stackTrace}",
                this
            );
        }
    }
}