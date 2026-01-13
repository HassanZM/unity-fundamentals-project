using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    // Const variables
    private const float NoMovement = 0f;

    // Inspector visible variables
    [Header("General")]
    [SerializeField, Tooltip("Speed for left/right player movement")]
    private float moveSpeed;
    [SerializeField, Tooltip("Set the time in seconds for how long it takes to respawn.")]
    private float respawnTimer;
    [SerializeField, Tooltip("Allows the toggling of the legacy input system.")]
    private bool legacyInput;
    [Header("Jump")]
    [SerializeField, Tooltip("Speed of the player jump force.")]
    private float jumpSpeed;
    [SerializeField, Tooltip("Allows the fall speed of the jump to be faster or slower.")]
    private float jumpGravityScale = 3.0f;
    [SerializeField, Tooltip("Turns on the double jump option for players.")]
    private bool allowDoubleJump;
    [Header("Ground Check")]
    [SerializeField, Tooltip("Set the layer for the player ground check.")]
    private LayerMask groundLayer;
    [SerializeField, Tooltip("The distance to send a ray-cast to check whether a player is grounded.")]
    private float groundedRayDistance;
    [SerializeField, Tooltip("Allows the player to jump after moving off a platform. (Seconds)")]
    private float coyoteTime = 0.07f;
    [Header("Dash")]
    [SerializeField, Tooltip("Speed for the Dash.")]
    private float dashSpeed;
    [SerializeField, Tooltip("Cooldown for the Dash ability in seconds.")]
    private float dashCooldown;
    [SerializeField, Tooltip("The distance the Dash ability moves you.")]
    private float dashDistance;

    // Inspector invisible variables.
    public Rigidbody2D Rigidbody { get; private set; }
    private bool dashAvailable;
    private bool doubleJumpAvailable;
    private bool isGrounded;
    private bool isDashing;
    private bool legacyJumpPressed;
    private bool legacyDashPressed;
    private float defaultGravityScale;
    private float coyoteTimeTimer;
    private int jumpCounter;

    public float MovementDirection { get; private set; }

    public float MoveSpeed => moveSpeed;
    public bool IsGrounded => GetGroundedStatus();
    public bool IsIdle => Rigidbody.linearVelocityX == 0;

    private void Start()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        MovementDirection = NoMovement;
        isGrounded = true;
        isDashing = false;
        dashAvailable = true;
        doubleJumpAvailable = allowDoubleJump;
        defaultGravityScale = Rigidbody.gravityScale;

        if (legacyInput)
        {
            GetComponent<PlayerInput>().enabled = false;
        }
    }

    private void Update()
    {
        coyoteTimeTimer = isGrounded switch
        {
            true => coyoteTime,
            _ => coyoteTimeTimer - Time.deltaTime
        };

        // BUG: Breaks jumping on slopes.
        if (isGrounded && Rigidbody.linearVelocity.y == NoMovement)
        {
            jumpCounter = 0;
        }

        HandleLegacyInput();
    }

    private void FixedUpdate()
    {
        if (legacyInput)
        {
            return;
        }

        HandleMovement();

    }

    private void HandleLegacyInput()
    {
        if (!legacyInput)
        {
            return;
        }

        MovementDirection = Input.GetAxisRaw("Horizontal");
        legacyJumpPressed = Input.GetKeyDown(KeyCode.Space);
        legacyDashPressed = Input.GetKeyDown(KeyCode.J);

        HandleMovement();

        if (legacyJumpPressed)
        {
            OnJump();
        }

        if (legacyDashPressed)
        {
            OnDash();
        }
    }

    private void HandleMovement()
    {
        isGrounded = GetGroundedStatus();
        // Only allow movement when the player is Grounded and is not Dashing.
        if (!isGrounded || isDashing)
        {
            return;
        }
        Rigidbody.linearVelocity = new Vector2(MovementDirection * MoveSpeed, Rigidbody.linearVelocity.y);

        if (Rigidbody.linearVelocityX < 0)
        {
            transform.rotation = Quaternion.AngleAxis(180f, Vector3.up);
        }
        else
        {
            transform.rotation = Quaternion.AngleAxis(0f, Vector3.up);
        }
    }

    public void OnMove(InputValue value)
    {
        // When a move key is pressed, get whether they are moving in a negative (left) or positive (right) direction.
        MovementDirection = value.Get<float>();
    }

    public void OnJump()
    {
        jumpCounter++;

        // Only allow jumping when the player is within coyote time unless they have double jump available.
        switch (doubleJumpAvailable)
        {
            case true when jumpCounter == 2:
            {
                _ = StartCoroutine(HandleDoubleJump());
                jumpCounter = 0;
                break;
            }
            case true when coyoteTimeTimer < 0f:
            case false when coyoteTimeTimer < 0f:
            {
                return;
            }
        }

        Rigidbody.linearVelocity = new Vector2(
            Rigidbody.linearVelocity.x,
            0
        );
        Rigidbody.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
        _ = StartCoroutine(HandleJumpGravity());
    }

    public void OnDash()
    {
        bool playerMoving = MovementDirection != 0f;
        if (!dashAvailable || !playerMoving)
        {
            return;
        }

        _ = StartCoroutine(HandleDash());
        _ = StartCoroutine(HandleDashCooldown());
    }

    private bool GetGroundedStatus()
    {
        Vector2 direction = Vector2.down;

        CapsuleCollider2D playerCollider = GetComponent<CapsuleCollider2D>();
        Vector2 leftPos = transform.position;
        leftPos.x -= playerCollider.size.x / 2;
        Vector2 rightPos = transform.position;
        rightPos.x += playerCollider.size.x / 2;

        Debug.DrawRay(leftPos, direction, Color.green);
        Debug.DrawRay(rightPos, direction, Color.green);

        RaycastHit2D leftHit = Physics2D.Raycast(leftPos, direction, groundedRayDistance, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(rightPos, direction, groundedRayDistance, groundLayer);
        return leftHit.collider || rightHit.collider;
    }

    private IEnumerator HandleDash()
    {
        Vector2 dashStartPos = transform.position;
        Vector2 dashForce = new(MovementDirection * dashSpeed, Rigidbody.linearVelocity.y);

        Rigidbody.AddForce(dashForce, ForceMode2D.Impulse);
        isDashing = true;
        float originalGravity = Rigidbody.gravityScale;
        Rigidbody.gravityScale = 0f;

        yield return new WaitUntil(() =>
        {
            bool isDashFinished = Vector2.Distance(dashStartPos, transform.position) >= dashDistance;
            return isDashFinished;
        });
        isDashing = false;
        Rigidbody.gravityScale = originalGravity;

        // Apply the negative dash force to remove the force of the dash once it's finished.
        Rigidbody.AddForce(-dashForce, ForceMode2D.Impulse);
    }

    private IEnumerator HandleDashCooldown()
    {
        dashAvailable = false;
        yield return new WaitForSeconds(dashCooldown);
        dashAvailable = true;
    }

    private IEnumerator HandleDoubleJump()
    {
        doubleJumpAvailable = false;
        yield return new WaitUntil(() => isGrounded);
        doubleJumpAvailable = true;
    }

    private IEnumerator HandleDeath()
    {
        Time.timeScale = 0.0f;
        yield return new WaitForSecondsRealtime(respawnTimer);
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator HandleJumpGravity()
    {
        yield return new WaitUntil(() => Rigidbody.linearVelocity.y < 0);
        Rigidbody.gravityScale = jumpGravityScale;
        yield return new WaitUntil(() => isGrounded || Rigidbody.linearVelocity.y >= 0);
        Rigidbody.gravityScale = defaultGravityScale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            _ = StartCoroutine(HandleDeath());
        }
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.CompareTag("DeathZone"))
        {
            _ = StartCoroutine(HandleDeath());
        }
    }


}
