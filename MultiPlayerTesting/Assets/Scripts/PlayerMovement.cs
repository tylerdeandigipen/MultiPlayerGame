using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Samples;

[RequireComponent(typeof(NetworkObject))]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField]
    private float cameraZoffset = 0f;
    [SerializeField]
    private float walkSpeed = 10f;
    [SerializeField]
    private float runSpeed = 15f;
    [SerializeField]
    private float mouseSensitivity = 1000f;

    float walkingSpeed = 0;
    public float jumpHeight = 3f;
    public float groundDistance = 0.4f;
    public float groundMagnet = 2;
    public GameObject groundCheck;
    public LayerMask groundMask;
    CharacterController controller;
    public float gravity = -9.8f;
    public Vector3 velocity;
    Camera camera_;
    public bool isGrounded = false;
    float xRot = 0f;
    private void Awake()
    {
        //rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        walkingSpeed = walkSpeed;
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
        isGrounded = Physics.CheckSphere(groundCheck.transform.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -groundMagnet * gravity);
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            walkSpeed = runSpeed;
        }
        else
        {
            walkSpeed = walkingSpeed;
        }
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * walkSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
        
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
