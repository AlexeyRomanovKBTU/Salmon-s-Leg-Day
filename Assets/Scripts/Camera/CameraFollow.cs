using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [Tooltip("Higher = faster follow (try 10-20)")]
    [SerializeField] private float smoothSpeed = 15f;
    [Tooltip("Snap instantly if target is farther than this")]
    [SerializeField] private float snapDistance = 8f;
    [SerializeField] private Vector3 offset;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        desiredPosition.z = transform.position.z;

        if (Vector3.Distance(transform.position, desiredPosition) > snapDistance)
            transform.position = desiredPosition;
        else
            transform.position = Vector3.Lerp(transform.position, desiredPosition, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));
    }
}