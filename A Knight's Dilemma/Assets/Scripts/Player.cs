using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private Boss boss;

    public int maxHealth = 5;
    public Text health;
    public Animator animator;
    public Rigidbody2D rb;
    public float jumpHeight = 5f;
    public bool onGround = true ;

    public float movement;
    public float moveSpeed = 7f;
    private bool facingRight = true;

    public Transform attackPoint;
    public float attackRadius = 1.4f;
    public LayerMask attackLayer;

    private bool checkpointReached = false;
    private float checkpointX = 0f;
    private float checkpointY = 0f;

    public AudioClip attackSound;
    public AudioClip parrySound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        checkpointX = PlayerPrefs.GetFloat("CheckpointX", 0f); 
        checkpointY = PlayerPrefs.GetFloat("CheckpointY", 0f);
    }

    void Update()
    {
        if(maxHealth <= 0)
        {
            Die();
        }

        health.text = maxHealth.ToString();

        movement = Input.GetAxis("Horizontal"); 

        if(movement < 0f && facingRight)
        {
            transform.eulerAngles = new Vector3(0f, -180, 0f);
            facingRight = false;
        }

        else if(movement > 0f && !facingRight)
        {
            transform.eulerAngles = new Vector3(0f, 0f, 0f);
            facingRight = true; 
        }

        if (Input.GetKey(KeyCode.Space) && onGround)
        {
            Jump(); 
            onGround = false;
            animator.SetBool("Jump", true);
        }

        if(Mathf.Abs(movement) > 0f)
        {
            animator.SetFloat("Run", 1f);
        }

        else if(movement < 0.1f)
        {
            animator.SetFloat("Run", 0f);
        }

        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("Attack");
        }

        if (Input.GetMouseButtonDown(1))
        {
            animator.SetTrigger("Parry");
        }
    }

    private void FixedUpdate()
    {
        transform.position += new Vector3(movement, 0f, 0f) * Time.fixedDeltaTime * moveSpeed;
    }

    void Jump()
    {
        rb.AddForce(new Vector2(0f, jumpHeight), ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            onGround = true;
            animator.SetBool("Jump", false);
        }

        if (collision.gameObject.tag == "Checkpoint")
        {
            onGround = true;
            animator.SetBool("Jump", false);

            checkpointReached = true; 

            checkpointX = transform.position.x;
            checkpointY = transform.position.y;

            Debug.Log("Checkpoint reached at: " + checkpointX + ", " + checkpointY);
        }
    }

    public void Attack()
    {
        audioSource.PlayOneShot(attackSound);

        Collider2D collideInfo = Physics2D.OverlapCircle(attackPoint.position, attackRadius, attackLayer);
        if (collideInfo)
        {
            if(collideInfo.gameObject.GetComponent<SwordEnemy>() != null)
            {
                collideInfo.gameObject.GetComponent<SwordEnemy>().TakeDamage(1);
            }
            if (collideInfo.gameObject.GetComponent<Enemy2>() != null)
            {
                collideInfo.gameObject.GetComponent<Enemy2>().TakeDamage(1);
            }
            if (collideInfo.gameObject.GetComponent<Slime>() != null)
            {
                collideInfo.gameObject.GetComponent<Slime>().TakeDamage(1);
            }
            if (collideInfo.gameObject.GetComponent<Boss>() != null)
            {
                collideInfo.gameObject.GetComponent<Boss>().TakeDamage(1);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        if (currentState.IsName("Player_Parry")) 
        {
            animator.SetTrigger("Parry_Success");

            audioSource.PlayOneShot(parrySound);

            Collider2D enemyCollider = Physics2D.OverlapCircle(attackPoint.position, attackRadius, attackLayer);
            if (enemyCollider != null)
            {
                SwordEnemy enemy = enemyCollider.GetComponent<SwordEnemy>();
                if (enemy != null)
                {
                    enemy.TriggerHurtAnimation();
                }

                Enemy2 enemy2 = enemyCollider.GetComponent<Enemy2>();
                if (enemy2 != null)
                {
                    enemy2.TriggerHurtAnimation();
                }

                Boss boss = enemyCollider.GetComponent<Boss>();
                if (boss != null)
                {
                    boss.TriggerHurtAnimation();
                }
            }

            Debug.Log("Parry successful! No damage taken.");
            return;
        }

        if (maxHealth <= 0) return; 
        maxHealth -= damage;
    }

    public void Die()
    {
        Debug.Log("Dead");
        animator.SetTrigger("Died");

        StartCoroutine(WaitForDeathAnimation());
    }

    private System.Collections.IEnumerator WaitForDeathAnimation()
    {
        yield return new WaitForSeconds(GetCurrentAnimationLength("Player_Death") * 0.8f);

        if (checkpointReached)
        {
            transform.position = new Vector3(checkpointX, checkpointY, transform.position.z);
            animator.Play("Player_Idle");
            maxHealth = 5;
            boss = FindFirstObjectByType<Boss>();
            boss.poise = boss.poiseVal;
            boss.maxHealth = 20;
            boss.noDamageTimer = 0f;
            Debug.Log("Respawned at checkpoint.");
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            Debug.Log("No checkpoint, resetting scene.");
        }
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
}
