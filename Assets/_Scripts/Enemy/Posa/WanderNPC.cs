using UnityEngine;
using UnityEngine.AI;

public class WanderNPC : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator catAnim; // UPGRADED TO ANIMATOR!
    
    public float wanderRadius = 10f;
    public float waitTimer = 3f;
    private float timer;

    public string[] idleAnimations; 
    public string walkAnimName = "Walk_F"; 
    public float turnSpeed = 5f; 

    private bool isMoving = false;
    private string currentIdleAnim;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        catAnim = GetComponent<Animator>(); // UPGRADED TO ANIMATOR!
        timer = waitTimer;
        
        agent.updateRotation = false; 
        PickRandomIdle(); 
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= waitTimer)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            agent.SetDestination(newPos);
            timer = 0; 
            waitTimer = Random.Range(3f, 8f); 
            isMoving = true;
        }

        if (agent.velocity.magnitude > 0.1f)
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
                // CrossFadeInFixedTime perfectly blends Animator states!
                catAnim.CrossFadeInFixedTime(walkAnimName, 0.2f); 
                isMoving = true;
            }
        }
        else 
        {
            if (isMoving)
            {
                isMoving = false;
                PickRandomIdle();
                
                // Play the idle animation with a smooth 0.2 second blend
                catAnim.CrossFadeInFixedTime(currentIdleAnim, 0.2f);
            }
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
            currentIdleAnim = "A_Cat_Idle"; 
        }
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