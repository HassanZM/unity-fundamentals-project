using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController;
    private static readonly int idle = Animator.StringToHash("Idle");
    private static readonly int walk = Animator.StringToHash("Walk");
    private static readonly int jump = Animator.StringToHash("Jump");

    private static int currentState;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        currentState = idle;
        animator.CrossFade("Idle", 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        int state = GetState();
        if (state == currentState)
        {
            return;
        }
        animator.CrossFade(state, 0, 0);
        currentState = state;
    }

    private int GetState()
    {
        if (!playerController.IsGrounded)
        {
            return jump;
        }
        return playerController.IsIdle ? idle : walk;
    }
}
