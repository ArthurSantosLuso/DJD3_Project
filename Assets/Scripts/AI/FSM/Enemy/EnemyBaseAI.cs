using LibGameAI.FSMs;
using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class EnemyBaseAI : MonoBehaviour
{
    [SerializeField]
    protected float moveSpeed = 1.0f;
    [SerializeField, Min(0.1f)]
    protected float attackRange = 1.0f;

    protected NavMeshAgent agent;
    protected StateMachine stateMachine;

    private GameObject target;

    // ==== Setup =================================

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        // Stopping distance needs to be smaller than attack range.
        // This ensure the enemy actually enters the attack range zone.
        agent.stoppingDistance = attackRange * 0.8f;

        Initialize();
    }

    public void Initialize()
    {
        target = GameManager.Instance.Player;
        State chaseState = CreateChaseState();
        State attackState = CreateAttackState();

        AddTransitions(chaseState, attackState);

        stateMachine = new StateMachine(chaseState);
    }

    protected virtual void AddTransitions(State chaseState, State attackState)
    {
        chaseState.AddTransition(new Transition(IsInRange, null, attackState));
        attackState.AddTransition(new Transition(() => !IsInRange(), null, chaseState));
    }

    // ==== Update =================================
    protected virtual void Update()
    {
        Action action = stateMachine.Update();
        action?.Invoke();
    }

    // ==== Chase =================================
    protected virtual State CreateChaseState()
    {
        return new State("Chase", StartChasing, Chase, StopChasing);
    }

    private void StartChasing()
    {
        agent.isStopped = false;
    }

    protected virtual void Chase()
    {
        if (target != null)
        {
            agent.SetDestination(target.transform.position);
        }
    }

    private void StopChasing()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    // ==== Attack =================================
    protected virtual State CreateAttackState()
    {
        return new State("Attack", null, Attack, null);
    }

    protected abstract void Attack();

    // ==== Verifications =================================
    private bool IsInRange()
    {
        /*Vector3 toTarget = target.transform.position - transform.position;
        toTarget.y = 0.0f;*/


        float distance = Vector3.Distance(target.transform.position, transform.position);

        Debug.DrawLine(target.transform.position, transform.position, Color.magenta, 0.2f);

        return distance <= attackRange;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
