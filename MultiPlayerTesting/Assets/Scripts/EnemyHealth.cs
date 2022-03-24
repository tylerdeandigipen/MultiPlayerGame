using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class EnemyHealth : NetworkBehaviour
{
    private NetworkVariable<float> health = new NetworkVariable<float>();
    public float Maxhealth = 10;

    private void Start()
    {
        if (IsServer)
        {
            health.Value = Maxhealth;
        }
    }
    public void takeDamage(float damageToTake)
    {
        health.Value = health.Value - damageToTake;
    }

}
