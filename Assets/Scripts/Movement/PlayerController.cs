using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rb;
    public Animator anim;
    public PlayerInputHandler input;
    public Transform groundCheck;

    [Header("Leg Bones")]
    public Rigidbody2D leftFootRB;
    public Rigidbody2D rightFootRB;
    public Transform leftFootTransform;
    public Transform rightFootTransform;
    public Collider2D leftFootCollider;
    public Collider2D rightFootCollider;

    [Header("Body Bones")]
    public Rigidbody2D torsoRootRB;
    public Rigidbody2D lowerBodyRB;
    public Rigidbody2D upperBodyRB;
    public Rigidbody2D headRB;

    [Header("Ground Settings")]
    public float groundRadius = 0.4f;
    public LayerMask groundLayer;
    public float coyoteTime = 0.15f;

    [Header("Jump Settings")]
    public float initialJumpForce = 1000f;
    public float maxJumpForce = 3500f;
    public float jumpForceInc = 1500f;

    [Header("Walk Settings")]
    public float footForce = 60f;
    public float footDamping = 8f;
    public float footGrabRadius = 0.6f;
    [Tooltip("Max world-units/sec the drag target can chase the mouse — smooths out fast flicks")]
    public float footMaxMouseSpeed = 8f;
    [Tooltip("Max world-units/sec the foot itself can move while being dragged — prevents snap after unstick")]
    public float footMaxDragSpeed = 8f;
    [Tooltip("Max distance the active foot can be dragged from the anchor foot")]
    public float footMaxDragDistance = 2.5f;

    [Header("Leg Segment Transforms")]
    [Tooltip("Assign Left_Leg_1_Layer and Left_Leg_2_Layer")]
    public Transform leftLeg1;
    public Transform leftLeg2;
    [Tooltip("Assign Right_Leg_1_Layer and Right_Leg_2_Layer")]
    public Transform rightLeg1;
    public Transform rightLeg2;

    [Header("Body Damping")]
    [Tooltip("Linear drag on all body bones — higher values resist joint forces from leg drags")]
    public float bodyLinearDamping  = 1f;
    [Tooltip("Angular drag on all body bones — higher values reduce body spinning while walking")]
    public float bodyAngularDamping = 1f;

    [Header("Ragdoll Settings")]
    public float ragdollWait = 3f;

    [Header("Visuals")]
    public GameObject jumpArrowPivot;
    public SpriteRenderer arrowSprite;

    [HideInInspector] public Balance leftLeg1Bal;
    [HideInInspector] public Balance leftLeg2Bal;
    [HideInInspector] public Balance rightLeg1Bal;
    [HideInInspector] public Balance rightLeg2Bal;

    [HideInInspector] public float currentJumpForce;
    [HideInInspector] public float jumpAimAngle;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public float coyoteCounter;

    private PlayerBaseState _currentState;
    private PlayerStateFactory _states;

    public PlayerBaseState CurrentState { get => _currentState; set => _currentState = value; }

    private void Awake()
    {
        SetBodyBonesGravity(1f);
        if (leftFootRB  != null) { leftFootRB.bodyType  = RigidbodyType2D.Dynamic; leftFootRB.gravityScale  = 1f; }
        if (rightFootRB != null) { rightFootRB.bodyType = RigidbodyType2D.Dynamic; rightFootRB.gravityScale = 1f; }

        foreach (var b in new[] { torsoRootRB, lowerBodyRB, upperBodyRB, headRB })
        {
            if (b == null) continue;
            b.linearDamping  = bodyLinearDamping;
            b.angularDamping = bodyAngularDamping;
        }

        if (leftLeg1)  leftLeg1Bal  = leftLeg1.GetComponent<Balance>();
        if (leftLeg2)  leftLeg2Bal  = leftLeg2.GetComponent<Balance>();
        if (rightLeg1) rightLeg1Bal = rightLeg1.GetComponent<Balance>();
        if (rightLeg2) rightLeg2Bal = rightLeg2.GetComponent<Balance>();

        _states = new PlayerStateFactory(this);
        _currentState = _states.Idle();
        _currentState.EnterState();
    }

    private void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        if (isGrounded) coyoteCounter = coyoteTime;
        else coyoteCounter -= Time.deltaTime;

        _currentState.UpdateState();
        _currentState.CheckSwitchStates();
    }

    private void FixedUpdate()
    {
        if (_currentState is PlayerWalkState walk)
            walk.FixedUpdateState();
    }

    public void SwitchState(PlayerBaseState newState)
    {
        Debug.Log($"[StateMachine] {_currentState.GetType().Name} → {newState.GetType().Name}");
        _currentState.ExitState();
        _currentState = newState;
        _currentState.EnterState();
    }

    public Vector2 GetMouseWorldPos()
    {
        Vector3 p = new Vector3(
            input.MousePosition.x,
            input.MousePosition.y,
            Mathf.Abs(Camera.main.transform.position.z));
        return Camera.main.ScreenToWorldPoint(p);
    }

    public void SetBodyBonesGravity(float scale)
    {
        if (torsoRootRB != null) torsoRootRB.gravityScale = scale;
        if (lowerBodyRB != null) lowerBodyRB.gravityScale = scale;
        if (upperBodyRB != null) upperBodyRB.gravityScale = scale;
        if (headRB      != null) headRB.gravityScale      = scale;
    }

    public void ExitRagdoll()
    {
        ZeroBodyVelocities();
        ZeroLegVelocities();
    }

    private void ZeroBodyVelocities()
    {
        foreach (var rb in new[] { torsoRootRB, lowerBodyRB, upperBodyRB, headRB })
        {
            if (rb == null) continue;
            rb.linearVelocity  = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void ZeroLegVelocities()
    {
        foreach (var rb in new[] { leftFootRB, rightFootRB })
        {
            if (rb == null) continue;
            rb.linearVelocity  = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    public bool AreBothFeetGrounded() =>
        Physics2D.OverlapCircle(leftFootTransform.position,  groundRadius, groundLayer) &&
        Physics2D.OverlapCircle(rightFootTransform.position, groundRadius, groundLayer);

    public void ResetAllLegBalance()
    {
        foreach (var t in new[] { leftLeg1, leftLeg2, rightLeg1, rightLeg2 })
        {
            if (t == null) continue;
            var bal = t.GetComponent<Balance>();
            bal?.ResetToUpright();
        }
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }

        if (leftFootTransform != null)
        {
            bool leftGrounded = Physics2D.OverlapCircle(leftFootTransform.position, groundRadius, groundLayer);
            Gizmos.color = leftGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(leftFootTransform.position, groundRadius);
        }

        if (rightFootTransform != null)
        {
            bool rightGrounded = Physics2D.OverlapCircle(rightFootTransform.position, groundRadius, groundLayer);
            Gizmos.color = rightGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(rightFootTransform.position, groundRadius);
        }
    }
}
