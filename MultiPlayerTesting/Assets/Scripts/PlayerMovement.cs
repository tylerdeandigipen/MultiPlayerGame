using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Samples;

[RequireComponent(typeof(NetworkObject))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Camera")]
    [SerializeField]
    private float cameraZoffset = 0f;
    [SerializeField]
    private float mouseSensitivity = 1000f;
    Camera camera_;

    [Header("Walk Speeds")]
    [SerializeField]
    private float walkSpeed = 10f;
    [SerializeField]
    private float runSpeed = 15f;
    float walkingSpeed = 0;

    [Header("Ground")]
    [SerializeField]
    private GameObject groundCheck;
    [SerializeField]
    private LayerMask groundMask;
    [SerializeField]
    private float groundDrag = 1f;
    [SerializeField]
    private float groundDistance = 0.4f;
    bool isGrounded = false;

    [Header("Jumping")]
    [SerializeField]
    private float airMultiplier = .4f;
    [SerializeField]
    private float jumpHeight = 3f;

    [Header("Crouching")]
    [SerializeField]
    private float crouchScale = .5f;
    [SerializeField]
    private float crouchSpeed = 7f;
    float scale;
    bool isCrouching = false;

    [Header("Slopes")]
    [SerializeField]
    private float maxSlopeAngle;
    [SerializeField]
    private float slopeMagnet;
    private RaycastHit slopeHit;
    [SerializeField]
    bool onSlope;
    float angle;
    bool exitSlope = false;

    [Header("Guns")]
    [SerializeField]
    public GameObject defaultShootPos;

    //Hidden Player Atribuites
    private Transform playerTransform;
    Vector3 velocity;
    Vector3 spawnPoint; 
    float yRot = 0f;
    float currentRecoilX = 0f;
    float currentRecoilY = 0f;
    Rigidbody rb;

    private void Awake()
    {
        //rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        scale = transform.localScale.y;
        rb = this.GetComponent<Rigidbody>();
        spawnPoint = transform.position;
        walkingSpeed = walkSpeed;
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
        //ground check
        isGrounded = Physics.CheckSphere(groundCheck.transform.position, groundDistance, groundMask);
        if (isGrounded)
        {
            rb.drag = groundDrag;
            exitSlope = false;
        }
        else
            rb.drag = 0f;
        //MovementBinds
        if (isGrounded && Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = true;
            transform.localScale = new Vector3(transform.localScale.x, crouchScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            walkSpeed = crouchSpeed;
        }
        if (Input.GetKeyUp(KeyCode.LeftControl) && isCrouching == true)
        {
            isCrouching = false;
            walkSpeed = walkingSpeed;
            transform.localScale = new Vector3(transform.localScale.x, scale, transform.localScale.z);
        }
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            exitSlope = true;
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(transform.up * jumpHeight, ForceMode.Impulse); 
        }
        if (Input.GetKey(KeyCode.LeftShift) && isCrouching != true)
        {
            walkSpeed = runSpeed;
        }
        else if (isCrouching != true)
        {
            walkSpeed = walkingSpeed;
        }
        //movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        rb.useGravity = !onSlope;
        Vector3 moveDirection = transform.forward * z + transform.right * x;
        if (isGrounded && onSlope == true && !exitSlope)
        {
            rb.drag = groundDrag;
            rb.AddForce(Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized * walkSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        else if (isGrounded == true && onSlope == false)
        {
            rb.drag = groundDrag;
            rb.AddForce(moveDirection * walkSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.drag = 0f;
            rb.AddForce(moveDirection * walkSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        //check for slope GET THIS RAYCAST TO WORK FOR SLOPE THINGS TO HAPPEN
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, transform.localScale.y * .5f + .3f, 7))
        {
            Debug.DrawRay(transform.position, Vector3.down, Color.red, transform.localScale.y * .5f + .3f);
            angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            if (angle < maxSlopeAngle && angle != 0)
                onSlope = true;
            else
                onSlope = false;                
        }
        //limit speed
        //limit on slopes
        if (onSlope && !exitSlope)
        {
            if (rb.velocity.magnitude > walkSpeed)
                rb.velocity = rb.velocity.normalized * walkSpeed;
        }
        //limit mid air or on ground
        else
        {
            Vector3 currentSpeed = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (currentSpeed.magnitude > walkSpeed)
            {
                Vector3 limitedSpeed = currentSpeed.normalized * walkSpeed;
                rb.velocity = new Vector3(limitedSpeed.x, rb.velocity.y, limitedSpeed.z);
            }
        } 
        
        //reset player
        if (transform.position.y < -25)
        {
            transform.position = spawnPoint;
        }
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

        yRot -= mouseY;
        yRot += currentRecoilY;
        yRot = Mathf.Clamp(yRot, -90, 90);

        transform.Rotate(Vector3.up * (mouseX + currentRecoilX));        
        camera_.transform.localRotation = Quaternion.Euler(yRot, 0, 0);
        currentRecoilY = 0;
        currentRecoilX = 0f;
    }

    public void RecoilMath(float recoilAmmountX, float recoilAmmountY, float timePressed, float maxRecoilTime, float xRecoilDir, float yRecoilDir)
    {
        currentRecoilX = xRecoilDir * Mathf.Abs((Random.value - .5f) / 2) * recoilAmmountX;
        currentRecoilY = yRecoilDir * Mathf.Abs(((Random.value) / 2) * recoilAmmountY * (timePressed >= maxRecoilTime ? recoilAmmountY / 4 : recoilAmmountY));
    }

}
