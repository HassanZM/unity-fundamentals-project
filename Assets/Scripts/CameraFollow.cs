using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    [SerializeField, Tooltip("Offset distance in the x & y directions.")] 
    private Vector2 offset = new(0f, 0f);
    [SerializeField, Tooltip("Smoothing time for camera follow in seconds.")] 
    private float smoothTime = 0.15f;
    [SerializeField, Tooltip("Set a minimum camera value in the Y direction.")]
    private float yMinClamp = 0;
    [SerializeField, Tooltip("Set a maximum camera value in the Y direction.")]
    private float yMaxClamp = 10;

    private Vector3 velocity;


    // Update is called once per frame
    private void Update()
    {
        Vector3 targetPos = target.position;
        targetPos.x += offset.x;
        targetPos.y = Mathf.Clamp(targetPos.y + offset.y, yMinClamp, yMaxClamp);
        targetPos.z = transform.position.z;
        
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }
}
