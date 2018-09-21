using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerControllerNetwork : NetworkBehaviour
{
    [Header("Player")]
    [SerializeField] float speed = 9.0f;
    [SerializeField] float speedAcc = 2.0f;
    [SerializeField] float jumpHeight = 5.0f;
    [SerializeField] Transform feet;
    [SerializeField] Vector2 groundSize = new Vector2(1.0f, 0.1f);
    [SerializeField] LayerMask groundLayer;

    [Header("Propellant")]
    [SerializeField] GameObject propellingObj;
    [SerializeField] float force = 9000.0f;
    [SerializeField] AnimationCurve hammerCurve;
    [SerializeField] Vector2 hammerSize = new Vector2(0.5f, 0.5f);
    [SerializeField] float timeBeforePropelling = 0.1f;
    [SyncVar] float propelTimer;

    enum HammerSteps
    {
        GOING,
        SLAMMING,
        RETURNING,
        IDLE
    }
    [SyncVar] HammerSteps hammerState;

    [HideInInspector] public Rigidbody2D rigid;
    SpriteRenderer sprite;
    float lastHoriDirection;
    [SyncVar] Vector2 slamDirection, hammerDirection;

    // Use this for initialization
    void Start ()
    {
        propelTimer = 0.0f;
        lastHoriDirection = 1.0f;
        hammerState = HammerSteps.IDLE;
       
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    public override void OnStartLocalPlayer()
    {
        GetComponent<SpriteRenderer>().color = Color.blue;
    }

    // Update is called once per frame
    void Update () {
        if (!isLocalPlayer)
            return;

        //Acceleration
        float horizontal = GameInput.GetAxisRaw(GameInput.AxisType.HORIZONTAL);

        if (!Mathf.Approximately(horizontal, 0.0f))
            lastHoriDirection = horizontal;

        Vector2 newSpeed = new Vector2(horizontal * speed * 20, rigid.velocity.y);
        ApplyForceAcc(rigid.velocity, newSpeed, speedAcc);
        CmdApplyForceAcc(rigid.velocity, newSpeed, speedAcc);

        //In air
        bool grounded = Physics2D.OverlapBox(feet.position, groundSize, 0, groundLayer);
        if (GameInput.GetInputDown(GameInput.InputType.JUMP) && grounded)
        {
            grounded = false;
            ApplyJumpForce(jumpHeight);
            CmdApplyJumpForce(jumpHeight);
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
                hammerDirection = slamDirection;
                propelTimer = Utility.StartTimer(timeBeforePropelling);
                hammerState = HammerSteps.GOING;
            }
        }
    }
    private void FixedUpdate()
    {
        // Cap speed
        if (rigid.velocity.x > speed || rigid.velocity.x < -speed)
        {
            ApplyForceAcc(rigid.velocity, new Vector2(0.0f, rigid.velocity.y), 0.2f);
            CmdApplyForceAcc(rigid.velocity, new Vector2(0.0f, rigid.velocity.y), 0.2f);
        }
        HammerSlam();
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
                    // TODO : A CHANGER POUR UNE FONCTION PLUS CORRECT
                    player.GetComponent<PlayerControllerNetwork>().GetHit(direction, force);
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
        rigid.AddForce(force / divider);
    }

    [Command]
    private void CmdApplyForceAcc(Vector2 actualValue, Vector2 desiredValue, float divider)
    {
        Vector2 force = desiredValue - actualValue;
        rigid.AddForce(force / divider);
    }

    public void GetHit(Vector2 direction, float force)
    {
        ApplyForceDirection(direction, force);
        CmdApplyForceDirection(direction, force);
    }

    private void ApplyJumpForce(float height)
    {
        float jumpForce = Mathf.Sqrt(Mathf.Abs(2.0f * Physics2D.gravity.y * height));

        Vector2 direction = Vector2.up;
        if (height < 0.0f)
            direction = Vector2.down;

        rigid.AddForce(direction * rigid.mass * 50 * jumpForce);
    } 

    [Command]
    private void CmdApplyJumpForce(float height)
    {
        float jumpForce = Mathf.Sqrt(Mathf.Abs(2.0f * Physics2D.gravity.y * height));

        Vector2 direction = Vector2.up;
        if (height < 0.0f)
            direction = Vector2.down;

        rigid.AddForce(direction * rigid.mass * 50 * jumpForce);
    }

    private void ApplyForceDirection(Vector2 direction, float force)
    {
        rigid.AddForce(direction * force);
    }

    [Command]
    private void CmdApplyForceDirection(Vector2 direction, float force)
    {
        rigid.AddForce(direction * force);
    }
}
