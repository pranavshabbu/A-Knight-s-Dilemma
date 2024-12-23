using JetBrains.Annotations;
using UnityEngine;

public class SwordEnemy : MonoBehaviour
{
    public int maxHealth = 3;

    public bool facingLeft = true;
    public float moveSpeed = 2f;
    public Transform checkPoint;
    public float distance = 2f;
    public LayerMask layerMask;
    public bool inRange = false;
    public Transform player;
    public float attackRange = 8f;
    public float retrieveDistance = 1f;
    public float chaseSpeed = 3f;
    public Animator animator;

    public Transform attackPoint;
    public float attackRadius = 1.4f;
    public LayerMask attackLayer;

    private bool isChasingPlayer = false;
    private bool isReturningToStart = false;
    private Vector2 startingPosition;

    void Start()
    {
        startingPosition = transform.position; // Save the initial position as the patrol area
    }

    void Update()
    {
        // Check for death
        if (maxHealth <= 0)
        {
            Die();
        }

        // Determine if the player is in attack range
        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            inRange = true;
            isChasingPlayer = true;
            isReturningToStart = false; // Stop returning if chasing the player
        }
        else
        {
            inRange = false;
        }

        if (isChasingPlayer)
        {
            HandleChasingPlayer();
        }
        else if (isReturningToStart)
        {
            ReturnToStartPosition();
        }
        else
        {
            Patrol();
        }
    }

    void HandleChasingPlayer()
    {
        if (inRange)
        {
            // Turn towards player
            if (player.position.x > transform.position.x && facingLeft)
            {
                transform.eulerAngles = new Vector3(0, -180, 0);
                facingLeft = false;
            }
            else if (player.position.x < transform.position.x && !facingLeft)
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
                facingLeft = true;
            }

            // Chase player
            if (Vector2.Distance(transform.position, player.position) > retrieveDistance)
            {
                animator.SetBool("Attack", false);
                transform.position = Vector2.MoveTowards(transform.position, player.position, chaseSpeed * Time.deltaTime);
            }
            else
            {
                animator.SetBool("Attack", true);
            }
        }
        else
        {
            // Stop chasing and start returning to start position
            animator.SetBool("Attack", false);
            isChasingPlayer = false;
            isReturningToStart = true;
        }
    }

    void ReturnToStartPosition()
    {
        // Move back to the starting position
        transform.position = Vector2.MoveTowards(transform.position, startingPosition, moveSpeed * Time.deltaTime);

        // Turn towards the starting position
        if (transform.position.x > startingPosition.x && facingLeft)
        {
            transform.eulerAngles = new Vector3(0, -180, 0);
            facingLeft = false;
        }
        else if (transform.position.x < startingPosition.x && !facingLeft)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
            facingLeft = true;
        }

        // Stop returning when the enemy reaches the starting position
        if (Vector2.Distance(transform.position, startingPosition) < 0.1f)
        {
            isReturningToStart = false;
        }
    }

    void Patrol()
    {
        transform.Translate(Vector2.left * Time.deltaTime * moveSpeed);

        // Use a raycast to check for patrol boundaries
        RaycastHit2D hit = Physics2D.Raycast(checkPoint.position, Vector2.down, distance, layerMask);

        if (hit == false && facingLeft)
        {
            transform.eulerAngles = new Vector3(0, -180, 0);
            facingLeft = false;
        }
        else if (hit == false && !facingLeft)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
            facingLeft = true;
        }
    }

    public void Attack()
    {
        Collider2D collideInfo = Physics2D.OverlapCircle(attackPoint.position, attackRadius, attackLayer);

        if (collideInfo)
        {
            if (collideInfo.gameObject.GetComponent<Player>() != null)
            {
                collideInfo.gameObject.GetComponent<Player>().TakeDamage(1);
            }
        }
    }

    public void TriggerHurtAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }
    }

    public void TakeDamage(int damage)
    {
        if (maxHealth <= 0) return;
        maxHealth -= damage;
    }

    void Die()
    {
        Destroy(this.gameObject);
    }
}
