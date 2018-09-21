using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class FlagBehavior : MonoBehaviour {

    Rigidbody2D rigid;

	// Use this for initialization
	void Start () {
        rigid = GetComponent<Rigidbody2D>();
        rigid.bodyType = RigidbodyType2D.Dynamic;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
