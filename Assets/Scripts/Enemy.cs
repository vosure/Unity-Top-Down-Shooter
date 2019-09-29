using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
    public enum State { Idle, Chasing, Attacking };
    State currentState;

    NavMeshAgent pathfinder;
    Transform target;
    Material skinMaterial;
    Color originalColor;

    float attackDistanceThreashold = 1.5f;
    float timeBetweenAttacks = 1.0f;

    float nextAttackTime;

    float myCollisionRadius;
    float targetCollisionRadius;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        currentState = State.Chasing;

        pathfinder = GetComponent<NavMeshAgent>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
        skinMaterial = GetComponent<Renderer>().material;
        originalColor = skinMaterial.color;

        myCollisionRadius = GetComponent<CapsuleCollider>().radius;
        myCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
       
        StartCoroutine(UpdatePath());
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > nextAttackTime)
        {
            float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;
            if (sqrDstToTarget < Mathf.Pow(attackDistanceThreashold + myCollisionRadius + targetCollisionRadius, 2))
            {
                nextAttackTime = Time.time + timeBetweenAttacks;
                StartCoroutine(Attack());
            }
        }
    }

    IEnumerator Attack()
    {
        currentState = State.Attacking;
        pathfinder.enabled = false;

        Vector3 originalPosition = transform.position;
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - dirToTarget * myCollisionRadius;

        float attackSpeed = 3;
        float percent = 0;

        skinMaterial.color = Color.yellow;

        while (percent <= 1)
        {
            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

            yield return null;
        }

        skinMaterial.color = originalColor;
        currentState = State.Chasing;
        pathfinder.enabled = true;
    }

    IEnumerator UpdatePath()
    {
        float refreshRate = 0.25f;

        while (target != null)
        {
            if (currentState == State.Chasing)
            {
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreashold / 2);
                if (!dead)
                {
                    pathfinder.SetDestination(targetPosition);
                }

            }
                yield return new WaitForSeconds(refreshRate);
        }
    }


}
