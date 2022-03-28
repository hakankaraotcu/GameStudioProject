using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private Animator anim;
    private Collider2D coll;
    public int healthPotion;
    private Vector2 moveDir;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private LayerMask ground;
    [SerializeField] private int maxhealth = 1000;
    [SerializeField] private int currentHealth;
    [SerializeField] private int previousHealth;
    [SerializeField] private float speed = 8f;
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float hurtForce = 7f;
    [SerializeField] private int damage = 40;
    [SerializeField] private int getDamage = 0;
    [SerializeField] private float attackRate = 2f;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private float nextAttackTime = 0f;
    [SerializeField] private float dashForce;
    [SerializeField] private float startDashTimer;
    [SerializeField] private float currentDashTimer;
    [SerializeField] private float dashDirection;
    private bool isDashing;
    private float rollDir;
    private float dirX;
    private enum State { idle, running, jumping, falling, rolling};
    private State state = State.idle;

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        currentHealth = maxhealth;
        previousHealth = currentHealth;
    }

    // Update is called once per frame
    private void Update()
    {
        if((Time.time >= nextAttackTime) && (state == State.idle || state == State.running || state == State.jumping || state == State.falling || state == State.rolling))
        {
            Movement();
        }
        
        AnimState();
        anim.SetInteger("state", (int)state);
    }

    private void Movement()
    {
        dirX = Input.GetAxisRaw("Horizontal");
        moveDir = new Vector2(dirX * speed, rb.velocity.y);
        rb.velocity = moveDir;

        if (dirX < 0)
        {
            transform.localScale = new Vector2(-3, 3);
        }
        else if (dirX > 0)
        {
            transform.localScale = new Vector2(3, 3);
        }
        if(dirX != 0)
        {
            rollDir = dirX;
        }
        if(Input.GetKeyDown(KeyCode.LeftControl) && coll.IsTouchingLayers(ground))
        {
            isDashing = true;
            currentDashTimer = startDashTimer;
            rb.velocity = Vector2.zero;
            if(dirX != 0)
            {
                dashDirection = dirX;
            }
            else
            {
                dashDirection = rollDir;
            }
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
            rb.constraints = RigidbodyConstraints2D.FreezePositionY;
        }
        if (Input.GetButtonDown("Jump") && coll.IsTouchingLayers(ground))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            state = State.jumping;
            isDashing = false;
        }
        if (isDashing)
        {
            state = State.rolling;
            rb.velocity = transform.right * dashDirection * dashForce;
        }
        else if (Time.time >= nextAttackTime && state == State.idle || state == State.running)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                Attack();
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }
    }

    public void Attack()
    {
        anim.SetTrigger("attack");

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<Skeleton>().TakeDamage(damage);
        }
    }

    public void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Collectible")
        {
            Destroy(collision.gameObject);
            healthPotion += 1;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if(other.gameObject.tag == "Enemy")
        {
            anim.SetTrigger("hurt");

            if(other.gameObject.transform.position.x > transform.position.x)
            {
                if (coll.IsTouchingLayers(ground))
                {
                    rb.velocity = new Vector2(-hurtForce, rb.velocity.y);
                }
                else
                {
                    rb.velocity = new Vector2(-hurtForce, 10f);
                }
            }
            else
            {
                if (coll.IsTouchingLayers(ground))
                {
                    rb.velocity = new Vector2(hurtForce, rb.velocity.y);
                }
                else
                {
                    rb.velocity = new Vector2(hurtForce, 10f);
                }
            }
        }
    }

    private void AnimState()
    {
        if (state == State.jumping)
        {
            if (rb.velocity.y < 0.1f)
            {
                state = State.falling;
            }
        }
        else if(state == State.rolling)
        {
            currentDashTimer -= Time.deltaTime;
            if (currentDashTimer <= 0)
            {
                isDashing = false;
                if(dirX != 0)
                {
                    state = State.running;
                    gameObject.GetComponent<BoxCollider2D>().enabled = true;
                    rb.constraints = RigidbodyConstraints2D.None;
                    rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                }
                else
                {
                    state = State.idle;
                    gameObject.GetComponent<BoxCollider2D>().enabled = true;
                    rb.constraints = RigidbodyConstraints2D.None;
                    rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                }
            }
            
        }
        else if (state == State.falling)
        {
            if (coll.IsTouchingLayers(ground))
            {
                state = State.idle;
            }
        }
        else if (Mathf.Abs(rb.velocity.x) > Mathf.Epsilon)
        {
            state = State.running;
        }
        else if (state == State.idle)
        {
            if (!coll.IsTouchingLayers(ground))
            {
                state = State.falling;
            }
        }
        else
        {
            state = State.idle;
        }
    }

    public void TakeDamage(int damage)
    {
        previousHealth = currentHealth;
        getDamage = damage;
        currentHealth -= getDamage;

        anim.SetTrigger("hurt");

        if (currentHealth <= 0)
        {
            Debug.Log("died");
            anim.SetBool("isDead", true);
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            GetComponent<Collider2D>().enabled = false;
        }
    }
}
