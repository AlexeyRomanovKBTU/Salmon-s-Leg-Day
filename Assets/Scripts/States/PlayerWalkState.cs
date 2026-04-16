using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{
    private GameObject _activeLeg;
    private TargetJoint2D _activeJoint;
    private Rigidbody2D _activeRB;

    public PlayerWalkState(PlayerController ctx, PlayerStateFactory factory) : base(ctx, factory) {}

    public override void EnterState() 
    {
        Debug.Log("Entered Walk State");
    }

    public override void UpdateState() 
    {
        // 1. Check if we are trying to grab and why it might fail
        if (Ctx.input.IsClicking && _activeLeg == null) 
        {
            TryGrabLeg();
        }

        if (Ctx.input.IsClicking && _activeJoint != null)
        {
            _activeJoint.target = Ctx.GetMouseWorldPos();
        }

        // 2. Check the release logic
        if (!Ctx.input.IsClicking && _activeLeg != null)
        {
            Debug.Log($"Releasing leg: {_activeLeg.name}. Switching to Idle.");
            // Resetting local references before switching state is safer
            _activeLeg = null;
            _activeJoint = null;
            _activeRB = null;
            
            Ctx.SwitchState(Factory.Idle());
        }
    }

    private void TryGrabLeg()
    {
        Vector2 mousePos = Ctx.GetMouseWorldPos();
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            GameObject hitObj = hit.collider.gameObject;

            if (hitObj == Ctx.leftLegFeet || hitObj == Ctx.rightLegFeet)
            {
                GameObject otherLeg = (hitObj == Ctx.leftLegFeet) ? Ctx.rightLegFeet : Ctx.leftLegFeet;
                bool isOtherGrounded = Ctx.IsLegGrounded(otherLeg);

                Debug.Log($"Attempting to grab: {hitObj.name}. Other leg ({otherLeg.name}) Grounded: {isOtherGrounded}");

                if (isOtherGrounded)
                {
                    _activeLeg = hitObj;
                    _activeJoint = (hitObj == Ctx.leftLegFeet) ? Ctx.leftTargetJoint : Ctx.rightTargetJoint;
                    _activeRB = _activeLeg.GetComponent<Rigidbody2D>();

                    Debug.Log($"Grab Successful! Active Leg: {_activeLeg.name}");

                    // Anchor the pivot leg
                    Rigidbody2D otherRB = otherLeg.GetComponent<Rigidbody2D>();
                    otherRB.bodyType = RigidbodyType2D.Kinematic;
                    otherRB.linearVelocity = Vector2.zero;

                    // Enable dragging for active leg
                    _activeRB.bodyType = RigidbodyType2D.Dynamic;
                    _activeJoint.enabled = true;
                }
            }
        }
        else 
        {
             // Log this if you're clicking but nothing is happening
             Debug.Log("Raycast hit nothing at: " + mousePos);
        }
    }

    public override void ExitState() 
    {
        Debug.Log("Exiting Walk State");
        Ctx.SetAllLegsDynamic();
        // Crucial: Clear references when leaving the state
        _activeLeg = null;
        _activeJoint = null;
    }

    public override void CheckSwitchStates()
    {
        if (!Ctx.input.IsClicking && _activeLeg == null) Ctx.SwitchState(Factory.Idle());
    }
}