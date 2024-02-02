using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wizard : RangedEnemy
{
    // Start is called before the first frame update
    void Start()
    {
        maxHP = 3;
        currentHP = maxHP;
        movementSpeed = 2;
        visionRange = 9;
        attackRange = 8;
        dangerRange = 5;
        damage = 5;
        attackCooldown = 4;
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
