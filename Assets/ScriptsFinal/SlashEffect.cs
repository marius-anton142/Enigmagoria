using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashEffect : MonoBehaviour
{
    public List<Sprite> frames;
    public float[] switchTimes = { 0.02f, 0.028f, 0.11f };
    public float totalLifetime = 0.15f;

    private SpriteRenderer sr;

    public void Initialize(List<Sprite> sprites, float angle, float scale, bool flipY)
    {
        frames = sprites;
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = frames[0];
        sr.sortingLayerName = "UI";
        sr.sortingOrder = 99;

        transform.rotation = Quaternion.Euler(0, 0, angle);
        transform.localScale = new Vector3(scale, flipY ? scale : -scale, 1f);

        StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        for (int i = 1; i < frames.Count; i++)
        {
            float waitTime = switchTimes[i] - switchTimes[i - 1];
            yield return new WaitForSeconds(waitTime);
            sr.sprite = frames[i];
        }

        yield return new WaitForSeconds(totalLifetime - switchTimes[frames.Count - 1]);
        Destroy(gameObject);
    }
}
