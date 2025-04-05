using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class TankMiniBoss : MonoBehaviour
{
    [Header("Boss Stats")]
    [SerializeField] private float HP = 500f;
    [SerializeField] private float damage = 50f;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float attackCooldown = 4f;
    [SerializeField] private float chargeDamage = 75f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float chargeSpeed = 25f;
    [SerializeField] private float chargeDuration = 2f;
    [SerializeField] private float chargeCooldown = 10f;

    private float lastAttackTime = 0f;
    private float lastChargeTime = 0f;
    private bool isCharging = false;
    private bool hasGivenEXP = false;

    private Animator animator;
    private NavMeshAgent agent;
    private Transform player;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (animator == null) Debug.Log("animator not found");
        if (agent == null) Debug.Log("agent not found");
        player = GameObject.FindWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("No player with tag 'Player' found!");
        }

        agent.speed = moveSpeed;
        Debug.Log($"Tank MiniBoss spawned: {gameObject.name}");
    }

    void Update()
    {
        if (player == null || HP <= 0) return;

        float distance = Vector3.Distance(transform.position, player.position);


        if (!isCharging)
        {
            if (distance > attackRange)
            {
              //  Debug.Log($"distance = {distance}");
                HandleMovement();
            }
            else
            {
                HandleAttack();
            }
        }
        
        if (Time.time - lastChargeTime >= chargeCooldown && !isCharging)
        {
            Debug.Log($"last charge time {lastChargeTime}, going to charge!");
            StartCoroutine(ChargeAttack());
            return;
        }
    }

    private void HandleMovement()
    {
        animator.SetBool("isRunning", true);
        animator.SetBool("isAttacking", false);

        Debug.Log("is handling movement");
        if (!agent.isStopped)
        {
         //   Debug.Log("agent hasn't stopped");
            agent.SetDestination(player.position);
        } 
    }

    private void HandleAttack()
    {
        animator.SetBool("isAttacking", true);

        if (Time.time - lastAttackTime > attackCooldown)
        {
            AttackPlayer();
            lastAttackTime = Time.time;
        }
    }

    private void AttackPlayer()
    {
        Debug.Log("Tank attacks the player");
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.TakeDamage(damage);
        }
        else
        {
            Debug.LogError("PlayerStats not found on player.");
        }
    }

    private IEnumerator ChargeAttack()
    {
        isCharging = true;
        agent.isStopped = true;
        animator.SetTrigger("Shout");
        animator.SetBool("isRunning", false);
      //  Debug.Log("🦍 Tank MiniBoss is preparing to charge!");
        yield return new WaitForSeconds(1f); // Delay before charge
        agent.isStopped = false;
        animator.SetBool("isCharging", true);
        animator.SetBool("isRunning", false);
        moveSpeed = 14f;
        agent.speed = chargeSpeed;
        agent.acceleration = 15f;
        float chargeEndTime = Time.time + chargeDuration;
        agent.SetDestination(player.position);
        while (Time.time < chargeEndTime)
        {
            yield return null;
        }

        // Reset after charging
        animator.SetBool("isCharging", false);
        animator.SetBool("isRunning", true);
        moveSpeed = 10f;
        agent.speed = moveSpeed;
        agent.acceleration = 12f;
        lastChargeTime = Time.time;
        isCharging = false;
       
    }

    public void TakeDamage(float damageAmount)
    {
        if (HP <= 0) return;

        HP -= damageAmount;
        animator.SetTrigger("Hit");
        Debug.Log($"{gameObject.name} took {damageAmount} damage. HP: {HP}");

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
        agent.enabled = false;

        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.UpdateTypeUI("miniBoss");
        }

        GameEvents.TriggerMiniBossDefeated();
        hasGivenEXP = true;

        Debug.Log($"{gameObject.name} defeated!");
        Destroy(gameObject, 1f);
    }
}
