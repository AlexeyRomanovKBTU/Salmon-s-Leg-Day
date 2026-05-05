using UnityEngine;
using UnityEngine.U2D.IK;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rb;
    public Animator anim;
    public PlayerInputHandler input;
    public Transform groundCheck;

    [Header("IK")]
    public IKManager2D ikManager;
    public Transform ikTargetLeft;
    public Transform ikTargetRight;

    [Header("Leg Bones (for ground check + ragdoll)")]
    public Rigidbody2D leftFootRB;
    public Rigidbody2D rightFootRB;
    public Transform leftFootTransform;
    public Transform rightFootTransform;
    public Collider2D leftFootCollider;
    public Collider2D rightFootCollider;

    [Header("Body Bones (for ragdoll)")]
    public Rigidbody2D torsoRootRB;
    public Rigidbody2D lowerBodyRB;
    public Rigidbody2D upperBodyRB;
    public Rigidbody2D headRB;

    [Header("Ground Settings")]
    public float groundRadius = 0.4f;
    public LayerMask groundLayer;
    public float coyoteTime = 0.15f;
    public float legGroundDist = 0.2f;

    [Header("Jump Settings")]
    public float initialJumpForce = 1000f;
    public float maxJumpForce = 3500f;
    public float jumpForceInc = 1500f;

    [Header("Walk Settings")]
    [Tooltip("Max speed cap for foot target movement (world units/sec)")]
    public float ikTargetSpeed = 8f;
    [Tooltip("Time in seconds for the foot to smoothly reach its target — lower = snappier")]
    public float footSmoothTime = 0.12f;
    [Tooltip("Radius of the CircleCast used to stop the IK target at obstacles. " +
             "Match this to the radius of your foot CircleCollider2D.")]
    public float ikTargetRadius = 0.1f;
    [Tooltip("How high the foot lifts off the ground while being dragged")]
    public float footLiftHeight = 0.4f;
    [Tooltip("World-space grab radius around each foot — increase to make feet easier to click")]
    public float footGrabRadius = 0.4f;
    [Tooltip("Horizontal force pulling the torso toward the dragged foot")]
    public float bodyLeanForce = 8f;
    [Tooltip("Force pushing the torso back when it drifts further than one leg-length from the anchor foot")]
    public float bodyAnchorForce = 12f;
    [Tooltip("Impulse applied to the torso when a foot is planted — scales with step distance")]
    public float plantImpulse = 2f;

    [Header("Ragdoll Settings")]
    public float ragdollWait = 3f;

    [Header("Visuals")]
    public GameObject jumpArrowPivot;
    public SpriteRenderer arrowSprite;

    [HideInInspector] public float currentJumpForce;
    [HideInInspector] public float jumpAimAngle;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public float coyoteCounter;

    private PlayerBaseState _currentState;
    private PlayerStateFactory _states;

    public PlayerBaseState CurrentState { get => _currentState; set => _currentState = value; }

    private void Awake()
    {
        SetBodyBonesGravity(0f);
        if (torsoRootRB != null)
        {
            torsoRootRB.linearDamping  = 3f;
            torsoRootRB.angularDamping = 3f;
        }
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

    public bool IsLegGrounded(Transform foot)
    {
        RaycastHit2D hit = Physics2D.Raycast(
            foot.position, Vector2.down, legGroundDist, groundLayer);
        return hit.collider != null;
    }

    public void EnableIK(bool enabled)
    {
        if (ikManager != null)
            ikManager.enabled = enabled;
    }

    public void SetLegsPhysicsMode(RigidbodyType2D bodyType)
    {
        leftFootRB.bodyType  = bodyType;
        rightFootRB.bodyType = bodyType;
    }

    public void SetBodyBonesGravity(float scale)
    {
        if (torsoRootRB != null)  torsoRootRB.gravityScale  = scale;
        if (lowerBodyRB != null)  lowerBodyRB.gravityScale  = scale;
        if (upperBodyRB != null)  upperBodyRB.gravityScale  = scale;
        if (headRB != null)       headRB.gravityScale       = scale;
    }

    public void EnterRagdoll()
    {
        EnableIK(false);
        SetLegsPhysicsMode(RigidbodyType2D.Dynamic);
        SetBodyBonesGravity(1f);
    }

    public void ExitRagdoll()
    {
        SetLegsPhysicsMode(RigidbodyType2D.Kinematic);
        EnableIK(true);
        SetBodyBonesGravity(0f);
        ZeroBodyVelocities();
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
}
