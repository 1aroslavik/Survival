using UnityEngine;

public class ViewModelFix : MonoBehaviour
{
    Vector3 pos;
    Quaternion rot;

    void Awake()
    {
        pos = transform.localPosition;
        rot = transform.localRotation;
    }

    void LateUpdate()
    {
        transform.localPosition = pos;
        transform.localRotation = rot;
    }
}
