using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class EnemyMove : MonoBehaviour
{
    // states: patroling
    // sus / investigating
    // alerted / aware of player (doing this three times is 100% gg)

    // we have a suspicion bar that goes up when 
    // 1) near player
    // 2) able to see player

    public LayerMask whoIsPlayer;
    public Slider slider;
    public Image fill;
    public UnityEngine.Color susColor, alertColor;

    public float walkSpeed; // patrol, multiply by 1.5 or something when investigating
    public float runSpeed; // chase player, multiply by 1.5 when gg

    private float moveSpeed;

    public Animator anim;
    public Rigidbody rb;

    public float susBar = 0f;
    private int susTimes = 0; // increase by 1 everytime player detected and sus bar gone up to 70%; should be at 2 out of 3 in second phase

    public Transform investigateSource; // if its noise, it'll be one place; if its player, it'll update to last seen every 0.25 seconds
    private Transform player;
    private float investigatingTime = 0f; // investigate for 4 seconds, then 8 seconds, then gg

    //private bool gg = false;

    private float playerVisibleDuration = 0f;
    private bool playerVisible = false;

    [Header("Patrol points")]
    public Vector3[] points;
    private int section = 0;

    public States state;
    public enum States
    {
        patrol,
        sus,
        investigate,
        alert // also aware of player
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        state = States.patrol;
    }

    private void Update()
    {
        //if(!gg)
        StateHandler();
        PlayAnimations();
        slider.value = susBar;
        if (susBar > 95) fill.color = alertColor; else fill.color = susColor;

        //if (gg)
        //{
        //    //runSpeed = 1.5f;
        //    Chase(player.position);
        //}
    }

    public void NoiseHeard(Transform location)
    {
        state = States.investigate;
        investigateSource = location;
    }

    public float waitTime;
    private void Move(Vector3 point)
    {
        Vector3 _dir = (point - transform.position).normalized;
        transform.rotation = (Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_dir), Time.deltaTime * 5f));
        

        if (Vector3.Distance(transform.position, point) < 0.25f)
        {
            rb.velocity = Vector3.zero;
            if (waitTime > 0f) return;
            waitTime = Random.Range(0.3f, 3f);
            Invoke(nameof(IncreaseSection), waitTime);
        }
        else
        {
            waitTime = 0f;
            // move to point using pathfinding
            Vector3 dir = point - transform.position;
            rb.velocity = moveSpeed * dir.normalized;
        }
        
    }

    private void IncreaseSection()
    {
        section++;
        if (section >= points.Length) section = 0;
        CancelInvoke(nameof(IncreaseSection));
    }

    private void Chase(Vector3 pos)
    {
        // pathfind to pos
        Vector3 _dir = (pos - transform.position).normalized;
        transform.rotation = (Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_dir), Time.deltaTime * 5f));

        rb.velocity = _dir * moveSpeed;
    }

    private void CheckSus()
    {
        Vector3 dir = investigateSource.position - transform.position;
        RaycastHit hit;
        if(Physics.Raycast(transform.position + (transform.up * 1.5f), dir.normalized, out hit, 10f, whoIsPlayer))
        {
            // check if the angle between raycast and forward are over 80
            // if its other 80 then skip this first if
            playerVisibleDuration = 4f;

            if (Vector3.Angle(transform.forward, dir) < 80f)
            {
                if (hit.collider.CompareTag("Player"))
                {
                    //if (susTimes == 3) gg = true;

                    RaiseSus();
                    playerVisibleDuration += Time.deltaTime;
                    playerVisible = true;
                }
            }
            else
            {
                playerVisibleDuration -= Time.deltaTime;
                playerVisible = false;
            }

            //if (playerVisibleDuration <= 0)
            //{
            //    // time out sus
            //    TimeoutSus();
            //}

        }
    }

    private void RaiseSus()
    {
        susBar += Time.deltaTime * 40;
        susBar = Mathf.Clamp(susBar, 0, 100);
        if (susBar > 71 && susBar < 72) { susTimes++; susBar = 73; }
        if (susBar > 95) state = States.alert;
        //susBar = Mathf.Min(susBar, 100);
    }

    private void TimeoutSus()
    {
        state = States.patrol;
        if (player == null) susBar = 0f;
    }

    bool updatingSource = false;
    private IEnumerator UpdateSource(float time)
    {
        updatingSource = true;
        yield return new WaitForSeconds(time);
        if (player != null)
            investigateSource = player;
        updatingSource = false;
    }

    private void Investigate()
    {
        transform.LookAt(investigateSource.position);

        // pathfind to investigateSource.position
        rb.velocity = (investigateSource.position - transform.position).normalized * moveSpeed;

        investigatingTime += Time.deltaTime;
        if(investigatingTime >= 4 * (susTimes == 0 ? 1 : susTimes))
        {
            state = States.patrol;
        }
    }

    private void StateHandler()
    {
        if (state == States.patrol)
        {
            investigatingTime = 0;
            moveSpeed = walkSpeed;
            Move(points[section]);    
        }
        else if (state == States.sus)
        {
            moveSpeed = walkSpeed;
            if (!updatingSource)
                StartCoroutine(UpdateSource(0.2f));
            CheckSus();
            investigatingTime = 0;
        }
        else if (state == States.investigate)
        {
            // susbar is > 95 or noise heard
            moveSpeed = walkSpeed * 1.25f;
            if (!updatingSource)
                StartCoroutine(UpdateSource(0.15f));
            Investigate();
        }
        else if (state == States.alert)
        {
            moveSpeed = runSpeed;
            if (!updatingSource)
                StartCoroutine(UpdateSource(0.08f));
            if (player == null)
                Chase(investigateSource.position);
            else
            Chase(player.position);
        }
    }

    private void PlayAnimations()
    {
        // if velocity is walk speed * 1 or 1.25 then play walk animation at 1 or 1.25 speed
        // if velocity is zero, play idle animation
        // if velocity is run speed then play run animtion, if gg, play run at 1.5 speed

    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Debug.Log("Assigned player variable");
            player = other.transform;
            investigateSource = player;
            susBar = 15f;
            state = States.sus;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            //if (!gg)
            {
                player = null;
                susBar = 0;
            }
        }
    }

    private void OnDrawGizmos()
    {
        foreach (var point in points)
        {
            Gizmos.DrawSphere(point, 0.25f);
        }
    }

}
