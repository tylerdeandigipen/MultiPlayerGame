using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Samples;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(ClientNetworkTransform))]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField]
    private float walkSpeed = 3.5f;

    [SerializeField]
    private Vector2 defaultPositionRange = new Vector2(4, -4);
    Rigidbody rb;
    [SerializeField]
    private NetworkVariable<float> xCord = new NetworkVariable<float>();

    [SerializeField]
    private NetworkVariable<float> zCord = new NetworkVariable<float>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        if (IsOwner)
        {
            transform.position = new Vector3(0,0,0);
        }
    }
    void FixedUpdate()
    {
        if (IsClient && IsOwner)
        {
            ClientInput();
        }
    }
    private void ClientInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        if (vertical > 0 || vertical < 0)
            rb.AddForce(vertical > 0 ? Vector3.forward * walkSpeed : Vector3.back * walkSpeed);
        if (horizontal > 0 || horizontal < 0)
            rb.AddForce(horizontal > 0 ? Vector3.right * walkSpeed : Vector3.left * walkSpeed);
        zCord.Value = this.transform.position.z;
        xCord.Value = this.transform.position.x;
    }
}
