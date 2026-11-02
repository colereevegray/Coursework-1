using UnityEngine;
using System.Collections.Generic;

public class SpiderLegController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Movement movementScript;
    [SerializeField] private CircleCollider2D[] spiderColliders;

    [System.Serializable]
    public class Leg
    {
        public string legName;
        public Transform ikTarget;
        public GameObject physicalLeg;
        public Vector2 localRestPosition;
        [HideInInspector] public Vector2 currentWorldPosition;
        [HideInInspector] public Vector2 targetWorldPosition;
        [HideInInspector] public bool isStepping = false;
        [HideInInspector] public float stepProgress = 1f;
        public int gaitGroup = 0; // 0 or 1, legs in different groups move at different times.
    }

    [Header("Legs Configuration")]
    [SerializeField] private List<Leg> legs = new List<Leg>();
    [SerializeField] private float rayLength = 2.5f;
    [SerializeField] private float stepThreshold = 0.4f;
    [SerializeField] private float stepSpeed = 16f;
    [SerializeField] private float stepHeight = 0.25f;

    private void Awake()
    {
        if (movementScript == null)
            movementScript = GetComponent<Movement>();
    }

    private void Start()
    {

        foreach (var leg in legs)
        {
            if (leg.ikTarget != null)
            {
                Vector2 initialWorldPos = transform.TransformPoint(leg.localRestPosition);
                leg.currentWorldPosition = initialWorldPos;
                leg.targetWorldPosition = initialWorldPos;
                leg.ikTarget.position = initialWorldPos;
            }
        }
    }

    private void Update()
    {
        if (movementScript == null) return;


        int activeLegCount = Mathf.Clamp((int)movementScript.jumpCount, 0, legs.Count);
        LayerMask combinedMask = movementScript.groundLayerMask | movementScript.glassLayerMask;

        bool colliderSelect = activeLegCount ==0;

        if (colliderSelect)
        {
            spiderColliders[0].enabled = false;
            spiderColliders[1].enabled = true;
        }
        else
        {
            spiderColliders[0].enabled = true;
            spiderColliders[1].enabled = false;
       }

        for (int i = 0; i < legs.Count; i++)
        {
            Leg leg = legs[i];
            if (leg.ikTarget == null) continue;

            bool shouldBeActive = i < activeLegCount;

            if (leg.ikTarget.gameObject.activeSelf != shouldBeActive)
            {
                leg.ikTarget.gameObject.SetActive(shouldBeActive);
            }
            if (leg.physicalLeg != null && leg.physicalLeg.activeSelf != shouldBeActive)
            {
                leg.physicalLeg.SetActive(shouldBeActive);
            }
            if (!shouldBeActive) continue;


            Vector2 idealWorldRest = transform.TransformPoint(leg.localRestPosition);

            Vector2 rayOrigin = idealWorldRest + ((Vector2)transform.up * 0.6f);
            Vector2 rayDirection = -transform.up;

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayLength + 0.6f, combinedMask);
            Debug.DrawRay(rayOrigin, rayDirection * (rayLength + 0.6f), hit ? Color.green : Color.red);

            if (hit)
            {
                Vector2 hitPoint = hit.point;
                float distanceToRest = Vector2.Distance(leg.currentWorldPosition, hitPoint);
                bool otherGroupIsStepping = false;
                foreach (var otherLeg in legs)
                {
                    if(otherLeg.gaitGroup != leg.gaitGroup && otherLeg.isStepping)
                    {
                        otherGroupIsStepping = true;
                        break;
                    }
                }

                if (!leg.isStepping && distanceToRest > stepThreshold && !otherGroupIsStepping)
                {
                    leg.isStepping = true;
                    leg.stepProgress = 0f;
                    leg.targetWorldPosition = hitPoint;
                }
            }

            if (leg.isStepping)
            {
                leg.stepProgress += Time.deltaTime * stepSpeed;
                Vector2 nextPos = Vector2.Lerp(leg.currentWorldPosition, leg.targetWorldPosition, leg.stepProgress);

                float arc = Mathf.Sin(leg.stepProgress * Mathf.PI) * stepHeight;
                nextPos += (Vector2)transform.up * arc;

                leg.currentWorldPosition = nextPos;

                if (leg.stepProgress >= 1f)
                {
                    leg.currentWorldPosition = leg.targetWorldPosition;
                    leg.isStepping = false;
                }
            }

            leg.ikTarget.position = leg.currentWorldPosition;
        }
    }
}