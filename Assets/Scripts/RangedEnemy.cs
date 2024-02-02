using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : Enemy
{
    [SerializeField] protected double dangerRange;
    [SerializeField] protected GameObject EnemyArrowPrefab;
    // Start is called before the first frame update
    void Start()
    {
        maxHP = 10;
        currentHP = maxHP;
        movementSpeed = 1;
        visionRange = 7;
        attackRange = 5;
        dangerRange = 3;
        damage = 1;
        attackCooldown = 6;
        lastAttackTime = Time.time - attackCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        DestroyIfPlayerIsDead();
        Movement();
        Attack();
        PlayerAttack();
    }

    protected override void Movement()
    {
        double distance = Vector2.Distance(transform.position, playerGO.transform.position);
        if(attackRange < distance && distance <= visionRange)
        {
            Vector2 direction = playerGO.transform.position - transform.position;
            direction.Normalize();
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.position = Vector2.MoveTowards(this.transform.position, playerGO.transform.position, movementSpeed * Time.deltaTime);
        }
        else if(distance <= dangerRange)
        {
            Vector2 direction = playerGO.transform.position - transform.position;
            Vector2 runTowards = 2 * transform.position - playerGO.transform.position;
            direction.Normalize();
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.position = Vector2.MoveTowards(this.transform.position, runTowards, movementSpeed * Time.deltaTime);
        }
        transform.rotation = Quaternion.Euler(Vector3.forward * 0);
    }
    protected override void Attack()
    {
        double distance = Vector2.Distance(transform.position, playerGO.transform.position);
        if(distance <= attackRange && attackCooldown <= Time.time - lastAttackTime)
        {
            Vector2 direction = playerGO.transform.position - transform.position;
            direction.Normalize();
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle - 90);
            Arrow arrow = Instantiate(EnemyArrowPrefab, gameObject.transform.position, rotation).GetComponent<Arrow>();
            lastAttackTime = Time.time;
        }   
    }
}
