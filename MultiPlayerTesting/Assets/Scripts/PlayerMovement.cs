using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Samples;

[RequireComponent(typeof(NetworkObject))]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField]
    private float slowSpeed = .95f;

    [SerializeField]
    private float cameraZoffset = 0f;

    [SerializeField]
    private float walkSpeed = 3.5f;

    [SerializeField]
    private float mouseSensitivity = 3.5f;

    CharacterController controller;
    Rigidbody rb;
    Camera camera_;
    float xRot = 0f;
    private void Awake()
    {
        //rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        controller = this.GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        if (IsOwner)
        {
            camera_ = FindObjectOfType<Camera>();
            transform.position = new Vector3(0, 0, 0);
            parentCamera();
        }
    }

    private void Update()
    {
        if (IsClient && IsOwner)
        {
            ClientInput();
            cameraControl();
        }
    }
    private void ClientInput()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * walkSpeed * Time.deltaTime);
    }
    private void parentCamera()
    {
        camera_.transform.parent = transform;
        camera_.transform.position = new Vector3(transform.position.x, transform.position.y + cameraZoffset, transform.position.z);
    }
    private void cameraControl()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90, 90);

        camera_.transform.localRotation = Quaternion.Euler(xRot, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }

}
