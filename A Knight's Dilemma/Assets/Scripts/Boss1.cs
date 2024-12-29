using UnityEngine;
using UnityEngine.SceneManagement;

public class Boss : MonoBehaviour
{
    public int poise = 3; 
    public int poiseVal = 3; 
    private bool isHurt = false;
    public int maxHealth = 20;

    public Transform player;
    public float chaseRange = 10f; 
    public float attackRange = 2f; 
    public float chaseSpeed = 3f; 

    public Transform attackPoint1;
    public Transform attackPoint2;
    public float attackRadius1 = 1.8f;
    public float attackRadius2 = 1.5f;
    public LayerMask attackLayer;

    public Animator animator; 
    public bool facingLeft = true; 

    private string[][] movesets =
    {
        new string[] { "Attack2" },
        new string[] { "Attack1", "Attack1", "Attack1" },
        new string[] { "Attack1", "Attack1", "Attack2" },
        new string[] { "Attack2", "Attack1", "Attack1" }
    };

    private bool isPerformingCombo = false;

    public GameObject gameOverPanel;

    private bool isTakingNoDamage = true;
    public float noDamageTimer = 0f;
    private bool timerActive = false;

    public GameObject alternateGameOverPanel; 
    private bool alternateEndingDisabled = false;


    void Update()
    {
        if (maxHealth <= 0)
        {
            Die();
            return;
        }

        if (timerActive && !alternateEndingDisabled)
        {
            if (isTakingNoDamage)
            {
                noDamageTimer += Time.deltaTime;
                if (noDamageTimer >= 12f)
                {
                    TriggerAlternateGameOver();
                }
            }
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!isHurt)
        {
            if (distanceToPlayer <= chaseRange && distanceToPlayer > attackRange)
            {
                animator.SetBool("Attack1", false);
                animator.SetBool("Attack2", false);

                StopAllCoroutines(); 
                isPerformingCombo = false;
                ChasePlayer();
            }
            else if (distanceToPlayer <= attackRange)
            {
                StartAttack();
            }
            else
            {
                animator.SetBool("Run", false);
                animator.SetBool("Attack1", false);
                animator.SetBool("Attack2", false);

                StopAllCoroutines();
                isPerformingCombo = false;
            }
        }
    }


    private void ChasePlayer()
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

        animator.SetBool("Run", true); 
        transform.position = Vector2.MoveTowards(transform.position, player.position, chaseSpeed * Time.deltaTime);
    }

    private void StartAttack()
    {
        animator.SetBool("Run", false); 
        if (!timerActive)
        {
            timerActive = true;
            noDamageTimer = 0f;
        }
        if (!isPerformingCombo)
        {
            PerformRandomMoveset();
        }
    }

    private void PerformRandomMoveset()
    {
        if (isPerformingCombo) return;

        string[] selectedMoveset = movesets[Random.Range(0, movesets.Length)];

        StartCoroutine(PerformCombo(selectedMoveset));
    }

    private System.Collections.IEnumerator PerformCombo(string[] moveset)
    {
        isPerformingCombo = true;

        foreach (var attack in moveset)
        {
            if (isHurt)
            {
                isPerformingCombo = false;
                yield break; 
            }

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer > attackRange)
            {
                Debug.Log("Player moved out of attack range; interrupting combo.");
                isPerformingCombo = false;
                yield break;
            }

            animator.SetBool(attack, true);
            Debug.Log($"Boss performs: {attack}");

            yield return new WaitForSeconds(GetCurrentAnimationLength(attack));

            animator.SetBool(attack, false);
        }

        isPerformingCombo = false;
    }

    public void PerformAttack()
    {
        Transform currentAttackPoint = animator.GetCurrentAnimatorStateInfo(0).IsName("Attack1") ? attackPoint1 : attackPoint2;
        float currentAttackRadius = animator.GetCurrentAnimatorStateInfo(0).IsName("Attack1") ? attackRadius1 : attackRadius2;

        Collider2D collideInfo = Physics2D.OverlapCircle(currentAttackPoint.position, currentAttackRadius, attackLayer);

        if (collideInfo && collideInfo.gameObject.GetComponent<Player>() != null)
        {
            collideInfo.gameObject.GetComponent<Player>().TakeDamage(1);
        }
    }

    public void TriggerHurtAnimation()
    {
        poise--;
        if (poise <= 0)
        {
            poise = poiseVal; 
            isHurt = true;    
            isPerformingCombo = false; 
            animator.SetTrigger("Hurt"); 
            Debug.Log("Boss is staggered!");

            StopAllCoroutines(); 
            StartCoroutine(ResumeAttackAfterHurt());
        }
    }

    private System.Collections.IEnumerator ResumeAttackAfterHurt()
    {
        yield return new WaitForSeconds(GetCurrentAnimationLength("Poise")); 
        
        isHurt = false; 
        isPerformingCombo = false; 
    }

    private float GetCurrentAnimationLength(string animationName)
    {
        var clips = animator.runtimeAnimatorController.animationClips;
        foreach (var clip in clips)
        {
            if (clip.name == animationName)
            {
                return clip.length;
            }
        }
        Debug.LogWarning($"Animation {animationName} not found.");
        return 0.5f; 
    }

    public void TakeDamage(int damage)
    {
        if (maxHealth <= 0) return;
        maxHealth -= damage;
        alternateEndingDisabled = true;
        isTakingNoDamage = false;
        timerActive = false;
        noDamageTimer = 0f;
    }

    public void Die()
    {
        animator.SetTrigger("Dead");
        StartCoroutine(WaitForDeathAnimation());
    }

    private System.Collections.IEnumerator WaitForDeathAnimation()
    {
        yield return new WaitForSeconds(GetCurrentAnimationLength("Death") * 0.8f);
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void TriggerAlternateGameOver()
    {
        int i = 5;
        animator.SetTrigger("AltGameOver");
        animator.SetBool("Attack1", false);
        animator.SetBool("Attack2", false);
        animator.SetBool("Run", false);
        animator.ResetTrigger("Hurt");
        while (i >= 0)
        {
            i--;
        }
        alternateGameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResetScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint1 != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint1.position, attackRadius1);
        }

        if (attackPoint2 != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(attackPoint2.position, attackRadius2);
        }
    }
}
