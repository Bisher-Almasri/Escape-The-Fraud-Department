using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]
public class AI : MonoBehaviour
{
    public Transform player;
    public float updatePathInterval = 0.5f;
    public float moveSpeed = 3.5f;

    private NavMeshAgent agent;
    private float nextUpdateTime;
    private GameObject plane;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (transform.childCount > 0)
            plane = transform.GetChild(0).gameObject;
        else
            Debug.LogWarning("AI has no child plane!");

        agent.speed = moveSpeed;
        agent.angularSpeed = 120f;
        agent.acceleration = 8f;
        agent.stoppingDistance = 1.2f;
    }

    void Update()
    {
        FollowPlayer();

        if (plane != null)
        {
            var localRot = plane.transform.localEulerAngles;
            localRot.x = 90f;
            localRot.y = 0f;
            localRot.z = 0f;
            plane.transform.localEulerAngles = localRot;
        }
    }

    private void FollowPlayer()
    {
        if (player == null) return;

        if (Time.time >= nextUpdateTime)
        {
            agent.SetDestination(player.position);
            nextUpdateTime = Time.time + updatePathInterval;
        }

        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion lookRot = Quaternion.LookRotation(agent.velocity.normalized);
            Vector3 euler = lookRot.eulerAngles;
            euler.x = 0f;
            euler.z = 0f;
            lookRot = Quaternion.Euler(euler);

            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
        }
    }
}
