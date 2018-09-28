using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(BoxCollider2D))]
public class FlagBehavior : NetworkBehaviour
{
    [Header("Network")]
    [SerializeField] [Range(1, 30)] float syncPosRate = 5;
    float lastSyncTimer = 0.0f;
    [SerializeField] GameManager.PLAYER_TEAM team;
    [SerializeField] Transform teamBase;
    [SerializeField] BoxCollider2D groundCollider;

    [Header("Physics")]
    [SerializeField] float gravityScaleFlag = 0.7f;
    [SerializeField] Vector2 offsetWhenTaken = new Vector2(0.0f, 0.8f);

    [SyncVar] bool isTaken;
    [SyncVar] bool isInBase;
    LocalPlayerController player;
    Rigidbody2D rigid;

    // Use this for initialization
    void Start () {
        isTaken = false;
        isInBase = true;
        rigid = GetComponent<Rigidbody2D>();
        rigid.bodyType = RigidbodyType2D.Static;
        groundCollider.isTrigger = true;

        if (!teamBase)
        {
            GameObject initBase = null;
            if(team == GameManager.PLAYER_TEAM.RED)
                initBase = GameObject.FindGameObjectsWithTag("RedBase")[0];
            else if(team == GameManager.PLAYER_TEAM.BLUE)
                initBase = GameObject.FindGameObjectsWithTag("BlueBase")[0];

            if(!initBase != null)
            {
                teamBase = initBase.transform.Find("FlagBase");
            }
        }

        if (teamBase)
        {
            transform.position = teamBase.position;

        }
    }

    private void Update()
    {
        if (isTaken)
        {
            transform.position = (Vector2)player.transform.position + offsetWhenTaken;
        }
    }

    // Update is called once per frame
    void FixedUpdate () {
        UpdatePositionNetwork();
    }

    private void UpdatePositionNetwork()
    {
        if (!isTaken && rigid.velocity.x != 0.0f && rigid.velocity.y != 0.0f)
        {
            RpcUpdatePosNetwork(rigid.velocity);
            Debug.Log("coucouu");
        }
    }

    [ClientRpc]
    private void RpcUpdatePosNetwork(Vector2 speed)
    {
        if(!isServer)
            rigid.velocity = speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player" && !isTaken)
        {
            if(team != collision.GetComponent<LocalPlayerController>().team)
            {
                player = collision.GetComponent<LocalPlayerController>();
                if(player.flag == null)
                {
                    isTaken = true;
                    groundCollider.isTrigger = true;
                    rigid.gravityScale = 0.0f;
                    player.flag = this;                
                }
            }
            else
            {
                if(!isInBase)
                {
                    ReturnToBase();
                }
            }
        }
        else if (collision.tag == "RedBase" && team == GameManager.PLAYER_TEAM.BLUE ||
            collision.tag == "BlueBase" && team == GameManager.PLAYER_TEAM.RED)
        {
            GameManager._instance.TeamWin(team);
        }
    }

    public void DropFlag()
    {
        isTaken = false;
        groundCollider.isTrigger = false;
        rigid.bodyType = RigidbodyType2D.Dynamic;
        rigid.velocity = player.rigid.velocity;
        rigid.gravityScale = gravityScaleFlag;

        player = null;
    }

    private void ReturnToBase()
    {
        isInBase = true;
        transform.position = teamBase.position;
        groundCollider.isTrigger = true;
        rigid.gravityScale = 0.0f;
    }
}
