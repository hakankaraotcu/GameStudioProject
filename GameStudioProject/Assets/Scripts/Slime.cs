using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : Enemy
{
    public int maxhealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private float wayPointLeft;
    [SerializeField] private float wayPointRight;
    [SerializeField] private float walkLength = 10f;
    [SerializeField] private float followRange = 0.5f;
    [SerializeField] private LayerMask enemyLayers;

    public Transform followArea;
    private bool facingLeft = true;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        anim.SetBool("isTrap", true);
        currentHealth = maxhealth;
    }

    // Update is called once per frame
    void Update()
    {
        Collider2D[] followEnemies = Physics2D.OverlapCircleAll(followArea.position, followRange, enemyLayers);

        if (anim.GetBool("canMove") && followEnemies.Length == 1)
        {
            if (TargetInDistance() && followEnabled)
            {
                PathFollow();
            }
        }
        else if (anim.GetBool("canMove") && anim.GetBool("isWalking"))
        {
            Movement();
        }
    }

    private void Movement()
    {
        if (facingLeft)
        {
            if (transform.position.x > wayPointLeft)
            {
                if (transform.localScale.x != -3)
                {
                    transform.localScale = new Vector3(-3, 3);
                }
                rb.velocity = new Vector2(-walkLength, rb.velocity.y);
                anim.SetBool("isWalking", true);
            }
            else
            {
                facingLeft = false;
                anim.SetBool("isWalking", false);
            }
        }
        else
        {
            if (transform.position.x < wayPointRight)
            {
                if (transform.localScale.x != 3)
                {
                    transform.localScale = new Vector3(3, 3);
                }
                rb.velocity = new Vector2(walkLength, rb.velocity.y);
                anim.SetBool("isWalking", true);
            }
            else
            {
                facingLeft = true;
                anim.SetBool("isWalking", false);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if(other.gameObject.tag == "Player")
        {
            anim.SetBool("isTrap", false);
        }
    }

    private void SetMove()
    {
        anim.SetBool("canMove", true);
    }

    public void TakeDamage(int damage)
    {
        if (!isAttacking)
        {
            currentHealth -= damage;

            anim.SetTrigger("hurt");

            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    public void Die()
    {
        anim.SetBool("isParrying", false);
        anim.SetBool("isDead", true);
        PermanentUI.GetInstance().exp += 125;
        PermanentUI.GetInstance().GainExperience();

        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }

    public void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(followArea.position, followRange);
    }
}
