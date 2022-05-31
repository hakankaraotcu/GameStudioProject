using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    public Animator anim, enemyAnim;
    private Collider2D coll;
    private Image image;
    private int timer;
    private IInteractable interactable;
    [SerializeField] private float colliderDistance;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private float range;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayers, bossLayers;
    [SerializeField] private LayerMask ground;
    [SerializeField] private LayerMask platform;
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
    public bool isAttacking = false;
    public bool isBlocking = false;
    public bool isMeditate = false;
    public bool isParrying = false;
    public bool isCounterAttack = false;
    public bool canMove = true;
    public static PlayerController instance;
    private float rollDir;
    private float dirX;
    private int playerLayer, platformLayer, enemyLayer, bossLayer, interactableLayer;
    private bool jumpOffCoroutineIsRunning = false;
    private enum State { idle, running, jumping, falling, rolling, climb};
    private State state = State.idle;

    public HealthBar healthBar;
    public StaminaBar staminaBar;

    public bool canClimb = false;
    public bool bottomLadder = false;
    public bool topLadder = false;
    public bool checkLadder = false;
    public Ladder ladder;
    private float naturalGravity;
    [SerializeField] float climbSpeed = 3f;

    [SerializeField] private GameObject tip, tip1;

    private int jumpCount = 0;
    public int extraJumps = 1;

    private static PlayerController player;

    private PlayerController()
    {
        if (player == null)
        {
            player = this;
        }
    }

    public static PlayerController GetInstance()
    {
        return player;
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        PermanentUI.GetInstance().currentHealth = PermanentUI.GetInstance().maxHealth;
        healthBar.SetMaxHealth(PermanentUI.GetInstance().maxHealth);
        PermanentUI.GetInstance().currentStamina = PermanentUI.GetInstance().maxStamina;
        staminaBar.SetMaxStamina(PermanentUI.GetInstance().maxStamina);
        PermanentUI.GetInstance().maxHealthPotion = 2;
        playerLayer = LayerMask.NameToLayer("Player");
        platformLayer = LayerMask.NameToLayer("Platform");
        enemyLayer = LayerMask.NameToLayer("Enemies");
        bossLayer = LayerMask.NameToLayer("Bosses");
        interactableLayer = LayerMask.NameToLayer("Interactable");
        naturalGravity = rb.gravityScale;
    }

    private void Update()
    {
        if (state == State.climb)
        {
            Climb();
        }
        else if (canMove)
        {
            Movement();
        }
        Attack();
        Block();
        Meditate();
        Interactable();

        AnimState();
        anim.SetInteger("state", (int)state);
    }

    private void Attack()
    {
        if(Input.GetKeyDown(KeyCode.F) && !isAttacking && (coll.IsTouchingLayers(ground) || coll.IsTouchingLayers(platform)))
        {
            isAttacking = true;
        }
        if (isCounterAttack)
        {
            if (Input.GetKeyDown(KeyCode.R) && enemyAnim.GetBool("isParrying"))
            {
                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
                if (hitEnemies.Length != 0)
                {
                    image.enabled = false;
                    anim.SetTrigger("specialAttack");
                    isCounterAttack = false;
                }
            }
            else if ((int)Time.time == timer && !anim.GetCurrentAnimatorStateInfo(0).IsName("SpecialAttack"))
            {
                enemyAnim.SetBool("isParrying", false);
                image.enabled = false;
                isCounterAttack = false;
            }
        }
    }

    private void Block()
    {
        if (Input.GetKey(KeyCode.E) && (coll.IsTouchingLayers(ground) || coll.IsTouchingLayers(platform)))
        {
            isBlocking = true;
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            isBlocking = false;
        }
    }

    private void DealtDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        Collider2D[] hitBoss = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, bossLayers);

        if(hitEnemies != null)
        {
            foreach (Collider2D enemy in hitEnemies)
            {
                enemy.transform.gameObject.SendMessage("TakeDamage", damage);
            }
        }
        if(hitBoss != null)
        {
            foreach(Collider2D boss in hitBoss)
            {
                boss.transform.gameObject.SendMessage("TakeDamage", damage);
            }
        }
    }

    private void CounterAttack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        if (hitEnemies != null)
        {
            foreach (Collider2D enemy in hitEnemies)
            {
                enemy.transform.gameObject.SendMessage("Executed");
            }
        }
    }

    private void Meditate()
    {
        if (Input.GetKeyDown(KeyCode.Q) && (coll.IsTouchingLayers(ground) || coll.IsTouchingLayers(platform)) && PermanentUI.GetInstance().currentHealth < PermanentUI.GetInstance().maxHealth && PermanentUI.GetInstance().currentStamina > 0)
        {
            isMeditate = true;
        }
        if (Input.GetKeyUp(KeyCode.Q))
        {
            isMeditate = false;
        }
    }

    private void Healing()
    {
        PermanentUI.GetInstance().currentHealth += (PermanentUI.GetInstance().maxHealth * 5) / 100;
        PermanentUI.GetInstance().currentStamina -= (PermanentUI.GetInstance().maxStamina * 5) / 100;
        if (PermanentUI.GetInstance().currentHealth > PermanentUI.GetInstance().maxHealth)
        {
            isMeditate = false;
            PermanentUI.GetInstance().currentHealth = PermanentUI.GetInstance().maxHealth;
        }
        else if(PermanentUI.GetInstance().currentStamina <= 0)
        {
            isMeditate = false;
            PermanentUI.GetInstance().currentStamina = 0;
        }
        healthBar.SetHealth(PermanentUI.GetInstance().currentHealth);
        staminaBar.SetStamina(PermanentUI.GetInstance().currentStamina);
    }

    private void AddLife()
    {
        PermanentUI.GetInstance().healthPotion -= 1;
        PermanentUI.GetInstance().potions[PermanentUI.GetInstance().healthPotion].gameObject.SetActive(false);
        PermanentUI.GetInstance().currentHealth += (PermanentUI.GetInstance().maxHealth * 25) / 100;
        if (PermanentUI.GetInstance().currentHealth > PermanentUI.GetInstance().maxHealth)
        {
            PermanentUI.GetInstance().currentHealth = PermanentUI.GetInstance().maxHealth;
        }
        healthBar.SetHealth(PermanentUI.GetInstance().currentHealth);
    }

    private void CheckMove()
    {
        canMove = !canMove;
    }

    private void Movement()
    {
        dirX = Input.GetAxis("Horizontal");

        if (canClimb && Mathf.Abs(Input.GetAxis("Vertical")) > .1f)
        {
            state = State.climb;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            transform.position = new Vector3(ladder.transform.position.x, rb.position.y);
            rb.gravityScale = 0f;
        }
        if (dirX < 0)
        {
            rb.velocity = new Vector2(dirX * speed, rb.velocity.y);
            transform.localScale = new Vector2(-3, 3);
        }
        else if (dirX > 0)
        {
            rb.velocity = new Vector2(dirX * speed, rb.velocity.y);
            transform.localScale = new Vector2(3, 3);
        }
        if(dirX != 0)
        {
            rollDir = dirX;
        }
        if(Input.GetKeyDown(KeyCode.LeftControl) && (coll.IsTouchingLayers(ground) || coll.IsTouchingLayers(platform)))
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
            boxCollider.offset = new Vector3(0.003989443f, -0.3499389f, 1);
            boxCollider.size = new Vector3(0.1551138f, 0.06578049f, 1);
            print(boxCollider.bounds);
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
            Physics2D.IgnoreLayerCollision(playerLayer, bossLayer, true);
        }
        if (Input.GetButtonDown("Jump") && ((((coll.IsTouchingLayers(ground) || isDashing) || (coll.IsTouchingLayers(platform) || isDashing))) || jumpCount < extraJumps) && !Input.GetKey(KeyCode.S))
        {
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
            Physics2D.IgnoreLayerCollision(playerLayer, bossLayer, false);
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpCount++;
            state = State.jumping;
            isDashing = false;
        }
        else if(Input.GetButtonDown("Jump") && ((coll.IsTouchingLayers(ground) || isDashing) || (coll.IsTouchingLayers(platform) || isDashing)) && Input.GetKey(KeyCode.S))
        {
            StartCoroutine("JumpOff");
        }
        if(state == State.jumping)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, platformLayer, true);
        }
        else if(state == State.falling && !jumpOffCoroutineIsRunning)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, platformLayer, false);
        }
        if (isDashing)
        {
            state = State.rolling;
            rb.velocity = transform.right * dashDirection * dashForce;
        }
        if (Input.GetKeyDown(KeyCode.T) && PermanentUI.GetInstance().healthPotion > 0 && PermanentUI.GetInstance().currentHealth < PermanentUI.GetInstance().maxHealth)
        {
            AddLife();
        }
    }

    private void Climb()
    {
        if (Input.GetButtonDown("Jump"))
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            canClimb = false;
            rb.gravityScale = 3;
            state = State.jumping;
            checkLadder = false;
            isDashing = false;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            return;
        }
        else if(Input.GetAxisRaw("Vertical") != 0 && topLadder && !checkLadder)
        {
            transform.position = new Vector2(transform.position.x, ladder.transform.position.y + 2);
            checkLadder = true;
        }
        else if (Input.GetAxisRaw("Vertical") == -1 && bottomLadder)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            canClimb = false;
            rb.gravityScale = 3;
            state = State.jumping;
            checkLadder = false;
            isDashing = false;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);
            return;
        }

        float vDirection = Input.GetAxis("Vertical");

        if (Input.GetAxisRaw("Vertical") == 1 && !topLadder)
        {
            rb.velocity = new Vector2(0f, vDirection * climbSpeed);
        }
        else if(Input.GetAxisRaw("Vertical") == -1 && !bottomLadder)
        {
            rb.velocity = new Vector2(0f, vDirection * climbSpeed);
        }
        else if (!canClimb)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            canClimb = false;
            rb.gravityScale = 3;
            state = State.jumping;
            checkLadder = false;
            isDashing = false;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);
            return;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    IEnumerator JumpOff()
    {
        jumpOffCoroutineIsRunning = true;
        Physics2D.IgnoreLayerCollision(playerLayer, platformLayer, true);
        yield return new WaitForSeconds (0.5f);
        Physics2D.IgnoreLayerCollision(playerLayer, platformLayer, false);
        jumpOffCoroutineIsRunning = false;
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
            if (PermanentUI.GetInstance().healthPotion < PermanentUI.GetInstance().maxHealthPotion)
            {
                Destroy(collision.gameObject);
                PermanentUI.GetInstance().potions[PermanentUI.GetInstance().healthPotion].gameObject.SetActive(true);
                PermanentUI.GetInstance().healthPotion += 1;
            }
        }
        if(collision.tag == "Interactable")
        {
            interactable = collision.GetComponent<IInteractable>();
        }
        if (collision.tag == "Shop")
        {
            interactable = collision.GetComponent<IInteractable>();
            tip.gameObject.SetActive(true);
        }
        if (collision.tag == "God")
        {
            interactable = collision.GetComponent<IInteractable>();
            tip1.gameObject.SetActive(true);
        }
        if (collision.tag == "Checkpoint")
        {
            interactable = collision.GetComponent<IInteractable>();
        }
        if (collision.tag == "Secret")
        {
            interactable = collision.GetComponent<IInteractable>();
        }
        if (collision.tag == "Trap")
        {
            PlayerRespawn.GetInstance().Respawn();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Interactable")
        {
            if(interactable != null)
            {
                interactable.StopInteract();
                interactable = null;
            }
        }
        if (collision.tag == "Shop")
        {
            if (interactable != null)
            {
                interactable.StopInteract();
                interactable = null;
                tip.gameObject.SetActive(false);
            }
        }
        if (collision.tag == "God")
        {
            if (interactable != null)
            {
                interactable.StopInteract();
                interactable = null;
                tip1.gameObject.SetActive(false);
            }
        }
        if (collision.tag == "Checkpoint")
        {
            if (interactable != null)
            {
                interactable.StopInteract();
                interactable = null;
            }
        }
        if (collision.tag == "Secret")
        {
            if (interactable != null)
            {
                interactable.StopInteract();
                interactable = null;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if(other.gameObject.tag == "Enemy")
        {
            anim.SetTrigger("hurt");
            PermanentUI.GetInstance().currentHealth -= 20;
            healthBar.SetHealth(PermanentUI.GetInstance().currentHealth);

            if (PermanentUI.GetInstance().currentHealth <= 0)
            {
                Die();
            }

            if(other.gameObject.transform.position.x > transform.position.x)
            {
                if (coll.IsTouchingLayers(ground) || coll.IsTouchingLayers(platform))
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
                if (coll.IsTouchingLayers(ground) || coll.IsTouchingLayers(platform))
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
        if(state == State.climb)
        {

        }
        else if (state == State.jumping)
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
                if(rb.velocity.y < .1f)
                {
                    state = State.falling;
                    boxCollider.offset = new Vector3(0.003989443f, -0.1246257f, 1);
                    boxCollider.size = new Vector3(0.1551138f, 0.5164068f, 1);
                    Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
                    Physics2D.IgnoreLayerCollision(playerLayer, bossLayer, false);
                }
                else if(dirX != 0)
                {
                    state = State.running;
                    boxCollider.offset = new Vector3(0.003989443f, -0.1246257f, 1);
                    boxCollider.size = new Vector3(0.1551138f, 0.5164068f, 1);
                    Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
                    Physics2D.IgnoreLayerCollision(playerLayer, bossLayer, false);
                }
                else
                {
                    state = State.idle;
                    boxCollider.offset = new Vector3(0.003989443f, -0.1246257f, 1);
                    boxCollider.size = new Vector3(0.1551138f, 0.5164068f, 1);
                    Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
                    Physics2D.IgnoreLayerCollision(playerLayer, bossLayer, false);
                }
            }
            
        }
        else if (state == State.falling)
        {
            if (coll.IsTouchingLayers(ground) || coll.IsTouchingLayers(platform))
            {
                state = State.idle;
                jumpCount = 0;
            }
        }
        else if (Mathf.Abs(rb.velocity.x) > 1f && (coll.IsTouchingLayers(ground) || coll.IsTouchingLayers(platform)))
        {
            state = State.running;
        }
        else if (state == State.idle)
        {
            if (!coll.IsTouchingLayers(ground) && !coll.IsTouchingLayers(platform))
            {
                state = State.falling;
            }
        }
        else
        {
            state = State.idle;
            canMove = true;
        }
    }

    public void NoCounterTakeDamage(int damage)
    {
        if (!isBlocking)
        {
            canMove = true;
            getDamage = damage;
            PermanentUI.GetInstance().currentHealth -= getDamage;
            healthBar.SetHealth(PermanentUI.GetInstance().currentHealth);

            anim.SetTrigger("hurt");

            if (PermanentUI.GetInstance().currentHealth <= 0)
            {
                Die();
            }

            else if (transform.localScale.x == 3)
            {
                if (coll.IsTouchingLayers(ground) || coll.IsTouchingLayers(platform))
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
                if (coll.IsTouchingLayers(ground) || coll.IsTouchingLayers(platform))
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

    public void TakeDamage(Animator enemyAnimation, int damage, Image executionImage)
    {
        if (!isBlocking)
        {
            canMove = true;
            getDamage = damage;
            PermanentUI.GetInstance().currentHealth -= getDamage;
            healthBar.SetHealth(PermanentUI.GetInstance().currentHealth);

            anim.SetTrigger("hurt");

            if (PermanentUI.GetInstance().currentHealth <= 0)
            {
                Die();
            }

            else if (transform.localScale.x == 3)
            {
                if (coll.IsTouchingLayers(ground) || coll.IsTouchingLayers(platform))
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
                if (coll.IsTouchingLayers(ground) || coll.IsTouchingLayers(platform))
                {
                    rb.velocity = new Vector2(hurtForce, rb.velocity.y);
                }
                else
                {
                    rb.velocity = new Vector2(hurtForce, 10f);
                }
            }
        }
        else if (isParrying)
        {
            isCounterAttack = true;
            timer = (int) Time.time + 3;
            image = executionImage;
            enemyAnim = enemyAnimation;
            enemyAnim.SetBool("isParrying", true);
            enemyAnim.Play("Parry");
            image.enabled = true;
        }
    }

    public void Interactable()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            RaycastHit2D hit = Physics2D.Raycast(boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,
                       new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y));

            if(hit.collider.tag == "Interactable" || hit.collider.tag == "God" || hit.collider.tag == "Shop" || hit.collider.tag == "Checkpoint" || hit.collider.tag == "Confiner" || hit.collider.tag == "Secret")
            {
                Interact();
            }
        }
    }

    public void Interact()
    {
        if(interactable != null)
        {
            interactable.Interact();
        }
    }

    public void Die()
    {
        anim.SetBool("isDead", true);
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        GetComponent<Collider2D>().enabled = false;
        PlayerRespawn.GetInstance().Respawn();
        PermanentUI.GetInstance().LoseExperience(250);
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,
                       new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y));
    }
}
