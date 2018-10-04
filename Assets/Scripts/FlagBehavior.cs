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
    [SerializeField] ServerManagement.PLAYER_TEAM team;
    [SerializeField] Transform teamBase;

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

        if (!teamBase)
        {
            GameObject initBase = null;
            if(team == ServerManagement.PLAYER_TEAM.RED)
                initBase = GameObject.FindGameObjectsWithTag("RedBase")[0];
            else if(team == ServerManagement.PLAYER_TEAM.BLUE)
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
            if (!player && isServer)
                RpcDropFlag();
            else
                transform.position = (Vector2)player.transform.position + offsetWhenTaken;
        }
    }

    // Update is called once per frame
    void FixedUpdate () {
        if (!isServer)
            return;

        UpdatePositionNetwork();
    }

    private void UpdatePositionNetwork()
    {
        if (!isTaken && rigid.velocity.x != 0.0f && rigid.velocity.y != 0.0f)
        {
            RpcUpdateNetwork(transform.position, rigid.velocity);
        }
    }

    [ClientRpc]
    private void RpcUpdateNetwork(Vector2 position, Vector2 speed)
    {
        if(!isServer)
        {
            rigid.velocity = speed;
            transform.position = position;

        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isServer)
            return;

        if(collision.tag == "Player" && !isTaken)
        {
            if(team != collision.GetComponent<LocalPlayerController>().team)
            {
                player = collision.GetComponent<LocalPlayerController>();
                if(player.flag == null)
                {
                    RpcTakeFlag(player.playerId);
                }
            }
            else
            {
                if(!isInBase)
                {
                    RpcReturnToBase();
                }
            }
        }
        else if (collision.tag == "RedBase" && team == ServerManagement.PLAYER_TEAM.BLUE ||
            collision.tag == "BlueBase" && team == ServerManagement.PLAYER_TEAM.RED)
        {
            ServerManagement._instance.TeamWin(team);
        }
    }

    [ClientRpc]
    public void RpcDropFlag()
    {
        isTaken = false;
        ChangeBodyType(true);
        player = null;
    }

    [ClientRpc]
    public void RpcTakeFlag(short playerId)
    {
        ServerManagement._instance.UpdateCurrentPlayers();
        player = ServerManagement._instance.GetPlayerObjFromId(playerId);
        if (!player)
            return;

        player.flag = this;

        isTaken = true;
        isInBase = false;
        ChangeBodyType(false);
    }

    [ClientRpc]
    private void RpcReturnToBase()
    {
        player = null;

        isInBase = true;
        isTaken = false;

        transform.position = teamBase.position;
        ChangeBodyType(false);
    }

    private void ChangeBodyType(bool isDynamic)
    {
        if(isDynamic)
        {
            rigid.bodyType = RigidbodyType2D.Dynamic;
            if(player)
                rigid.velocity = player.rigid.velocity;
            rigid.gravityScale = gravityScaleFlag;
        }
        else
        {
            rigid.bodyType = RigidbodyType2D.Static;
            rigid.velocity = new Vector2();
            rigid.gravityScale = 0.0f;
        }

        if(isServer)
            RpcUpdateNetwork(transform.position, rigid.velocity);
    }
}
