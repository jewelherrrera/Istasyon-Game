using UnityEngine;
using UnityEngine.AI;

public class WanderNPC : MonoBehaviour
{
    public enum AIState { Idling, Wandering, Following }
    
    [Header("Current State (View Only)")]
    public AIState currentState = AIState.Idling;

    public NavMeshAgent agent;
    public Animator catAnim; 
    
    [Header("Wander Settings")]
    public float wanderRadius = 10f;
    
    [Header("Idle Settings")]
    public float minIdleTime = 5f;   
    public float maxIdleTime = 15f;  
    private float timer;
    private float waitTimer;

    public string[] idleAnimations; 
    
    [Header("Follow Player Settings")]
    public Transform player; 
    [Range(0f, 1f)] public float followChance = 0.3f; 
    public float minFollowTime = 10f;
    public float maxFollowTime = 25f;
    public float followStoppingDistance = 2.5f;

    // ---> NEW: Independent Global Meow System <---
    [Header("Audio Settings (Meows)")]
    public AudioSource catAudioSource;
    public AudioClip[] meowClips;
    public float minMeowDelay = 8f;   // NEW: Minimum seconds between meows
    public float maxMeowDelay = 20f;  // NEW: Maximum seconds between meows
    private float meowTimer;
    private float nextMeowTime;

    [Header("Movement Settings")]
    public string walkAnimName = "Walk_F"; 
    public string runAnimName = "Run_F"; 
    public string jumpAnimName = "A_Cat_Jump"; 
    public float walkSpeed = 1.5f;       
    public float runSpeed = 3.5f;        
    public float jumpSpeed = 4.0f;         
    public float turnSpeed = 5f; 

    private bool isMoving = false;
    private string currentIdleAnim;
    private string currentMoveAnim; 

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        catAnim = GetComponent<Animator>(); 
        
        agent.updateRotation = false; 
        currentMoveAnim = walkAnimName; 
        PickRandomIdle(); 
        
        waitTimer = Random.Range(minIdleTime, maxIdleTime);
        
        // Start the vocal timer immediately
        ResetMeowTimer();
    }

    void Update()
    {
        // 1. GLOBAL MEOW LOGIC (Happens randomly no matter what the cat is doing)
        HandleRandomMeows();

        // 2. BRAIN LOGIC
        if (currentState == AIState.Idling)
        {
            HandleIdling();
        }
        else if (currentState == AIState.Wandering)
        {
            HandleWandering();
        }
        else if (currentState == AIState.Following)
        {
            HandleFollowing();
        }

        // 3. MOVEMENT & ANIMATION LOGIC
        UpdateMovementAndAnimations();
    }

    void HandleRandomMeows()
    {
        meowTimer += Time.deltaTime;
        
        if (meowTimer >= nextMeowTime)
        {
            // The fix: ONLY play a meow if the cat isn't already making a sound!
            if (catAudioSource != null && !catAudioSource.isPlaying && meowClips.Length > 0)
            {
                PlayRandomMeow();
            }
            
            // Reset the timer to pick a new random gap for the next meow
            ResetMeowTimer();
        }
    }

    void HandleIdling()
    {
        timer += Time.deltaTime;

        if (timer >= waitTimer)
        {
            timer = 0f; 

            // Roll dice: Follow player or Wander?
            if (player != null && Random.value <= followChance)
            {
                currentState = AIState.Following;
                waitTimer = Random.Range(minFollowTime, maxFollowTime);
                agent.stoppingDistance = followStoppingDistance;
                
                SetMoveStyle(Random.Range(0, 3));
            }
            else
            {
                currentState = AIState.Wandering;
                agent.stoppingDistance = 0f;
                Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
                agent.SetDestination(newPos);
                
                SetMoveStyle(Random.Range(0, 3));
            }
        }
    }

    void HandleWandering()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (agent.velocity.magnitude < 0.1f)
            {
                EnterIdleState();
            }
        }
    }

    void HandleFollowing()
    {
        timer += Time.deltaTime;

        if (player != null)
        {
            agent.SetDestination(player.position);
        }

        if (timer >= waitTimer)
        {
            EnterIdleState();
        }
    }

    void EnterIdleState()
    {
        currentState = AIState.Idling;
        timer = 0f;
        waitTimer = Random.Range(minIdleTime, maxIdleTime);
        agent.stoppingDistance = 0f;
        agent.ResetPath(); 
    }

    void UpdateMovementAndAnimations()
    {
        if (agent.velocity.magnitude > 0.1f || agent.pathPending)
        {
            Vector3 lookDirection = agent.velocity.normalized;
            lookDirection.y = 0; 
            
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
            }

            if (!isMoving)
            {
                catAnim.CrossFadeInFixedTime(currentMoveAnim, 0.2f); 
                isMoving = true;
            }
        }
        else 
        {
            if (isMoving) 
            {
                isMoving = false;
                PickRandomIdle();
                catAnim.CrossFadeInFixedTime(currentIdleAnim, 0.2f);
            }
            else if (currentState == AIState.Following && player != null)
            {
                Vector3 lookDirection = (player.position - transform.position).normalized;
                lookDirection.y = 0;
                if (lookDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * (turnSpeed / 2f));
                }
            }
        }
    }

    void SetMoveStyle(int moveRoll)
    {
        if (moveRoll == 0) 
        {
            currentMoveAnim = walkAnimName;
            agent.speed = walkSpeed; 
        }
        else if (moveRoll == 1)
        {
            currentMoveAnim = runAnimName;
            agent.speed = runSpeed; 
        }
        else 
        {
            currentMoveAnim = jumpAnimName;
            agent.speed = jumpSpeed; 
        }
    }

    void PickRandomIdle()
    {
        if (idleAnimations.Length > 0)
        {
            int randomIndex = Random.Range(0, idleAnimations.Length);
            currentIdleAnim = idleAnimations[randomIndex];
        }
        else
        {
            currentIdleAnim = "Base"; 
        }
    }

    void PlayRandomMeow()
    {
        int rand = Random.Range(0, meowClips.Length);
        catAudioSource.PlayOneShot(meowClips[rand]);
    }

    void ResetMeowTimer()
    {
        meowTimer = 0f;
        // It will now wait a random amount of time between minMeowDelay and maxMeowDelay
        nextMeowTime = Random.Range(minMeowDelay, maxMeowDelay); 
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }
}