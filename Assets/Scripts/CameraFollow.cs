using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;
    void FixedUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = target.position + offset;
        desiredPosition.z = transform.position.z;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}