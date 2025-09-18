using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    [SerializeField] private int hp = 3;
    [SerializeField] private Sprite doorOpenSprite;
    [SerializeField] private Sprite doorOpenFloorSprite;
    [SerializeField] private AudioPlayer audioPlayer;

    private SpriteRenderer sr;
    private SpriteRenderer childSr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (transform.childCount > 0)
        {
            childSr = transform.GetChild(0).GetComponent<SpriteRenderer>();
        }

        audioPlayer = GameObject.FindGameObjectWithTag("AudioSource").GetComponent<AudioPlayer>();
    }

    public int GetHp() { return hp; }
    public void DecreaseHp()
    {
        hp--;
        FindObjectOfType<AudioPlayer>().PlayDoorBumpSound();

        if (hp == -1 && doorOpenSprite != null)
        {
            sr.sprite = doorOpenSprite;

            // Change child sprite if assigned
            if (childSr != null && doorOpenFloorSprite != null)
            {
                childSr.sprite = doorOpenFloorSprite;
            }

            Destroy(GetComponent<Rigidbody2D>());
            Destroy(GetComponent<PolygonCollider2D>());

            RDG.Vibration.Vibrate(3);
            FindObjectOfType<AudioPlayer>().PlayDoorOpenSound();
        }
    }
}
