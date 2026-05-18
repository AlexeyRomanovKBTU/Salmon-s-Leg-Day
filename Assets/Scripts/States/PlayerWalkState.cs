using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{
    private Rigidbody2D _activeFootRB;
    private Rigidbody2D _anchorFootRB;
    private Balance _legBal1;
    private Balance _legBal2;
    private Vector2 _smoothedDragTarget;
    private Vector2 _anchorLockedPos;

    public PlayerWalkState(PlayerController ctx, PlayerStateFactory factory) : base(ctx, factory) {}

    public override void EnterState() { }

    public override void UpdateState()
    {
        if (_anchorFootRB != null && !Physics2D.OverlapCircle(_anchorLockedPos, Ctx.groundRadius, Ctx.groundLayer))
        {
            ReleaseFoot();
            return;
        }

        if (Ctx.input.IsClicking && _activeFootRB == null)
            TryGrabFoot();

        if (!Ctx.input.IsClicking && _activeFootRB != null)
            ReleaseFoot();
    }

    public void FixedUpdateState()
    {
        if (_activeFootRB == null) return;

        _smoothedDragTarget = Vector2.MoveTowards(
            _smoothedDragTarget,
            Ctx.GetMouseWorldPos(),
            Ctx.footMaxMouseSpeed * Time.fixedDeltaTime);

        if (_anchorFootRB != null)
        {
            Vector2 fromAnchor = _smoothedDragTarget - _anchorLockedPos;
            if (fromAnchor.magnitude > Ctx.footMaxDragDistance)
                _smoothedDragTarget = _anchorLockedPos + fromAnchor.normalized * Ctx.footMaxDragDistance;
        }

        Vector2 toTarget = _smoothedDragTarget - _activeFootRB.position;
        Vector2 targetVel = toTarget * Ctx.footForce;

        float t = 1f - Mathf.Exp(-Ctx.footDamping * Time.fixedDeltaTime);
        Vector2 newVel = Vector2.Lerp(_activeFootRB.linearVelocity, targetVel, t);

        _activeFootRB.linearVelocity = Vector2.ClampMagnitude(newVel, Ctx.footMaxDragSpeed);
        _activeFootRB.angularVelocity = Mathf.Lerp(_activeFootRB.angularVelocity, 0f, t);

        if (_anchorFootRB != null)
        {
            _anchorFootRB.position = _anchorLockedPos;
            _anchorFootRB.linearVelocity = Vector2.zero;
            _anchorFootRB.angularVelocity = 0f;
        }
    }

    private void TryGrabFoot()
    {
        bool leftGrounded = Physics2D.OverlapCircle(Ctx.leftFootTransform.position, Ctx.groundRadius, Ctx.groundLayer);
        bool rightGrounded = Physics2D.OverlapCircle(Ctx.rightFootTransform.position, Ctx.groundRadius, Ctx.groundLayer);

        if (!leftGrounded && !rightGrounded) return;

        Vector2 mousePos = Ctx.GetMouseWorldPos();

        bool hitLeft = Ctx.leftFootCollider.OverlapPoint(mousePos);
        bool hitRight = Ctx.rightFootCollider.OverlapPoint(mousePos);

        float distLeft = Vector2.Distance(mousePos, Ctx.leftFootTransform.position);
        float distRight = Vector2.Distance(mousePos, Ctx.rightFootTransform.position);

        if (!hitLeft && !hitRight)
        {
            hitLeft = distLeft <= Ctx.footGrabRadius;
            hitRight = distRight <= Ctx.footGrabRadius;
        }

        if (!hitLeft && !hitRight) return;

        if (hitLeft && hitRight)
        {
            if (distLeft > distRight) hitLeft = false;
            else hitRight = false;
        }

        if (hitLeft && leftGrounded && !rightGrounded) return;
        if (hitRight && rightGrounded && !leftGrounded) return;

        _activeFootRB = hitLeft ? Ctx.leftFootRB : Ctx.rightFootRB;
        _smoothedDragTarget = _activeFootRB.position;

        _anchorFootRB = hitLeft ? Ctx.rightFootRB : Ctx.leftFootRB;
        if (_anchorFootRB != null)
        {
            _anchorLockedPos = _anchorFootRB.position;
            _anchorFootRB.linearVelocity = Vector2.zero;
            _anchorFootRB.angularVelocity = 0f;
            _anchorFootRB.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        _legBal1 = hitLeft ? Ctx.leftLeg1Bal : Ctx.rightLeg1Bal;
        _legBal2 = hitLeft ? Ctx.leftLeg2Bal : Ctx.rightLeg2Bal;
        _legBal1?.Freeze();
        _legBal2?.Freeze();
    }

    private void ReleaseFoot()
    {
        _activeFootRB.linearVelocity = Vector2.zero;
        _activeFootRB.angularVelocity = 0f;

        _legBal1?.Freeze();
        _legBal2?.Freeze();

        if (_anchorFootRB != null)
        {
            _anchorFootRB.constraints = RigidbodyConstraints2D.None;
            _anchorFootRB = null;
        }

        _activeFootRB = null;
        _legBal1 = null;
        _legBal2 = null;
    }

    public override void ExitState()
    {
        if (_activeFootRB != null) ReleaseFoot();
    }

    public override void CheckSwitchStates()
    {
        if (Ctx.input.RagdollTriggered)
        {
            Ctx.input.ResetRagdollTrigger();
            Ctx.SwitchState(Factory.Ragdoll());
            return;
        }

        if ((Ctx.input.IsHoldingJump || Ctx.input.JumpBuffered)
            && _activeFootRB == null
            && (Ctx.isGrounded || Ctx.coyoteCounter > 0))
        {
            Ctx.input.UseJumpBuffer();
            Ctx.SwitchState(Factory.Jump());
            return;
        }

        if (!Ctx.input.IsClicking && _activeFootRB == null)
            Ctx.SwitchState(Factory.Idle());
    }
}
