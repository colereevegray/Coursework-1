using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
public class JumpIndicator : MonoBehaviour
{
    public InputAction lookAction;
    [SerializeField] private Transform rotateAround;
    [SerializeField] private float targetAngle;
    private bool enable;
    public float TargetAngle => targetAngle;
    public float rotateSpeed = 50f;

    void Start()
    {
        lookAction = InputSystem.actions.FindAction("Point");
        enable = true;
    }

    void Update()
    {
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
