using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using NetworkLobby;

public class LocalPlayerController : NetworkBehaviour
{
    [Header("Player")]
    [SerializeField] [SyncVar] public short playerId;
    [SerializeField] float speed = 9.0f;
    [SerializeField] float speedAcc = 2.0f;
    [SerializeField] float jumpHeight = 5.0f;
    [SerializeField] Vector2 feetPosition = new Vector2(0.0f, -0.6f);
    [SerializeField] Vector2 groundSize = new Vector2(0.9f, 0.1f);
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float invincibilityTime = 0.1f;
    float timerInvincibility = 0.0f;

    [Header("Health")]
    [SerializeField] int maxHealth = 3;
    [SerializeField] int currentHealth;

    [Header("Flag")]
    [SerializeField] public FlagBehavior flag;

    [Header("Propellant")]
    [SerializeField] GameObject propellingObj;
    [SerializeField] float force = 9000.0f;
    [SerializeField] AnimationCurve hammerCurve;
    [SerializeField] Vector2 hammerSize = new Vector2(0.5f, 0.5f);
    [SerializeField] float timeBeforePropelling = 0.1f;
    float propelTimer = 0.0f;

    [Header("Network Infos")]
    [SyncVar(hook = "OnChangeName")] public string playerName = "...";
    [SyncVar(hook = "OnChangeTeam")] public LobbyPlayer.PlayerTeam team;
    [SyncVar(hook = "OnChangeSkin")] public SkinManager.SkinType skin;

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
    float horizontal, lastHoriDirection;
    bool jump, isHit, grounded;
    Color baseColor = Color.white;
    Vector2 slamDirection, oldSlamDirection, hammerDirection;
    Vector2 spawnPosition;

    // Use this for initialization
    void Start ()
    {
        lastHoriDirection = 1.0f;
        hammerState = HammerSteps.IDLE;
        jump = false;
        isHit = false;

        currentHealth = maxHealth;

        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        spawnPosition = GameObject.FindGameObjectWithTag("RedBase").transform.position;
        if (isLocalPlayer)
            Camera.main.GetComponent<CameraBehavior>().player = gameObject;
    }

    public void SetupBeginGame()
    {
        jump = false;
        isHit = false;
        lastHoriDirection = 1.0f;
        hammerState = HammerSteps.IDLE;

        currentHealth = maxHealth;

        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();

        if (isLocalPlayer)
            Camera.main.GetComponent<CameraBehavior>().player = gameObject;
        /*
        OnChangeName(name);
        OnChangeTeam(team);
        OnChangeSkin(skin);
        */
    }

    // Update is called once per frame // USED FOR INPUTS
    void Update () {
        if (!isLocalPlayer)
            return;

        //Acceleration
        horizontal = GameInput.GetAxisRaw(GameInput.AxisType.HORIZONTAL);

        if (!Mathf.Approximately(horizontal, 0.0f))
            lastHoriDirection = horizontal;

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
            else // Right/Left attack, depending on lastHorizontal
            {
                if (direction.x < 0.0f)
                    slamDirection = Vector2.right;
                else
                    slamDirection = Vector2.left;
            }

            if(hammerState == HammerSteps.IDLE)
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

        if (!isLocalPlayer)
            return;

        if (Mathf.Abs(horizontal) > 0.01)
        {
            Vector2 newSpeed = new Vector2(horizontal * speed * 20, rigid.velocity.y);
            ApplyForceAcc(rigid.velocity, newSpeed, speedAcc, true );
        }
        
        UpdatePlayerNetwork(NetworkUpdtMethod.SYNC_BOTH);

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
                    propellingObj.GetComponent<SpriteRenderer>().color = Color.blue;
                }
                else
                {
                    float dist = hammerCurve.Evaluate(1 - (Utility.GetTimerRemainingTime(propelTimer) / timeBeforePropelling));
                    propellingObj.transform.localPosition = hammerDirection * dist;
                }
                break;
            case HammerSteps.SLAMMING:
                hammerState = HammerSteps.RETURNING;
                propellingObj.GetComponent<SpriteRenderer>().color = Color.green;
                if (isServer)
                {
                    List<GameObject> players = propellingObj.GetComponent<PropellingBehavior>().GetTouchingPlayers();

                    Vector2 direction = slamDirection;

                    // Pour empecher le fait de slam dans la direction opposé une fois que le marteau arrive à sa destination
                    if (Mathf.Approximately(slamDirection.x - hammerDirection.x, 0.0f) || 
                        (Mathf.Approximately(direction.x, 0.0f) && Mathf.Approximately(direction.y, 0.0f))) 
                        direction = hammerDirection;

                    //Pour le propulser un poil en l'air
                    if (Mathf.Approximately(direction.y, 0.0f))
                        direction.y = 0.3f;

                    foreach (GameObject player in players)
                    {
                        player.GetComponent<LocalPlayerController>().CmdGetHit(direction * force);
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
        //UpdatePlayerNetwork(NetworkUpdtMethod.SYNC_BOTH);
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
        //UpdatePlayerNetwork(NetworkUpdtMethod.SYNC_BOTH);
        ApplyForce(direction * rigid.mass * 50 * jumpForce);
    }
    #endregion

    #region Network Movement
    [Command]
    public void CmdGetHit(Vector2 force)
    {
        RpcGetHit(force);
    }

    [ClientRpc]
    public void RpcGetHit(Vector2 force)
    {
        if(Utility.IsOver(timerInvincibility))
        {
            timerInvincibility = Utility.StartTimer(invincibilityTime);
            //Update pos + vitesse
            //UpdatePlayerNetwork(NetworkUpdtMethod.SYNC_BOTH);
            ApplyForce(force);
            if(isLocalPlayer)
            {
                currentHealth--;
                if(currentHealth <= 0)
                {
                    TriggerDeath();
                }
            }
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
        if(!isLocalPlayer)
            rigid.AddForce(force);
    }

    private void UpdatePlayerNetwork(NetworkUpdtMethod method)
    {
        if(method == NetworkUpdtMethod.SYNC_BOTH)
        {
            CmdUpdatePlayer(NetworkUpdtMethod.SYNC_POS, transform.position);
            CmdUpdatePlayer(NetworkUpdtMethod.SYNC_VEL, rigid.velocity);
        }
        else
        {
            if(method == NetworkUpdtMethod.SYNC_POS)
                CmdUpdatePlayer(method, transform.position);
            else
                CmdUpdatePlayer(method, rigid.velocity);
        }
    }

    [Command]
    private void CmdUpdatePlayer(NetworkUpdtMethod method, Vector2 value)
    {
        if (isServer)
            RpcUpdatePlayer(method, value);
    }

    [ClientRpc]
    private void RpcUpdatePlayer(NetworkUpdtMethod method, Vector2 value)
    {
        if (!isLocalPlayer)
        {
            if (!rigid)
                rigid = GetComponent<Rigidbody2D>();

            if (method == NetworkUpdtMethod.SYNC_POS)
                transform.position = value;
            else if(method == NetworkUpdtMethod.SYNC_VEL)
                rigid.velocity = value;
        }
    }
    #endregion

    private void InvicibilityAnimation()
    {
        if(!Utility.IsOver(timerInvincibility))
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
    
    private void TriggerDeath()
    {
        //Respawn();
        CmdDropFlag();
    }

    [Command]
    private void CmdDropFlag()
    {
        if(flag != null)
        {
            flag.RpcDropFlag();
            flag = null;
        }
    }

    #region SyncVarsTriggers
    private void OnChangeName(string newName)
    {
        name = newName;
        //Change le nom au dessus du joueur avec la nouvelle entrée
    }

    private void OnChangeTeam(LobbyPlayer.PlayerTeam newTeam)
    {
        team = newTeam;
        sprite.sprite = SkinManager._instance.GetSprite(skin, team);
        if(team == LobbyPlayer.PlayerTeam.BLUE)
            spawnPosition = GameObject.FindGameObjectWithTag("BlueBase").transform.Find("PlayerSpawn").position;
        else
            spawnPosition = GameObject.FindGameObjectWithTag("RedBase").transform.Find("PlayerSpawn").position;
        transform.position = spawnPosition;
    }

    private void OnChangeSkin(SkinManager.SkinType newType)
    {
        skin = newType;
        sprite.sprite = SkinManager._instance.GetSprite(skin, team);
    }
    #endregion
}
