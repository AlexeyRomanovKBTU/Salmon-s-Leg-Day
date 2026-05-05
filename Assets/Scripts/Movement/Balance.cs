using UnityEngine;

public class Balance : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rootRB;
    public Transform leftFoot;
    public Transform rightFoot;

    [Header("Standing Settings")]
    public float standHeight = 2.5f;
    [SerializeField] private float uprightForce = 3f;
    [SerializeField] private float dampingForce = 5f;
    [SerializeField] private float rotationForce = 3f;
    [SerializeField] private float maxForce = 15f;

    [Header("Foot Grounding")]
    public float legGroundDist = 0.3f;
    public LayerMask groundLayer;

    private void Awake()
    {
        if (rootRB == null) return;

        // Measure the actual resting height so initial posError is ~0 and no startup force spike occurs
        Transform a = leftFoot  != null ? leftFoot  : rightFoot;
        Transform b = rightFoot != null ? rightFoot : leftFoot;
        if (a != null)
        {
            float footY = (a != null && b != null && a != b)
                ? ((Vector2)a.position + (Vector2)b.position).y * 0.5f
                : a.position.y;
            standHeight = rootRB.position.y - footY;
        }
    }

    private bool IsFootGrounded(Transform foot)
    {
        if (foot == null) return false;
        return Physics2D.Raycast(
            foot.position + Vector3.up * 0.05f,
            Vector2.down,
            legGroundDist + 0.05f,
            groundLayer).collider != null;
    }

    private void FixedUpdate()
    {
        if (rootRB == null) return;

        bool leftGrounded  = IsFootGrounded(leftFoot);
        bool rightGrounded = IsFootGrounded(rightFoot);

        if (!leftGrounded && !rightGrounded) return;

        Vector2 balancePoint = leftGrounded && rightGrounded
            ? ((Vector2)leftFoot.position + (Vector2)rightFoot.position) * 0.5f
            : leftGrounded ? (Vector2)leftFoot.position : (Vector2)rightFoot.position;

        float   yError = balancePoint.y + standHeight - rootRB.position.y;
        Vector2 force  = new Vector2(0f, yError * uprightForce - rootRB.linearVelocity.y * dampingForce);
        rootRB.AddForce(Vector2.ClampMagnitude(force, maxForce));

        float rotError = Mathf.DeltaAngle(rootRB.rotation, 0f);
        float torque   = rotError * rotationForce - rootRB.angularVelocity * (rotationForce * 1.2f);
        rootRB.AddTorque(Mathf.Clamp(torque, -maxForce, maxForce));
    }
}
