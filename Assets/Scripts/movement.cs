using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
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

    // Wall walking Variables
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] float stickyRadius = 1f;
    private float orientationAngle;

    [SerializeField] private float surfaceHangTime = 0.45f;
    private float surfaceLostTimer = 0f;
    private Vector2 lastValidNormal = Vector2.up;
    private float jumpLockoutTimer = 0f;

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
        orientationAngle = (transform.eulerAngles.z - 90f) * Mathf.Deg2Rad;
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector2 targetVelocity = transform.right*(input.x * maxSpeed); // restricts movement to left and right keys
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, acceleration * Time.deltaTime);

        if (jumpRequested)
        {
            PerformJump();
            jumpRequested = false;
        }

        if (jumpLockoutTimer > 0f)
        {
            jumpLockoutTimer -= Time.fixedDeltaTime;
        }

        Vector2 normal = DetectSurface(20, Mathf.PI, lastValidNormal);
        if (jumpLockoutTimer > 0f)
        {
            normal = Vector2.zero;
            surfaceLostTimer = 0f;
        }
        else if (normal != Vector2.zero)
        {
            lastValidNormal = normal;
            surfaceLostTimer = surfaceHangTime;
        }
        else if (surfaceLostTimer > 0f)
        {
            surfaceLostTimer -= Time.fixedDeltaTime;
            normal = lastValidNormal;
        }

        if (normal != Vector2.zero)
        {
            float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            rb.gravityScale = 0;
            rb.AddForce(-normal * 9.81f, ForceMode2D.Force);
        }
        else
        {
            rb.gravityScale=jumpGravity;
        }

    }

    void PerformJump()
    {
        Vector2 launchDir = jumpIndicator.LaunchDirection;
        Vector2 jumpVelocity =  launchDir * jumpPower;


        Vector2 currentVel = rb.linearVelocity;

        rb.linearVelocity = currentVel + jumpVelocity;
        surfaceLostTimer = 0f;
        jumpLockoutTimer = 0.2f;
    }

    RaycastHit2D[] getRays(int rayCount, float raySpread)
    {
        float spread = raySpread / (rayCount - 1);
        float halfSpread = raySpread / 2f;
        RaycastHit2D[] rayDirections = new RaycastHit2D[rayCount];

        for (int i = 0; i < rayCount; i++)
        {
            float currentRayAngle = (i * spread) - halfSpread;
            Vector2 rayDirection = new Vector2(
                Mathf.Cos(orientationAngle + currentRayAngle),
                Mathf.Sin(orientationAngle + currentRayAngle)
            );
            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, stickyRadius, groundLayerMask);
            if (hit)
            {
                rayDirections[i] = hit;
            }
        }
        return rayDirections;
    }

    Vector2 DetectSurface(int rayCount, float raySpread, Vector2 currentNormal)
    {
        RaycastHit2D[] rays = getRays(rayCount, raySpread);
        List<(Vector2 start, Vector2 end, Vector2 normal)> surfaces = new List<(Vector2, Vector2, Vector2)>();

        bool isTracking = false;
        Vector2 startPoint = default;
        Vector2 endPoint = default;
        Vector2 accumulatedNormal = Vector2.zero;
        int hitCount = 0;

        for (int i = 0; i < rays.Length; i++)
        {
            bool hasHit = rays[i].collider != null;

            if (hasHit)
            {
                if (!isTracking)
                {
                    isTracking = true;
                    startPoint = rays[i].point;
                    accumulatedNormal = Vector2.zero;
                    hitCount = 0;
                }
                endPoint = rays[i].point;
                accumulatedNormal += rays[i].normal;
                hitCount++;
            }

            if (isTracking && (!hasHit || i == rays.Length - 1))
            {
                Vector2 avgNormal = hitCount > 0 ? (accumulatedNormal / hitCount).normalized : Vector2.up;
                surfaces.Add((startPoint, endPoint, avgNormal));
                isTracking = false;
            }
        }

        float currentMinScore = Mathf.Infinity;
        Vector2 bestNormal = Vector2.zero;

        foreach (var surface in surfaces)
        {
            Vector2 midpoint = new Vector2(
                (surface.start.x + surface.end.x) / 2f,
                (surface.start.y + surface.end.y) / 2f
            );
            float currentDistance = Vector2.SqrMagnitude((Vector2)transform.position - midpoint);

            // when deciding what surface to stick to this favours the current surface
            float score = currentDistance - (Vector2.Dot(surface.normal, currentNormal) * 0.4f);

            if (score < currentMinScore)
            {
                currentMinScore = score;
                bestNormal = surface.normal;
            }
        }

        return bestNormal;
    }
}