using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public bool IsHoldingJump { get; private set; }
    public bool RagdollTriggered { get; private set; }
    public Vector2 MousePosition { get; private set; }
    public bool IsClicking { get; private set; }

    [SerializeField] private float _jumpBufferTime = 0.2f;
    private float _jumpBufferCounter;
    public bool JumpBuffered => _jumpBufferCounter > 0;

    public void OnPointerPosition(InputAction.CallbackContext context)
        => MousePosition = context.ReadValue<Vector2>();

    public void OnGrab(InputAction.CallbackContext context)
    {
        if (context.started || context.performed) IsClicking = true;
        if (context.canceled) IsClicking = false;
    }

    public void OnMove(InputAction.CallbackContext context)
        => MoveInput = context.ReadValue<Vector2>();

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started) _jumpBufferCounter = _jumpBufferTime;
        IsHoldingJump = context.performed || context.started;
        if (context.canceled) _jumpBufferCounter = 0;
    }

    public void OnRagdoll(InputAction.CallbackContext context)
    {
        if (context.started) RagdollTriggered = true;
    }

    private void Update()
    {
        if (_jumpBufferCounter > 0)
            _jumpBufferCounter -= Time.deltaTime;
    }

    public void UseJumpBuffer()        => _jumpBufferCounter = 0;
    public void ResetRagdollTrigger()  => RagdollTriggered = false;
}