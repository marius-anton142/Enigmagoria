using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public GameObject playerGO;
    public Player player;
    [SerializeField] protected double maxHP;
    [SerializeField] protected double currentHP;
    [SerializeField] protected float movementSpeed;
    [SerializeField] protected double visionRange;
    [SerializeField] protected double attackRange;
    [SerializeField] protected double damage;
    protected float attackCooldown;
    protected float lastAttackTime;

    public void setPlayer(Player player)
    {
        this.player = player;
    }

    public void setPlayerGO(GameObject playerGO)
    {
        this.playerGO = playerGO;
    }

    public void TakeDamage(double amount) 
    {
        currentHP -= amount;
        if(currentHP <= 0)
        {
            Destroy(gameObject);
        }
    }

    protected void DestroyIfPlayerIsDead()
    {
        if(!playerGO.activeSelf)
        {
            Destroy(gameObject);
        }
    }

    protected void PlayerAttack()
    {
        player.Attack(this);
    }

    protected abstract void Movement();

    protected abstract void Attack();
}
