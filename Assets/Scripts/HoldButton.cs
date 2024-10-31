using UnityEngine;
using UnityEngine.EventSystems;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public PlayerScript playerScript;
    public Vector3 direction;

    private bool isHolding = false;

    private void Update()
    {
        if (isHolding && !playerScript.isSliding && playerScript.bumpsStuck <= 0)
        {
            playerScript.Move(direction);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
        //Debug.Log("ONPDOWN");
        if (!playerScript.isSliding && playerScript.bumpsStuck > 0)
        {
            playerScript.Move(direction);
            //Debug.Log("CLICKED");
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
    }
}
