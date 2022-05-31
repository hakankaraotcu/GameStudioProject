using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Bosses
{
    public int maxHealth = 400;
    [SerializeField] private int currentHealth;
    [SerializeField] private float wayPointLeft;
    [SerializeField] private float wayPointRight;
    [SerializeField] private float walkLength = 10f;
    [SerializeField] private float followRange = 0.5f;
    [SerializeField] private LayerMask enemyLayers;

    public Transform followArea;

    [SerializeField] private HealthBar healthBar;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        if(currentHealth <= 500)
        {
            attackRate = 0.6f;
        }
        Collider2D[] followEnemies = Physics2D.OverlapCircleAll(followArea.position, followRange, enemyLayers);

        if (followEnemies.Length == 1)
        {
            healthBar.gameObject.SetActive(true);
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
            healthBar.SetHealth(currentHealth);

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
        healthBar.gameObject.SetActive(false);
        PermanentUI.GetInstance().exp += 1000;
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
