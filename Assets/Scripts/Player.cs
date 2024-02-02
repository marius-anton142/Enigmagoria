using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class Player : MonoBehaviour
{
    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator animator;
    private Animator attackAnimator;
    [SerializeField] private double maxHP;
    [SerializeField] private double currentHP = 10;
    [SerializeField] private double meleeAttackRange;
    [SerializeField] private double meleeDamage;
    [SerializeField] private float attackCooldown;
    private float lastAttackTime;
    private bool isAttackRanged;
    [SerializeField] private float movementSpeed;
    private Quaternion targetRotation; // noua variabila pentru rotatia tinta
    [SerializeField] private float turnSpeed; // noua variabila pentru viteza de rotatie
    [SerializeField] private float armor;
    private float regenTime = 4;
    private float timeLeftTillRegen;
    private float hpRegenQuantity = 1;

    public event Action HpUpdate;


    // Start is called before the first frame update

    void Start()
    {
        maxHP = 10;
        currentHP = maxHP;
        meleeAttackRange = 2;
        meleeDamage = 3;
        attackCooldown = 5;
        lastAttackTime = Time.time - attackCooldown;
        isAttackRanged = false;
        movementSpeed = 8;
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        animator = GetComponent<Animator>();
        timeLeftTillRegen = regenTime;

        //set animatorAttack from child
        attackAnimator = transform.GetChild(0).GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // Aici am introdus regenerarea hp-ului, care se face la fiecare 4 secunde cu 1 punct
        timeLeftTillRegen -= Time.deltaTime;
        if (timeLeftTillRegen < 0)
        {
            if(currentHP + hpRegenQuantity > maxHP)
                currentHP = maxHP;
            else
            {
                currentHP += hpRegenQuantity;
            }
            HpUpdate?.Invoke();;
            timeLeftTillRegen = regenTime;
        }
    }

    public float HpRegenQuantity
    {
        get => hpRegenQuantity;

        set => hpRegenQuantity = value;
    }

    public float AttackCooldown
    {
        get => attackCooldown;
        set => attackCooldown = value;
    }
    public double MeleeDamage
    {
        get => meleeDamage;

        set => meleeDamage = value;
    }

    public float MovementSpeed
    {
        get => movementSpeed;
        set => movementSpeed = value;
    }

    public void TakeDamage(double amount)
    {
        currentHP -= amount * (1 -  (0.5 * armor / 100));

        if (currentHP <= 0)
        {
            gameObject.SetActive(false);
        }
        HpUpdate?.Invoke();
    }

    public void Attack(Enemy enemy)
    {
        if (isAttackRanged)
        {
            RangedAttack(enemy);
        }
        else
        {
            MeleeAttack(enemy);
        }
    }

    private void RangedAttack(Enemy enemy)
    {
        // TO DO
    }

    private void MeleeAttack(Enemy enemy)
    {
        double distance = Vector2.Distance(transform.position, enemy.transform.position);
        if (distance <= meleeAttackRange && attackCooldown <= Time.time - lastAttackTime)
        {
            enemy.TakeDamage(meleeDamage);
            lastAttackTime = Time.time;
            //set trigger for attack animation
            attackAnimator.SetTrigger("Attack");
        }
    }

    private void OnMovement(InputValue value)
    {
        movement = value.Get<Vector2>();
        if (movement.x != 0 || movement.y != 0)
        {
            animator.SetFloat("X", movement.x);
            animator.SetFloat("Y", movement.y);
            animator.SetBool("IsWalking", true);
        }   
        else
        {
            animator.SetBool("IsWalking", false);
        }
    }

    private void FixedUpdate()
    {
        if (movement.x != 0 || movement.y != 0)
        {
            rb.velocity = movement * movementSpeed;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }


    }
    
    // De Event-ul HpUpdate este legata bara de viata din UI care trebuie actualizata odata cu actualizarea vietii
    
    public double CurrentHP
    {
        get => currentHP;
        set
        {
            if (currentHP + value > maxHP)
                currentHP = maxHP;
            else
                currentHP += value;
            HpUpdate?.Invoke();
        }
    }
    
    public float Armor
    {
        get => armor;
        set => armor = value;
    }
    
    // Functie apelata cand intram in contact cu un obiect care are un collider, facuta pentru colectarea itemelor
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var collectible = collision.GetComponent<Collectible>();
        if(collectible != null) collectible.Collect();
    }
}
