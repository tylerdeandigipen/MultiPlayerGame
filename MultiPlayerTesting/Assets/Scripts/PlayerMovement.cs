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
    public float mouseSensitivity = 1000f;
    Camera camera_;

    [Header("Walk Speeds")]
    [SerializeField]
    private float walkSpeed = 10f;
    [SerializeField]
    private float runSpeed = 15f;
    [SerializeField]
    private float sideToSideWieght = 1f;
    float walkingSpeed = 0;
    [SerializeField]
    float currentVelocity;
    public float x;
    public float z;

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
    [SerializeField]
    float jumpCooldown;
    bool isJumping;

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
    float slopeCheckLength;
    [SerializeField]
    private float slopeMagnet;
    [SerializeField]
    bool onSlope;
    [SerializeField]
    float slopeDrag;
    float angle;
    RaycastHit slopeHit;
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
            currentVelocity = new Vector3 (rb.velocity.x, 0f, rb.velocity.z).magnitude;
        }
    }
    private void ClientInput()
    {
        checkStates();
        crouch();
        movement();
        limitSpeed();
        //jump
        if (isGrounded && Input.GetKeyDown(KeyCode.Space) && isJumping != true)
        {
            jump();
            Invoke(nameof(resetJump), jumpCooldown);
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
    void jump()
    {
        isJumping = true;
        exitSlope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpHeight, ForceMode.Impulse);
    }

    void resetJump()
    {
        isJumping = false;
        exitSlope = false;
    }

    void limitSpeed()
    {
        //limit on slopes
        if (onSlope && !exitSlope)
        {            
            if (rb.velocity.magnitude > walkSpeed)
                rb.velocity = rb.velocity.normalized * walkSpeed;
            currentVelocity = rb.velocity.magnitude;
        }
        //limit mid air or on ground
        else
        {
            Vector3 currentSpeed = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (currentSpeed.magnitude > walkSpeed)
            {
                Vector3 limitedSpeed = currentSpeed.normalized * walkSpeed;
                rb.velocity = new Vector3(limitedSpeed.x, rb.velocity.y, limitedSpeed.z);
                currentVelocity = rb.velocity.magnitude;
            }
        }
    }

    void movement()
    {
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");
        rb.useGravity = !onSlope;
        Vector3 moveDirection;
        if (walkSpeed == runSpeed)
        {
            if(z > 0)
                moveDirection = transform.forward * z + ((transform.right * x) * sideToSideWieght);
            else
                moveDirection = ((transform.forward * z) * sideToSideWieght) + ((transform.right * x) * sideToSideWieght);
        }
        else
            moveDirection = transform.forward * z + transform.right * x;
        if (isGrounded && onSlope == true && !exitSlope)
        {
            rb.drag = groundDrag;
            rb.AddForce(Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized * walkSpeed * 20f, ForceMode.Force);
            rb.velocity = rb.velocity * slopeDrag;

            //if (rb.velocity.y > 0)
            //rb.AddForce(Vector3.down * 60f, ForceMode.Force);
        }
        else if (isGrounded)
        {
            rb.drag = groundDrag;
            rb.AddForce(moveDirection * walkSpeed * 20f, ForceMode.Force);
        }
        else if (walkSpeed == runSpeed)
        {
            rb.drag = 0f;
            rb.AddForce(moveDirection * walkSpeed * 20f * airMultiplier, ForceMode.Force);
        }
        if (Input.GetKey(KeyCode.LeftShift) && !isCrouching)
        {
            walkSpeed = runSpeed;
        }
        else if (isCrouching != true)
        {
            walkSpeed = walkingSpeed;
        }
    }

    void crouch()
    {
        if (isGrounded && Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = true;
            transform.localScale = new Vector3(transform.localScale.x, crouchScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            walkSpeed = crouchSpeed;
        }
        if (Input.GetKeyUp(KeyCode.LeftControl) && isCrouching)
        {
            isCrouching = false;
            walkSpeed = walkingSpeed;
            transform.localScale = new Vector3(transform.localScale.x, scale, transform.localScale.z);
        }
    }

    void checkStates()
    {
        isGrounded = Physics.CheckSphere(groundCheck.transform.position, groundDistance, groundMask);
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
            rb.drag = 0f;

        //check for slope
        Debug.DrawRay(new Vector3(this.transform.position.x, this.transform.position.y - (transform.localScale.y / 2), this.transform.position.z), Vector3.down + new Vector3(0, -slopeCheckLength, 0), Color.red, 1);
        Physics.Raycast(new Vector3(this.transform.position.x, this.transform.position.y - (transform.localScale.y / 2), this.transform.position.z), Vector3.down, out slopeHit, slopeCheckLength);
        if (slopeHit.collider != null)
        {
            angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            if (angle < maxSlopeAngle && angle != 0)
                onSlope = true;
            else
                onSlope = false;
        }
        else
            onSlope = false;
        exitSlope = !onSlope;
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
