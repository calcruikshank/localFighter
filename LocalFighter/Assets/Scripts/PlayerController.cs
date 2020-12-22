using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rb;
    Vector2 mousePosition, movement, inputMovement, lastMoveDir, oppositeForce;


    [SerializeField] float moveSpeed, punchRange, punchSpeed, returnSpeed, currentPercentage, brakeSpeed, stunnedTimer, shieldSpeed, grabTimer;

    public bool punchedRight, punchedLeft, returningRight, returningLeft, pummeledLeft, pummeledRight, isGrabbing, isGrabbed, shieldingRight, shieldingLeft, isBlockingRight, isBlockingLeft, readyToPummelRight, readyToPummelLeft, canDash = false;

    public Transform rightHandTransform, leftHandTransform, grabPosition, grabbedPosition;
    public CircleCollider2D rightHandCollider, leftHandCollider;
    public PlayerController opponent;
    public int team, punchesToRelease;
    public GameManager gameManager;
    public TMP_Text percentageText;
    public int stocksLeft = 3;
    public SpriteRenderer playerBody;
    public GameObject shield;
    public float halfShieldRemaining, totalShieldRemaining, actualShield = 225f / 255f;
    public float dashedTimer;
    public ScreenShake cameraShake;
    public GameObject teleportAnimation;

    private State state;
    private enum State
    {
        Normal,
        Knockback,
        Diving,
        Grabbed,
        Grabbing,
        Stunned,
        Dashing
    }

    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        cameraShake = FindObjectOfType<ScreenShake>();
        rb = GetComponent<Rigidbody2D>();


        state = State.Normal;
    }

    void Start()
    {
        totalShieldRemaining = 225f / 255f;
        gameManager.SetTeam(this);
        if (team == 0)
        {
            Color redColor = new Color(255f / 255f, 97f / 255f, 96f / 255f);
            playerBody.material.SetColor("_Color", redColor);
            shield.GetComponent<SpriteRenderer>().material.SetColor("_Color", redColor);
        }
        if (team == 1)
        {
            Color blueColor = new Color(124f / 255f, 224f / 255f, 224f / 255f);
            playerBody.material.SetColor("_Color", blueColor);
            shield.GetComponent<SpriteRenderer>().material.SetColor("_Color", blueColor);
        }
        rightHandCollider = rightHandTransform.GetComponent<CircleCollider2D>();
        leftHandCollider = leftHandTransform.GetComponent<CircleCollider2D>();
        canDash = true;
    }


    void Update()
    {
        switch (state)
        {
            case State.Normal:
                HandleMovement();
                HandleThrowingHands();
                HandleShielding();
                break;
            case State.Knockback:
                HandleKnockback();
                HandleThrowingHands();
                break;
            case State.Grabbed:
                HandleGrabbed();
                HandleShielding();
                HandleThrowingHands();
                break;
            case State.Grabbing:
                HandleMovement();
                HandlePummel();
                break;
            case State.Stunned:
                HandleStunned();
                break;
            case State.Dashing:
                HandleDash();
                HandleThrowingHands();
                HandleShielding();
                break;
        }
    }


    void FixedUpdate()
    {
        switch (state)
        {
            case State.Normal:
                FixedHandleMovement();
                break;
            case State.Grabbing:
                FixedHandleMovement();
                break;

        }
    }


    public void HandleMovement()
    {
        movement.x = inputMovement.x;
        movement.y = inputMovement.y;
        movement = movement;
        if (movement.x != 0 || movement.y != 0)
        {
            lastMoveDir = movement;
        }
        stunnedTimer = 0;
        returnSpeed = 4f;
        isGrabbed = false;
        dashedTimer = 0f;
        canDash = true;
    }

    public void FixedHandleMovement()
    {
        rb.velocity = movement * moveSpeed;
    }


    public void Knockback(float damage, Vector2 direction)
    {
        StartCoroutine(cameraShake.Shake(.03f, .3f));

        currentPercentage += damage;
        percentageText.text = (currentPercentage + "%");
        brakeSpeed = 20f;
        // Debug.Log(damage + " damage");
        //Vector2 direction = new Vector2(rb.position.x - handLocation.x, rb.position.y - handLocation.y); //distance between explosion position and rigidbody(bluePlayer)
        //direction = direction.normalized;
        float knockbackValue = (14 * ((currentPercentage + damage) * (damage / 3)) / 100) + 7; //knockback that scales
        rb.AddForce(direction * knockbackValue, ForceMode2D.Impulse);
        isGrabbed = false;
        //Debug.Log(currentPercentage + "current percentage");
        state = State.Knockback;
    }


    public void HandleKnockback()
    {

        movement.x = inputMovement.x;
        movement.y = inputMovement.y;
        movement = movement;
        if (movement.x != 0 || movement.y != 0)
        {
            lastMoveDir = movement;
        }
        if (rb.velocity.magnitude <= 5f)
        {
            rb.velocity = new Vector2(0, 0);
            state = State.Normal;
        }
        if (rb.velocity.magnitude > 0)
        {
            oppositeForce = -rb.velocity;
            brakeSpeed = brakeSpeed + (100f * Time.deltaTime);
            rb.AddForce(oppositeForce * Time.deltaTime * brakeSpeed);
            rb.AddForce(lastMoveDir * Time.deltaTime * brakeSpeed * .5f); //DI
        }
    }

    public void HandleThrowingHands()
    {
        if (punchedRight && returningRight == false)
        {
            rightHandCollider.enabled = true;
            rightHandTransform.localPosition = Vector3.MoveTowards(rightHandTransform.localPosition, new Vector2(punchRange, .4f), punchSpeed * Time.deltaTime);
            if (rightHandTransform.localPosition.x >= punchRange)
            {
                returningRight = true;
            }
        }
        if (returningRight)
        {
            punchedRight = false;


            rightHandTransform.localPosition = Vector3.MoveTowards(rightHandTransform.localPosition, new Vector2(0, 0), returnSpeed * Time.deltaTime);


            if (rightHandTransform.localPosition.x <= 1f)
            {
                rightHandCollider.enabled = false;
            }
            if (rightHandTransform.localPosition.x <= 0f)
            {
                returningRight = false;
            }
        }
        if (punchedLeft && returningLeft == false)
        {
            leftHandCollider.enabled = true;
            leftHandTransform.localPosition = Vector3.MoveTowards(leftHandTransform.localPosition, new Vector2(punchRange, -.4f), punchSpeed * Time.deltaTime);
            if (leftHandTransform.localPosition.x >= punchRange)
            {
                returningLeft = true;
            }
        }
        if (returningLeft)
        {
            punchedLeft = false;
            leftHandTransform.localPosition = Vector3.MoveTowards(leftHandTransform.localPosition, new Vector2(0, 0), returnSpeed * Time.deltaTime);

            if (leftHandTransform.localPosition.x <= 1f)
            {
                leftHandCollider.enabled = false;
            }
            if (leftHandTransform.localPosition.x <= 0f)
            {
                returningLeft = false;
            }
        }
    }

    public void HandlePummel()
    {
        grabTimer += Time.deltaTime;
        if (grabTimer > (opponent.currentPercentage / 50f) + .2f)
        {
            returningLeft = true;
            returningRight = true;
            isGrabbing = false;
            opponent.rb.velocity = Vector3.zero;
            opponent.Throw(this.grabPosition.right);
            grabTimer = 0;
            state = State.Normal;
        }
        if (pummeledLeft)
        {
            leftHandCollider.enabled = true;
            leftHandTransform.localPosition = Vector3.MoveTowards(leftHandTransform.localPosition, new Vector2(punchRange, -.4f), punchSpeed * Time.deltaTime);
        }
        if (pummeledRight)
        {
            rightHandCollider.enabled = true;
            rightHandTransform.localPosition = Vector3.MoveTowards(rightHandTransform.localPosition, new Vector2(punchRange, .4f), punchSpeed * Time.deltaTime);
        }
        if (!pummeledRight)
        {
            rightHandCollider.enabled = false;
            rightHandTransform.localPosition = Vector3.MoveTowards(rightHandTransform.localPosition, new Vector2(0, 0), returnSpeed * Time.deltaTime);
            readyToPummelRight = true;
        }
        if (!pummeledLeft)
        {
            leftHandCollider.enabled = false;
            leftHandTransform.localPosition = Vector3.MoveTowards(leftHandTransform.localPosition, new Vector2(0, 0), returnSpeed * Time.deltaTime);
            readyToPummelLeft = true;
        }
        if (!pummeledLeft && !pummeledRight)
        {
            returningLeft = true;
            returningRight = true;
            isGrabbing = false;
            opponent.rb.velocity = Vector3.zero;
            opponent.Throw(this.grabPosition.right);
            grabTimer = 0;
            state = State.Normal;
        }
        if (pummeledLeft && !pummeledRight)
        {
            leftHandCollider.enabled = false;
        }
        if (!pummeledLeft && pummeledRight)
        {
            rightHandCollider.enabled = false;
        }
        if (pummeledLeft && pummeledRight)
        {
            isGrabbing = true;
        }

    }
    public void Grab(PlayerController opponentCheck)
    {
        moveSpeed = 5f;
        returningLeft = false;
        returningRight = false;
        punchedLeft = false;
        punchedRight = false;
        isGrabbing = true;
        opponent = opponentCheck;
        leftHandTransform.localPosition = new Vector2(punchRange, -.4f);
        rightHandTransform.localPosition = new Vector2(punchRange, .4f);
        state = State.Grabbing;
    }
    public void Grabbed(Transform player)
    {
        punchesToRelease = 0;
        grabbedPosition = player; //the transform that grabbed you is equal to the player that grabbed you grab position
        returningLeft = true;
        returningRight = true;
        state = State.Grabbed;
    }

    public void HandleGrabbed()
    {
        isGrabbed = true;
        isBlockingLeft = false;
        isBlockingRight = false;

        transform.position = grabbedPosition.position;
    }
    public void Throw(Vector2 direction)
    {
        grabTimer = 0;
        moveSpeed = 10f;
        brakeSpeed = 20f;
        float knockbackValue = 20f;
        rb.AddForce(direction * knockbackValue, ForceMode2D.Impulse);

        //Debug.Log(currentPercentage + "current percentage");
        state = State.Knockback;
    }
    public void HandleShielding()
    {
        if (actualShield <= 25f / 255f)
        {
            state = State.Stunned;
        }
        //Debug.Log(isBlockingLeft + " left" + isBlockingRight + " right");
        if (shieldingRight && !shieldingLeft)
        {



            //rightHandTransform.localScale = new Vector2(.75f, 1.25f);
            rightHandTransform.localPosition = Vector3.MoveTowards(rightHandTransform.localPosition, new Vector2(0, .4f), punchSpeed * Time.deltaTime);

        }

        if (!shieldingRight)
        {
            rightHandCollider.radius = .5f;
            rightHandCollider.isTrigger = true;
            rightHandTransform.localScale = new Vector2(1, 1);
            rightHandTransform.localPosition = Vector3.MoveTowards(rightHandTransform.localPosition, new Vector2(0, 0), returnSpeed * Time.deltaTime);
        }

        if (shieldingLeft && shieldingRight)
        {
            //leftHandTransform.localScale = new Vector2(.75f, 1.25f);
            leftHandTransform.localPosition = Vector3.MoveTowards(leftHandTransform.localPosition, new Vector2(0, -.4f), punchSpeed * Time.deltaTime);
            //rightHandTransform.localScale = new Vector2(.75f, 1.25f);
            rightHandTransform.localPosition = Vector3.MoveTowards(rightHandTransform.localPosition, new Vector2(0, .4f), punchSpeed * Time.deltaTime);
        }

        if (shieldingLeft && !shieldingRight)
        {

            //leftHandTransform.localScale = new Vector2(.75f, 1.25f);
            leftHandTransform.localPosition = Vector3.MoveTowards(leftHandTransform.localPosition, new Vector2(0, -.4f), punchSpeed * Time.deltaTime);
        }

        if (!shieldingLeft)
        {
            leftHandCollider.radius = .5f;
            leftHandCollider.isTrigger = true;
            leftHandTransform.localScale = new Vector2(1, 1);
            leftHandTransform.localPosition = Vector3.MoveTowards(leftHandTransform.localPosition, new Vector2(0, 0), returnSpeed * Time.deltaTime);
        }
        if (rightHandTransform.localPosition.x == 0 && rightHandTransform.localPosition.y == .4f)
        {
            isBlockingRight = true;
        }
        if (rightHandTransform.localPosition.y != .4f)
        {
            isBlockingRight = false;
        }
        if (leftHandTransform.localPosition.y != -.4f)
        {
            isBlockingLeft = false;
        }
        if (leftHandTransform.localPosition.x == 0 && leftHandTransform.localPosition.y == -.4f)
        {
            isBlockingLeft = true;
        }
        if (isBlockingRight && !isBlockingLeft)
        {
            totalShieldRemaining -= (50f / 255f) * Time.deltaTime;
            //Debug.Log(halfShieldRemaining);
            //Debug.Log(totalShieldRemaining);
            shield.SetActive(true);
            halfShieldRemaining = totalShieldRemaining / 2;
            actualShield = halfShieldRemaining;
            Color tmp = shield.GetComponent<SpriteRenderer>().color;
            tmp.a = actualShield;
            shield.GetComponent<SpriteRenderer>().color = tmp;
            /*rightHandCollider.enabled = true;
            rightHandCollider.radius = .75f;
            rightHandCollider.isTrigger = false;*/
            moveSpeed = 0f;

        }
        if (isBlockingLeft && !isBlockingRight)
        {
            totalShieldRemaining -= (20f / 255f) * Time.deltaTime;
            //Debug.Log(halfShieldRemaining);
            //Debug.Log(totalShieldRemaining);
            shield.SetActive(true);
            halfShieldRemaining = totalShieldRemaining / 2;
            actualShield = halfShieldRemaining;
            Color tmp = shield.GetComponent<SpriteRenderer>().color;
            tmp.a = actualShield;
            shield.GetComponent<SpriteRenderer>().color = tmp;
            /*leftHandCollider.enabled = true;
            leftHandCollider.radius = .75f;
            leftHandCollider.isTrigger = false;*/
            moveSpeed = 0f;

        }
        if (isBlockingLeft && isBlockingRight)
        {

            actualShield = totalShieldRemaining;
            totalShieldRemaining -= (20f / 255f) * Time.deltaTime;

            shield.SetActive(true);
            Color tmp = shield.GetComponent<SpriteRenderer>().color;
            tmp.a = actualShield;
            shield.GetComponent<SpriteRenderer>().color = tmp;
            moveSpeed = 0f;

        }
        if (!isBlockingRight && !isBlockingLeft)
        {
            shield.SetActive(false);
            if (totalShieldRemaining < 225f / 255f)
            {
                totalShieldRemaining += 5f / 255f * Time.deltaTime;
            }
            moveSpeed = 10f;
        }


    }

    public void EndPunchRight()
    {
        returningRight = true;
        punchedRight = false;
        rightHandCollider.enabled = false;
        rightHandTransform.localScale = new Vector2(1, 1);
        pummeledLeft = false;
        state = State.Normal;
    }

    public void EndPunchLeft()
    {
        returningLeft = true;
        punchedLeft = false;
        leftHandCollider.enabled = false;
        leftHandTransform.localScale = new Vector2(1, 1);
        pummeledRight = false;
        state = State.Normal;
    }
    public void Respawn()
    {
        canDash = true;
        currentPercentage = 0;
        rb.velocity = Vector3.zero;
        transform.position = new Vector2(0, 0);
        percentageText.text = (currentPercentage + "%");
        totalShieldRemaining = 225f / 255f;
    }

    public void HandleStunned()
    {
        EndPunchLeft();
        EndPunchRight();
        shield.SetActive(false);
        leftHandTransform.localPosition = Vector3.MoveTowards(leftHandTransform.localPosition, new Vector2(0, 0), returnSpeed * Time.deltaTime);
        rightHandTransform.localPosition = Vector3.MoveTowards(rightHandTransform.localPosition, new Vector2(0, 0), returnSpeed * Time.deltaTime);
        leftHandTransform.localScale = new Vector2(1, 1);
        rightHandTransform.localScale = new Vector2(1, 1);
        rb.velocity = Vector3.zero;
        isBlockingLeft = false;
        isBlockingRight = false;
        totalShieldRemaining = 200f / 255f;
        actualShield = totalShieldRemaining;
        stunnedTimer += Time.deltaTime;
        if (stunnedTimer >= 3f)
        {
            state = State.Normal;
        }
    }

    public void TakePummelDamage()
    {
        currentPercentage++;
        percentageText.text = (currentPercentage + "%");
    }

    public void HandleDash()
    {
        returnSpeed = 1.25f;
        rb.velocity = Vector3.zero;
        shieldingRight = false;
        isBlockingRight = false;

        dashedTimer += Time.deltaTime;
        if (rightHandTransform.localPosition.x == 0 && punchedRight == false)
        {

            state = State.Normal;
            dashedTimer = 0f;
            canDash = true;

        }
        //state = State.Normal;
    }
    public void Dash(Vector3 direction)
    {
        if (canDash)
        {
            Debug.Log(direction);
            Instantiate(teleportAnimation, transform.position, Quaternion.identity);
            direction = direction.normalized;
            float dashDistance = 5f;
            transform.position += direction * dashDistance;
            state = State.Dashing;
            canDash = false;

            punchedRight = true;
        }

    }
    public void AddDamage(float damage)
    {
        currentPercentage += damage;
        percentageText.text = (currentPercentage + "%");
    }
    public void EndGrab()
    {
        returningLeft = true;
        punchedLeft = false;
        leftHandCollider.enabled = false;
        leftHandTransform.localScale = new Vector2(1, 1);
        pummeledRight = false;
        returningRight = true;
        punchedRight = false;
        rightHandCollider.enabled = false;
        rightHandTransform.localScale = new Vector2(1, 1);
        pummeledLeft = false;
        state = State.Normal;
    }




    #region InputRegion
    void OnKeyboardMove(InputValue value)
    {
        inputMovement = value.Get<Vector2>();
        FaceJoystick();
    }
    void OnMouseLook(InputValue value)
    {
        if (state != State.Normal) return;
        if (value != null)
        {
            mousePosition = value.Get<Vector2>();
        }
        if (mousePosition.magnitude >= .9f)
        {

            Dash(mousePosition.normalized);
            Debug.Log(canDash);

        }
        //if(usingMouse) FaceMouse();

    }
    void FaceMouse()
    {

        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector2 direction = new Vector2(mousePosition.x - transform.position.x, mousePosition.y - transform.position.y);
        transform.right = direction;
    }
    void FaceJoystick()
    {
        Vector2 joystickPosition = inputMovement.normalized;
        if (joystickPosition.x != 0 || joystickPosition.y != 0)
        {
            Vector2 lastLookedPosition = joystickPosition;
            //Vector2 direction = new Vector2(mousePosition.x - transform.position.x, mousePosition.y - transform.position.y);
            transform.right = lastLookedPosition;
        }


    }
    void OnPunchRight()
    {
        pummeledRight = true;
        if (state == State.Grabbing) return;
        if (state == State.Dashing) return;
        if (state == State.Knockback) return;
        if (returningRight) return;
        punchedRight = true;
        shieldingRight = false;
    }
    void OnPunchLeft()
    {
        pummeledLeft = true;
        if (state == State.Grabbing) return;
        if (state == State.Dashing) return;
        if (state == State.Knockback) return;
        if (returningLeft) return;
        punchedLeft = true;
        shieldingLeft = false;
    }
    void OnReleasePunchRight()
    {
        pummeledRight = false;
    }
    void OnReleasePunchLeft()
    {
        pummeledLeft = false;
    }

    void OnShieldRight()
    {
        if (state == State.Grabbed) return;
        if (state == State.Dashing) return;
        if (punchedRight || returningRight)
        {
            return;
        }

        shieldingRight = true;
    }

    void OnShieldLeft()
    {
        if (state == State.Grabbed) return;
        if (state == State.Dashing) return;
        if (punchedLeft || returningLeft)
        {
            return;
        }
        shieldingLeft = true;
    }

    void OnReleaseShieldRight()
    {
        shieldingRight = false;
    }
    void OnReleaseShieldLeft()
    {
        shieldingLeft = false;
    }
    #endregion

}
