﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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
    float timerInvincibility;

    [Header("Propellant")]
    [SerializeField] GameObject propellingObj;
    [SerializeField] float force = 9000.0f;
    [SerializeField] AnimationCurve hammerCurve;
    [SerializeField] Vector2 hammerSize = new Vector2(0.5f, 0.5f);
    [SerializeField] float timeBeforePropelling = 0.1f;
    float propelTimer;

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
    bool jump, isHit;
    bool grounded;
    Color baseColor = Color.white;
    Vector2 slamDirection;
    [SyncVar] Vector2 hammerDirection;

    // Use this for initialization
    void Start ()
    {
        propelTimer = 0.0f;
        timerInvincibility = 0.0f;

        lastHoriDirection = 1.0f;
        hammerState = HammerSteps.IDLE;
        jump = false;
        isHit = false;
        
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    public override void OnStartLocalPlayer()
    {
        baseColor = Color.blue;
        GetComponent<SpriteRenderer>().color = baseColor;
        Camera.main.GetComponent<CameraBehavior>().player = gameObject;
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
            if (GameInput.GetInput(GameInput.InputType.DOWN)) // Down attack
            {
                slamDirection = Vector2.down;
            }
            else if (GameInput.GetInput(GameInput.InputType.UP))// Up attack
            {
                slamDirection = Vector2.up;
            }
            else // Right/Left attack, depending on lastHorizontal
            {
                if (lastHoriDirection > 0.0f)
                    slamDirection = Vector2.right;
                else
                    slamDirection = Vector2.left;
            }

            if(hammerState == HammerSteps.IDLE)
            {
                CmdStartHammering();
            }
        }
        //Tests
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (Mathf.Approximately(rigid.velocity.x, 0.0f) &&
                Mathf.Approximately(rigid.velocity.y, 0.0f) &&
                (Mathf.Approximately(horizontal, 0.0f) ||
                !jump))
                UpdatePlayerNetwork(NetworkUpdtMethod.SYNC_BOTH);
        }
    }

    private void FixedUpdate()
    {
        grounded = Physics2D.OverlapBox(rigid.position + feetPosition, groundSize, 0, groundLayer);

        HammerSlam();
        InvicibilityAnimation();

        if (!isLocalPlayer)
            return;

        Vector2 newSpeed = new Vector2(horizontal * speed * 20, rigid.velocity.y);
        ApplyForceAcc(rigid.velocity, newSpeed, speedAcc);

        // Cap speed
        if (rigid.velocity.x > speed || rigid.velocity.x < -speed)
        {
            ApplyForceAcc(rigid.velocity, new Vector2(0.0f, rigid.velocity.y), 0.2f);
        }

        if (jump)
        {
            ApplyJumpForce(jumpHeight);
            jump = false;
        }
    }

    [Command]
    private void CmdStartHammering()
    {
        hammerDirection = slamDirection;
        propelTimer = Utility.StartTimer(timeBeforePropelling);
        hammerState = HammerSteps.GOING;
        RpcStartHammering();
    }

    [ClientRpc]
    private void RpcStartHammering()
    {
        propelTimer = Utility.StartTimer(timeBeforePropelling);
        hammerState = HammerSteps.GOING;
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
                List<GameObject> players = propellingObj.GetComponent<PropellingBehavior>().GetTouchingPlayers();

                Vector2 direction = slamDirection;

                // Pour empecher le fait de slam dans la direction opposé une fois que le marteau arrive à sa destination
                if (slamDirection.x - hammerDirection.x > slamDirection.x) 
                    direction = hammerDirection;

                //Pour le propulser un poil en l'air
                if (Mathf.Approximately(direction.y, 0.0f))
                    direction.y = 0.3f;

                foreach (GameObject player in players)
                {
                    player.GetComponent<LocalPlayerController>().RpcGetHit(direction, force);
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
    }
    
    private void ApplyForceAcc(Vector2 actualValue, Vector2 desiredValue, float divider)
    {
        Vector2 force = desiredValue - actualValue;
        CmdApplyForce(force / divider);
        UpdatePlayerNetwork(NetworkUpdtMethod.SYNC_BOTH);
    }

    [ClientRpc]
    public void RpcGetHit(Vector2 direction, float force)
    {
        if(Utility.IsOver(timerInvincibility))
        {
            timerInvincibility = Utility.StartTimer(invincibilityTime);
            CmdApplyForce(direction * force);
            //Update pos + vitesse
            UpdatePlayerNetwork(NetworkUpdtMethod.SYNC_BOTH);
        }
    }

    private void ApplyJumpForce(float height)
    {
        float jumpForce = Mathf.Sqrt(Mathf.Abs(2.0f * Physics2D.gravity.y * height));

        Vector2 direction = Vector2.up;
        if (height < 0.0f)
            direction = Vector2.down;

        rigid.velocity = new Vector2(rigid.velocity.x, 0.0f);

        CmdApplyForce(direction * rigid.mass * 50 * jumpForce);
        //Update pos + vitesse
        UpdatePlayerNetwork(NetworkUpdtMethod.SYNC_BOTH);
    } 

    [Command]
    private void CmdApplyForce(Vector2 force)
    {
        rigid.AddForce(force);
        if (isServer)
            RpcApplyForce(force);
    }

    [ClientRpc]
    private void RpcApplyForce(Vector2 force)
    {
        if(!isServer)
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
            if (method == NetworkUpdtMethod.SYNC_POS)
                transform.position = value;
            else if(method == NetworkUpdtMethod.SYNC_VEL)
                rigid.velocity = value;
        }
    }

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
}
