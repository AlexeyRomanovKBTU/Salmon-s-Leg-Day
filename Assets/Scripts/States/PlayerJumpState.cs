using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    public PlayerJumpState(PlayerController ctx, PlayerStateFactory factory) : base(ctx, factory) {}

    public override void EnterState()
    {
        Ctx.anim.Play("Sit");
        Ctx.currentJumpForce = Ctx.initialJumpForce;

        if (Ctx.jumpArrowPivot != null)
        {
            Ctx.jumpArrowPivot.SetActive(true);
            Ctx.jumpArrowPivot.transform.localScale = new Vector3(1f, 0.1f, 1f);
        }
    }

    public override void UpdateState()
    {
        if (Ctx.currentJumpForce < Ctx.maxJumpForce)
            Ctx.currentJumpForce += Ctx.jumpForceInc * Time.deltaTime;

        float moveX = Ctx.input.MoveInput.x;
        Ctx.balance.targetRotation = Mathf.Clamp(Ctx.balance.targetRotation + (-moveX * Time.deltaTime * 60f), -45f, 45f);

        UpdateArrowVisuals();
    }

    private void UpdateArrowVisuals()
    {
        if (Ctx.jumpArrowPivot == null) return;

        float visualAngle = Ctx.balance.targetRotation; 
        Ctx.jumpArrowPivot.transform.rotation = Quaternion.Euler(0, 0, visualAngle);

        float chargePercent = (Ctx.currentJumpForce - Ctx.initialJumpForce) / (Ctx.maxJumpForce - Ctx.initialJumpForce);
        chargePercent = Mathf.Clamp01(chargePercent);

        float visualY = Mathf.Lerp(0.1f, 1.0f, chargePercent);
        Ctx.jumpArrowPivot.transform.localScale = new Vector3(1f, visualY, 1f);
        
        if (Ctx.arrowSprite != null)
        {
            Ctx.arrowSprite.color = Color.Lerp(Color.white, Color.red, chargePercent);
        }
    }

    public override void ExitState()
    {
        float angle = (Ctx.balance.targetRotation + 90f) * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        Ctx.rb.AddForce(dir * Ctx.currentJumpForce);
        
        Ctx.balance.targetRotation = 0;

        if (Ctx.jumpArrowPivot != null)
            Ctx.jumpArrowPivot.SetActive(false);
    }

    public override void CheckSwitchStates()
    {
        if (!Ctx.input.IsHoldingJump)
        {
            Ctx.SwitchState(Factory.Idle());
        }
    }
}