using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    [SerializeField] float speed = 5.0f;
    [SerializeField] float speedAcc = 5.0f;

    [SerializeField] float jumpHeight = 5.0f;

    [SerializeField] Transform feet;
    [SerializeField] Vector2 groundSize = new Vector2(1.0f, 0.2f);
    [SerializeField] LayerMask groundLayer;

    Rigidbody2D rigid;

    // Use this for initialization
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        StartCoroutine("ApplyRandomForce");
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        Vector2 newSpeed = new Vector2(horizontal * speed, rigid.velocity.y);

        ApplyForceAcc(rigid.velocity, newSpeed, speedAcc);
        
        bool grounded = Physics2D.OverlapBox(feet.position, groundSize, 0, groundLayer);

        if (Input.GetButtonDown("Jump") && grounded)
        {
            grounded = false;
            ApplyJumpForce(jumpHeight);
        }

        Debug.Log(rigid.velocity);  

        if(rigid.velocity.x > speed)
        {
            ApplyForceAcc(rigid.velocity, new Vector2(0.0f, rigid.velocity.y), 0.2f);
        }
    }

    IEnumerator ApplyRandomForce()
    {
        while(true)
        {
            rigid.AddForce(new Vector2(3000.0f, 300.0f));
            yield return new WaitForSeconds(2.0f);
        }
    }

    private void ApplyForceAcc(Vector2 actualValue, Vector2 desiredValue, float divider)
    {
        Vector2 force = desiredValue - actualValue;
        rigid.AddForce(force / divider, ForceMode2D.Impulse);
    }

    private void ApplyJumpForce(float height)
    {
        float jumpForce = Mathf.Sqrt(Mathf.Abs(2.0f * Physics2D.gravity.y * height));

        Vector2 direction = Vector2.up;
        if (height < 0.0f)
            direction = Vector2.down;

        rigid.AddForce(direction * rigid.mass * jumpForce, ForceMode2D.Impulse);
    }
}
