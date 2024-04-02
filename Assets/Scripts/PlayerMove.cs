using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMove : MonoBehaviour
{
    // move
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public bool hasTreasure = false;

    public float gravity;
    public float groundDrag;

    public float stepRangeRadius;

    public Animator anim;

    bool inAnimation = false;

    // ground
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    // jump
    public float jumpForce;
    public float jumpCd;
    public float airMultiplier;

    // step
    public GameObject stepUpper, stepLower;
    public float stepHeight = 0.3f;
    public float stepSmooth = 0.1f;

    bool readyToJump = true;

    bool canMove = true;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    float airTime = 0f;

    Vector3 moveDir;

    bool jumpChange = false;

    Rigidbody rb;

    public MoveStates state;
    public enum MoveStates
    {
        walk,
        sprint, 
        slide,
        air
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        float playerSize = stepUpper.transform.parent.localScale.y;

        //stepUpper.transform.localPosition = new Vector3(stepUpper.transform.localPosition.x, (stepLower.transform.localPosition.y + (stepHeight/playerSize)), stepUpper.transform.localPosition.z);
    }

    private void Update()
    {
        // ground check
        //grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        Vector3 targetPos = transform.position;

        //RaycastHit hit;
        //grounded = Physics.SphereCast(transform.position, 0.3f, Vector3.down, out hit, playerHeight * 0.35f, whatIsGround);

        Collider[] hits = Physics.OverlapSphere(transform.position - (Vector3.up * playerHeight * 0.35f), 0.3f, whatIsGround);
        grounded = hits.Length > 0;

        //Vector3 raycastHitPoint = hit.point;
        //targetPos.y = raycastHitPoint.y;

        if(grounded && readyToJump)
        {
            transform.position = targetPos;
        }
       

        if (grounded == false) jumpChange = true;

        UpdateAirTime();
        GetInput();
        SpeedControl();
        StateHandler();
        PlayAnimations();
        //UpdateSteps();

        // friction
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
        //ExtraGravity();
        //UpdateSteps();
        //Step();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position - (Vector3.up * playerHeight * 0.35f), 0.3f);

        Gizmos.DrawLine(transform.position, transform.GetChild(0).transform.forward + (Vector3.down * playerHeight * 0.35f));
    }

    private void UpdateAirTime()
    {
        if (!grounded) airTime += Time.deltaTime;
        else airTime = 0;
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //if(Input.GetKey(KeyCode.Space) && readyToJump && grounded)
        //{
        //    canMove = false;
        //    readyToJump = false;
        //    anim.SetBool("Falling", true);
        //    Invoke(nameof(Jump), 0.4f);
        //
        //    Invoke(nameof(ResetJump), jumpCd + 0.4f);
        //}
    }

    private void PlayAnimations()
    {
        if (inAnimation) return;

        //if (state == MoveStates.air && readyToJump)
        //{
        //    anim.SetBool("Override", true);
        //}
        //else if (grounded)
        //{
        //    anim.SetBool("Falling", false);
        //    anim.SetBool("Override", false);
        //}

        if(jumpChange && grounded)
        {
            if(airTime > 0.25f)
            anim.SetBool("Landed", true);
            jumpChange = false;
            Invoke(nameof(ResetFallAnim), 0.01f);
        }
            
        // idle
        if (moveDir == Vector3.zero || state == MoveStates.air)
        {
            if (anim.GetFloat("Blend") < 0.99f)
                StartCoroutine(ChangeAnimState(0, 2f));
            else StartCoroutine(ChangeAnimState(0, 6f));
        }
        // run
        else if (moveSpeed == sprintSpeed)
        {
            if(anim.GetFloat("Blend") > 0.25f)
                StartCoroutine(ChangeAnimState(1f, 3f));
            else
                StartCoroutine(ChangeAnimState(1f, 8f));
        }
        // walk
        else if (moveSpeed == walkSpeed)
        {
            if(anim.GetFloat("Blend") < 0.4f)
            StartCoroutine(ChangeAnimState(0.5f, 8f));
            else
            StartCoroutine(ChangeAnimState(0.5f, 3f));
            
        }

        //if (rb.velocity.magnitude == 0) anim.SetInteger("State", 0);
        //else if (moveSpeed == sprintSpeed) anim.SetInteger("State", 2);
        //else if (moveSpeed == walkSpeed) anim.SetInteger("State", 1);
    }

    private void StateHandler()
    {
        if(grounded && Input.GetKey(KeyCode.LeftShift))
        {
            state = MoveStates.sprint;
            moveSpeed = sprintSpeed;
        }
        else if(grounded)
        {
            state = MoveStates.walk;
            moveSpeed = walkSpeed;
        }
        else
        {
            state = MoveStates.air;
        }
    }

    private void Slide()
    {
        moveDir = rb.velocity.normalized;

        rb.AddForce(moveDir * moveSpeed * 400f);
        rb.drag += 5f;
        canMove = false;
    }

    private void MovePlayer()
    {
        if (!canMove) return;
        moveDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        transform.position = new Vector3(transform.position.x, 0.11f, transform.position.z);

        if(grounded)
            rb.AddForce(moveDir.normalized * moveSpeed * 10f, ForceMode.Force);
        else if(!grounded)
            rb.AddForce(moveDir.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 vel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if(vel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = vel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        canMove = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        anim.SetBool("Falling", false);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void ResetFallAnim()
    {
        anim.SetBool("Landed", false);
    }

    private void ExtraGravity()
    {
        if (rb.velocity.y < 0f) rb.AddForce(transform.up * -gravity);
    }
    
    private void UpdateSteps()
    {
        //if(grounded)
        {
            RaycastHit hit;
            Physics.Raycast(transform.position, Vector3.down, out hit, 1f, whatIsGround);
            // get distance to ground
            //float dist = hit.distance;
            transform.position = new Vector3(transform.position.x, 0.11f, transform.position.z);
        }
    }

    private void Step()
    {
        RaycastHit hitLower;
        if(Physics.Raycast(stepLower.transform.position, transform.TransformDirection(Vector3.forward), out hitLower, 0.5f, whatIsGround)) {
            Debug.Log("Lower Bound HITTING");
            RaycastHit hitUpper;
            if (!Physics.Raycast(stepUpper.transform.position, transform.TransformDirection(Vector3.forward), out hitUpper, 0.35f, whatIsGround))
            {
                Debug.Log("UPPER Bound NOT HITTING");
                rb.position -= new Vector3(0f, -stepSmooth, 0f);
            }
        }

        RaycastHit hitLower45;
        if (Physics.Raycast(stepLower.transform.position, transform.TransformDirection(1.5f, 0, 1f), out hitLower45, 0.5f))
        {
            RaycastHit hitUpper;
            if (!Physics.Raycast(stepUpper.transform.position, transform.TransformDirection(1.5f, 0, 1f), out hitUpper, 0.35f))
            {
                rb.position -= new Vector3(0f, -stepSmooth, 0f);
            }
        }

        RaycastHit hitLower_45;
        if (Physics.Raycast(stepLower.transform.position, transform.TransformDirection(-1.5f, 0, 1f), out hitLower_45, 0.5f))
        {
            RaycastHit hitUpper;
            if (!Physics.Raycast(stepUpper.transform.position, transform.TransformDirection(-1.5f, 0, 1f), out hitUpper, 0.35f))
            {
                rb.position -= new Vector3(0f, -stepSmooth, 0f);
            }
        }
    }

    private IEnumerator ChangeAnimState(float value, float accel)
    {
        inAnimation = true;
        float t = 0;
        float v = anim.GetFloat("Blend");
        while(t < 1)
        {
            yield return new WaitForEndOfFrame();
            anim.SetFloat("Blend", Mathf.Lerp(v, value, t));
            t += Time.deltaTime * accel;
        }
        anim.SetFloat("Blend", Mathf.Lerp(v, value, 1));
        inAnimation = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            //if (other.GetComponent<EnemyMove>().susBar > 50) //gg
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                SceneManager.LoadScene("LOSE");
            }
        }
        if (other.CompareTag("Treasure"))
        {
            hasTreasure = true;
            other.transform.parent.gameObject.SetActive(false);
        }
    }
}
