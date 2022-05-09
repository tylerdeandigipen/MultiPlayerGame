using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class EnemyHealth : NetworkBehaviour
{
    public NetworkVariable<float> health = new NetworkVariable<float>(
        default,
        NetworkVariableBase.DefaultReadPerm, // Everyone
        NetworkVariableWritePermission.Server);
    public float Maxhealth = 10;
    [SerializeField]
    public float currentHealth;

    private void Start()
    {
        currentHealth = Maxhealth;
        health.Value = Maxhealth;
    }
    public void takeDamage(float damageToTake)
    {
        health.Value = health.Value - damageToTake;
    }
    private void Update()
    {
        currentHealth = health.Value;
    }

}
