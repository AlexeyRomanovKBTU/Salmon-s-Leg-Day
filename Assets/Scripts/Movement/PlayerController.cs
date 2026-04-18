using UnityEngine;
using UnityEngine.U2D.IK;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rb;
    public Animator anim;
    public PlayerInputHandler input;
    public Balance balance;
    public Balance upperBodyBalance;
    public Balance headBalance;
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
    [Tooltip("How fast the IK target moves toward the mouse in world units per second")]
    public float ikTargetSpeed = 8f;
    [Tooltip("Max distance the IK target can be from the foot's current position")]
    public float maxLegReach = 2f;
    [Tooltip("Radius of the CircleCast used to stop the IK target at obstacles. " +
             "Match this to the radius of your foot CircleCollider2D.")]
    public float ikTargetRadius = 0.1f;

    [Header("Ragdoll Settings")]
    public float ragdollWait = 3f;

    [Header("Visuals")]
    public GameObject jumpArrowPivot;
    public SpriteRenderer arrowSprite;
    public Balance bodyBalance;

    [HideInInspector] public float currentJumpForce;
    [HideInInspector] public float jumpAimAngle;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public float coyoteCounter;

    private PlayerBaseState _currentState;
    private PlayerStateFactory _states;

    public PlayerBaseState CurrentState { get => _currentState; set => _currentState = value; }

    private void Awake()
    {
        _states = new PlayerStateFactory(this);
        _currentState = _states.Idle();
        _currentState.EnterState();
    }

    private void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        if (isGrounded) coyoteCounter = coyoteTime;
        else coyoteCounter -= Time.deltaTime;

        if (balance != null)            balance.isGrounded      = isGrounded;
        if (upperBodyBalance != null) upperBodyBalance.isGrounded = isGrounded;
        if (headBalance != null)      headBalance.isGrounded      = isGrounded;

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

    // ── IK helpers ───────────────────────────────────────────────────────────

    public void EnableIK(bool enabled)
    {
        if (ikManager != null)
            ikManager.enabled = enabled;
    }

    // ── Ragdoll helpers ───────────────────────────────────────────────────────

    // Switch all leg bones between Kinematic (IK mode) and Dynamic (ragdoll mode)
    public void SetLegsPhysicsMode(RigidbodyType2D bodyType)
    {
        leftFootRB.bodyType  = bodyType;
        rightFootRB.bodyType = bodyType;
    }

    // Full ragdoll: disable IK, set legs dynamic so everything flops
    public void EnterRagdoll()
    {
        EnableIK(false);
        SetLegsPhysicsMode(RigidbodyType2D.Dynamic);
    }

    // Recover: re-enable IK, set legs back to kinematic
    public void ExitRagdoll()
    {
        SetLegsPhysicsMode(RigidbodyType2D.Kinematic);
        EnableIK(true);
    }
}