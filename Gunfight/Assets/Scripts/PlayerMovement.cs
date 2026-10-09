using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using static UnityEngine.GraphicsBuffer;

//   Player 1: A / D to move, W to jump
//   Player 2: Left / Right arrows to move, Up arrow to jump
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public enum PlayerId { Player1, Player2 }

    [SerializeField] private PlayerId playerId = PlayerId.Player1;
    [SerializeField] private Transform body;
    [SerializeField] private Animator anim;
    // RUN PROPERTIES
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float groundAccel = 90f;
    [SerializeField] private float groundDecel = 110f;
    [SerializeField] private float airAccel = 60f;
    [SerializeField] private float airDecel = 40f;

    // JUMP PROPERTIES
    [SerializeField] private bool allowJump = true;
    [SerializeField] private float jumpSpeed = 14f;
    [SerializeField] private float jumpCutMultiplier = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float maxFallSpeed = 20f;
    [SerializeField] private float fallSpeedAccel = 0.8f;

    // AIR DASH PROPERTIES
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashEndMultiplier = 0.5f;   // speed multiplier when the dash ends
    [SerializeField] private AudioClip dashSound;
    [SerializeField, Range(0f, 3f)] private float dashVolume = 1f;
    [SerializeField] private float groundDashCooldown = 0.4f;
    private bool groundDashRequested;
    private float groundDashCooldownTimer;
    private bool hasAirDash = true;
    private bool dashRequested;
    private float dashTimer;
    private Vector2 dashDir;

    [SerializeField] private float dropThroughTime = 0.3f;
    private bool droppingThrough;
    [SerializeField] private Collider2D Player1Collider;
    [SerializeField] private Collider2D Player2Collider;

    private Rigidbody2D rb;

    private InputActionMap playerActionMap;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dropAction;
    private InputAction shootAction;

    private InputAction aimAction;

    [HideInInspector] public Vector2 rightStickDirection;
    [HideInInspector] public bool inputLocked;
    private readonly ContactPoint2D[] contacts = new ContactPoint2D[8];
    private float jumpBufferTimer;
    private bool jumpReleased;
    private bool dropReleased;
    [SerializeField] private AudioClip landSound;
    private bool wasGrounded = true;
    private float airTime;
    private Player player_movement;

    public PlayerId Id => playerId;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player_movement = GetComponent<Player>();
    }

    void Awake()
    {
        Debug.Log("PlayerId: " + (int)playerId);
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;   // removes jitter
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        /*
        // Bindings are built per player so both can use the same keyboard at once
        moveAction = new InputAction("Move", InputActionType.Value);
        jumpAction = new InputAction("Jump", InputActionType.Button);
        dropAction = new InputAction("Drop", InputActionType.Button);

        if (playerId == PlayerId.Player1)
        {
            moveAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/a")
                .With("Positive", "<Keyboard>/d");
            jumpAction.AddBinding("<Keyboard>/w");
            dropAction.AddBinding("<Keyboard>/s");
        }
        else
        {
            moveAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/leftArrow")
                .With("Positive", "<Keyboard>/rightArrow");
            jumpAction.AddBinding("<Keyboard>/upArrow");
            dropAction.AddBinding("<Keyboard>/downArrow");
        }
        */

        playerActionMap = new InputActionMap("PlayerControls_" + playerId);
        // New Gamepad controls


        // I hope you know your Easts and Wests lol
        dropAction = playerActionMap.AddAction("Drop", InputActionType.Value);
        dropAction.AddBinding("<Gamepad>/leftStick/y");
        moveAction = playerActionMap.AddAction("Move", InputActionType.Value);
        moveAction.AddBinding("<Gamepad>/leftStick/x");
        jumpAction = playerActionMap.AddAction("Jump", InputActionType.Button);
        //jumpAction.AddBinding("<Gamepad>/buttonWest");
        //jumpAction.AddBinding("<Gamepad>/buttonNorth");
        jumpAction.AddBinding("<Gamepad>/leftTrigger");
        jumpAction.AddBinding("<Gamepad>/leftShoulder");

        aimAction = playerActionMap.AddAction("AimDirection", InputActionType.Value);
        aimAction.AddBinding("<Gamepad>/rightStick");

        shootAction = playerActionMap.AddAction("Shoot", InputActionType.Button);
        shootAction.AddBinding("<Gamepad>/rightTrigger");
        shootAction.AddBinding("<Gamepad>/rightShoulder");
        //shootAction.AddBinding("<Gamepad>/buttonSouth");
        //shootAction.AddBinding("<Gamepad>/buttonEast");



        // Check if enough gamepads are connected, if so bind them based on player id
        if (Gamepad.all.Count > (int)playerId)
        {
            Gamepad assignedGamepad = Gamepad.all[(int)playerId];
            playerActionMap.devices = new[] { assignedGamepad };

        }
        else
        {
            Debug.LogWarning("Waiting for both gamepads to be plugged in!!!");
        }

    }

    void OnEnable()
    {
        rb.linearVelocity = Vector2.zero;
        moveAction.Enable();
        jumpAction.Enable();
        dropAction.Enable();
        playerActionMap.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        dropAction.Disable();
        playerActionMap.Disable();

        rb.linearVelocity = Vector2.zero;
    }

    void OnDestroy()
    {
        moveAction.Dispose();
        jumpAction.Dispose();
        dropAction.Dispose();
        playerActionMap.Dispose();
    }

    void Update()
    {
        rightStickDirection = inputLocked ? Vector2.zero : aimAction.ReadValue<Vector2>();
        Vector3 target = new Vector3(1.0f, 1.0f, 1f);
        body.transform.localScale = Vector3.Lerp(body.transform.localScale, target, 10f * Time.deltaTime);
        bool grounded = IsGrounded();

        if (!inputLocked && allowJump && jumpAction.WasPressedThisFrame())
        {
            if (!grounded && hasAirDash)
            {
                // air dash instead of jumping if in the air
                dashRequested = true;
            }
            else
            {
                jumpBufferTimer = jumpBufferTime;
                body.transform.localScale = new Vector3(0.8f, 1.4f, 1f);
            }
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        // shoot button with no gun while grounded to ground dash
        groundDashCooldownTimer -= Time.deltaTime;
        if (!inputLocked && grounded && !player_movement.hasGun
            && player_movement.lastShotFrame != Time.frameCount
            && shootAction.WasPressedThisFrame()
            && dashTimer <= 0f && groundDashCooldownTimer <= 0f)
        {
            groundDashRequested = true;
        }

        if (jumpAction.WasReleasedThisFrame())
            jumpReleased = true;

        float input = dropAction.ReadValue<float>();

        if (!inputLocked && input < -0.6f)
        {
            dropReleased = true;
        }

        if (anim != null)
        {
            float facing = 1f;
            if (player_movement.flipped_facing)
            {
                facing = -1f;
            }
            anim.SetFloat("Speed", rb.linearVelocity.x * facing);
            anim.SetFloat("VelY", rb.linearVelocity.y);
            anim.SetBool("Grounded", grounded);
            anim.SetBool("Dashing", dashTimer > 0f);
        }
    }

    void FixedUpdate()
    {
        bool grounded = IsGrounded();
        Vector2 vel = rb.linearVelocity;
        if (grounded && !wasGrounded && airTime > 0.1f)
        {
            AudioManager.Instance.PlaySfx(landSound, 0.1f);
        }
        airTime = grounded ? 0f : airTime + Time.fixedDeltaTime;
        wasGrounded = grounded;

        // get air dash back once on ground
        if (grounded && dashTimer <= 0f) hasAirDash = true;

        // Use air dash
        if (dashRequested)
        {
            dashRequested = false;

            Vector2 stick = new Vector2(moveAction.ReadValue<float>(), dropAction.ReadValue<float>());
            if (stick.magnitude < 0.2f)
            {
                stick = new Vector2(player_movement.flipped_facing ? -1f : 1f, 0f);
            }

            hasAirDash = false;
            StartDash(stick);
        }

        // Start a ground dash
        if (groundDashRequested)
        {
            groundDashRequested = false;
            groundDashCooldownTimer = groundDashCooldown;
            StartDash(new Vector2(player_movement.flipped_facing ? -1f : 1f, 0f));
        }

        // fixed speed in one direction and no gravity
        if (dashTimer > 0f)
        {
            dashTimer -= Time.fixedDeltaTime;
            vel = dashDir * dashSpeed;
            if (dashTimer <= 0f) vel *= dashEndMultiplier;
            dropReleased = false;
            jumpReleased = false;
            rb.linearVelocity = vel;
            return;
        }

        float input = inputLocked ? 0f : moveAction.ReadValue<float>();
        float target = input * moveSpeed;

        float rate;
        if (Mathf.Abs(input) > 0.01f)
        {
            // Air acceleration and deceleration is slower
            rate = grounded ? groundAccel : airAccel;
            // Check if the player is trying to move in the opposite direction

            if (Mathf.Sign(target) != Mathf.Sign(vel.x) && Mathf.Abs(vel.x) > 0.01f)
                rate = grounded ? groundDecel : airDecel;
        }
        else
        {
            rate = grounded ? groundDecel : airDecel;
        }
        // Apply movement by rate over time
        vel.x = Mathf.MoveTowards(vel.x, target, rate * Time.fixedDeltaTime);

        if (jumpBufferTimer > 0f && IsGrounded())
        {
            vel.y = jumpSpeed;
            jumpBufferTimer = 0f;
            jumpReleased = false;
        }

        // Cut the jump short if jump is released early
        if (jumpReleased && vel.y > 0f)
            vel.y *= jumpCutMultiplier;
        jumpReleased = false;

        vel.y += -fallSpeedAccel;
        vel.y = Mathf.Max(vel.y, -maxFallSpeed);
        
        if (dropReleased)
        {
            if (grounded == false)
            {
                vel.y = -maxFallSpeed;
            }
            else
            {
                DropThrough();
            }
            dropReleased = false;
        }
        
        rb.linearVelocity = vel;

    }

    void DropThrough()
    {
        if (droppingThrough) return;

        int count = rb.GetContacts(contacts);
        for (int i = 0; i < count; i++)
        {
            // Get collision that isn't ours
            Collider2D other = contacts[i].collider.attachedRigidbody == rb ? contacts[i].otherCollider : contacts[i].collider;

            // Only drop through one way platforms
            if (contacts[i].normal.y > 0.5f && other.GetComponent<PlatformEffector2D>() != null)
            {
                StartCoroutine(IgnorePlatform(other));
                break;
            }
        }
    }
    void StartDash(Vector2 direction)
    {
        dashDir = direction.normalized;
        dashTimer = dashDuration;
        jumpBufferTimer = 0f;
        AudioManager.Instance.PlaySfx(dashSound, 0.1f, dashVolume);
    }
    IEnumerator IgnorePlatform(Collider2D platform)
    {
        droppingThrough = true;

        Collider2D[] mine = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D c in mine)
            Physics2D.IgnoreCollision(c, platform, true);

        yield return new WaitForSeconds(dropThroughTime);

        foreach (Collider2D c in mine)
            if (c != null && platform != null)
                Physics2D.IgnoreCollision(c, platform, false);

        droppingThrough = false;
    }

    // Grounded when touching something that pushes up on us
    public bool IsGrounded()
    {
        if (rb.linearVelocity.y > 0.1f) return false;

        int count = rb.GetContacts(contacts);
        for (int i = 0; i < count; i++)
        {
            // Skips the off platform effector
            if (!contacts[i].enabled) continue;

            if (contacts[i].normal.y > 0.5f) return true;
        }
        return false;
    }

    public PlayerId GetPlayerId()
    {
        return playerId;
    }
    public bool IsDashing() 
    { 
        return dashTimer > 0f; 
    }
    public Vector2 DashDirection() 
    { 
        return dashDir;
    }
    public bool ShootPressed()
    {
        return !inputLocked && shootAction.WasPressedThisFrame();
    }
    public float GetMoveInput()
    {
        return inputLocked ? 0f : moveAction.ReadValue<float>();
    }
    public void KnockDown()
    {
        anim.SetBool("Dead", true);
    }
    public void ResetAnim()
    {
        if (anim == null) return;
        anim.SetBool("Dead", false);

        // Reset any values from last round
        anim.SetFloat("Speed", 0f);
        anim.SetFloat("VelY", 0f);
        anim.SetBool("Grounded", true);
        anim.SetBool("Dashing", false);
        groundDashRequested = false;
        groundDashCooldownTimer = 0f;
        dashTimer = 0f;
        hasAirDash = true;
        anim.Play("Idle", 0, 0f); // spawn as idle
    }
}
