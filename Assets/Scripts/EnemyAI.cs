using UnityEngine;
using UnityEngine.AI;

public enum gameState { patrol, follow, attack, blind }

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public gameState currentState;
    public float followDistance = 10f;
    public float attackDistance = 2f;
    public float blindTimer = 0f;
    public float patrolDistance = 10f;
    public float waypointTolerance = 0.5f;
    public float patrolSpeed = 1.5f;
    public float followSpeed = 3.5f;

    private NavMeshAgent agent;
    private Transform player;

    private Vector3 puntA;
    private Vector3 puntB;
    private bool gaNaarB = true;
    private gameState previousState;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("Geen object met tag 'Player' gevonden!");
        }

        puntA = GetNavMeshPoint(transform.position);
        puntB = GetNavMeshPoint(transform.position + transform.forward * patrolDistance);

        previousState = gameState.blind; 
    }

    Vector3 GetNavMeshPoint(Vector3 target)
    {
        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return transform.position;
    }

    void Update()
    {
        if (blindTimer > 0f)
        {
            blindTimer -= Time.deltaTime;
            currentState = gameState.blind;
        }
        else if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= attackDistance)
            {
                currentState = gameState.attack;
            }
            else if (distanceToPlayer <= followDistance)
            {
                currentState = gameState.follow;
            }
            else
            {
                currentState = gameState.patrol;
            }
        }
        else
        {
            currentState = gameState.patrol;
        }

        if (currentState == gameState.patrol && previousState != gameState.patrol)
        {
            agent.isStopped = false;
            agent.ResetPath();
            agent.SetDestination(gaNaarB ? puntB : puntA);
        }

        previousState = currentState;

        switch (currentState)
        {
            case gameState.attack:
                agent.isStopped = false;
                agent.speed = followSpeed;
                agent.stoppingDistance = attackDistance * 0.8f;
                agent.SetDestination(player.position);

                Debug.Log("Attacking!");

                Vector3 dir = player.position - transform.position;
                dir.y = 0;
                if (dir != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                        Quaternion.LookRotation(dir), 5f * Time.deltaTime);
                }
                break;

            case gameState.follow:
                agent.isStopped = false;
                agent.speed = followSpeed;
                agent.stoppingDistance = attackDistance * 0.8f;
                agent.SetDestination(player.position);
                break;

            case gameState.patrol:
                agent.isStopped = false;
                agent.speed = patrolSpeed;
                agent.stoppingDistance = 0f;

                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + waypointTolerance)
                {
                    gaNaarB = !gaNaarB;
                    agent.SetDestination(gaNaarB ? puntB : puntA);
                }
                break;

            case gameState.blind:
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                break;
        }
    }
}