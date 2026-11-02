using UnityEngine;
using UnityEngine.InputSystem;

public class plankbehavior : MonoBehaviour
{

    private Rigidbody2D rb;
    private InputAction jumpAction;
    private InputAction moveAction;
    private bool jump = false;
    private bool jumpFin = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        jumpAction = InputSystem.actions.FindAction("Jump");
        moveAction = InputSystem.actions.FindAction("Move");
    }

    void Update()
    {
        if (moveAction.WasPressedThisFrame() && !jumpAction.WasPressedThisFrame())
        {
            jumpFin = false;
            jump = false;
        }   
        if (jumpAction.WasPressedThisFrame())
        {
            jump = true;
            jumpFin = false;
        }
        if (jumpAction.WasReleasedThisFrame() && jump)
        {
            jumpFin = true;
            jump = false;
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") && jumpFin)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }
}
