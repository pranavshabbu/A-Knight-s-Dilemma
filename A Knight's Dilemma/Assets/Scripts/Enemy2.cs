using Unity.VisualScripting;
using UnityEngine;

public class Enemy2 : MonoBehaviour
{
    public int maxHealth = 5;
    public static int poiseval = 3;
    public int poise = poiseval;

    public bool facingLeft = true;
    public float moveSpeed = 2f;
    public Transform checkPoint;
    public float distance = 2f;
    public LayerMask layerMask;
    public bool inRange = false;
    public Transform player;
    public float attackRange = 8f;
    public float retrieveDistance = 2.5f;
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
        startingPosition = transform.position; 
    }

    void Update()
    {
        if (maxHealth <= 0)
        {
            Die();
        }

        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            inRange = true;
            isChasingPlayer = true;
            isReturningToStart = false; 
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
            animator.SetBool("Attack", false);
            isChasingPlayer = false;
            isReturningToStart = true;
        }
    }

    void ReturnToStartPosition()
    {
        transform.position = Vector2.MoveTowards(transform.position, startingPosition, moveSpeed * Time.deltaTime);

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

        if (Vector2.Distance(transform.position, startingPosition) < 0.1f)
        {
            isReturningToStart = false;
        }
    }

    void Patrol()
    {
        transform.Translate(Vector2.left * Time.deltaTime * moveSpeed);

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
        poise--;
        if (poise == 0)
        {
            poise = poiseval;
            if (animator != null)
            {
                animator.SetTrigger("Hurt");
            }
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

    // private void OnDrawGizmosSelected()
    // {
    //     if (attackPoint == null)
    //     {
    //         return;
    //     }
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    // }
}
