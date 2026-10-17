using UnityEngine;
using UnityEngine.InputSystem;
public class movement : MonoBehaviour
{
    Rigidbody2D rb;
    InputAction moveAction;
    public float acceleration = 25f ;
    public float maxSpeed = 8f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        moveAction = InputSystem.actions.FindAction("Move");
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector2 targetVelocity = new Vector2(input.x*maxSpeed, 0);
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, acceleration * Time.deltaTime);

    }
}
