using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Zombie : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float HP = 100f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackCooldown = 2f;

    [Header("Movement")]
    [SerializeField] private float stepInterval = 0.1f;
    [SerializeField] private float strideLength = 1.2f;

    private float nextStepTime = 0f;
    private float lastAttackTime = 0f;
    private int attackToggle = 0;

    private Animator animator;
    private NavMeshAgent agent;
    private Transform player;
    private bool hasGivenEXP = false;
    private Coroutine knockbackRoutine;

    //healthpack
    [SerializeField] private GameObject healthPackPrefab;
    [SerializeField] private float healthPackDropChance = 0.05f;

    //ammo pack
    [SerializeField] private GameObject AmmoPackPrefab;
    [SerializeField] private float AmmoPackDropChance = 0.05f;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("No player found! Make sure your player has the 'Player' tag.");
        }

        agent.speed = strideLength / stepInterval;

        //Anti-clumping 
        agent.radius = 1f;
        agent.avoidancePriority = Random.Range(20, 80); // Random priority helps spreading
    }

    void Update()
    {
        if (player == null || HP <= 0) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            HandleMovement();
        }
        else
        {
            HandleAttack();
        }
    }

    private void HandleMovement()
    {
        animator.SetBool("isRunning", true);
        animator.SetBool("isAttacking", false);

        if (Time.time >= nextStepTime)
        {
            //Anti-clump offset
            Vector3 offset = Random.insideUnitSphere * 0.3f;
            offset.y = 0;

            Vector3 target = player.position + offset;
            agent.SetDestination(target);

            Debug.DrawLine(transform.position, agent.destination, Color.green, stepInterval);

            nextStepTime = Time.time + stepInterval;
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

    void AttackPlayer()
    {
        Debug.Log("Zombie attacks the player!");

        animator.SetInteger("attackIndex", attackToggle);
        attackToggle = 1 - attackToggle;

        if (attackToggle == 1)
        {
            SoundManager.Instance.zombieSound.Play();
        }

        PlayerStats playerHealth = player.GetComponent<PlayerStats>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
        else
        {
            Debug.LogError("PlayerHealth component is missing!");
        }
    }

    public void TakeDamage(float damageAmount, Vector3 hitSource)
    {
        HP -= damageAmount;

        animator.SetTrigger("Hit");

        if (knockbackRoutine != null) StopCoroutine(knockbackRoutine);
        knockbackRoutine = StartCoroutine(SmoothKnockback(hitSource));

        if (HP <= 0)
        {
            Die();
        }
    }

    private IEnumerator SmoothKnockback(Vector3 hitSource)
    {
        Vector3 start = transform.position;
        Vector3 direction = (transform.position - hitSource);
        direction.y = 0f;
        direction.Normalize();

        Vector3 target = start + direction * 2f;

        float duration = 0.3f;
        float elapsed = 0f;

        agent.isStopped = true;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!animator.GetBool("isDead"))
        {
            transform.position = target;
            agent.isStopped = false;
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
            stats.UpdateTypeUI("zombie");
        }

        TryDropHealthPack();
        TryDropAmmoPack();

        hasGivenEXP = true;
        Destroy(gameObject, 1.5f);
    //    Debug.Log("Zombie died!");
    }

    private void TryDropHealthPack()
    {
        if (healthPackPrefab != null && Random.value < healthPackDropChance)
        {
            Instantiate(healthPackPrefab, transform.position, Quaternion.identity);
           // Debug.Log("Health Pack Dropped!");
        }
    }

    private void TryDropAmmoPack()
    {
        if (AmmoPackPrefab != null && Random.value < AmmoPackDropChance)
        {
            Instantiate(AmmoPackPrefab, transform.position, Quaternion.identity);
           // Debug.Log("Ammo Pack Dropped!");
        }
    }
}
