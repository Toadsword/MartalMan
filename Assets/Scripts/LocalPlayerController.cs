﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using NetworkLobby;

public class LocalPlayerController : NetworkBehaviour
{
    [Header("Player")]
    [SerializeField] float speed = 9.0f;
    [SerializeField] float speedAcc = 2.0f;
    [SerializeField] float jumpHeight = 5.0f;
    [SerializeField] Vector2 feetPosition = new Vector2(0.0f, -0.6f);
    [SerializeField] Vector2 groundSize = new Vector2(0.9f, 0.1f);
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float invincibilityTime = 0.1f;
    float timerInvincibility = 0.0f;
    [SerializeField] float invulnerabilityTimeRespawn = 1.0f;
    [SerializeField] float timeToRespawn = 5.0f;
    float timerRespawn = 0.0f;
    [SerializeField] float timeBetweenWalkSound = 0.3f;
    float timerWalk;

    [Header("UI")]
    [SerializeField] private Text nameText;

    [Header("Flag")]
    [SerializeField] public FlagBehavior flag;

    [Header("Propellant")]
    [SerializeField] GameObject propellingObj;
    [SerializeField] float force = 9000.0f;
    [SerializeField] AnimationCurve hammerCurve;
    [SerializeField] float timeBeforePropelling = 0.1f;
    float propelTimer = 0.0f;

    [Header("Network Infos")]
    [SyncVar] public short playerId;
    [SyncVar(hook = "OnChangeName")] public string playerName = "...";
    [SyncVar(hook = "OnChangeTeam")] public LobbyPlayer.PlayerTeam playerTeam;
    [SyncVar(hook = "OnChangeSkin")] public SkinManager.SkinType playerSkin;
    public NetworkConnection conn;
    [SerializeField] int UpdateEachXFrames = 7;
    int frameSkipped = 0;

    enum HammerSteps
    {
        GOING,
        SLAMMING,
        RETURNING,
        IDLE
    }
    HammerSteps hammerState;

    enum NetworkUpdtMethod
    {
        SYNC_POS,
        SYNC_VEL,
        SYNC_BOTH
    }

    [HideInInspector] public Rigidbody2D rigid;
    SpriteRenderer sprite;
    float horizontal;
    bool jump, grounded;
    public bool isDead;
    Color baseColor = Color.white;
    Vector2 slamDirection, oldSlamDirection, hammerDirection;
    Vector2 spawnPosition;

    // Use this for initialization
    void Start ()
    {
        SetupStart();
    }
    public void SetupStart()
    {
        hammerState = HammerSteps.IDLE;
        jump = false;

        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        UpdateSpawnPosition();

        if (isLocalPlayer)
        {
            Camera.main.GetComponent<CameraBehavior>().player = gameObject;
            GameManager._instance.localPlayer = this;
        }

        flag = null;

        OnChangeName(playerName);
        OnChangeTeam(playerTeam);
        OnChangeSkin(playerSkin);
    }

    // USED FOR INPUTS AND ANIMATION
    void Update () {
        if (!isLocalPlayer || isDead)
            return;

        //Acceleration
        horizontal = GameInput.GetAxisRaw(GameInput.AxisType.HORIZONTAL);

        //In air
        if (GameInput.GetInputDown(GameInput.InputType.JUMP) && grounded)
        {
            jump = true;
        }

        //Hammer
        if (GameInput.GetInputDown(GameInput.InputType.ATTACK) || hammerState != HammerSteps.IDLE)
        {
            Vector2 direction = GameInput.GetDirection(GameInput.DirectionType.MOUSE, transform.position);
            if (direction.y > 0.0f && Mathf.Abs(direction.y) > Mathf.Abs(direction.x)) // Down attack
            {
                slamDirection = Vector2.down;
            }
            else if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))// Up attack
            {
                slamDirection = Vector2.up;
            }
            else // Right/Left attack
            {
                if (direction.x < 0.0f)
                    slamDirection = Vector2.right;
                else
                    slamDirection = Vector2.left;
            }

            if (hammerState == HammerSteps.IDLE)
            {
                oldSlamDirection = slamDirection;
                StartHammering();
            }
        }

        if(GameInput.GetInputDown(GameInput.InputType.DROP_FLAG))
        {
            CmdDropFlag();
        }
    }

    private void FixedUpdate()
    {
        if(!rigid)
            rigid = GetComponent<Rigidbody2D>();

        grounded = Physics2D.OverlapBox(rigid.position + feetPosition, groundSize, 0, groundLayer);

        HammerSlam();
        InvicibilityAnimation();

        // Cap speed
        if (rigid.velocity.x > speed || rigid.velocity.x < -speed)
        {
            ApplyForceAcc(rigid.velocity, new Vector2(0.0f, rigid.velocity.y), 0.2f, false);
        }

        if (Mathf.Abs(rigid.velocity.x) > 0.1f)
        {
            sprite.flipX = rigid.velocity.x < 0.0f;
            propellingObj.GetComponent<SpriteRenderer>().flipX = rigid.velocity.x < 0.0f;
        }

        if (isDead)
        {
            if (Utility.IsOver(timerRespawn))
            {
                if (isServer)
                    RpcRespawn();
            }
            else
            {
                if(isLocalPlayer) GameManager._instance.UpdateRespawnText("Respawning in " + Mathf.Round(Utility.GetTimerRemainingTime(timerRespawn)) + " seconds.");
            }
            return;
        }

        if (!isLocalPlayer)
            return;

        if(UpdateEachXFrames >= frameSkipped)
        {
            UpdatePlayerNetwork(NetworkUpdtMethod.SYNC_BOTH);
            frameSkipped = 0;
        }
        else
        {
            frameSkipped++;
        }

        if (Mathf.Abs(horizontal) > 0.01)
        {
            Vector2 newSpeed = new Vector2(horizontal * speed * 20, rigid.velocity.y);
            ApplyForceAcc(rigid.velocity, newSpeed, speedAcc, true);

            if (Utility.IsOver(timerWalk))
            {
                timerWalk = Utility.StartTimer(timeBetweenWalkSound);
                SoundManager._instance.PlaySound(SoundManager.SoundList.WALK);
            }
        }

        if (jump)
        {
            ApplyJumpForce(jumpHeight);
            jump = false;
        }
    }

    #region Hammer
    private void StartHammering()
    {
        hammerDirection = slamDirection;
        propelTimer = Utility.StartTimer(timeBeforePropelling);
        hammerState = HammerSteps.GOING;
        SoundManager._instance.PlaySound(SoundManager.SoundList.HAND_MOVEMENT);
        CmdStartHammering(slamDirection);
    }

    [Command]
    private void CmdStartHammering(Vector2 slamDirection)
    {
        RpcStartHammering(slamDirection);
    }

    [ClientRpc]
    private void RpcStartHammering(Vector2 argSlamDirection)
    {
        if (!isLocalPlayer)
        {
            hammerDirection = argSlamDirection;
            oldSlamDirection = argSlamDirection;
            propelTimer = Utility.StartTimer(timeBeforePropelling);
            hammerState = HammerSteps.GOING;
        }
    }
    
    private void HammerSlam()
    {
        switch (hammerState)
        {
            case HammerSteps.GOING:
                if (Utility.IsOver(propelTimer))
                {
                    hammerState = HammerSteps.SLAMMING;
                }
                else
                {
                    float dist = hammerCurve.Evaluate(1 - (Utility.GetTimerRemainingTime(propelTimer) / timeBeforePropelling));
                    propellingObj.transform.localPosition = hammerDirection * dist;
                }
                break;
            case HammerSteps.SLAMMING:
                hammerState = HammerSteps.RETURNING;
                if (isServer)
                {
                    List<GameObject> players = propellingObj.GetComponent<PropellingBehavior>().GetTouchingPlayers();

                    // To propell the player a bit in height.
                    if (Mathf.Approximately(hammerDirection.y, 0.0f))
                        hammerDirection.y = 0.3f;

                    // Propell all the players touched
                    foreach (GameObject player in players)
                    {
                        player.GetComponent<LocalPlayerController>().CmdGetHit(hammerDirection * force);
                    }
                }
                propelTimer = Utility.StartTimer(timeBeforePropelling);
                break;
            case HammerSteps.RETURNING:
                if (Utility.IsOver(propelTimer))
                {
                    hammerState = HammerSteps.IDLE;
                }
                else
                {
                    float dist = hammerCurve.Evaluate(Utility.GetTimerRemainingTime(propelTimer) / timeBeforePropelling);
                    propellingObj.transform.localPosition = hammerDirection * dist;
                }
                break;
            case HammerSteps.IDLE:
                propellingObj.transform.localPosition = new Vector2(0.0f, 0.0f);
                break;
        }

        if(isLocalPlayer && hammerState != HammerSteps.IDLE)
        {
            if(oldSlamDirection != slamDirection)
            {
                CmdUpdateHammerSlam(slamDirection);
                oldSlamDirection = slamDirection;
            }
        }
    }

    [Command]
    private void CmdUpdateHammerSlam(Vector2 slamDirection)
    {
        RpcUpdateHammerSlam(slamDirection);
    }

    [ClientRpc]
    private void RpcUpdateHammerSlam(Vector2 argSlamDirection)
    {
        if (!isLocalPlayer)
            slamDirection = argSlamDirection;
    }
    #endregion

    #region Movements
    private void ApplyForceAcc(Vector2 actualValue, Vector2 desiredValue, float divider, bool doNetwork)
    {
        Vector2 force = desiredValue - actualValue;
        UpdatePlayerNetwork(NetworkUpdtMethod.SYNC_BOTH);
        ApplyForce(force / divider, doNetwork);
    }

    private void ApplyForce(Vector2 force, bool doNetwork = true)
    {
        rigid.AddForce(force);
        if (doNetwork)
            CmdApplyForce(force);
    }

    private void ApplyJumpForce(float height)
    {
        float jumpForce = Mathf.Sqrt(Mathf.Abs(2.0f * Physics2D.gravity.y * height));

        Vector2 direction = Vector2.up;
        if (height < 0.0f)
            direction = Vector2.down;

        rigid.velocity = new Vector2(rigid.velocity.x, 0.0f);

        //Update pos + vitesse
        UpdatePlayerNetwork(NetworkUpdtMethod.SYNC_BOTH);
        ApplyForce(direction * rigid.mass * 50 * jumpForce);
    }
    #endregion

    #region Network Movement
    [Command]
    public void CmdGetHit(Vector2 force)
    {
        if(!isDead)
            RpcGetHit(force);
    }

    [ClientRpc]
    public void RpcGetHit(Vector2 force)
    {
        // Can't get hit while invincible
        if(Utility.IsOver(timerInvincibility))
        {
            timerInvincibility = Utility.StartTimer(invincibilityTime);
            SoundManager._instance.PlaySound(SoundManager.SoundList.IS_HIT);
            SoundManager._instance.PlaySound(SoundManager.SoundList.PUNCH);
            ApplyForce(force);
        }
    }
    
    [Command]
    private void CmdApplyForce(Vector2 force)
    {
        RpcApplyForce(force);
    }

    [ClientRpc]
    private void RpcApplyForce(Vector2 force)
    {
        //Already applied in ApplyForce(), in local
        if(!isLocalPlayer)
            rigid.AddForce(force);
    }

    private void UpdatePlayerNetwork(NetworkUpdtMethod method)
    {
        //int time = NetworkTransport.GetNetworkTimestamp();
        if (method == NetworkUpdtMethod.SYNC_BOTH)
        {
            CmdUpdatePlayer(NetworkUpdtMethod.SYNC_VEL, rigid.velocity/*, time*/);
            CmdUpdatePlayer(NetworkUpdtMethod.SYNC_POS, transform.position/*, time*/);
        }
        else
        {
            if(method == NetworkUpdtMethod.SYNC_POS)
                CmdUpdatePlayer(method, transform.position/*, time*/);
            else
                CmdUpdatePlayer(method, rigid.velocity/*, time*/);
        }
    }

    [Command]
    private void CmdUpdatePlayer(NetworkUpdtMethod method, Vector2 value/*, int time*/)
    {
        if (isServer)
        {
            RpcUpdatePlayer(method, value/*, time*/);
        }
    }

    [ClientRpc]
    private void RpcUpdatePlayer(NetworkUpdtMethod method, Vector2 value/*, int time*/)
    {
        //Add interpolation of players and speed considering the time it took for the network to arrive here
        //byte e;
        //int delay = NetworkTransport.GetRemoteDelayTimeMS(conn.hostId, conn.connectionId, time, out e);

        if (!isLocalPlayer)
        {
            if (!rigid)
                rigid = GetComponent<Rigidbody2D>();

            if (method == NetworkUpdtMethod.SYNC_POS)
                transform.position = /*rigid.velocity * (float)delay +*/ value;
            else if(method == NetworkUpdtMethod.SYNC_VEL)
                rigid.velocity = /*Physics2D.gravity * (float)delay +*/ value;
        }
    }
    #endregion

    [ClientRpc]
    private void RpcTriggerDeath()
    {
        gameObject.layer = LayerMask.NameToLayer("DeadPlayer");
        isDead = true;
        timerRespawn = Utility.StartTimer(timeToRespawn);
        UpdateSkin();
        if(flag)
            CmdDropFlag();
    }

    [ClientRpc]
    private void RpcRespawn()
    {
        if (isLocalPlayer)
            GameManager._instance.UpdateRespawnText("");

        transform.position = spawnPosition;
        isDead = false;
        UpdateSkin();
        gameObject.layer = LayerMask.NameToLayer("Player");
        timerInvincibility = Utility.StartTimer(invulnerabilityTimeRespawn);
    }

    [Command]
    private void CmdDropFlag()
    {
        if (flag != null)
        {
            flag.RpcDropFlag();
            flag = null;
        }
    }

    #region SyncVarsTriggers
    private void OnChangeName(string newName)
    {
        //Change le nom au dessus du joueur avec la nouvelle entrée
        nameText.text = "<b>" + newName + "</b>";
    }

    private void OnChangeTeam(LobbyPlayer.PlayerTeam newTeam)
    {
        UpdateSkin();
        UpdateSpawnPosition();
    }

    private void OnChangeSkin(SkinManager.SkinType newType)
    {
        UpdateSkin();
    }
    #endregion

    private void UpdateSpawnPosition()
    {
        if (playerTeam == LobbyPlayer.PlayerTeam.BLUE)
            spawnPosition = GameObject.FindGameObjectWithTag("BlueBase").transform.Find("PlayerSpawn").position;
        else
            spawnPosition = GameObject.FindGameObjectWithTag("RedBase").transform.Find("PlayerSpawn").position;
        transform.position = spawnPosition;
    }

    private void UpdateSkin()
    {
        if (!sprite)
            sprite = GetComponent<SpriteRenderer>();
        sprite.sprite = SkinManager._instance.GetSprite(playerSkin, playerTeam, isDead);
    }

    private void InvicibilityAnimation()
    {
        if (!Utility.IsOver(timerInvincibility))
        {
            if (sprite.color == baseColor)
                sprite.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            else
                sprite.color = baseColor;
        }
        else
        {
            sprite.color = baseColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Spike" && isServer && !isDead && Utility.IsOver(timerInvincibility))
        {
            RpcTriggerDeath();
        }
    }
}
