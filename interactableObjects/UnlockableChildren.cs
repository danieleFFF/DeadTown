using UnityEngine;

public class UnlockableChildren : MonoBehaviour
{
    private UnlockableObj fatherScript;

    void Start()
    {
        fatherScript = GetComponentInParent<UnlockableObj>();

        if (fatherScript == null)
        {
            Debug.LogError("This object doesn't have a father with UnlockableObj!");
            return;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        fatherScript.SetPlayerStatus(true, other);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        fatherScript.SetPlayerStatus(false, other);
    }
}
