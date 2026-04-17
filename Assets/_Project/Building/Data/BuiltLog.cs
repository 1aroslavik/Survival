using UnityEngine;

public class BuiltLog : MonoBehaviour
{
    public LogType logType;

    void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("BuiltLog");
    }
}