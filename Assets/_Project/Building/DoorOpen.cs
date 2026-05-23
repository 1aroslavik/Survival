using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    public static DoorOpen currentDoor;

    public float openAngle = 90f;
    public float speed = 2f;

    private bool isOpen = false;

    private Quaternion closedRotation;
    private Quaternion openedRotation;

    private GameObject openText;

    void Start()
    {
        closedRotation = transform.rotation;

        openedRotation = Quaternion.Euler(
            transform.eulerAngles + new Vector3(0, openAngle, 0)
        );

        openText = GameObject.Find("OpenText");
    }

    void Update()
    {
        Ray ray = Camera.main.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0)
        );

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 3f))
        {
            DoorOpen door = hit.collider.GetComponentInParent<DoorOpen>();

            if (door != null)
            {
                currentDoor = door;
            }
            else
            {
                currentDoor = null;
            }
        }
        else
        {
            currentDoor = null;
        }

        if (openText != null)
        {
            openText.SetActive(currentDoor == this);
        }

        if (currentDoor == this && Input.GetKeyDown(KeyCode.E))
        {
            isOpen = !isOpen;
        }

        Quaternion targetRotation = isOpen
            ? openedRotation
            : closedRotation;

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * speed
        );
    }
}