using UnityEditor.Experimental.GraphView;
using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
[RequireComponent (typeof(CapsuleCollider2D))]
public class WanderingEnemyController : MonoBehaviour
{
    [SerializeField]
    private int speed;
    [SerializeField, Tooltip("The distance to send a ray-cast to check whether the enemy is going to go off a ledge.")]
    private float ledgeCheckDistance;

    public Rigidbody2D Rb2D => rb2d;
    private Rigidbody2D rb2d;

    private bool hasFoundLedge = false;
    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        rb2d.linearVelocity = new Vector2(speed, rb2d.linearVelocity.y);
    }

    void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (!hasFoundLedge && CheckChangeDirection() || CheckObstacle())
        {
            hasFoundLedge = true;
            ChangeDirection();
        }
        if (!CheckChangeDirection())
        {
            hasFoundLedge = false;
        }
        VerticalTransformFlip();
    }

    private bool CheckChangeDirection()
    {
        Vector2 direction = Vector2.down;

        CapsuleCollider2D collider = GetComponent<CapsuleCollider2D>();
        Vector2 leftPos = transform.position;
        leftPos.x -= collider.size.x / 2;
        Vector2 rightPos = transform.position;
        rightPos.x += collider.size.x / 2;

        Debug.DrawRay(leftPos, direction * ledgeCheckDistance, Color.red);
        Debug.DrawRay(rightPos, direction * ledgeCheckDistance, Color.red);

        RaycastHit2D leftHit = Physics2D.Raycast(leftPos, direction, ledgeCheckDistance);
        RaycastHit2D rightHit = Physics2D.Raycast(rightPos, direction, ledgeCheckDistance);
        
        return !leftHit || !rightHit;
    }

    private bool CheckObstacle()
    {
        Vector2 direction = (rb2d.linearVelocityX < 0) ? Vector2.left : Vector2.right;
        Vector2 position = new(transform.position.x, transform.position.y);
        RaycastHit2D[] obstacleHits = Physics2D.RaycastAll(position, direction, ledgeCheckDistance);
        Debug.DrawRay(position, direction, Color.red);
        foreach (RaycastHit2D hit in obstacleHits)
        {
            if (hit.transform.CompareTag("Ground") && hit) return true;
        }
        return false;
    }

    public void ChangeDirection()
    {
        Debug.Log(rb2d.linearVelocity);
        rb2d.linearVelocity = new Vector2(-rb2d.linearVelocityX, rb2d.linearVelocityY);
        Debug.Log(rb2d.linearVelocity);
    }

    private void VerticalTransformFlip()
    {
        if (rb2d.linearVelocityX < 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
        }

        if (rb2d.linearVelocityX > 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
    }
}
