using UnityEngine;

// Keeps the salmon upright by applying forces toward a standing position
// calculated from the midpoint between the two feet.
// Only active when grounded — in the air everything flops freely.
public class Balance : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The root Rigidbody2D to apply forces to (Salmon root)")]
    public Rigidbody2D rootRB;

    [Tooltip("Left foot IK target transform")]
    public Transform leftFootTarget;

    [Tooltip("Right foot IK target transform")]
    public Transform rightFootTarget;

    [Header("Standing Settings")]
    [Tooltip("How high above the feet midpoint the root should float. " +
             "Match this to roughly half your salmon's height.")]
    public float standHeight = 2.5f;

    [Tooltip("How strongly the root is pulled to the target standing position. " +
             "Start around 15, raise for stiffer feel.")]
    [SerializeField] private float uprightForce = 15f;

    [Tooltip("Damps lateral and vertical velocity to prevent oscillation. " +
             "Start around 4.")]
    [SerializeField] private float dampingForce = 4f;

    [Tooltip("How strongly rotation is corrected toward upright (0 degrees). " +
             "Keep low — 3 to 8.")]
    [SerializeField] private float rotationForce = 5f;

    [Tooltip("Max force that can be applied per frame. Safety clamp.")]
    [SerializeField] private float maxForce = 30f;

    [Header("State")]
    [Tooltip("Set by PlayerController — balance only acts when this is true")]
    public bool isGrounded = false;

    private void FixedUpdate()
    {
        if (!isGrounded) return;
        if (rootRB == null || leftFootTarget == null || rightFootTarget == null) return;

        // ── Calculate the ideal standing position ────────────────────────────
        // Midpoint between the two feet, offset upward by standHeight
        Vector2 feetMid = ((Vector2)leftFootTarget.position
                         + (Vector2)rightFootTarget.position) * 0.5f;

        Vector2 targetPos = feetMid + Vector2.up * standHeight;
        Vector2 currentPos = rootRB.position;

        // ── Position correction force ─────────────────────────────────────────
        Vector2 posError  = targetPos - currentPos;
        Vector2 damping   = -rootRB.linearVelocity * dampingForce;
        Vector2 force     = posError * uprightForce + damping;

        // Clamp to maxForce so it never violently snaps the chain
        if (force.magnitude > maxForce)
            force = force.normalized * maxForce;

        rootRB.AddForce(force);

        // ── Rotation correction torque ────────────────────────────────────────
        // Gently drives the root back to 0 degrees (upright)
        float rotError = Mathf.DeltaAngle(rootRB.rotation, 0f);
        float torque   = rotError * rotationForce - rootRB.angularVelocity * (rotationForce * 0.5f);
        torque         = Mathf.Clamp(torque, -maxForce, maxForce);
        rootRB.AddTorque(torque);
    }
}