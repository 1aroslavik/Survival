using UnityEngine;
using System.Collections;

public class BookController : MonoBehaviour
{
    public HandsController hands;

    // ❗ вместо префаба — ссылка на объект в сцене
    public GameObject bookHandsObject;

    public GameObject bookUI;

    public MonoBehaviour playerController;
    public MonoBehaviour mouseLook;

    Animator anim;

    bool isOpen;
    bool isClosing;

    void Start()
    {
        // на старте выключаем книгу
        if (bookHandsObject != null)
            bookHandsObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (!isOpen)
                OpenBook();
            else
                CloseBook();
        }

        if (isClosing && anim != null)
        {
            AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);

            if (state.IsName("Book_Holster") && state.normalizedTime >= 1f)
            {
                // ❗ просто выключаем
                bookHandsObject.SetActive(false);

                isClosing = false;
                bookUI.SetActive(false);
            }
        }
    }

    void OpenBook()
    {
        // 🔥 УБИРАЕМ оружие/предмет из рук
        if (hands != null)
            hands.ClearHands();

        // включаем книгу
        bookHandsObject.SetActive(true);

        StartCoroutine(SetupAnimator());

        bookUI.SetActive(true);
        EnableUIControl();

        isOpen = true;
    }

    IEnumerator SetupAnimator()
    {
        yield return null;

        anim = bookHandsObject.GetComponentInChildren<Animator>();

        if (anim != null)
        {
            anim.SetBool("isOpen", true);
        }
        else
        {
            Debug.LogError("Animator NOT FOUND!");
        }
    }

    void CloseBook()
    {
        if (anim != null)
        {
            anim.SetBool("isOpen", false);
            isClosing = true;
        }

        DisableUIControl();
        isOpen = false;
    }

    void EnableUIControl()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerController != null)
            playerController.enabled = false;

        if (mouseLook != null)
            mouseLook.enabled = false;
    }
    public void CloseBookFromUI()
    {
        if (!isOpen) return;

        CloseBook();
    }
    void DisableUIControl()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerController != null)
            playerController.enabled = true;

        if (mouseLook != null)
            mouseLook.enabled = true;
    }
}