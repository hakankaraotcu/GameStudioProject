using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private float speed;
    [SerializeField] private float resetTime;
    private float lifetime;
    private Animator anim;
    private BoxCollider2D coll;

    private bool hit;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        coll = GetComponent<BoxCollider2D>();
    }

    public void ActivateProjectile()
    {
        hit = false;
        lifetime = 0;
        gameObject.SetActive(true);
        coll.enabled = true;
    }
    private void Update()
    {
        if (hit) return;
        float movementSpeed = speed * Time.deltaTime;
        transform.Translate(movementSpeed, 0, 0);

        lifetime += Time.deltaTime;
        if (lifetime > resetTime)
            gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Confiner" || collision.tag == "Player")
        {
            if(collision.tag == "Player")
            {
                hit = true;
                if (collision.tag == "Player")
                {
                    collision.GetComponent<PlayerController>().NoCounterTakeDamage(damage);
                }
                coll.enabled = false;

                if (anim != null)
                    anim.SetTrigger("explode");
                else
                    gameObject.SetActive(false);
            }
        }
    }
    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
