using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeScript : MonoBehaviour
{
    public AudioPlayer audioPlayer;
    public float hp = 100;

    private SpriteRenderer spriteRenderer;

    public Material whiteFlashMaterial;
    private Material originalMaterial;
    private Coroutine flashCoroutine;
    public GameObject DungeonManager;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioPlayer = GameObject.FindGameObjectWithTag("AudioSource").GetComponent<AudioPlayer>();
        DungeonManager = GameObject.FindGameObjectWithTag("DungeonManager");
        originalMaterial = spriteRenderer.material;
    }

    public void ApplyKnockback(Vector2 direction, float force, float knockTime, float damageOther)
    {
        TakeDamage(damageOther);

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashWhite(0.15f));
    }

    public void TakeDamage(float damage)
    {
        hp -= damage;

        if (hp <= 0)
        {
            SetStateToDead();
        }
        else
        {
            FindObjectOfType<AudioPlayer>().PlayHitSound();
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
        FindObjectOfType<AudioPlayer>().PlayKillSound();
        RDG.Vibration.Vibrate(200);

        Vector3Int flooredPosition = new Vector3Int(
            Mathf.FloorToInt(transform.position.x),
            Mathf.FloorToInt(transform.position.y),
            Mathf.FloorToInt(transform.position.z)
        );

        DungeonManager.GetComponent<DungeonGenerationScript01>().RemoveTreeAtPosition(flooredPosition);
    }
}
