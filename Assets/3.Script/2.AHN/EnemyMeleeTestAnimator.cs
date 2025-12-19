using UnityEngine;
using UnityEngine.AI;

public class NPCControllerOnlySpeed : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent; 

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        float currentSpeed = agent.velocity.magnitude;

        animator.SetFloat("Speed", currentSpeed);
    }
}