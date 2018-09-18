using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerControllerNetwork : NetworkBehaviour
{

    [SerializeField] float speed = 5.0f;

    Rigidbody2D rigid;

	// Use this for initialization
	void Start () {
        rigid = GetComponent<Rigidbody2D>();
    }

    public override void OnStartLocalPlayer()
    {
        GetComponent<SpriteRenderer>().color = Color.blue;
    }

    // Update is called once per frame
    void Update () {
        if (!isLocalPlayer)
        {
            return;
        }

        float horizontal = Input.GetAxis("Horizontal");

        rigid.velocity = new Vector2(horizontal * speed, rigid.velocity.y);

        if(Input.GetButtonDown("Jump"))
        {
            rigid.AddForce(Vector2.up * 500);
        }
    }
}
