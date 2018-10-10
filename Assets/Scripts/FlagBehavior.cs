using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using NetworkLobby;

/**
 * FLAG BEHAVIOR
 * Script attributed to Flags. They are the main objectives of the players.
 * To win, a flag must reach the other player's base while keeping their
 */
[RequireComponent(typeof(BoxCollider2D))]
public class FlagBehavior : NetworkBehaviour
{
    [Header("Network")]
    [SerializeField] LobbyPlayer.PlayerTeam team;
    [SerializeField] Transform teamBase;

    [Header("Physics")]
    [SerializeField] float gravityScaleFlag = 0.7f;
    [SerializeField] Vector2 offsetWhenTaken = new Vector2(0.0f, 0.8f);

    [Header("Network Update")]
    [SerializeField] float timeRefreshPosEvery = 0.4f;
    float timerRefreshNetwork;

    [SyncVar] bool isTaken;
    [SyncVar] bool isInBase;
    LocalPlayerController player;
    Rigidbody2D rigid;

    // Use this for initialization
    void Start () {
        SetupStart();
    }

    public void SetupStart()
    {
        isTaken = false;
        isInBase = true;
        player = null;
        rigid = GetComponent<Rigidbody2D>();
        rigid.bodyType = RigidbodyType2D.Static;

        if (!teamBase)
        {
            GameObject initBase = null;
            if (team == LobbyPlayer.PlayerTeam.RED)
                initBase = GameObject.FindGameObjectWithTag("RedBase");
            else
                initBase = GameObject.FindGameObjectWithTag("BlueBase");

            if (!initBase != null)
            {
                teamBase = initBase.transform.Find("FlagBase");
            }
        }

        if (teamBase)
        {
            transform.position = teamBase.position;
        }
        timerRefreshNetwork = Utility.StartTimer(timeRefreshPosEvery);
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
        if (!isTaken && !Mathf.Approximately(rigid.velocity.x, 0.0f) && !Mathf.Approximately(rigid.velocity.y, 0.0f))
        {
            if(Utility.IsOver(timerRefreshNetwork))
            {
                RpcUpdateNetwork(transform.position, rigid.velocity);
                timerRefreshNetwork = Utility.StartTimer(timeRefreshPosEvery);
            }
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
            LocalPlayerController pPlayer = collision.GetComponent<LocalPlayerController>();
            if (pPlayer.isDead)
                return;

            if (team != pPlayer.playerTeam)
            {
                if(pPlayer.flag == null)
                {
                    player = pPlayer;
                    RpcTakeFlag(pPlayer.playerId);
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
        else if (collision.tag == "RedBase" && team == LobbyPlayer.PlayerTeam.BLUE ||
            collision.tag == "BlueBase" && team == LobbyPlayer.PlayerTeam.RED)
        {
            ServerManagement._instance.RpcEndGame(team);
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
        // TODO : UPDATE LE FAIT DE POUVOIR CHOPPER LE PLAYER FACILEMENT
        LocalPlayerController[] listPlayers = GameObject.FindObjectsOfType<LocalPlayerController>();
        foreach (var pPlayer in listPlayers)
        {
            if(pPlayer.playerId == playerId)
            {
                player = pPlayer;
                break;
            }
        }

        if (!player)
            return;

        SoundManager._instance.PlaySound(SoundManager.SoundList.TAKE_FLAG);
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

        SoundManager._instance.PlaySound(SoundManager.SoundList.FLAG_BACK);
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
            rigid.bodyType = RigidbodyType2D.Kinematic; // To allow collision detection
            rigid.velocity = new Vector2();
            rigid.gravityScale = 0.0f;
        }

        if(isServer)
            RpcUpdateNetwork(transform.position, rigid.velocity);
    }
}
