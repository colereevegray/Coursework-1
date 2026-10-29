using UnityEngine;

public class WaspMovement : MonoBehaviour
{

    private float direction = 1f;
    private Rigidbody2D rb;
    private float maxSpeed = 5f;
    private float acceleration = 12f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //rb.bodyType=RigidbodyType2D.Kinematic;
        rb.freezeRotation = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 targetVelocity = transform.up*( maxSpeed * direction); 
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, acceleration * Time.deltaTime);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player"))
        {
            direction = direction * -1;
        }
    }
    
}