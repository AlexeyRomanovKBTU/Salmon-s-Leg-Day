using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{
    private Transform _activeTarget;
    private Transform _anchorTarget;
    private Transform _activeFootBone;
    private Vector2   _desiredTargetPos;
    private Vector2   _footVelocity;
    private Vector2   _anchorWorldPos;
    private float     _maxBodyDist;
    private Vector2   _footGrabPos;

    public PlayerWalkState(PlayerController ctx, PlayerStateFactory factory) : base(ctx, factory) {}

    public override void EnterState()
    {
        Ctx.ikTargetLeft.position  = Ctx.leftFootTransform.position;
        Ctx.ikTargetRight.position = Ctx.rightFootTransform.position;
        Ctx.SetLegsPhysicsMode(RigidbodyType2D.Kinematic);
        Ctx.EnableIK(true);
    }

    public override void UpdateState()
    {
        if (Ctx.input.IsClicking && _activeTarget == null)
            TryGrabFoot();

        if (Ctx.input.IsClicking && _activeTarget != null)
            UpdateDesiredPosition();

        if (!Ctx.input.IsClicking && _activeTarget != null)
            PlantFoot();
    }

    public void FixedUpdateState()
    {
        if (_activeTarget == null) return;

        MoveFoot();
        PullBody();
    }

    private void MoveFoot()
    {
        Vector2 currentPos = _activeTarget.position;
        Vector2 moveDir    = _desiredTargetPos - currentPos;
        float   moveDist   = moveDir.magnitude;

        Vector2 safeTarget = _desiredTargetPos;
        if (moveDist > 0.001f)
        {
            RaycastHit2D hit = Physics2D.CircleCast(
                currentPos, Ctx.ikTargetRadius,
                moveDir.normalized, moveDist, Ctx.groundLayer);

            if (hit.collider != null)
            {
                if (hit.distance > 0f)
                    safeTarget = hit.centroid;
                else if (Vector2.Dot(moveDir.normalized, hit.normal) < 0f)
                    safeTarget = currentPos;
            }
        }

        _activeTarget.position = Vector2.SmoothDamp(
            currentPos, safeTarget, ref _footVelocity,
            Ctx.footSmoothTime, Ctx.ikTargetSpeed);
    }

    private void PullBody()
    {
        Vector2 toFoot = _desiredTargetPos - (Vector2)Ctx.torsoRootRB.position;
        Ctx.torsoRootRB.AddForce(toFoot * Ctx.bodyLeanForce);
    }

    private void TryGrabFoot()
    {
        Vector2 mousePos = Ctx.GetMouseWorldPos();
        float   grab     = Ctx.footGrabRadius;

        float distLeft  = Vector2.Distance(mousePos, (Vector2)Ctx.leftFootCollider.bounds.center);
        float distRight = Vector2.Distance(mousePos, (Vector2)Ctx.rightFootCollider.bounds.center);

        bool hitLeft  = distLeft  <= grab;
        bool hitRight = distRight <= grab;

        if (!hitLeft && !hitRight) return;

        if (hitLeft && hitRight)
        {
            if (distLeft <= distRight) hitRight = false;
            else                       hitLeft  = false;
        }

        Transform otherFoot = hitLeft ? Ctx.rightFootTransform : Ctx.leftFootTransform;
        if (!Ctx.IsLegGrounded(otherFoot)) return;

        _activeTarget     = hitLeft ? Ctx.ikTargetLeft      : Ctx.ikTargetRight;
        _anchorTarget     = hitLeft ? Ctx.ikTargetRight     : Ctx.ikTargetLeft;
        _activeFootBone   = hitLeft ? Ctx.leftFootTransform : Ctx.rightFootTransform;
        _desiredTargetPos = _activeTarget.position;
        _footGrabPos      = _activeTarget.position;
        _footVelocity     = Vector2.zero;
        _anchorWorldPos   = _anchorTarget.position;
        _maxBodyDist      = Vector2.Distance(Ctx.torsoRootRB.position, _anchorTarget.position);
    }

    private void UpdateDesiredPosition()
    {
        _desiredTargetPos = Ctx.GetMouseWorldPos();
    }

    private void PlantFoot()
    {
        if (_activeTarget != null)
        {
            // Land on the ground beneath the current IK position, not the raw mouse position
            Vector2 footPos = _activeTarget.position;
            RaycastHit2D hit = Physics2D.Raycast(footPos + Vector2.up * 0.5f, Vector2.down, 2f, Ctx.groundLayer);
            Vector2 plantPos = hit.collider != null ? hit.point : _desiredTargetPos;
            _activeTarget.position = plantPos;

            Vector2 step = new Vector2(plantPos.x - _footGrabPos.x, 0f);
            Ctx.torsoRootRB.AddForce(step * Ctx.plantImpulse, ForceMode2D.Impulse);
        }
        _activeTarget   = null;
        _anchorTarget   = null;
        _activeFootBone = null;
        _footVelocity   = Vector2.zero;
        _anchorWorldPos = Vector2.zero;
        _maxBodyDist    = 0f;
    }

    public override void ExitState()
    {
        _activeTarget   = null;
        _anchorTarget   = null;
        _activeFootBone = null;
        _footVelocity   = Vector2.zero;
        _anchorWorldPos = Vector2.zero;
        _maxBodyDist    = 0f;
    }

    public override void CheckSwitchStates()
    {
        if (Ctx.input.RagdollTriggered)
        {
            Ctx.SwitchState(Factory.Ragdoll());
            return;
        }

        if (!Ctx.input.IsClicking && _activeTarget == null)
            Ctx.SwitchState(Factory.Idle());
    }
}
