using UnityEngine;

public class Slime : MonoBehaviour
{
    public int maxHealth = 1;

    public bool facingLeft = true;
    public float moveSpeed = 2f;
    public Transform checkPoint;
    public float distance = 2f;
    public LayerMask layerMask;
    public Transform player;
    public Animator animator;
    public float attackRadius = 1f;
    public float retrieveDistance = 3f;
    public float chaseSpeed = 2f;
    public float attackRange = 8f;
    public bool inRange = false;

    public Transform attackPoint;
    public LayerMask attackLayer;

    void Start()
    {
        
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
        }
        else
        {
            inRange = false;
        }

        if (inRange)
        {
            if (player.position.x > transform.position.x && facingLeft == true)
            {
                transform.eulerAngles = new Vector3(0, -180, 0);
                facingLeft = false;
            }
            else if (player.position.x < transform.position.x && facingLeft == false)
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
            transform.Translate(Vector2.left * Time.deltaTime * moveSpeed);

            RaycastHit2D hit = Physics2D.Raycast(checkPoint.position, Vector2.down, distance, layerMask);

            if (hit == false && facingLeft)
            {
                transform.eulerAngles = new Vector3(0, -180, 0);
                facingLeft = false;
            }
            else if (hit == false && facingLeft == false)
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
                facingLeft = true;
            }
        }


    }

    public void Attack()
    {
        Collider2D collideInfo = Physics2D.OverlapCircle(attackPoint.position, attackRadius, attackLayer);

        if (collideInfo)
        {
            Debug.Log("Collision");
            if (collideInfo.gameObject.GetComponent<Player>() != null)
            {
                collideInfo.gameObject.GetComponent<Player>().TakeDamage(1);
            }
        }
        else
        {
            Debug.Log("No collision");
        }
    }

    public void TakeDamage(int damage)
    {
        if (maxHealth <= 0) return;
        maxHealth -= damage;
    }

    //public void TriggerHurtAnimation()
    //{
    //    if (animator != null)
    //    {
    //        animator.SetTrigger("Hurt");
    //    }
    //}
    void Die()
    {
        Destroy(this.gameObject);
        //Debug.Log("Dead");
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
        {
            return;
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
