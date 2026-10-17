using System;
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

        rb.freezeRotation = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector2 targetVelocity = transform.right* (input.x * maxSpeed); // restricts movement to left and right keys
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, acceleration * Time.deltaTime);

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // normal direction of surface
            Vector2 normal = collision.GetContact(0).normal; 

            // calculate rotation angle to align 'up' with wall normal
            float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg -90f;
            transform.rotation = Quaternion.Euler(0,0, angle); 

            rb.gravityScale =0;
            rb.AddForce(-normal * 9.81f, ForceMode2D.Force);
        }
    }
}
