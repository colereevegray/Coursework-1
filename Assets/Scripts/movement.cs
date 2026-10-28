using System;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;
public class Movement : MonoBehaviour
{
    Rigidbody2D rb;
    InputAction moveAction;
    InputAction jumpAction;
    Collider2D  col;
    // Left/Right Movement variables
    [SerializeField] public float acceleration = 25f ;
    [SerializeField] public float maxSpeed = 8f;

    // Jump variables
    [SerializeField] private JumpIndicator jumpIndicator;
    [SerializeField] private TextMeshProUGUI text_obj;
    private float jumpCount;
    [SerializeField] public float jumpPower;
    [SerializeField] public float jumpGravity;
    private bool jumpRequested = false;

    // Wall walking Variables
    [SerializeField] private int rayNumber;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private LayerMask glassLayerMask;
    [SerializeField] float stickyRadius = 1f;
    [SerializeField] private float maxGlassSlopeAngle;

    [SerializeField] private float surfaceHangTime = 0.45f;
    private float surfaceLostTimer = 0f;
    private Vector2 lastValidNormal = Vector2.up;
    private float jumpLockoutTimer = 0f;

    void Awake() // Apparently good practice to use for fetching other components part of the same game object, runs before Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        jumpIndicator = GetComponentInChildren<JumpIndicator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        rb.freezeRotation = true;
        jumpCount = 8;
    }

    void Update()
    {
        if (jumpAction.WasPressedThisFrame() && jumpCount > 0)
        {
            jumpRequested = true;
            jumpCount -= 1;
            text_obj.text = "Number of legs: " + jumpCount.ToString();
        }
    }
    void FixedUpdate()
    {
        // orientationAngle = (transform.eulerAngles.z - 90f) * Mathf.Deg2Rad;
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector2 targetVelocity = transform.right*(input.x * maxSpeed); // restricts movement to left and right keys
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, acceleration * Time.deltaTime);
        Vector2 normal = DetectSurface(rayNumber, 2.5f*Mathf.PI/3, lastValidNormal);

        if (jumpRequested)
        {
            PerformJump();
            jumpRequested = false;
            // orientationAngle = Mathf.Atan(rb.linearVelocity.y/rb.linearVelocity.x)*Mathf.Rad2Deg;
            // lastValidNormal = new Vector2(1,1).normalized;
            // normal = DetectSurface(rayNumber, Mathf.PI/3, lastValidNormal);
        }

        if (jumpLockoutTimer > 0f)
        {
            jumpLockoutTimer -= Time.fixedDeltaTime;
        }

        if (jumpLockoutTimer > 0f && normal != Vector2.zero)
        {
            float surfaceSimilarity = Vector2.Dot(normal, lastValidNormal);
            // check to see if a surface detected during jump is new, ignore it if nit
            if (surfaceSimilarity > 0.5f)
            {
                normal = Vector2.zero;
            }
            else
            {
                // It's a brand new surface, cancel lockout to stick instantly
                jumpLockoutTimer = 0f;
            }
        }

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
            // normal = DetectSurface(rayNumber, Mathf.PI, lastValidNormal);
        }

        if (normal != Vector2.zero)
        {
            float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

            Quaternion newRotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                600f * Time.fixedDeltaTime
            );

            Quaternion deltaRotation = newRotation * Quaternion.Inverse(transform.rotation);
            rb.linearVelocity = deltaRotation * rb.linearVelocity;

            transform.rotation = Quaternion.Euler(0, 0, angle);
            rb.gravityScale = 0;
            rb.AddForce(-normal * 10.81f, ForceMode2D.Force);
        }
        else
        {
            rb.gravityScale=jumpGravity;

            if (rb.linearVelocity.sqrMagnitude > 0.1f)
            {
                float airAngle = -Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg - 90f;
                Quaternion targetAirRotation = Quaternion.Euler(0, 0, airAngle);

                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetAirRotation,
                    // 720f * Time.fixedDeltaTime
                    15000f * Time.fixedDeltaTime
                );
            }
        }

    }

    void PerformJump()
    {
        Vector2 launchDir = jumpIndicator.LaunchDirection;
        Vector2 jumpVelocity =  launchDir * jumpPower;


        Vector2 currentVel = rb.linearVelocity;

        rb.linearVelocity = currentVel + jumpVelocity;
        surfaceLostTimer = 0f;
        jumpLockoutTimer = 0.4f;
    }

    RaycastHit2D[] getRays(int rayCount, float raySpread)
    {
        float activeRadius = stickyRadius;
        if (jumpLockoutTimer > 0.1f)
        {
            raySpread *= 0.5f;
            // activeRadius *= 2f;
        }

        float spread = raySpread / (rayCount - 1);
        float halfSpread = raySpread / 2f;
        RaycastHit2D[] rayDirections = new RaycastHit2D[rayCount];
        Vector2 rayOrigin = col != null ? (Vector2)col.bounds.center : (Vector2)transform.position;

        // float orientationAngle = (transform.eulerAngles.z - 90f) * Mathf.Deg2Rad;
        float orientationAngle;
        if (jumpLockoutTimer > 0f && rb.linearVelocity.sqrMagnitude > 0.5f)
        {
            orientationAngle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x);
        }
        else
        {
            orientationAngle = (transform.eulerAngles.z - 90f) * Mathf.Deg2Rad;
        }

        LayerMask combinedLayerMask = groundLayerMask | glassLayerMask;

        for (int i = 0; i < rayCount; i++)
        {
            float currentRayAngle = (i * spread) - halfSpread;
            Vector2 rayDirection = new Vector2(
                Mathf.Cos(orientationAngle + currentRayAngle),
                Mathf.Sin(orientationAngle + currentRayAngle)
            );

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, activeRadius, combinedLayerMask);
            if (hit && ((glassLayerMask & (1<<hit.collider.gameObject.layer)) !=0))
            {
                if(Vector2.Angle(hit.normal, Vector2.up) > maxGlassSlopeAngle)
                {
                    hit = default;
                }
            }
            Debug.DrawRay(rayOrigin, rayDirection * activeRadius, hit ? Color.green : Color.red);

            if (hit)
            {
                rayDirections[i] = hit;
            }
        }
        return rayDirections;
    }

    Vector2 DetectSurface(int rayCount, float raySpread, Vector2 currentNormal)
    {
        float rayCastDirection = Mathf.Atan2(currentNormal.y, currentNormal.x);
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
        Vector2 rayOrigin = col != null ? (Vector2)col.bounds.center : (Vector2)transform.position;
        foreach (var surface in surfaces)
        {
            Vector2 midpoint = new Vector2(
                (surface.start.x + surface.end.x) / 2f,
                (surface.start.y + surface.end.y) / 2f
            );
            float currentDistance = Vector2.SqrMagnitude(rayOrigin - midpoint);

            // when deciding what surface to stick to this favours the current surface
            float score = currentDistance - (Vector2.Dot(surface.normal, currentNormal) * 0.6f);

            if (score < currentMinScore)
            {
                currentMinScore = score;
                bestNormal = surface.normal;
            }
        }

        return bestNormal;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("EvilCenti"))
        {
            Vector2 knockBack = (transform.position - other.transform.position).normalized;
            float knockBackForce = 15f;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockBack * knockBackForce, ForceMode2D.Impulse);

            jumpCount -= 1;
            text_obj.text = "Number of legs: " + jumpCount.ToString();
        } 
    }
}