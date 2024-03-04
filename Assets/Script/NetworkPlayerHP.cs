using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerHP : NetworkBehaviour
{
    //Network variable HP for players
    [HideInInspector]
    public NetworkVariable<int> HP = new NetworkVariable<int>();
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        HP.Value = 100;
    }
}
