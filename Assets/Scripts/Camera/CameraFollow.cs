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

    private void OnDrawGizmos()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        desiredPosition.z = transform.position.z;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.15f);
        Gizmos.DrawSphere(desiredPosition, snapDistance);

        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
        Gizmos.DrawWireSphere(desiredPosition, snapDistance);
    }
}