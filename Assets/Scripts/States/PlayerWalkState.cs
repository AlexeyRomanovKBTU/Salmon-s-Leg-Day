using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{
    private Transform _activeTarget;   // IK target being moved
    private Transform _anchorTarget;   // IK target staying still
    private Transform _activeFootBone; // actual foot bone for reach clamping
    private Vector2   _desiredTargetPos;

    public PlayerWalkState(PlayerController ctx, PlayerStateFactory factory) : base(ctx, factory) {}

    public override void EnterState()
    {
        Debug.Log("Entered Walk State");
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

        Vector2 currentPos = _activeTarget.position;
        Vector2 moveDir    = _desiredTargetPos - currentPos;
        float   moveDist   = moveDir.magnitude;

        if (moveDist < 0.001f) return;

        // CircleCast toward desired position — stops target at walls/ground
        // so the foot can never be dragged through solid objects
        RaycastHit2D hit = Physics2D.CircleCast(
            currentPos,
            Ctx.ikTargetRadius,
            moveDir.normalized,
            moveDist,
            Ctx.groundLayer);

        Vector2 safeTarget = hit.collider != null ? hit.centroid : _desiredTargetPos;

        _activeTarget.position = Vector2.MoveTowards(
            currentPos,
            safeTarget,
            Ctx.ikTargetSpeed * Time.fixedDeltaTime);
    }

    private void TryGrabFoot()
    {
        Vector2 mousePos = Ctx.GetMouseWorldPos();

        // Use OverlapPoint instead of Raycast — more reliable for clicking
        // on kinematic RB colliders that may not respond to raycasts
        Collider2D col = Physics2D.OverlapPoint(mousePos);

        if (col == null)
        {
            Debug.Log("No collider at mouse position.");
            return;
        }

        // The collider may be on the flat sprite OR the bone — check both.
        // Ctx.leftFootCollider / rightFootCollider are the actual collider
        // references set in PlayerController, so we compare against those.
        bool hitLeft  = col == Ctx.leftFootCollider;
        bool hitRight = col == Ctx.rightFootCollider;

        if (!hitLeft && !hitRight)
        {
            Debug.Log($"Clicked {col.gameObject.name} — not a foot.");
            return;
        }

        Transform otherFoot = hitLeft ? Ctx.rightFootTransform : Ctx.leftFootTransform;

        if (!Ctx.IsLegGrounded(otherFoot))
        {
            Debug.Log("Grab blocked: anchor foot not grounded.");
            return;
        }

        _activeTarget    = hitLeft ? Ctx.ikTargetLeft      : Ctx.ikTargetRight;
        _anchorTarget    = hitLeft ? Ctx.ikTargetRight     : Ctx.ikTargetLeft;
        _activeFootBone  = hitLeft ? Ctx.leftFootTransform : Ctx.rightFootTransform;
        _desiredTargetPos = _activeTarget.position;

        Debug.Log($"Grabbed {(hitLeft ? "left" : "right")} foot.");
    }

    private void UpdateDesiredPosition()
    {
        Vector2 mousePos = Ctx.GetMouseWorldPos();

        // Clamp reach from the actual foot bone position, not the IK target.
        // The IK target may lag behind the foot, so clamping from the target
        // position would give an incorrect reach radius.
        Vector2 footPos = _activeFootBone.position;
        Vector2 delta   = mousePos - footPos;

        if (delta.magnitude > Ctx.maxLegReach)
            mousePos = footPos + delta.normalized * Ctx.maxLegReach;

        _desiredTargetPos = mousePos;
    }

    private void PlantFoot()
    {
        // Freeze desired pos at current target position — foot stays planted
        _desiredTargetPos = _activeTarget.position;
        _activeTarget     = null;
        _anchorTarget     = null;
        _activeFootBone   = null;
        Debug.Log("Foot planted.");
    }

    public override void ExitState()
    {
        _activeTarget   = null;
        _anchorTarget   = null;
        _activeFootBone = null;
        Debug.Log("Exited Walk State");
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