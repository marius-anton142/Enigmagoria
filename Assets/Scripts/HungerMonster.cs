using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HungerMonster : MeleeEnemy
{
    // Start is called before the first frame update
    void Start()
    {
        maxHP = 2;
        currentHP = maxHP;
        movementSpeed = 1.5f;
        visionRange = 3;
        attackRange = 1;
        damage = 2;
        attackCooldown = 2;
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
}
