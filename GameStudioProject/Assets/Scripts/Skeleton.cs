using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : Enemy
{
    [SerializeField] private int maxhealth = 100;
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
        currentHealth = maxhealth;
    }

    // Update is called once per frame
    void Update()
    {
        Collider2D[] followEnemies = Physics2D.OverlapCircleAll(followArea.position, followRange, enemyLayers);

        if(followEnemies.Length == 1)
        {
            if (TargetInDistance() && followEnabled)
            {
                PathFollow();
            }
        }
        else if (anim.GetBool("isWalking"))
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
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        anim.SetTrigger("hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
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