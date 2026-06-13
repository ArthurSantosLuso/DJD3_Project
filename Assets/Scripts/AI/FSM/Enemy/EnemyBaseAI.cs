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
    [SerializeField, Min(0f)]
    private float fleeDelay = 1.5f; // seconds inside avoidance range before fleeing

    [SerializeField]
    private string playerTag = "Player";

    protected NavMeshAgent agent;
    protected StateMachine stateMachine;
    protected Animator animator;
    protected GameObject target;
    protected float timer;
    private float defaultStoppingDistance;
    private bool isStaggered = false;
    private float fleeDelayTimer = 0f; // counts up while inside avoidance range


    // ==== Setup =================================

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange * 0.8f;
        defaultStoppingDistance = agent.stoppingDistance;
        animator = GetComponent<Animator>();

        Initialize();
    }

    public void Initialize()
    {
        target = LevelManager.Instance.Player;
        State chaseState = CreateChaseState();
        State attackState = CreateAttackState();

        AddTransitions(chaseState, attackState);

        stateMachine = new StateMachine(chaseState);
    }

    protected virtual void AddTransitions(State chaseState, State attackState)
    {
        State staggerState = CreateStaggerState();

        if (avoindanceRange > 0f && avoindanceRange >= attackRange)
        {
            Debug.LogWarning($"[{gameObject.name}] avoindanceRange ({avoindanceRange}) must be " +
                             $"smaller than attackRange ({attackRange}). Avoidance disabled.");
            avoindanceRange = 0f;
        }

        // Chase state transitions
        chaseState.AddTransition(new Transition(IsInRange, null, attackState));

        if (avoindanceRange > 0f)
        {
            State runawayState = CreateRunawayState();

            // The transition fires only after the enemy has been inside the
            // avoidance zone continuously for fleeDelay seconds.
            // IsInAvoidanceRange still guards the timer so it resets the
            // moment the player steps out, preventing a stale countdown.
            chaseState.AddTransition(new Transition(ShouldFlee, null, runawayState));
            attackState.AddTransition(new Transition(ShouldFlee, null, runawayState));
            runawayState.AddTransition(new Transition(() => !IsInAvoidanceRange(), null, chaseState));
        }

        chaseState.AddTransition(new Transition(() => isStaggered, null, staggerState));
        attackState.AddTransition(new Transition(() => !IsInRange(), null, chaseState));
        attackState.AddTransition(new Transition(() => isStaggered, null, staggerState));
        staggerState.AddTransition(new Transition(IsStaggerOver, null, chaseState));
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

        // Tick the flee delay while the player is inside the avoidance zone.
        // Reset it as soon as they step out, so the countdown only counts
        // continuous exposure — not accumulated time across separate encounters.
        if (IsInAvoidanceRange())
            fleeDelayTimer += Time.deltaTime;
        else
            fleeDelayTimer = 0f;
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
        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    protected abstract void Attack();

    private void StopAttacking()
    {
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
        fleeDelayTimer = 0f; // reset so returning to chase doesn't instantly flee again
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

    // Returns true only after the enemy has been inside the avoidance zone
    // for at least fleeDelay seconds. The timer is ticked by Chase() and
    // TickFleeTimer() so both the chase and attack states contribute.
    private bool ShouldFlee()
    {
        if (!IsInAvoidanceRange())
        {
            fleeDelayTimer = 0f;
            return false;
        }
        return fleeDelayTimer >= fleeDelay;
    }

    // Called every frame by the attack state action so the flee delay also
    // accumulates while the enemy is standing still and shooting.
    protected void TickFleeTimer()
    {
        if (IsInAvoidanceRange())
            fleeDelayTimer += Time.deltaTime;
        else
            fleeDelayTimer = 0f;
    }

    protected bool CheckIfCanProceed()
    {
        if (animator.IsInTransition(0)) return false;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return !stateInfo.IsName("Attack") && !stateInfo.IsName("Hit");
    }

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