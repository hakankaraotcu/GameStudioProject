using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D coll;
    public int healthPotion;
    private Vector2 moveDir;
    [SerializeField] private LayerMask ground;
    [SerializeField] private float speed = 8f;
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float hurtForce = 7f;
    private enum State { idle, running, jumping, falling, rolling, hurt};
    private State state = State.idle;

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    private void Update()
    {
        if(state != State.hurt)
        {
            Movement();
        }
        AnimState();
        anim.SetInteger("state", (int)state);
    }

    private void Movement()
    {
        float dirX = Input.GetAxisRaw("Horizontal");
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

        if (Input.GetButtonDown("Jump") && coll.IsTouchingLayers(ground))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            state = State.jumping;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Collectible")
        {
            Destroy(collision.gameObject);
            healthPotion += 1;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Enemy")
        {
            state = State.hurt;
            if(collision.gameObject.transform.position.x > transform.position.x)
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
        if(state == State.hurt)
        {
            if(Mathf.Abs(rb.velocity.x) < .1f)
            {
                state = State.idle;
            }
        }
        else if (state == State.jumping)
        {
            if (rb.velocity.y < 0.1f)
            {
                state = State.falling;
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
        else
        {
            state = State.idle;
        }
    }
}
