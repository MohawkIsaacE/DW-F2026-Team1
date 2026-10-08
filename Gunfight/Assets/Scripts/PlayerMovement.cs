using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
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


    private Rigidbody2D rb;

    private InputActionMap playerActionMap;
    private InputAction moveAction;
    private InputAction jumpAction;

    private InputAction aimAction;

    [HideInInspector] public Vector2 rightStickDirection;
    private readonly ContactPoint2D[] contacts = new ContactPoint2D[8];
    private float jumpBufferTimer;
    private bool jumpReleased;
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

        if (playerId == PlayerId.Player1)
        {
            moveAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/a")
                .With("Positive", "<Keyboard>/d");
            jumpAction.AddBinding("<Keyboard>/w");
        }
        else
        {
            moveAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/leftArrow")
                .With("Positive", "<Keyboard>/rightArrow");
            jumpAction.AddBinding("<Keyboard>/upArrow");
        }
        */

        playerActionMap = new InputActionMap("PlayerControls_" + playerId);

        // New Gamepad controls
        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Gamepad>/leftStick/left")
            .With("Positive", "<Gamepad>/leftStick/right");

        jumpAction = new InputAction("Jump", InputActionType.Button);
        jumpAction.AddBinding("<Gamepad>/buttonSouth");

        aimAction = playerActionMap.AddAction("AimDirection", InputActionType.Value);
        aimAction.AddBinding("<Gamepad>/rightStick");

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
        playerActionMap.Enable();
    }

    void OnDisable()
    {
        playerActionMap.Disable();

        rb.linearVelocity = Vector2.zero;
    }

    void OnDestroy()
    {
        moveAction.Dispose();
        jumpAction.Dispose();
    }

    void Update()
    {
        rightStickDirection = aimAction.ReadValue<Vector2>();
        Vector3 target = new Vector3(1.0f, 1.0f, 1f);
        body.transform.localScale = Vector3.Lerp(body.transform.localScale, target, 10f * Time.deltaTime);
        bool grounded = IsGrounded();

        if (allowJump && jumpAction.WasPressedThisFrame() && IsGrounded())
        {
            jumpBufferTimer = jumpBufferTime;
            body.transform.localScale = new Vector3(0.8f, 1.4f, 1f);
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }
        if (jumpAction.WasReleasedThisFrame())
            jumpReleased = true;

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
        }
    }

    void FixedUpdate()
    {
        bool grounded = IsGrounded();
        Vector2 vel = rb.linearVelocity;

        float input = moveAction.ReadValue<float>();
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

        if (jumpBufferTimer > 0f)
        {
            vel.y = jumpSpeed;
            jumpBufferTimer = 0f;
            jumpReleased = false;
        }

        // Cut the jump short if jump is released early
        if (jumpReleased && vel.y > 0f)
            vel.y *= jumpCutMultiplier;
        jumpReleased = false;

        // Incur a max fall speed to prevent falling at infinite speed
        vel.y += -0.5f;
        vel.y = Mathf.Max(vel.y, -maxFallSpeed);

        rb.linearVelocity = vel;

    }

    // Grounded when touching something that pushes up on us
    private bool IsGrounded()
    {
        int count = rb.GetContacts(contacts);
        for (int i = 0; i < count; i++)
        {
            if (contacts[i].normal.y > 0.5f) return true;
        }
        return false;
    }

    public PlayerId GetPlayerId()
    {
        return playerId;
    }
}
