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
    [SerializeField, Min(0f)]
    protected float avoindanceRange = 0f;
    [SerializeField, Min(0.1f)]
    protected float timeToAttack = 0.1f;
    [SerializeField, Min(0.1f)]
    protected float staggerTime = 0.1f;

    [SerializeField]
    private string playerTag = "Player";
        
    protected NavMeshAgent  agent;
    protected StateMachine  stateMachine;
    protected Animator      animator;
    protected GameObject    target;
    protected float         timer;
    //protected bool          isAttackState = false;
    private float           defaultStoppingDistance;
    private bool            isStaggered = false;


    // ==== Setup =================================

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        // Stopping distance needs to be smaller than attack range.
        // This ensure the enemy actually enters the attack range zone.
        agent.stoppingDistance = attackRange * 0.8f;
        defaultStoppingDistance = agent.stoppingDistance;
        animator = GetComponent<Animator>();

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
        State staggerState = CreateStaggerState();

        chaseState.AddTransition(new Transition(() => isStaggered, null, staggerState));
        attackState.AddTransition(new Transition(() => isStaggered, null, staggerState));

        staggerState.AddTransition(new Transition(IsStaggerOver, null, chaseState));

        if (avoindanceRange > 0f)
        {
            State runawayState = CreateRunawayState();

            chaseState.AddTransition(new Transition(IsInAvoidanceRange, null, runawayState));
            attackState.AddTransition(new Transition(IsInAvoidanceRange, null, runawayState));
            runawayState.AddTransition(new Transition(() => !IsInAvoidanceRange(), null, chaseState));
        }

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
        animator.SetBool("isRunning", true);
    }

    protected virtual void Chase()
    {
        if (!CheckIfCanProceed()) return;

        if (target != null)
        {
            agent.SetDestination(target.transform.position);
        }
    }

    protected virtual void StopChasing()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        animator.SetBool("isRunning", false);
    }

    // ==== Attack =================================
    protected virtual State CreateAttackState()
    {
        return new State("Attack", StartAttacking, Attack, StopAttacking);
    }

    private void StartAttacking()
    {
        //isAttackState = true;

        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    protected abstract void Attack();

    private void StopAttacking()
    {
        //isAttackState = false;
        timer = 0f;

        agent.isStopped = false;
    }

    // ==== Runaway =================================
    protected virtual State CreateRunawayState()
    {
        return new State("Runaway", StartRunaway, Runaway, StopRunaway);
    }

    private void StartRunaway()
    {
        agent.isStopped = false;
        agent.stoppingDistance = 0f;
    }

    protected void Runaway()
    {
        if (target == null) return;

        Vector3 dirAway = (transform.position - target.transform.position).normalized;
        Vector3 fleePoint = transform.position + dirAway * avoindanceRange;

        if (NavMesh.SamplePosition(fleePoint, out NavMeshHit hit, avoindanceRange, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void StopRunaway()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        agent.stoppingDistance = defaultStoppingDistance;
    }

    // ==== Stagger =================================
    protected virtual State CreateStaggerState()
    {
        return new State("Stagger", StartStagger, Stagger, StopStagger);
    }

    private void StartStagger()
    {
        isStaggered = false;
        timer = 0f;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        animator?.SetTrigger("Hit");
    }

    protected void Stagger()
    {
        timer += Time.deltaTime;
    }

    private void StopStagger()
    {
        timer = 0f;

        agent.isStopped = false;
        agent.stoppingDistance = defaultStoppingDistance;
    }

    private bool IsStaggerOver() => timer >= staggerTime;

    public void TriggerStagger()
    {
        isStaggered = true;
    }

    // ==== Verifications =================================
    private bool IsInRange()
    {
        float distance = Vector3.Distance(target.transform.position, transform.position);

        Debug.DrawLine(target.transform.position, transform.position, Color.magenta, 0.2f);

        return distance <= attackRange;
    }

    private bool IsInAvoidanceRange()
    {
        if (target == null) return false;

        if (!target.CompareTag(playerTag)) return false;

        float distance = Vector3.Distance(target.transform.position, transform.position);
        return distance <= avoindanceRange;
    }

    protected bool CheckIfCanProceed()
    {
        if (animator.IsInTransition(0)) return false;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return !stateInfo.IsName("Attack") && !stateInfo.IsName("Hit");
    }

    // ==== Debug =================================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (avoindanceRange > 0f)
        {
            Gizmos.color = new Color(0.2f, 0.9f, 0.9f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, avoindanceRange);
        }
    }
}
