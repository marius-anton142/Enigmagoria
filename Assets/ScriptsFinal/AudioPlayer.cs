using UnityEngine;
using System.Collections.Generic;

public class AudioPlayer : MonoBehaviour
{
    public AudioSource audioSource;

    [SerializeField] List<AudioClip> stone;
    [SerializeField] List<AudioClip> slide;
    [SerializeField] List<AudioClip> slash;
    [SerializeField] List<AudioClip> pickup;
    [SerializeField] List<AudioClip> bump;
    [SerializeField] List<AudioClip> door_bump;
    [SerializeField] List<AudioClip> door_open;
    [SerializeField] List<AudioClip> walk;
    [SerializeField] List<AudioClip> walk_critter;
    [SerializeField] List<AudioClip> walk_knight;
    [SerializeField] List<AudioClip> hit;
    [SerializeField] List<AudioClip> hit_player;
    [SerializeField] List<AudioClip> kill;
    [SerializeField] List<AudioClip> cobweb;
    [SerializeField] List<AudioClip> cobweb_stuck;
    [SerializeField] List<AudioClip> cobweb_break;


    public void PlayStoneSound() => PlayRandom(stone);
    public void PlaySlideSound() => PlayRandom(slide);
    public void PlaySlashSound() => PlayRandom(slash);
    public void PlayPickupSound() => PlayRandom(pickup);
    public void PlayBumpSound() => PlayRandom(bump);
    public void PlayDoorBumpSound() => PlayRandom(door_bump);
    public void PlayDoorOpenSound() => PlayRandom(door_open);
    public void PlayWalkSound() => PlayRandom(walk);
    public void PlayWalkCritterSound() => PlayRandom(walk_critter);
    public void PlayWalkKnightSound() => PlayRandom(walk_knight);
    public void PlayHitSound() => PlayRandom(hit);
    public void PlayHitPlayerSound() => PlayRandom(hit_player);
    public void PlayKillSound() => PlayRandom(kill);
    public void PlayCobwebSound() => PlayRandom(cobweb);
    public void PlayCobwebStuckSound() => PlayRandom(cobweb_stuck);
    public void PlayCobwebBreakSound() => PlayRandom(cobweb_break);

    private void PlayRandom(List<AudioClip> clips)
    {
        if (clips == null || clips.Count == 0) return;
        AudioClip clip = clips[Random.Range(0, clips.Count)];
        audioSource.PlayOneShot(clip);
    }
}
