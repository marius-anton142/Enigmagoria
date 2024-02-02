using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TRex : MeleeEnemy
{
    // Start is called before the first frame update
    void Start()
    {
        maxHP = 6;
        currentHP = maxHP;
        movementSpeed = 0.3f;
        visionRange = 3;
        attackRange = 1;
        damage = 3;
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
