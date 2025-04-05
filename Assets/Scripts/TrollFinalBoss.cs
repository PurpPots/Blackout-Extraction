using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using NUnit.Framework;

public class TrollFinalBoss : MonoBehaviour
{
    public enum BossPhase { Phase1, Phase2}

    [Header("Stats")]
    [SerializeField] private BossHealthBarUI bossHealthUI;

    [SerializeField] private float HP = 300f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private float maxHP = 300f;

    [Header("Stomp Attack")]
    [SerializeField] private float stompRange = 100f;
    [SerializeField] private float stompDamage = 40f;
    [SerializeField] private float stompCooldown = 7f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;

    [Header("Phase Thresholds")]
    [SerializeField] private float phase2Threshold = 0.5f;

    private BossPhase currentPhase = BossPhase.Phase1;
    private bool isVulnerable = true;

    private float lastAttackTime = 0f;
    private float lastStompTime = Mathf.NegativeInfinity;
    private int attackToggle = 0;

    private Animator animator;
    private NavMeshAgent agent;
    private Transform player;
    private bool hasGivenEXP = false;
    private Coroutine damageWindowRoutine = null;
    private bool isDealingDamage = false;
    private bool isCurrentlyAttacking = false;
    private bool isAngry = false;
    private bool isCharging = false;
    private bool isStompping = false;




    private void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;

        if (bossHealthUI == null)
        {
            bossHealthUI = FindFirstObjectByType<BossHealthBarUI>();
            if (bossHealthUI == null)
            {
                Debug.LogWarning("BossHealthUI not found in scene.");
            }
        }

        if (bossHealthUI != null)
        {
            bossHealthUI.ShowBar();
            bossHealthUI.SetHealth(HP, maxHP);
        }



        player = GameObject.FindWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("No player with tag 'Player' found.");
        }

        Debug.Log("TrollFinalBoss initialized.");
    }

    private void Update()
    {
        if (player == null || HP <= 0) return;

        CheckPhaseTransition();

        switch (currentPhase)
        {
            case BossPhase.Phase1:
                Phase1Behavior();
                break;
            case BossPhase.Phase2:
                Phase2Behavior();
                break;
        }
    }

    // Phase logic
    private void CheckPhaseTransition()
    {
        float hpRatio = HP / maxHP;
        if (hpRatio <= phase2Threshold && currentPhase == BossPhase.Phase1)
        {
            currentPhase = BossPhase.Phase2;
            Debug.Log("Entered Phase 2!");
        }
    }


    //Phase 1: Track player and melee
    private void Phase1Behavior()
    {
        isVulnerable = true;
        if (isAngry) {agent.speed = moveSpeed * 1.7f;}
        else {agent.speed = moveSpeed;}

        float distance = Vector3.Distance(transform.position, player.position);
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float facingAngle = Vector3.Angle(transform.forward, directionToPlayer);

        bool inRange = distance <= attackRange;
        bool facingPlayer = facingAngle <= 45f;

        if (!isCurrentlyAttacking && (!inRange || !facingPlayer))
            {
                // Keep moving & turning until in range and facing
                agent.isStopped = false;
                agent.SetDestination(player.position);

                Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

                animator.SetBool("isWalking", true);
                animator.SetBool("isAttacking", false);
            }
        else if (!isCurrentlyAttacking && inRange && facingPlayer)
            {
                // Ready to attack
                agent.isStopped = true;
                isAngry = false;
                animator.SetBool("isWalking", false);
                HandleAttack();
            }
    }



    // Phase 2: Includes stomp charge
    private void Phase2Behavior()
    {

        // Adjust speed depending on whether it's charging
        agent.speed = isCharging ? moveSpeed * 10f : moveSpeed;

        //Handle stomp cooldown
        if (Time.time - lastStompTime >= stompCooldown && !isStompping)
        {
            StartCoroutine(ChargeAndStomp());
            lastStompTime = Time.time;
        }
        else if (!isStompping)
        {
            isVulnerable = true;
            Phase1Behavior();
            isVulnerable = false;
        }
    }

    private void HandleAttack()
    {
        animator.SetBool("isAttacking", true);
        animator.SetBool("isWalking", false);

        if (damageWindowRoutine == null)
        {
            damageWindowRoutine = StartCoroutine(PerformMeleeAttack());
        }
    }


    private IEnumerator PerformMeleeAttack()
    {
        isCurrentlyAttacking = true;
        agent.isStopped = true;

        animator.SetInteger("attackIndex", attackToggle);

        // Wait before dealing damage (sync with animation swing)
        yield return new WaitForSeconds(0.8f);

        StartCoroutine(FlashDamageGizmo());
        // Apply damage only once
        float attackRadius = attackRange; // or use a different AOE if needed
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRadius);
        Vector3 forward = transform.forward;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            Vector3 directionToTarget = (hit.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(forward, directionToTarget);

            if (angle <= 90f) // Front-facing cone
            {
                // Deal damage
                PlayerStats stats = hit.GetComponentInParent<PlayerStats>();
                if (stats != null)
                {
                    stats.TakeDamage(damage);
                    Debug.Log("Player hit by melee swing (frontal cone)!");
                }

                // Knockback
                PlayerMovement movement = hit.GetComponentInParent<PlayerMovement>();
                if (movement != null)
                {
                    Debug.Log("PlayerMovement found (via parent), applying knockback!");
                    Vector3 knockDir = (hit.transform.position - transform.position).normalized;
                    movement.ApplyKnockback(knockDir, 90f, 50f);
                }
                else
                {
                    Debug.LogWarning("PlayerMovement not found in parent!");
                }
            }
        }

        attackToggle = 1 - attackToggle;

        // Optional wait for animation to finish before resuming
        yield return new WaitForSeconds(0.6f);

        agent.isStopped = false;
        isCurrentlyAttacking = false;
        damageWindowRoutine = null;
    }

    private IEnumerator ChargeAndStomp()
    {
        isStompping = true;
        isVulnerable = true;
        agent.isStopped = true;
        isCharging = true;

        //Save the player's position at time of decision
        Vector3 targetPosition = player.position;
        Debug.Log("Charging stomp... Target locked at " + targetPosition);

        //Small pause before charging (optional)
        yield return new WaitForSeconds(0.5f);

        //Charge at the saved position
        animator.SetBool("isRunning", true);
        agent.isStopped = false;
        agent.SetDestination(targetPosition);

        //Wait until close enough to the saved position
        while (Vector3.Distance(transform.position, targetPosition) > 1.5f)
        {
            if (!agent.hasPath || agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                Debug.LogWarning("Path to charge target invalid. Breaking out of loop.");
                break;
            }
            yield return null;
        }

        //stop and perform stomp
        agent.isStopped = true;
        animator.SetBool("isRunning", false);
        animator.SetTrigger("Stomp");

        yield return new WaitForSeconds(0.3f); // Wind-up time before impact
        animator.SetBool("isRunning", true);

        //PerformStompImpact();

        //yield return new WaitForSeconds(0.8f); // Recovery
        agent.isStopped = false;
        isVulnerable = false;
        isStompping = false;
    }



    public void PerformStompImpact()
    {
        Debug.Log("STOMP IMPACT");

        Collider[] hits = Physics.OverlapSphere(transform.position, stompRange);
        Vector3 forward = transform.forward;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            Vector3 directionToTarget = (hit.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(forward, directionToTarget);

                // Deal damage
                PlayerStats stats = hit.GetComponentInParent<PlayerStats>();
                if (stats != null)
                {
                    stats.TakeDamage(stompDamage);
                    Debug.Log("Player took STOMP damage");
                }

                // Knockback
                PlayerMovement movement = hit.GetComponentInParent<PlayerMovement>();
                if (movement != null)
                {
                    Vector3 knockDir = directionToTarget;
                    movement.ApplyKnockback(knockDir, 60f, 100f); // adjust force and lift
                    Debug.Log("Stomp knockback applied to player");
                }
                else
                {
                    Debug.LogWarning("PlayerMovement not found in parent during stomp");
                }
        
        }
    }

    public void TakeDamage(float amount)
    {
        if (!isVulnerable)
        {
            Debug.Log("Boss is invulnerable!");
            return;
        }
        if (bossHealthUI != null)
        {
            bossHealthUI.SetHealth(HP, maxHP);
        }

        if(!isAngry) {animator.SetTrigger("Hit");}
        isAngry = true;

        HP -= amount;

        Debug.Log($"Troll took {amount} damage! Remaining HP: {HP}");

        if (HP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (hasGivenEXP) return;

        animator.SetBool("isDead", true);
        agent.isStopped = true;

        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.UpdateTypeUI("miniBoss");
        }

        hasGivenEXP = true;
        GameEvents.TriggerFinalBossDefeated();
        if (bossHealthUI != null)
        {
            bossHealthUI.HideBar();
        }


        Destroy(gameObject, 2f);
        Debug.Log("Troll boss defeated!");
    }

    private IEnumerator FlashDamageGizmo()
    {
        isDealingDamage = true;
        yield return new WaitForSeconds(0.2f);
        isDealingDamage = false;
    }


    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !animator) return;

        // Melee AOE - Yellow when attacking, Red when damage hits
        if (animator.GetBool("isAttacking"))
        {
            Gizmos.color = isDealingDamage ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }

        // Stomp AOE - Cyan when charging or stomping
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("jump"))
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, stompRange);
        }
    }



}
