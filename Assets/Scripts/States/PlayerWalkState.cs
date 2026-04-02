using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{
    private float _stepTimer;
    private bool _firstStepDone;

    public PlayerWalkState(PlayerController ctx, PlayerStateFactory factory) : base(ctx, factory) {}
    
    public override void EnterState() 
    {
        _stepTimer = Ctx.stepWait;
        _firstStepDone = false;

        PerformStep(true);
    }

    public override void UpdateState() 
    {
        _stepTimer -= Time.deltaTime;

        if (_stepTimer <= 0 && !_firstStepDone)
        {
            PerformStep(false);
            _firstStepDone = true;
        }
    }

    public override void ExitState() {}

    private void PerformStep(bool isFirstStep)
    {
        float moveX = Ctx.input.MoveInput.x;
        
        Ctx.anim.Play(moveX > 0 ? "Walk_Right" : "Walk_Left");

        GameObject legObj = (moveX > 0) ? 
            (isFirstStep ? Ctx.middleLeftLeg : Ctx.middleRightLeg) : 
            (isFirstStep ? Ctx.middleRightLeg : Ctx.middleLeftLeg);

        if (legObj != null)
        {
            Rigidbody2D legRB = legObj.GetComponent<Rigidbody2D>();
            
            legRB.AddForce(new Vector2(moveX * Ctx.speedForce, 0));
            
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySlapsSFX(AudioManager.Instance.slaps);
        }
    }

    public override void CheckSwitchStates()
    {
        if (Ctx.input.MoveInput.x == 0 && _firstStepDone && _stepTimer <= -0.1f) 
        {
            Ctx.SwitchState(Factory.Idle());
        }
        
        if (Ctx.input.IsHoldingJump) 
        {
            Ctx.SwitchState(Factory.Jump());
        }

        if (_firstStepDone && _stepTimer <= -Ctx.stepWait)
        {
            EnterState();
        }
    }
}