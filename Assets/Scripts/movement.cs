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
    private float orientationAngle = (3*Mathf.PI)/2;
    private float stickyRadius = 1.2;

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

    RayCastHit2D[] getRays(int rayCount, float raySpread)
    {
        float spread = raySpread / (rayCount - 1);
        RayCastHit2D[] rayDirections = new RayCastHit2D[rayCount];
        for (int i=0; i<rayCount; i++)
        {
            float currentRayAngle = i*spread;
            Vector2 rayDirection = new Vector2(
                Mathf.Cos(orientationAngle+currentRayAngle),
                Mathf.Sin(orientationAngle+currentRayAngle)
            );
            RayCast2D hit = Physics2D.Raycast(transform.position, rayDirection, stickyRadius, groundLayerMask);
            if (hit){
                rayDirections[i] = hit;
            }
        }
        return rayDirections;
    }

    void DetectSurface(int rayCount, float raySpread)
    {
        RaycastHit2D[] rays = getRays(rayCount, raySpread);
        List<(Vector2, Vector2)> surfaces = new List<(Vector2, Vector2)>();

        bool isTracking = false;
        Vector2 startPoint = default;
        Vector2 endPoint = default;

        for (int i = 0; i < rays.Length; i++)
        {
            bool hasHit = rays[i].collider != null;

            if (hasHit)
            {
                if (!isTracking)
                {
                    isTracking = true;
                    startPoint = rays[i].point;
                }
                endPoint = rays[i].point;
            }

            if (isTracking && (!hasHit || i == rays.Length - 1))
            {
                surfaces.Add((startPoint, endPoint));
                isTracking = false;
            }
        }

        float currentMinDistance = Mathf.Infinity;
        float currentDistance;
        (Vector2, Vector2) closestSurface = default;
        foreach((Vector2, Vector2) surface in surfaces)
        {

            Vector2 midpoint = new Vector2(
                (surface.Item1.x + surface.Item2.x) / 2f,
                (surface.Item1.y + surface.Item2.y) / 2f
            );
            currentDistance = Vector2.SqrMagnitude(transform.position - midpoint);
            if (currentDistance < currentMinDistance)
            {
                currentMinDistance = currentDistance;
                closestSurface = surface;
            }
        }

        Vector2 normal = new Vector2(-(closestSurface.Item1.y - closestSurface.Item2.y), (closestSurface.Item1.x - closestSurface.Item2.x));
        normal = normal.normalized;
    }
}
