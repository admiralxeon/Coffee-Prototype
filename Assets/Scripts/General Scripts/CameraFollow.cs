using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 6f, -4f);
    public float smoothSpeed = 0.125f;
    
    [Header("Camera Bounds")]
    public bool useBounds = true;
    public Vector3 minBounds = new Vector3(-10f, 5f, -10f);
    public Vector3 maxBounds = new Vector3(10f, 15f, 10f);
    
    [Header("Look At Settings")]
    public bool lookAtTarget = true;
    public Vector3 lookAtOffset = Vector3.up;
    
    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        Vector3 desiredPosition = target.position + offset;
        
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
            desiredPosition.z = Mathf.Clamp(desiredPosition.z, minBounds.z, maxBounds.z);
        }
        
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
        
        if (lookAtTarget && target != null)
        {
            Vector3 lookTarget = target.position + lookAtOffset;
            transform.LookAt(lookTarget);
        }
    }
}