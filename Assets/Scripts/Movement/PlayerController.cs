using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rb;
    public Animator anim;
    public PlayerInputHandler input;
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
    public float bodyLinearDamping = 1f;
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
    private bool _suppressNextSave;
    private Rigidbody2D[] _allBodies;
    private Vector2[] _spawnPositions;
    private float[] _spawnRotations;
    private Vector2 _rbSpawnPos;
    private Vector2[] _checkpointPositions;
    private float[] _checkpointRotations;
    private bool _hasCheckpoint;
    private Camera _cam;
    private Rigidbody2D[] _bodyBones;
    private Rigidbody2D[] _footBones;

    public PlayerBaseState CurrentState { get => _currentState; set => _currentState = value; }

    private void Awake()
    {
        _cam = Camera.main;
        _bodyBones = new[] { torsoRootRB, lowerBodyRB, upperBodyRB, headRB };
        _footBones = new[] { leftFootRB, rightFootRB };

        SetBodyBonesGravity(1f);
        if (leftFootRB != null) { leftFootRB.bodyType = RigidbodyType2D.Dynamic; leftFootRB.gravityScale = 1f; }
        if (rightFootRB != null) { rightFootRB.bodyType = RigidbodyType2D.Dynamic; rightFootRB.gravityScale = 1f; }

        foreach (var b in _bodyBones)
        {
            if (b == null) continue;
            b.linearDamping = bodyLinearDamping;
            b.angularDamping = bodyAngularDamping;
        }

        if (leftLeg1) leftLeg1Bal = leftLeg1.GetComponent<Balance>();
        if (leftLeg2) leftLeg2Bal = leftLeg2.GetComponent<Balance>();
        if (rightLeg1) rightLeg1Bal = rightLeg1.GetComponent<Balance>();
        if (rightLeg2) rightLeg2Bal = rightLeg2.GetComponent<Balance>();

        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        _states = new PlayerStateFactory(this);
        _currentState = _states.Idle();
        _currentState.EnterState();

        _allBodies = GetAllBodies();
        _spawnPositions = new Vector2[_allBodies.Length];
        _spawnRotations = new float[_allBodies.Length];
        for (int i = 0; i < _allBodies.Length; i++)
        {
            _spawnPositions[i] = _allBodies[i].position;
            _spawnRotations[i] = _allBodies[i].rotation;
        }
        _rbSpawnPos = rb.position;
    }

    private void Start()
    {
        SaveData data = SaveSystem.Load();
        if (data == null || data.completed) return;

        if (data.bodyPosX != null && data.bodyPosX.Length == _allBodies.Length)
            StartCoroutine(RestoreFullState(data));
        else
            StartCoroutine(TeleportAfterPhysicsInit(new Vector2(data.playerX, data.playerY)));
    }

    private System.Collections.IEnumerator RestoreFullState(SaveData data)
    {
        var positions = new Vector2[_allBodies.Length];
        var rotations = new float[_allBodies.Length];
        for (int i = 0; i < _allBodies.Length; i++)
        {
            positions[i] = new Vector2(data.bodyPosX[i], data.bodyPosY[i]);
            rotations[i] = data.bodyRot[i];
        }
        yield return StartCoroutine(TeleportBodies(positions, rotations));
    }

    private System.Collections.IEnumerator TeleportAfterPhysicsInit(Vector2 savedPos)
    {
        Vector2 offset = savedPos - _rbSpawnPos;
        var positions = new Vector2[_allBodies.Length];
        var rotations = new float[_allBodies.Length];
        for (int i = 0; i < _allBodies.Length; i++)
        {
            positions[i] = _spawnPositions[i] + offset;
            rotations[i] = _spawnRotations[i];
        }
        yield return StartCoroutine(TeleportBodies(positions, rotations));
    }

    private System.Collections.IEnumerator TeleportBodies(Vector2[] positions, float[] rotations)
    {
        yield return new WaitForFixedUpdate();
        for (int i = 0; i < _allBodies.Length; i++)
        {
            _allBodies[i].position = positions[i];
            _allBodies[i].rotation = rotations[i];
            _allBodies[i].linearVelocity = Vector2.zero;
            _allBodies[i].angularVelocity = 0f;
        }
    }

    public void PlaceCheckpoint()
    {
        _checkpointPositions = new Vector2[_allBodies.Length];
        _checkpointRotations = new float[_allBodies.Length];
        for (int i = 0; i < _allBodies.Length; i++)
        {
            _checkpointPositions[i] = _allBodies[i].position;
            _checkpointRotations[i] = _allBodies[i].rotation;
        }
        _hasCheckpoint = true;
    }

    private SaveData BuildSaveData()
    {
        var posX = new float[_allBodies.Length];
        var posY = new float[_allBodies.Length];
        var rot  = new float[_allBodies.Length];
        for (int i = 0; i < _allBodies.Length; i++)
        {
            posX[i] = _allBodies[i].position.x;
            posY[i] = _allBodies[i].position.y;
            rot[i]  = _allBodies[i].rotation;
        }
        return new SaveData
        {
            playerX  = rb.position.x,
            playerY  = rb.position.y,
            completed = false,
            bodyPosX = posX,
            bodyPosY = posY,
            bodyRot  = rot
        };
    }

    private void OnApplicationQuit()
    {
        if (_suppressNextSave) return;
        SaveSystem.Save(BuildSaveData());
    }

    private Rigidbody2D[] GetAllBodies()
    {
        var set = new System.Collections.Generic.HashSet<Rigidbody2D>();
        foreach (var body in GetComponentsInChildren<Rigidbody2D>(true))
            set.Add(body);
        foreach (var body in new[] { rb, torsoRootRB, lowerBodyRB, upperBodyRB, headRB, leftFootRB, rightFootRB })
            if (body != null) set.Add(body);
        foreach (var t in new[] { leftLeg1, leftLeg2, rightLeg1, rightLeg2 })
        {
            if (t == null) continue;
            var legRb = t.GetComponent<Rigidbody2D>();
            if (legRb != null) set.Add(legRb);
        }
        var arr = new Rigidbody2D[set.Count];
        set.CopyTo(arr);
        return arr;
    }

    private void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current.f5Key.wasPressedThisFrame) SaveSystem.Save(BuildSaveData());
        if (UnityEngine.InputSystem.Keyboard.current.f6Key.wasPressedThisFrame) { SaveSystem.Delete(); _suppressNextSave = true; }
        if (UnityEngine.InputSystem.Keyboard.current.f7Key.wasPressedThisFrame) Debug.Log(Application.persistentDataPath + "/save.json");

        if (input.PlaceCheckpointTriggered)
        {
            PlaceCheckpoint();
            input.ResetPlaceCheckpointTrigger();
        }
        if (input.ReturnToCheckpointTriggered)
        {
            if (_hasCheckpoint) StartCoroutine(TeleportBodies(_checkpointPositions, _checkpointRotations));
            input.ResetReturnToCheckpointTrigger();
        }

        isGrounded = Physics2D.OverlapCircle(leftFootTransform.position, groundRadius, groundLayer)
                  || Physics2D.OverlapCircle(rightFootTransform.position, groundRadius, groundLayer);

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
#if UNITY_EDITOR
        Debug.Log($"[StateMachine] {_currentState.GetType().Name} → {newState.GetType().Name}");
#endif
        _currentState.ExitState();
        _currentState = newState;
        _currentState.EnterState();
    }

    public Vector2 GetMouseWorldPos()
    {
        Vector3 p = new Vector3(
            input.MousePosition.x,
            input.MousePosition.y,
            Mathf.Abs(_cam.transform.position.z));
        return _cam.ScreenToWorldPoint(p);
    }

    public void SetBodyBonesGravity(float scale)
    {
        if (_bodyBones == null) return;
        foreach (var b in _bodyBones)
            if (b != null) b.gravityScale = scale;
    }

    public void ExitRagdoll()
    {
        ZeroBodyVelocities();
        ZeroLegVelocities();
    }

    private void ZeroBodyVelocities()
    {
        foreach (var rb in _bodyBones)
        {
            if (rb == null) continue;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void ZeroLegVelocities()
    {
        foreach (var rb in _footBones)
        {
            if (rb == null) continue;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    public bool AreBothFeetGrounded() =>
        Physics2D.OverlapCircle(leftFootTransform.position,  groundRadius, groundLayer) &&
        Physics2D.OverlapCircle(rightFootTransform.position, groundRadius, groundLayer);

    public void ResetAllLegBalance()
    {
        leftLeg1Bal?.ResetToUpright();
        leftLeg2Bal?.ResetToUpright();
        rightLeg1Bal?.ResetToUpright();
        rightLeg2Bal?.ResetToUpright();
    }

    private void OnDrawGizmos()
    {
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
