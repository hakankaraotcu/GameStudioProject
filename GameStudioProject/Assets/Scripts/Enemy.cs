using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected Animator anim;
    protected Rigidbody2D rb;

    [Header("Pathfinding")]
    public Transform target;
    public float activateDistance = 50f;
    public float pathUpdateSeconds = 0.5f;

    [Header("Physics")]
    public float speed = 10f;
    public float nextWaypointDistance = 3f;

    [Header("Custom Behavior")]
    public bool followEnabled = true;
    public bool directionLookEnabled = true;

    private Path path;
    private int currentWaypoint = 0;

    public Transform attackPoint;
    public float attackRange = 0.5f;
    [SerializeField] protected LayerMask hitEnemyLayers;
    public int damage = 40;
    public float attackRate = 2f;
    public float nextAttackTime = 0f;

    Seeker seeker;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        InvokeRepeating("UpdatePath", 0f, pathUpdateSeconds);

        seeker.StartPath(rb.position, target.position, OnPathComplete);
    }

    private void UpdatePath()
    {
        if (followEnabled && TargetInDistance() && seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }

    public void PathFollow()
    {
        if (path == null)
        {
            return;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            return;
        }

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, hitEnemyLayers);

        if(hitEnemies.Length == 1)
        {
            if (Time.time >= nextAttackTime)
            {
                anim.SetTrigger("attack");
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }
        else
        {
            if (transform.position.x > target.position.x)
            {
                if (transform.localScale.x != -3)
                {
                    transform.localScale = new Vector3(-3, 3);
                }
                rb.velocity = new Vector2(-speed, rb.velocity.y);
            }
            else if (transform.position.x < target.position.x)
            {
                if (transform.localScale.x != 3)
                {
                    transform.localScale = new Vector3(3, 3);
                }
                rb.velocity = new Vector2(speed, rb.velocity.y);
            }
        }
    }

    public bool TargetInDistance()
    {
        return Vector2.Distance(transform.position, target.transform.position) < activateDistance;
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    public void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, hitEnemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            if(Time.time >= nextAttackTime)
            {
                enemy.GetComponent<PlayerController>().TakeDamage(damage);
            }
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

    public void Die()
    {
        Debug.Log("died");

        anim.SetBool("isDead", true);

        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }
}
