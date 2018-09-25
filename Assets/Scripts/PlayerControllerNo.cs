using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerNo : MonoBehaviour {

    [Header("Player")]
    [SerializeField] float speed = 5.0f;
    [SerializeField] float speedAcc = 5.0f;
    [SerializeField] float jumpHeight = 5.0f;
    [SerializeField] Transform feet;
    [SerializeField] Vector2 groundSize = new Vector2(1.0f, 0.2f);
    [SerializeField] LayerMask groundLayer;

    [Header("Propellant")]
    [SerializeField] GameObject propellingObj;
    [SerializeField] float force = 9000.0f;
    [SerializeField] AnimationCurve hammerCurve;
    [SerializeField] Vector2 hammerSize = new Vector2(0.5f, 0.5f);
    [SerializeField] float timeBeforePropelling = 0.2f;
    float propelTimer;

    enum HammerSteps
    {
        GOING,
        SLAMMING,
        RETURNING,
        IDLE
    }
    HammerSteps hammerState;

    [HideInInspector] public Rigidbody2D rigid;
    SpriteRenderer sprite;
    float lastHoriDirection;
    Vector2 slamDirection;

    // Use this for initialization
    void Start()
    {
        propelTimer = 0.0f;
        lastHoriDirection = 1.0f;
        hammerState = HammerSteps.IDLE;

        //StartCoroutine("ApplyRandomForce");
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    // Visuals and inputs
    void Update()
    {
        //Acceleration
        float horizontal = GameInput.GetAxisRaw(GameInput.AxisType.HORIZONTAL);

        if(!Mathf.Approximately(horizontal, 0.0f))
            lastHoriDirection = horizontal;

        Vector2 newSpeed = new Vector2(horizontal * speed * 20, rigid.velocity.y);
        
        ApplyForceAcc(rigid.velocity, newSpeed, speedAcc);

        //In air
        bool grounded = Physics2D.OverlapBox(feet.position, groundSize, 0, groundLayer);
        if (GameInput.GetInputDown(GameInput.InputType.JUMP) && grounded)
        {
            grounded = false;
            ApplyJumpForce(jumpHeight);
        }

        //Hammer
        if(GameInput.GetInputDown(GameInput.InputType.ATTACK))
        {
            if (GameInput.GetInput(GameInput.InputType.DOWN)) // Down attack
            {
                slamDirection = Vector2.down;
            }
            else if(GameInput.GetInput(GameInput.InputType.UP))// Up attack
            {
                slamDirection = Vector2.up;
            }
            else // Right/Left attack, depending on lastHorizontal
            {
                if(lastHoriDirection > 0.0f)
                    slamDirection = Vector2.right;
                else
                    slamDirection = Vector2.left;
            }
            propelTimer = Utility.StartTimer(timeBeforePropelling);
            hammerState = HammerSteps.GOING;
            StartCoroutine("HammerSlam");
        }
    }

    // Manage physics
    private void FixedUpdate()
    {
        // Cap speed
        if (rigid.velocity.x > speed || rigid.velocity.x < -speed)
        {
            ApplyForceAcc(rigid.velocity, new Vector2(0.0f, rigid.velocity.y), 0.2f);
        }
    }
    /*
    IEnumerator ApplyRandomForce()
    {
        while(true)
        {
            rigid.AddForce(new Vector2(9000.0f, 900.0f));
            yield return new WaitForSeconds(2.0f);
        }
    }
    */
    private IEnumerator HammerSlam()
    {
        while(true)
        {
            switch(hammerState)
            {
                case HammerSteps.GOING:
                    if(Utility.IsOver(propelTimer))
                    {
                        hammerState = HammerSteps.SLAMMING;
                        //propellingObj.SetActive(true);
                        propellingObj.GetComponent<SpriteRenderer>().color = Color.blue;
                    }
                    else
                    {
                        float dist = hammerCurve.Evaluate(1 - (Utility.GetTimerRemainingTime(propelTimer) / timeBeforePropelling));
                        propellingObj.transform.localPosition = slamDirection * dist;
                    }
                    break;
                case HammerSteps.SLAMMING:
                    hammerState = HammerSteps.RETURNING;
                    propellingObj.GetComponent<SpriteRenderer>().color = Color.green;
                    List<GameObject> players = propellingObj.GetComponent<PropellingBehavior>().GetTouchingPlayers();

                    Vector2 direction = slamDirection;
                    if (Mathf.Approximately(direction.y, 0.0f))
                    {
                        direction.y = 0.3f;
                    }
                    foreach(GameObject player in players)
                    {
                        if(player != this)
                            player.GetComponent<PlayerControllerNo>().rigid.AddForce(direction * force);
                    }
                    //propellingObj.SetActive(false);
                    propelTimer = Utility.StartTimer(timeBeforePropelling);
                    break;
                case HammerSteps.RETURNING:
                    if (Utility.IsOver(propelTimer))
                    {
                        hammerState = HammerSteps.IDLE;
                        StopCoroutine("HammerSlam");
                    }
                    else
                    {
                        float dist = hammerCurve.Evaluate(Utility.GetTimerRemainingTime(propelTimer) / timeBeforePropelling);
                        propellingObj.transform.localPosition = slamDirection * dist;
                    }
                    break;
                case HammerSteps.IDLE:
                    break;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private void ApplyForceAcc(Vector2 actualValue, Vector2 desiredValue, float divider)
    {
        Vector2 force = desiredValue - actualValue;
        rigid.AddForce(force / divider);
    }

    private void ApplyJumpForce(float height)
    {
        float jumpForce = Mathf.Sqrt(Mathf.Abs(2.0f * Physics2D.gravity.y * height));

        Vector2 direction = Vector2.up;
        if (height < 0.0f)
            direction = Vector2.down;

        rigid.AddForce(direction * rigid.mass * 50 * jumpForce);
    }
}
