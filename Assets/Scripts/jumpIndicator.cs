using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
public class JumpIndicator : MonoBehaviour
{
    public InputAction lookAction;
    public InputAction jumpAction;
    [SerializeField] private Transform rotateAround;
    [SerializeField] private float targetAngle;
    private bool enable;
    public float TargetAngle => targetAngle;
    public float rotateSpeed = 50f;
    public float opacity = 0;
    public Color col;
    public bool jump = false;
    
    private Renderer rend;

    void Start()
    {
        lookAction = InputSystem.actions.FindAction("Point");
        jumpAction = InputSystem.actions.FindAction("Jump");
        enable = true;
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            col = rend.material.color;
            col.a = 0f;
            rend.material.color = col;
        }

    }

    void Update()
    {
        MakeVisible();
        if (!enable) return;
        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(lookInput);
        float currentArrowAngle = transform.eulerAngles.z;


        Vector2 currentCursorAngle = mouseWorldPos - (Vector2)rotateAround.position;

        // Target angle is the main one to worry about here, the rest are basically just there to ensure that the arrow moves the correct amount to point in the correct direction
        targetAngle = Mathf.Atan2(currentCursorAngle.y, currentCursorAngle.x) * Mathf.Rad2Deg;
        targetAngle -= 90f;
        float angleDelta = Mathf.DeltaAngle(currentArrowAngle, targetAngle);

        this.transform.RotateAround(rotateAround.position, Vector3.forward, angleDelta*rotateSpeed*Time.fixedDeltaTime);
    }

    public void MakeVisible()
    {
        if (jumpAction.WasPressedThisFrame())
        {
            jump = true; 
            opacity = 0f;
        }
        if (jump && jumpAction.IsPressed())
        {
            opacity += Time.deltaTime * 2f;
            opacity = Mathf.Clamp01(opacity);
            if (rend != null)
            {
                col.a = opacity;
                rend.material.color = col;
            }
        }
        if (jumpAction.WasReleasedThisFrame() && jump)
        {
            opacity = 0f;
            if (rend != null)
            {
                col.a = opacity;
                rend.material.color = col;
            }
            jump = false;
        }
    }
    public void FreezeIndicator()
    {
        enable = false;
    }

    public void UnfreezeIndicator()
    {
        enable = true;
    }
    public Vector2 LaunchDirection => (transform.up).normalized;
}
