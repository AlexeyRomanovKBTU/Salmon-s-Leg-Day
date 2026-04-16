using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rb;
    public GameObject leftLegFeet, rightLegFeet;
    public Animator anim;
    public PlayerInputHandler input;
    public Balance balance;
    public Transform groundCheck;

    [Header("Mouse Interaction")]
    public TargetJoint2D leftTargetJoint;
    public TargetJoint2D rightTargetJoint;

    [Header("Settings")]
    public float speedForce = 10f;
    public float stepWait = 0.3f;
    public float initialJumpForce = 1000f;
    public float maxJumpForce = 3500f;
    public float jumpForceInc = 1500f;
    public float groundRadius = 0.4f;
    public LayerMask groundLayer;
    public float coyoteTime = 0.15f;
    public float ragdollWait = 3f;
    public float legGroundDist = 0.2f; // Distance for leg ground check

    [Header("Visuals")]
    public GameObject jumpArrowPivot;
    public SpriteRenderer arrowSprite;

    [HideInInspector] public float currentJumpForce;
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

        _currentState.UpdateState();
        _currentState.CheckSwitchStates();
    }

    public void SwitchState(PlayerBaseState newState)
    {
        _currentState.ExitState();
        newState.EnterState();
        _currentState = newState;
    }

    public Vector2 GetMouseWorldPos() 
    {
        Vector3 mousePoint = new Vector3(input.MousePosition.x, input.MousePosition.y, Mathf.Abs(Camera.main.transform.position.z));
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    public bool IsLegGrounded(GameObject leg)
    {
        // Fires a tiny ray down from the foot to see if it's standing on the ground layer
        RaycastHit2D hit = Physics2D.Raycast(leg.transform.position, Vector2.down, legGroundDist, groundLayer);
        return hit.collider != null;
    }

    public void SetAllLegsDynamic()
    {
        leftLegFeet.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        rightLegFeet.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        leftTargetJoint.enabled = false;
        rightTargetJoint.enabled = false;
    }
}