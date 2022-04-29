using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Samples;

[RequireComponent(typeof(NetworkObject))]
public class RBPlayerMovement : NetworkBehaviour
{

    //Assingables
    public Transform playerCam;
    public Transform orientation;

    //Other
    private Rigidbody rb;
    private Vector3 spawnPoint;

    //Rotation and look
    private float xRotation;
    public float sensitivity = 1000f;
    private float sensMultiplier = 1f;

    //Movement
    public float moveSpeed = 4500;
    public float runSpeed = 20;
    public float walkSpeed = 15;
    public bool grounded;
    public LayerMask whatIsGround;

    public float counterMovement = 0.175f;
    private float threshold = 0.01f;
    public float maxSlopeAngle = 35f;

    //Crouch & Slide
    private Vector3 crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 playerScale;
    public float slideForce = 400;
    public float slideCounterMovement = 0.2f;

    //Jumping
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;
    public float jumpForce = 275f;
    public float doubleJumpForce = 275f;
    public float glideDuration = 3;
    public float doubleJumpMass = 1f;
    private bool canDoubleJump = true;
    public int doubleJumps = 2;

    //Input
    float x, y;
    bool jumping, sprinting, crouching;

    //Sliding
    private Vector3 normalVector = Vector3.up;
    private Vector3 wallNormalVector;


    private float maxSpeed;
    private bool isRunning;
    private float mass;
    private int jumpsLeft;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();        
    }

    void Start()
    {
        if (IsOwner)
        {
            mass = rb.mass;
            spawnPoint = transform.position;
            playerCam = FindObjectOfType<Camera>().transform;
            transform.position = new Vector3(0, 0, 0);
            playerCam.transform.parent = transform;
            playerCam.transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
        }
        maxSpeed = walkSpeed;
        playerScale = transform.localScale;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void FixedUpdate()
    {
        Movement();
    }

    private void Update()
    {
        MyInput();
        Look();
    }

    /// <summary>
    /// Find user input. Should put this in its own class but im lazy
    /// </summary>
    private void MyInput()
    {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");   
        crouching = Input.GetKey(KeyCode.LeftControl);
        if (grounded)
        {
            jumpsLeft = doubleJumps;
            ResetJump();
            StopGlide();
        }
        //Crouching
        if (Input.GetKeyDown(KeyCode.LeftControl))
            StartCrouch();
        if (Input.GetKeyUp(KeyCode.LeftControl))
            StopCrouch();
        //Jumps
        if (Input.GetKeyDown(KeyCode.Space) && readyToJump)
            Jump();
        if (Input.GetKeyDown(KeyCode.Space) && !grounded)
            DoubleJump();
        if (Input.GetKey(KeyCode.LeftShift))
        {
            maxSpeed = runSpeed;
            isRunning = true;
        }
        else
        {
            maxSpeed = walkSpeed;
            isRunning = false;
        }

    }

    private void StartCrouch()
    {
        transform.localScale = crouchScale;
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        if (rb.velocity.magnitude > 0.5f)
        {
            if (grounded)
            {
                rb.AddForce(orientation.transform.forward * slideForce);
            }
        }
    }

    private void StopCrouch()
    {
        transform.localScale = playerScale;
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
    }

    private void Movement()
    {
        //Extra gravity
        rb.AddForce(Vector3.down * Time.deltaTime * 10);

        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement(x, y, mag);

       

        //Set max speed
        float maxSpeed = this.maxSpeed;

        //If sliding down a ramp, add force down so player stays grounded and also builds speed
        if (crouching && grounded && readyToJump)
        {
            rb.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }

        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (x > 0 && xMag > maxSpeed) x = 0;
        if (x < 0 && xMag < -maxSpeed) x = 0;
        if (y > 0 && yMag > maxSpeed) y = 0;
        if (y < 0 && yMag < -maxSpeed) y = 0;

        //Some multipliers
        float multiplier = 1f, multiplierV = 1f;

        // Movement in air
        bool doubleJumping = false;
        if (!grounded && !doubleJumping)
        {
            multiplier = 0.2f;
            multiplierV = 0.6f * .5f;
        }

        // Movement while sliding
        if (grounded && crouching) multiplierV = 0f;

        //Apply forces to move player
        if (isRunning == false)
        {
            rb.AddForce(orientation.transform.forward * y * moveSpeed * Time.deltaTime * multiplierV);
            rb.AddForce(orientation.transform.right * x * (moveSpeed * multiplier) * Time.deltaTime * multiplier);
        }
        else
        {
            rb.AddForce(orientation.transform.forward * y * moveSpeed * Time.deltaTime * multiplierV);
            rb.AddForce(orientation.transform.right * x * (moveSpeed * .6f) * Time.deltaTime * multiplier);
        }

        if (transform.position.y < -10)
        {
            transform.position = spawnPoint;
        }
    }

    private void Jump()
    {
        if (grounded && readyToJump)
        {
            readyToJump = false;

            //Add jump forces
            rb.AddForce(Vector2.up * jumpForce * 1.5f);
            rb.AddForce(normalVector * jumpForce * 0.5f);
            Vector3 vel = rb.velocity;
            if (rb.velocity.y < 0.5f)
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            if (rb.velocity.y > 0)
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }
    private void DoubleJump()
    {
        if (!grounded && jumpsLeft > 0)
        {
            rb.mass = doubleJumpMass;
            readyToJump = false;
            jumpsLeft -= 1;

            rb.AddForce(Vector2.up * doubleJumpForce * 1.5f);
            rb.AddForce(normalVector * doubleJumpForce * 0.5f);

            //If jumping while falling, reset y velocity.
            Vector3 vel = rb.velocity;
            if (rb.velocity.y < 0.5f)
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            else if (rb.velocity.y > 0)
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

            Invoke(nameof(StopGlide), glideDuration);
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
    private void StopGlide()
    {
        this.rb.mass = mass;
        ResetJump();
    }

    private float desiredX;
    float yRot = 0f;
    float currentRecoilX = 0f;
    float currentRecoilY = 0f;
    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        yRot -= mouseY;
        yRot += currentRecoilY;
        yRot = Mathf.Clamp(yRot, -90, 90);

        transform.Rotate(Vector3.up * (mouseX + currentRecoilX));
        playerCam.transform.localRotation = Quaternion.Euler(yRot, 0, 0);
        currentRecoilY = 0;
        currentRecoilX = 0f;
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!grounded || jumping) return;

        //Slow down sliding
        if (crouching)
        {
            rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
            return;
        }

        //Counter movement
        if (Mathf.Abs(mag.x) > threshold && Mathf.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Mathf.Abs(mag.y) > threshold && Mathf.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }

        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > maxSpeed)
        {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    /// <summary>
    /// Find the velocity relative to where the player is looking
    /// Useful for vectors calculations regarding movement and limiting movement
    /// </summary>
    /// <returns></returns>
    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool cancellingGrounded;

    /// <summary>
    /// Handle ground detection
    /// </summary>
    private void OnCollisionStay(Collision other)
    {
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (IsFloor(normal))
            {
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded()
    {
        grounded = false;
    }
    public void RecoilMath(float recoilAmmountX, float recoilAmmountY, float timePressed, float maxRecoilTime, float xRecoilDir, float yRecoilDir)
    {
        currentRecoilX = xRecoilDir * Mathf.Abs((Random.value - .5f) / 2) * recoilAmmountX;
        currentRecoilY = yRecoilDir * Mathf.Abs(((Random.value) / 2) * recoilAmmountY * (timePressed >= maxRecoilTime ? recoilAmmountY / 4 : recoilAmmountY));
    }

    public void Kill()
    {
        Debug.Log("die");
        transform.position = spawnPoint;
    }
}