using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerController ctx, PlayerStateFactory factory) : base(ctx, factory) {}

    public override void EnterState()
    {
        // IK stays enabled in idle — legs hold their last planted position
    }

    public override void UpdateState() { }

    public override void ExitState() { }

    public override void CheckSwitchStates()
    {
        if (Ctx.input.RagdollTriggered)
        {
            Ctx.SwitchState(Factory.Ragdoll());
            return;
        }

        if ((Ctx.input.IsHoldingJump || Ctx.input.JumpBuffered)
            && (Ctx.isGrounded || Ctx.coyoteCounter > 0))
        {
            Ctx.input.UseJumpBuffer();
            Ctx.SwitchState(Factory.Jump());
            return;
        }

        if (Ctx.input.IsClicking)
            Ctx.SwitchState(Factory.Walk());
    }
}