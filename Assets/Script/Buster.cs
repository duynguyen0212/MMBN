using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Buster : NetworkBehaviour
{
    public Transform bulletSpawnPoint;
    public PlayerHealth playerHealth;

    //Raycast infront of player to check collision and deal damage
    public void Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(bulletSpawnPoint.transform.position, bulletSpawnPoint.transform.forward, out hit, 50f))
        {
            if (hit.collider.CompareTag("Player1"))
            {
                playerHealth = hit.transform.GetComponent<PlayerHealth>();
                playerHealth.TakeDamageServerRpc();
            }
        }

    }
}
