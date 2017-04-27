using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MutantController : MonoBehaviour
{
    public GameObject player;
    public float enemySpeed;
    public AudioClip[] footstepSounds;
    public float randomWalkRadius;
    public float maxIdleWalkDistance;
    public float wait = 0;
    public int meleeCount = 3;
    public Transform eyes;
    public string playerName;
    private NavMeshAgent navMeshAgent;
    private Animator anim;
    private AudioSource sound;
    private int attackSequence;

    private enum State
    {
        Idle,
        Walk,
        Kill,
        Chase,
        Search
    }

    private State state = State.Idle;
    private bool alive = true;

    // Use this for initialization
    void Start()
    {
        anim = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = enemySpeed;
        sound = GetComponent<AudioSource>();
    }

    // Check if we can see the player
    public void CheckSight()
    {
        if (alive)
        {
            RaycastHit raycastHit;
            if (Physics.Linecast(eyes.position, player.transform.position, out raycastHit))
            {
                if (raycastHit.collider.gameObject.name == playerName)
                {
                    if (state != State.Kill)
                    {
                        state = State.Chase;
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // If alive, do all the things
        if (alive)
        {
            anim.SetFloat("Velocity", navMeshAgent.velocity.magnitude);
            StateManagement();
        }
        else
        {
            anim.SetBool("Dead", true);
        }

    }

    public void FootStepSounds(int _num)
    {
        sound.clip = footstepSounds[_num];
        sound.Play();
    }

    void StateManagement()
    {
        if (state == State.Idle)
        {
            Vector3 randomPosition = Random.insideUnitSphere * randomWalkRadius;
            NavMeshHit navMeshHit;
            NavMesh.SamplePosition(transform.position + randomPosition, out navMeshHit, maxIdleWalkDistance, NavMesh.AllAreas);
            navMeshAgent.SetDestination(navMeshHit.position);
            state = State.Walk;
        }

        if (state == State.Walk)
        {
            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && !navMeshAgent.pathPending)
            {
                state = State.Search;
                wait = 5F;
            }
        }

        if (state == State.Search)
        {
            if (wait > 0F)
            {
                wait -= Time.deltaTime;
                transform.Rotate(0F, 120 * Time.deltaTime, 0F);
            }
            else
            {
                state = State.Idle;
            }
        }

        if (state == State.Chase)
        {
            navMeshAgent.destination = player.transform.position;
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.collider.gameObject.tag == "Player")
        {
            IncrementAttackSequence();
            if (attackSequence == meleeCount)
            {
                anim.SetBool("Attack", false);
                attackSequence = 0;
            }
            else
            {
                IncrementAttackSequence();
                anim.SetBool("Attack", true);
            }
        }
    }

    public void IncrementAttackSequence()
    {
        ++attackSequence;
    }

    void OnCollisionExit(Collision other)
    {
        if (other.collider.gameObject.tag == "Player")
        {
            anim.SetBool("Attack", false);
        }
    }
}