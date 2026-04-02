using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rb;
    public GameObject middleLeftLeg, middleRightLeg;
    public Animator anim;
    public PlayerInputHandler input;
    public Balance balance;
    public Transform groundCheck;

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
        
        // Coyote Time Logic
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
}