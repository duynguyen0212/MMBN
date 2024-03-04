using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saber : MonoBehaviour
{
    public Transform saberRay;
    public PlayerHealth playerHealth;
    public void Slash()
    {
        // Check if the bulletSpawnPoint is not null
        if (saberRay != null)
        {
            // Get the position and forward direction of the bulletSpawnPoint
            Vector3 raycastOrigin = saberRay.position;
            Vector3 raycastDirection = saberRay.forward;

            // Perform the raycast
            RaycastHit hitInfo;
            if (Physics.Raycast(raycastOrigin, raycastDirection, out hitInfo, 5f))
            {
                if (hitInfo.collider.CompareTag("Player1"))
                {
                    playerHealth = hitInfo.transform.GetComponent<PlayerHealth>();
                    playerHealth.TakeDamageServerRpc();
                }
            }
            Debug.DrawRay(raycastOrigin, raycastDirection, Color.cyan, 5f);

        }
    }
}
