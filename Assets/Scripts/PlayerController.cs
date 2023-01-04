using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;
    public float airSpeedMultiplier;
    private Vector3 moveDir;
    public Transform orientation;
    [Header("Player Stats")]
    public float health;
    public float maxHealth = 100f;
    public float recoveryRate;
    [Header("Ground Check")]
    private bool isGrounded;
    public LayerMask whatIsGround;
    public float playerHeight;
    [Header("Crouching")] 
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;
    public bool isCrouching;
    [Header("Slopes")] 
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool offSlope;
    [Header("Jump")]
    public float jumpHeight = 3f;
    private bool canJump = true;
    [Header("Animation")]
    private Animator animator;


    private FiniteStateMachine fsm;
    private Rigidbody rb;
    public float pickupRange = 5f;
    public bool isSprinting;

    
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        //controller = GetComponent<CharacterController>();
        health = maxHealth;
        recoveryRate = 0.2f;
        StartCoroutine(RestoreHealth());
        startYScale = transform.localScale.y;

        fsm = new FiniteStateMachine();
        var walking = fsm.CreateState("Walking");
        var sprinting = fsm.CreateState("Sprinting");
        var crouching = fsm.CreateState("Crouching");
        var air = fsm.CreateState("Air");

        walking.onEnter = delegate
        {
            speed = walkSpeed;
        };
        walking.onFrame = delegate
        {
            if (!isGrounded)
            {
                fsm.TransitionTo(air);
            }
            if (isSprinting)
            {
                
                fsm.TransitionTo(sprinting);
            } else if (isCrouching)
            {
                fsm.TransitionTo(crouching);
            }
        };
        walking.onExit = delegate
        {
        };

        sprinting.onEnter = delegate
        {
            speed = sprintSpeed;
        };
        sprinting.onFrame = delegate
        {
            if (isGrounded)
            {
                if (!isSprinting)
                {
                    if (isCrouching)
                    {
                        fsm.TransitionTo(crouching);
                    }
                    else
                    {
                        fsm.TransitionTo(walking);
                    }
                }
            }
            else
            {
                fsm.TransitionTo(air);
            }
        };
        sprinting.onExit = delegate
        {

        };

        crouching.onEnter = delegate
        {
            speed = crouchSpeed;
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        };
        crouching.onFrame = delegate
        {
            if (isGrounded)
            {
                if (!isCrouching)
                {
                    if (isSprinting)
                    {
                        fsm.TransitionTo(sprinting);
                    }
                    else
                    {
                        fsm.TransitionTo(walking);
                    }
                }
            }
            else
            {
                fsm.TransitionTo(air);
            }
        };
        crouching.onExit = delegate
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        };

        air.onEnter = delegate
        {

        };
        air.onFrame = delegate
        {
            if (isGrounded)
            {
                Vector3 jumpForce = new Vector3(0, -0.4f, 0);
                fsm.TransitionTo(walking);
            }
        };
        air.onExit = delegate
        {
        };
    }

    // Update is called once per frame
    void Update()
    {
        
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        if (isGrounded)
        {
            offSlope = false;
        }
        fsm.Update();
        SpeedControl();
        UpdateAnim();

        if (isGrounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    public void UpdateAnim()
    {
        if (rb.velocity.magnitude > 0.1f)
        {
            if (fsm.CurrentState.name.Equals("Walking"))
            {
                animator.SetBool("IsWalking", true);
            }
            else
            {
                animator.SetBool("IsWalking", false);
            }

            if (fsm.CurrentState.name.Equals("Sprinting"))
            {
                animator.SetBool("IsSprinting", true);
            }
            else
            {
                animator.SetBool("IsSprinting", false);
            }

            if (fsm.CurrentState.name.Equals("Air"))
            {
                animator.SetBool("IsSprinting", false);
                animator.SetBool("IsWalking", false);
            }
            
        }
        else if (rb.velocity.magnitude <= 0.1f)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsSprinting", false);
        }
        
        
    }

    public void Move(Vector2 input)
    {
        moveDir = Vector3.zero;
        moveDir = orientation.forward * input.y + orientation.right * input.x;
        if (OnSlope())
        {
            rb.AddForce(GetSlopeDirection() * CalculateSpeed() * 25f, ForceMode.Force);
            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
            
        else if(isGrounded)
            rb.AddForce(moveDir.normalized * CalculateSpeed() * 25f, ForceMode.Force);
        else if (!isGrounded)
            rb.AddForce(moveDir.normalized * CalculateSpeed() * 25f * airSpeedMultiplier, ForceMode.Force);

        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if (OnSlope() && !offSlope)
        {
            if (rb.velocity.magnitude > speed)
            {
                rb.velocity = rb.velocity.normalized * CalculateSpeed();
            }
        }
        Vector3 vel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (vel.magnitude > speed)
        {
            Vector3 controlVel = vel.normalized * CalculateSpeed();

            rb.velocity = new Vector3(controlVel.x, rb.velocity.y, controlVel.z);
        }
    }

    private float CalculateSpeed()
    {
        float calculatedSpeed = speed;

        return calculatedSpeed;
    }
    public void Jump()
    {
        if (isGrounded && canJump)
        {
            canJump = false;
            offSlope = true;
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
            Invoke(nameof(ResetJump), 0.1f);
        }
    }

    public void ResetJump()
    {
        canJump = true;
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeDirection()
    {
        return Vector3.ProjectOnPlane(moveDir, slopeHit.normal);
    }
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            PlayerDeath();
        }
    }

    private void PlayerDeath()
    {
        SceneManager.LoadScene("GameOver");
        
    }

    private IEnumerator RestoreHealth()
    {
        while (true)
        {
            if (health < maxHealth)
            {
                health += recoveryRate;

                if (health > maxHealth)
                {
                    health = maxHealth;
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void IncreaseMaxHealth(float amount)
    {
        maxHealth += amount;
    }
}
