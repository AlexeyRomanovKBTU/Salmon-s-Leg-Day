using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public Rigidbody2D rb;

    public GameObject MiddleLeftLeg;
    public GameObject MiddleRightLeg;

    Rigidbody2D MiddleLeftLegRB;
    Rigidbody2D MiddleRightLegRB;

    public Animator anim;
    public AudioManager audioManager;

    [SerializeField] float speedForce = 500f;
    [SerializeField] float stepWait = .5f;
    [SerializeField] float initialJumpForce = 1000f;
    [SerializeField] float maxJumpForce = 3000f;
    [SerializeField] float jumpForceIncrement = 500f;
    [SerializeField] float ragdollWaitTime = 3f;

    public float positionRadius;
    public LayerMask ground;
    public Transform playerPos;

    private float currentJumpForce;
    private bool isOnGround;
    private bool ragdollInProgress = false;
    private bool isSitting = false;

    void Start()
    {
        MiddleLeftLegRB = MiddleLeftLeg.GetComponent<Rigidbody2D>();
        MiddleRightLegRB = MiddleRightLeg.GetComponent<Rigidbody2D>();
    }

    void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    void Update()
    {
//////////////////////////////////////////////////////////////////////////

    float horizontalInput = Input.GetAxisRaw("Horizontal");

// RAGDOLL
// =======================================================================
        if (Input.GetKeyDown(KeyCode.R) && !ragdollInProgress)
        {
            StartCoroutine(RagdollCoroutine());
        }

        else if (ragdollInProgress)
        {
            anim.Play("Ragdoll");
        }

// SIT
// =======================================================================
        else if (Input.GetKey(KeyCode.Space))
        {
            anim.Play("Sit");
            isSitting = true;
            if (horizontalInput != 0)
            {
                float change = Mathf.Sign(-horizontalInput) * Time.deltaTime * 30f;
                rb.GetComponent<Balance>().targetRotation = Mathf.Clamp(rb.GetComponent<Balance>().targetRotation + change, -30f, 30f);
            }
        }

// WALK
// =======================================================================
        else if (horizontalInput != 0 && !isSitting)
        {
            if (horizontalInput > 0)
            {
                anim.Play("Walk_Right");
                StartCoroutine(MoveRight(stepWait));
            }
            else
            {
                anim.Play("Walk_Left");
                StartCoroutine(MoveLeft(stepWait));
            }
        }
  
// IDLE
// =======================================================================
        else
        {
            anim.Play("Idle");
        }

// JUMP
// =======================================================================
        isOnGround = Physics2D.OverlapCircle(playerPos.position, positionRadius, ground);

        if (isOnGround && !ragdollInProgress)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                currentJumpForce = initialJumpForce;
                audioManager.PlaySlapsSFX(audioManager.slaps);
            }

            if (Input.GetKey(KeyCode.Space) && currentJumpForce < maxJumpForce)
            {
                currentJumpForce += jumpForceIncrement * Time.deltaTime;
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                float angleInRadians = (rb.GetComponent<Balance>().targetRotation + 90f) * Mathf.Deg2Rad;
                Vector2 jumpDirection = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
                rb.AddForce(jumpDirection  * currentJumpForce);
                audioManager.PlaySlapsSFX(audioManager.slaps);
                currentJumpForce = initialJumpForce;
                isSitting = false;
                rb.GetComponent<Balance>().targetRotation = 0f;
            }
        }

//////////////////////////////////////////////////////////////////////////
    }

    IEnumerator MoveRight(float seconds)
    {
        MiddleLeftLegRB.AddForce(Vector2.right  * speedForce * Time.deltaTime);
        audioManager.PlaySlapsSFX(audioManager.slaps);

        yield return new WaitForSeconds(seconds);

        MiddleRightLegRB.AddForce(Vector2.right * speedForce * Time.deltaTime);
        audioManager.PlaySlapsSFX(audioManager.slaps);
    }

    IEnumerator MoveLeft(float seconds)
    {
        MiddleRightLegRB.AddForce(Vector2.left * speedForce * Time.deltaTime);
        audioManager.PlaySlapsSFX(audioManager.slaps);
        
        yield return new WaitForSeconds(seconds);

        MiddleLeftLegRB.AddForce(Vector2.left * speedForce * Time.deltaTime);
        audioManager.PlaySlapsSFX(audioManager.slaps);
    }

    IEnumerator RagdollCoroutine()
    {
        audioManager.PlaySFX(audioManager.eugh);

        ragdollInProgress = true;

        while (!isOnGround)
        {
            yield return null;
        }

        yield return new WaitForSeconds(ragdollWaitTime);

        ragdollInProgress = false;
    }
}
