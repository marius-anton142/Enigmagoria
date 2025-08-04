using UnityEngine;
using UnityEngine.EventSystems;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public PlayerScript playerScript;
    public Vector3 direction;

    private bool isHolding = false;
    private CameraRigController cameraRig; // Renamed from FollowScript

    private void Start()
    {
        cameraRig = Camera.main.transform.parent.GetComponent<CameraRigController>(); // Camera is child, rig is parent
    }

    private void Update()
    {
        if (isHolding && !playerScript.isSliding && playerScript.bumpsStuck <= 0)
        {
            playerScript.Move(direction);
            cameraRig.OnArrowHeld(direction); // Notify CameraRigController
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
        cameraRig.OnArrowReleased(); // Notify CameraRigController
    }
}
