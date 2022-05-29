using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Bosses
{
    public int maxhealth = 400;
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

        if (followEnemies.Length == 1)
        {
            if (TargetInDistance() && followEnabled)
            {
                PathFollow();
            }
        }
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
        anim.SetBool("isDead", true);

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
