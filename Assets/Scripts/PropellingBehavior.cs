using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PropellingBehavior : MonoBehaviour {

    private List<GameObject> touchingPlayers;

    private void Start()
    {
        touchingPlayers = new List<GameObject>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            if(!touchingPlayers.Contains(collision.gameObject))
                touchingPlayers.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (touchingPlayers.Contains(collision.gameObject))
            touchingPlayers.Remove(collision.gameObject);
    }

    public List<GameObject> GetTouchingPlayers()
    {
        return touchingPlayers;
    }
}
