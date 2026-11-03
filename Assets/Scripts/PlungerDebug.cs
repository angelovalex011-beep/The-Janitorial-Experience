using UnityEngine;

public class PlungerDebug : MonoBehaviour
{
    void OnDisable()
    {
        Debug.Log("Plunger disabled!\n" + System.Environment.StackTrace, this);
    }

    void OnEnable()
{
    Debug.Log("Plunger ENABLED\n" + System.Environment.StackTrace, this);
}
}