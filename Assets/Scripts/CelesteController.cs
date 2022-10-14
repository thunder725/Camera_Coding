using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem;

/*  Script copy-pasted from my Year2 DemoreelProject
    Except there was no directional dash, was coded for Unity2D
    Had methods for musics, animation, sounds & everything
    so it still had to be changed a lot
    
    I'm happy I commented everything
*/



/*
    TO-DO :

        Directional Dash
    
*/
public class CelesteController : MonoBehaviour
{
    // RigidBody Parameters:  Mass = 0.1  /  Drag = 18  /  AngularDrag = 0  /  GravityScale = 23
    // Speed = 30 / JumpStrength = 6 / MaxJumpAirTime = 0.3 / JumpStayInTheAirMultiplier = 0.14
    [Header("Values Attribution")] // Values of when the music is Fast
    [SerializeField] float Speed;
    [SerializeField] float JumpStrength, MaxJumpAirTime, JumpStayInTheAirMultiplier;
    [SerializeField] float GravityScale;

    // DashCooldown = 0.6 / DashStrength = 5 / DashDuration = 0.18 / JumpBuffering = 0.08 / CoyoteTime = 0.09 / MaxSpeed = 8.8
    [Header("General Values Attribution")] // Values that get used in the scripts
    // Those values shouldn't change with the song
    [SerializeField] float dashCooldown;
    [SerializeField] float dashStrength, dashDuration, jumpBuffering, coyoteTime, maxSpeed;


    // Runtime values for the timers, those are the one that get changed by Time.deltaTime and get compared in scripts
    float currentDashCooldown, remainingDashDuration, remainingJumpAirTime, currentJumpbuffering, currentCoyoteTimer;
    int directionFacing; // 1 = right // -1 = left
    // General booleans for the movement

    public bool dashFinished, isGrounded;


    // Everything linked to the ground detection
    [Header("Ground Collision")]
    [SerializeField] float collisionWidth;
    [SerializeField] float collisionHeight;
    [SerializeField] Transform GroundDetectionPoint;
    [SerializeField] LayerMask platformLayer;

    [Header("References")]
    Rigidbody rbd;

    PlayerInputs _inputs;
    



    // ========== [GENERAL UNITY METHODS] ==========

    void Awake()
    {
        rbd = GetComponent<Rigidbody>();
        _inputs = new PlayerInputs();
    }

    void Start()
    {
        dashFinished = true;
        isGrounded = true;
    }

    void Update() // Mainly Timers and cooldowns
    {
        // === [Dash Cooldown] ===
        if (currentDashCooldown > 0)
        { currentDashCooldown -= Time.deltaTime; }

        // === [Jump Buffering] ===
        if (currentJumpbuffering > 0)
        { currentJumpbuffering -= Time.deltaTime; }

        // === [Coyote Time] ===
        if (currentCoyoteTimer > 0)
        { currentCoyoteTimer -= Time.deltaTime; }


        // === [Dash Time Handler] ===
        if (remainingDashDuration > 0)
        {
            remainingDashDuration -= Time.deltaTime;
        }
        else if (!dashFinished) // Enabled only once on the first frame where the dash is finished
        {
            // Free the height constraint from being locked
            rbd.constraints = RigidbodyConstraints.FreezeRotation;

            // One-Time boolean
            dashFinished = true;

        }

    }

    void FixedUpdate() // Physics Stuff
    {
        // Get Inputs

        // Movement
        Vector2 direction2 = _inputs.Default.Movement.ReadValue<Vector2>();
        if (direction2.x != 0)
        {
            Movement(direction2);
        }

        // Jump
        float JumpValue = _inputs.Default.A_Button.ReadValue<float>();
        if (JumpValue != 0) { GeneralJumpMethod(); }




        // === [Is the player grounded ?] === 
        Vector2 boxDimensions = new Vector2(collisionWidth, collisionHeight);

        // Check if there is a ground under the player :     Box Center                 Dimensions   Direction           Orientation  Distance  Layer       Querytrigger
        RaycastHit[] raycastHits = Physics.BoxCastAll(GroundDetectionPoint.position, boxDimensions, Vector3.right, Quaternion.identity, 0, platformLayer, QueryTriggerInteraction.Ignore);
        if (raycastHits.Length != 0)
        {
            // I'm On the ground
            if (!isGrounded)
            {
                // To say specifically that I Landed this frame
                LandingOnGround();
            }
        }
        else
        {
            // I'm in the air
            if (isGrounded)
            {
                // To say specifically that I Left the ground this frame, not just that i'm still airborne
                LeavingGround();
            }
        }


        // Apply gravity ourselves
        // Since we can't modify "GravityScale"
        // on a Rigidbody 3D
        
        rbd.AddForce(GravityScale * Vector3.down * 9.81f);

    }




    // ========== [MOVEMENT METHODS] ==========

    public void Movement(Vector2 directionalInput)
    {
        // Take only the x parameter
        Vector3 direction2 = new Vector3(directionalInput.x, 0);

        // Add it to the player
        rbd.AddForce(direction2.normalized * Speed);

        // If outside of a dash (so finished), limit the velocity. Should only matter in Fast Music if ever
        if (dashFinished)
        {
            rbd.velocity = new Vector3(Mathf.Clamp(rbd.velocity.x, -maxSpeed, maxSpeed), rbd.velocity.y);
        }

        // Method to detect when you are changing direction specifically
        int tempDirection = (int)Mathf.Sign(direction2.x);          // Current direction according to movement
        if (directionFacing != tempDirection)                       // Test it with the current direction according to the script
        {
            // If different, actually change direction for the script and everything
            ChangingDirectionFacing(tempDirection);
        }
    }

    void ChangingDirectionFacing(int newDirection)
    {
        directionFacing = newDirection;

        // Switch direction visually with the scale
        // (only on Z)
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, Mathf.Abs(transform.localScale.z) * directionFacing);
    }




    // ========== [JUMPING METHODS] ==========

    // This method is called each frame the Jump Button is pressed
    // It handles the "Stay in the air longer if button is pressed" shenanigans
    public void GeneralJumpMethod()
    {
        // If timer expired  =>  Either max jump height achieved  OR  jump button was released mid-air
        if (remainingJumpAirTime <= 0)
        {
            return;
        }

        // Increase height if kept pressed until a max height (depending on the timer currentJumpAirTime)
        Vector3 jumpInput = Vector3.up * JumpStayInTheAirMultiplier * JumpStrength * 5;
        rbd.velocity += jumpInput;
        remainingJumpAirTime -= Time.fixedDeltaTime;
    }



    // Called the frame the jump button is released, activates the "return" in the method above
    public void CancelJump()
    {
        // Prevent hovering mid-air by setting the "Has Achieved Max Jump Height" timer to "Yes I Have"
        remainingJumpAirTime = 0;
    }



    // Method called when pressing down the button, or when we land and the buffer is still active
    public void StartJump()
    {
        // Only actually jump if we're on the ground or we left the platform not long ago (Coyote Time)
        if (isGrounded || currentCoyoteTimer > 0)
        {
 

            // Cancels dash if done on the ground
            remainingDashDuration = 0;

            // Timer to check if max height has been achieved 
            remainingJumpAirTime = MaxJumpAirTime;

            // Actual Jump
            Vector3 jumpInput = Vector3.up * JumpStrength * 10;
            rbd.velocity = jumpInput;
        }
        else // Starting the Jump Buffer Timer. Should only work if jump button is pressed down
        {
            currentJumpbuffering = jumpBuffering;
        }
    }

    void LandingOnGround()
    {
        // Debug.Log("Landed");
        isGrounded = true;

        // Reset dash cooldown à la Hollow Knight
        currentDashCooldown = 0;

        currentCoyoteTimer = 0;

        // Jump immediately is buffering activated
        if (currentJumpbuffering > 0)
        {
            // Debug.Log("Jumping From Buffer");
            StartJump();
        }
    }

    void LeavingGround()
    {

        isGrounded = false;

        // Reset the buffering timer
        currentJumpbuffering = 0;


        // Because jumping uses this value to tell how long the player should recieve upwards force
        // It can differenciate being leaving the ground between falling and jumping
        if (remainingJumpAirTime > 0)
        {
            // Leaving the ground by Jumping
        }
        else
        {
            // Leaving the ground by falling from a platform -> Coyote Time
            currentCoyoteTimer = coyoteTime;
        }
    }






    // ========== [DASH METHODS] ==========


    public void Dash(Vector3 directionalInput)
    {
        if (currentDashCooldown > 0) // If cooldown not passed ignore everything
        {
            return;
        }

        // Hard set the velocity to stop the jump (better mid-air accuracy) as well as preventing going too fast on the ground.
        rbd.velocity = Vector2.zero;
        rbd.AddForce((directionalInput.normalized * dashStrength * 50));

        // Set Cooldown, max air time (which controls i-frames, time spent in the air, etc) and other things
        currentDashCooldown = dashCooldown;
        remainingDashDuration = dashDuration;
        dashFinished = false;

    }





    // ========== [MISC METHODS] ==========

    private void OnEnable()
    {
        _inputs.Enable();
        _inputs.Default.A_Button.started += StartJumpPressed;
        _inputs.Default.A_Button.canceled += CancelJumpPressed;
        _inputs.Default.B_Button.started += DashPressed;
    }

    private void OnDisable()
    {
        _inputs.Disable();
        _inputs.Default.A_Button.started -= StartJumpPressed;
        _inputs.Default.A_Button.canceled -= CancelJumpPressed;
        _inputs.Default.B_Button.started -= DashPressed;
    }

    void StartJumpPressed(InputAction.CallbackContext c)
    {
        StartJump();
    }

    void CancelJumpPressed(InputAction.CallbackContext c)
    {
        CancelJump();
    }

    void DashPressed(InputAction.CallbackContext c)
    {
        Vector3 directionalInput = _inputs.Default.Movement.ReadValue<Vector2>();
        if (directionalInput.sqrMagnitude != 0)
        {
            Dash(directionalInput);
        }
        else
        {
            Dash(Vector3.right);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // Collision box for checking if grounded
        Gizmos.DrawWireCube(GroundDetectionPoint.position, new Vector3(collisionWidth, collisionHeight, 0));
    }
}
