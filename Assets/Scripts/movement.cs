using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class Movement : MonoBehaviour
{
    Rigidbody2D rb;
    InputAction moveAction;
    InputAction jumpAction;
    // Left/Right Movement variables
    [SerializeField] public float acceleration = 25f ;
    [SerializeField] public float maxSpeed = 8f;

    // Jump variables
    [SerializeField] private JumpIndicator jumpIndicator;
    [SerializeField] public float jumpPower;
    [SerializeField] public float jumpGravity;
    private bool jumpRequested = false;


    void Awake() // Apparently good practice to use for fetching other components part of the same game object, runs before Start()
    {
        rb = GetComponent<Rigidbody2D>();
        jumpIndicator = GetComponentInChildren<JumpIndicator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (jumpAction.WasPressedThisFrame())
        {
            jumpRequested = true;
        }
    }
    void FixedUpdate()
    {

        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector2 targetVelocity = transform.right*(input.x * maxSpeed); // restricts movement to left and right keys
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, acceleration * Time.deltaTime);

        if (jumpRequested)
        {
            PerformJump();
            jumpRequested = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Wall"))
        {
            jumpIndicator.FreezeIndicator();
            // normal direction of surface
            Vector2 normal = collision.GetContact(0).normal;

            // calculate rotation angle to align 'up' with wall normal
            float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg -90f;
            transform.rotation = Quaternion.Euler(0,0, angle);

            rb.gravityScale =0;
            rb.AddForce(-normal * 9.81f, ForceMode2D.Force);
            jumpIndicator.UnfreezeIndicator();
        }
    }
    void OnCollisionExit2D(Collision2D collision)
    {
        rb.gravityScale = jumpGravity;
    }

    void PerformJump()
    {
        Vector2 launchDir = jumpIndicator.LaunchDirection;
        Vector2 jumpVelocity =  launchDir * jumpPower;


        Vector2 currentVel = rb.linearVelocity;

        rb.linearVelocity = currentVel + jumpVelocity;
    }

    void DetectSurface(int rayCount)
    {
        float raySpread = Mathf.PI / rayCount - 1;

    }
}
