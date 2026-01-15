using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class FallingBlockController : MonoBehaviour
{
    public enum State
    {
        Fall,
        Rest,
        Reset
    }
    public State CurrentState => currentState;

    [SerializeField, Tooltip("Mace downwards speed")]
    private float downwardSpeed;
    [SerializeField, Tooltip("Mace upwards speed")] 
    private float upwardSpeed;
    [SerializeField, Tooltip("Slam cooldown")]
    private float cooldown;

    private Vector3 restingPosition;
    private State currentState;
    
    private void Start()
    {
        restingPosition = transform.position;
        currentState = State.Fall;
    }

    
    private void Update()
    {
        switch (currentState)
        {
            case State.Fall:
            {
                float step = downwardSpeed * Time.deltaTime;
                transform.position += (Vector3.down * step);
                break;
            }
            case State.Reset:
            {
                HandleReset();
                break;
            }
            case State.Rest:
            default:
            {
                break;
            }
        }
    }

    private void HandleReset()
    {
        float step = upwardSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, restingPosition, step);
        if (transform.position != restingPosition)
        {
            return;
        }

        currentState = State.Rest;
        _ = StartCoroutine(HandleSlamCooldown());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Ground"))
        {
            currentState = State.Reset;
        }
    }

    private IEnumerator HandleSlamCooldown()
    {
        yield return new WaitForSeconds(cooldown);
        currentState = State.Fall;
    }
    
    
}

