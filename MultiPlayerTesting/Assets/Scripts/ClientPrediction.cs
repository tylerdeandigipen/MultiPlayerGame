using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Samples;
public struct InputPayload
{
    public int tick;
    public Vector3 inputVector;
}

public struct StatePayload
{
    public int tick;
    public Vector3 position;
}

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(ClientNetworkTransform))]
public class ClientPrediction : MonoBehaviour
{
    private float walkSpeed = 3.5f;
    Rigidbody rb;

    private float timer;
    private int currentTick;
    private float minTimeBetweenTicks;

    private const float SERVER_TICK_RATE = 30f;
    public const int BUFFER_SIZE = 1024;

    private StatePayload[] stateBuffer;
    private InputPayload[] inputBuffer;
    private StatePayload latestServerState;
    private StatePayload lastProcessedState;
    private float horizontalInput;
    private float verticalInput;
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        minTimeBetweenTicks = 1f / SERVER_TICK_RATE;

        stateBuffer = new StatePayload[BUFFER_SIZE];
        inputBuffer = new InputPayload[BUFFER_SIZE];
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        while (timer >= minTimeBetweenTicks)
        {
            timer -= minTimeBetweenTicks;
            HandleTick();
            currentTick++;
        }
    }

    void HandleTick()
    {
        int bufferIndex = currentTick % BUFFER_SIZE;

        InputPayload inputPayload = new InputPayload();
        inputPayload.tick = currentTick;
        inputPayload.inputVector = new Vector3(horizontalInput, 0, verticalInput);
        inputBuffer[bufferIndex] = inputPayload;

        stateBuffer[bufferIndex] = ProcessMovement(inputPayload);

        SendToServerServerRpc(inputPayload);
    }

    StatePayload ProcessMovement(InputPayload input)
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (vertical > 0 || vertical < 0)
            rb.AddForce(vertical > 0 ? Vector3.forward * walkSpeed : Vector3.back * walkSpeed);
        if (horizontal > 0 || horizontal < 0)
            rb.AddForce(horizontal > 0 ? Vector3.right * walkSpeed : Vector3.left * walkSpeed);

        return new StatePayload()
        {
            tick = input.tick,
            position = transform.position,
        };
    }


    [ServerRpc]
    public void SendToServerServerRpc(InputPayload inputPayload)
    {
       
    }
}
