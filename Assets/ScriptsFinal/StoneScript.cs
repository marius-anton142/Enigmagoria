using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneScript : MonoBehaviour
{
    public AudioPlayer audioPlayer;
    public string state = "idle";
    public float hp = 100;
    public float damage = 50f;
    public float knockTime = 0.5f;
    public float knockbackForce = 10.0f;
    public float knockResistance = 1f;

    public float collisionVelocityThreshold = 2.5f;

    private SpriteRenderer spriteRenderer;
    Rigidbody2D rb;
    public Material whiteFlashMaterial;
    private Material originalMaterial;
    private Coroutine flashCoroutine;
    public GameObject DungeonManager;

    private HashSet<GameObject> hitEntities;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        audioPlayer = GameObject.FindGameObjectWithTag("AudioSource").GetComponent<AudioPlayer>();
        DungeonManager = GameObject.FindGameObjectWithTag("DungeonManager");
        originalMaterial = spriteRenderer.material;
        hitEntities = new HashSet<GameObject>();
    }

    public void ApplyKnockback(Vector2 direction, float force, float knockTime, float damageOther)
    {
        TakeDamage(damageOther);

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.AddForce(direction * force * knockResistance, ForceMode2D.Impulse);
        SetStateToKnocked(knockTime);
        float distance = (force / rb.mass) / (1 + rb.drag);

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashWhite(0.15f));
    }

    public void TakeDamage(float damage)
    {
        //hp -= damage;

        if (hp <= 0)
        {
            SetStateToDead();
        }
        else
        {
            FindObjectOfType<AudioPlayer>().PlayStoneSound();
        }
    }

    private IEnumerator FlashWhite(float duration)
    {
        spriteRenderer.material = whiteFlashMaterial;
        yield return new WaitForSeconds(duration);
        spriteRenderer.material = originalMaterial;
    }

    void SetStateToDead()
    {
        state = "dead";
        FindObjectOfType<AudioPlayer>().PlayStoneSound();
        FindObjectOfType<AudioPlayer>().PlayKillSound();
        RDG.Vibration.Vibrate(15);
        Destroy(gameObject);
    }

    void SetStateToKnocked(float knockTime)
    {
        state = "knocked";
        StartCoroutine(ResetStateAfterKnock(knockTime * knockResistance));
    }

    private IEnumerator ResetStateAfterKnock(float maxKnockTime)
    {
        float timer = 0f;
        float velocityThreshold = 0.1f;

        while (timer < maxKnockTime)
        {
            // Break early if movement is nearly stopped
            if (rb.velocity.magnitude < velocityThreshold)
                break;

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        state = "idle";
        hitEntities = new HashSet<GameObject>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (rb.velocity.magnitude < collisionVelocityThreshold)
            return;

        GameObject hitObject = other?.gameObject;
        if (hitObject == null || hitEntities.Contains(hitObject) || hitObject == gameObject)
            return;

        if (other.CompareTag("Enemy"))
        {
            hitEntities.Add(hitObject);

            float velocityForce = rb.velocity.magnitude;
            Vector2 knockbackDir = (hitObject.transform.position - transform.position).normalized;

            PlayerScript player = other.GetComponent<PlayerScript>();
            EnemyAI enemy = other.GetComponent<EnemyAI>();

            if (player != null)
            {
                player.ApplyKnockback(knockbackDir, velocityForce, knockTime, damage);
            }
            else if (enemy != null)
            {
                enemy.ApplyKnockback(knockbackDir, velocityForce, knockTime, damage);
            }
        }
    }
}
