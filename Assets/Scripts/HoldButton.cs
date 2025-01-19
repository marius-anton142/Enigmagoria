using UnityEngine;
using UnityEngine.EventSystems;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public PlayerScript playerScript;
    public Vector3 direction;

    private bool isHolding = false;
    private FollowScript followScript;

    private void Start()
    {
        followScript = Camera.main.GetComponent<FollowScript>();
    }

    private void Update()
    {
        if (isHolding && !playerScript.isSliding && playerScript.bumpsStuck <= 0)
        {
            playerScript.Move(direction);
            followScript.OnArrowHeld(direction); // Notify FollowScript
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
        if (!playerScript.isSliding && playerScript.bumpsStuck > 0)
        {
            playerScript.Move(direction);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
        followScript.OnArrowReleased(); // Notify FollowScript when released
    }
}
