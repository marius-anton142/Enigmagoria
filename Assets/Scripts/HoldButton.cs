using UnityEngine;
using UnityEngine.EventSystems;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public PlayerScript playerScript;
    public Vector3 direction;

    private bool isHolding = false;

    private void Update()
    {
        if (isHolding)
        {
            playerScript.Move(direction);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
    }
}
