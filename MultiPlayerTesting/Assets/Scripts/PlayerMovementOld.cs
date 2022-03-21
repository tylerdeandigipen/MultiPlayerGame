using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovementOld : NetworkBehaviour
{
    [SerializeField]
    private float walkSpeed = 3.5f;

    [SerializeField]
    private Vector2 defaultPositionRange = new Vector2(4, -4);

    [SerializeField]
    private NetworkVariable<float> forwardBackPosition = new NetworkVariable<float>();

    [SerializeField]
    private NetworkVariable<float> leftRightPosition = new NetworkVariable<float>();

    //client cashing
    private float oldForwardBackPosition;
    private float oldLeftRightPosition;
    void Start()
    {
        transform.position = new Vector3(Random.Range(defaultPositionRange.x, defaultPositionRange.y), 0, Random.Range(defaultPositionRange.x, defaultPositionRange.y));
    }
    void FixedUpdate()
    {
        if (IsServer)
        {
            UpdateServer(); 
        }
        if (IsClient && IsOwner)
        {
            UpdateClient();
        }
    }
    private void UpdateServer()
    {
        transform.position = new Vector3(transform.position.x + leftRightPosition.Value, 0, transform.position.z + forwardBackPosition.Value);
    }

    private void UpdateClient()
    {
        float forwardBackward = 0;
        float leftright = 0;
        if (Input.GetKey(KeyCode.W))
        {
            forwardBackward += walkSpeed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            leftright -= walkSpeed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            forwardBackward -= walkSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            leftright += walkSpeed;
        }

        if (oldForwardBackPosition != forwardBackward || oldLeftRightPosition != leftright)
        {
            oldForwardBackPosition = forwardBackward;
            oldLeftRightPosition = leftright;
            //update server
            UpdateClientPositionServerRpc(forwardBackward, leftright);
        }
    }

    [ServerRpc]
    public void UpdateClientPositionServerRpc(float forwardBackward, float leftright)
    {
        forwardBackPosition.Value = forwardBackward;
        leftRightPosition.Value = leftright;
    }
}
