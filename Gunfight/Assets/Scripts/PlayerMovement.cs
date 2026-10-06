using UnityEngine;
using UnityEngine.InputSystem;

//   Player 1: A / D to move, W to jump
//   Player 2: Left / Right arrows to move, Up arrow to jump
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public enum PlayerId { Player1, Player2 }

    [SerializeField] private PlayerId playerId = PlayerId.Player1;
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private bool allowJump = true;
    [SerializeField] private float jumpSpeed = 9f;

    private Rigidbody2D rb;
    private InputAction moveAction;
    private InputAction jumpAction;
    private readonly ContactPoint2D[] contacts = new ContactPoint2D[8];

    public PlayerId Id => playerId;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

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
    }

    void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
    }

    void OnDestroy()
    {
        moveAction.Dispose();
        jumpAction.Dispose();
    }

    void Update()
    {
        if (allowJump && jumpAction.WasPressedThisFrame() && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpSpeed);
        }
    }

    void FixedUpdate()
    {
        float x = moveAction.ReadValue<float>();
        rb.linearVelocity = new Vector2(x * moveSpeed, rb.linearVelocity.y);
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
}
