using UnityEngine;

public class LimitCollider : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Il punto dove il player verrà teletrasportato")]
    public Transform spawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (spawnPoint != null)
            {
                TeleportPlayer(other.gameObject, spawnPoint.position);
                Debug.Log("Player caduto dalla mappa! Teletrasportato allo spawn.");
            }
            else
            {
                Debug.LogWarning("Spawn punto non assegnato nel LimitCollider! Il player non può essere teletrasportato.");
            }
        }
    }

    private void TeleportPlayer(GameObject player, Vector3 targetPosition)
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        
        if (cc != null)
        {
            cc.enabled = false;
            player.transform.position = targetPosition;
            cc.enabled = true;

            FPSInput input = player.GetComponent<FPSInput>();
            if (input != null)
                input.ResetVelocity();
        }
        else
        {
            player.transform.position = targetPosition;
        }
    }
}
