using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int maxhealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private float wayPointLeft;
    [SerializeField] private float wayPointRight;
    [SerializeField] private float walkLength = 10f;
    private Animator anim;
    private Rigidbody2D rb;
    public bool facingLeft = true;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxhealth;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (anim.GetBool("isWalking"))
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

    public void Die()
    {
        Debug.Log("died");

        anim.SetBool("isDead", true);

        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

    }
}
