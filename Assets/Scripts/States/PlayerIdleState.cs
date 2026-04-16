using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerController ctx, PlayerStateFactory factory) : base(ctx, factory) {}

    public override void EnterState() 
    {
        //Ctx.anim.Play("Idle");
        Ctx.SetAllLegsDynamic(); // Make sure we are floppy in idle
    }

    public override void UpdateState() { }
    public override void ExitState() { }

    public override void CheckSwitchStates()
    {
        if (Ctx.input.RagdollTriggered) Ctx.SwitchState(Factory.Ragdoll());
        else if (Ctx.input.IsHoldingJump && (Ctx.isGrounded || Ctx.coyoteCounter > 0)) Ctx.SwitchState(Factory.Jump());
        
        // Switch to walk if player clicks
        else if (Ctx.input.IsClicking) Ctx.SwitchState(Factory.Walk());
    }
}