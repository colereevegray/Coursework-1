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
    private float opacity = 0;
    private Color col;
    private bool jump = false;
    private float scale; 
    
    private Renderer rend;
    private Transform size;
    private Vector3 ogsize;

    void Start()
    {
        lookAction = InputSystem.actions.FindAction("Point");
        jumpAction = InputSystem.actions.FindAction("Jump");
        enable = true;
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            col = rend.material.color;
            col.a = 0.1f;
            rend.material.color = col;
        }
        size = GetComponent<Transform>();
        ogsize = size.localScale;
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
            scale = 0;
            jump = true; 
            opacity = 0.1f;
        }
        if (jump && jumpAction.IsPressed())
        {
            scale += Time.deltaTime * 0.1f;
            opacity += Time.deltaTime * 2f;
            opacity = Mathf.Clamp01(opacity);
            if (rend != null)
            {
                col.a = opacity;
                rend.material.color = col;
            }
            if (size.localScale.x < 5 && size.localScale.y < 5 && size.localScale.z < 5)
            {
                size.localScale += new Vector3(scale, scale, scale);
            }
        }
        if (jumpAction.WasReleasedThisFrame() && jump)
        {
            size.localScale = ogsize;
            opacity = 0.1f;
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
