using UnityEngine;
using UnityEngine.InputSystem;
public class jumpIndicator : MonoBehaviour
{
    public InputAction lookAction;
    [SerializeField] private Transform rotateAround;
    public float rotateSpeed = 50f;

    void Start()
    {
        lookAction = InputSystem.actions.FindAction("Point");
    }

    void FixedUpdate()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(lookInput);
        float currentArrowAngle = transform.eulerAngles.z;


        Vector2 currentCursorAngle = mouseWorldPos - (Vector2)rotateAround.position;

        float targetAngle = Mathf.Atan2(currentCursorAngle.y, currentCursorAngle.x) * Mathf.Rad2Deg;
        targetAngle -= 90f;
        float angleDelta = Mathf.DeltaAngle(currentArrowAngle, targetAngle);

        this.transform.RotateAround(rotateAround.position, Vector3.forward, angleDelta*rotateSpeed*Time.deltaTime);




    }
}
