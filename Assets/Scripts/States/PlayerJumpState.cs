using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    private bool _shouldJump;

    public PlayerJumpState(PlayerController ctx, PlayerStateFactory factory) : base(ctx, factory) {}

    public override void EnterState()
    {
        _shouldJump = false;
        Ctx.anim.Play("Sit");
        Ctx.currentJumpForce = Ctx.initialJumpForce;
        Ctx.jumpAimAngle = 0f;

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
        Ctx.jumpAimAngle = Mathf.Clamp(
            Ctx.jumpAimAngle + (-moveX * Time.deltaTime * 60f), -45f, 45f);

        UpdateArrowVisuals();
    }

    private void UpdateArrowVisuals()
    {
        if (Ctx.jumpArrowPivot == null) return;

        Ctx.jumpArrowPivot.transform.rotation =
            Quaternion.Euler(0, 0, Ctx.jumpAimAngle);

        float chargePercent = Mathf.InverseLerp(
            Ctx.initialJumpForce, Ctx.maxJumpForce, Ctx.currentJumpForce);

        Ctx.jumpArrowPivot.transform.localScale =
            new Vector3(1f, Mathf.Lerp(0.1f, 1f, chargePercent), 1f);

        if (Ctx.arrowSprite != null)
            Ctx.arrowSprite.color = Color.Lerp(Color.white, Color.red, chargePercent);
    }

    public override void ExitState()
    {
        if (_shouldJump)
        {
            float angle = (Ctx.jumpAimAngle + 90f) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Ctx.rb.AddForce(dir * Ctx.currentJumpForce);
        }

        Ctx.jumpAimAngle = 0f;

        if (Ctx.jumpArrowPivot != null)
            Ctx.jumpArrowPivot.SetActive(false);

        Ctx.anim.Play("Idle");
    }

    public override void CheckSwitchStates()
    {
        if (Ctx.input.RagdollTriggered)
        {
            Ctx.SwitchState(Factory.Ragdoll());
            return;
        }

        if (!Ctx.input.IsHoldingJump)
        {
            _shouldJump = true;
            Ctx.SwitchState(Factory.Idle());
        }
    }
}