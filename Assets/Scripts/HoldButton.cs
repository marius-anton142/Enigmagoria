using UnityEngine;
using UnityEngine.EventSystems;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public PlayerScript playerScript;
    public Vector3 direction;

    [SerializeField] private KeyCode hotkey = KeyCode.None; // set per arrow: W/A/S/D

    private bool isHolding = false;
    private CameraRigController cameraRig;

    private void Start()
    {
        if (Application.platform != RuntimePlatform.Android)
            GetComponent<UnityEngine.UI.Image>().enabled = false;

        cameraRig = Camera.main.transform.parent.GetComponent<CameraRigController>();
    }

    private void Update()
    {
        // Keyboard mirrors pointer
        if (hotkey != KeyCode.None)
        {
            if (Input.GetKeyDown(hotkey)) BeginHold();
            if (Input.GetKeyUp(hotkey)) EndHold();
        }

        // While held, repeatedly request moves; PlayerScript will ignore if mid-move
        if (isHolding && !playerScript.isSliding && playerScript.bumpsStuck <= 0)
        {
            playerScript.Move(direction);
            if (cameraRig) cameraRig.OnArrowHeld(direction);
        }
    }

    public void OnPointerDown(PointerEventData eventData) { BeginHold(); }
    public void OnPointerUp(PointerEventData eventData) { EndHold(); }

    private void BeginHold()
    {
        isHolding = true;

        // One immediate move if you're in the cobweb “bump out” case
        if (!playerScript.isSliding && playerScript.bumpsStuck > 0)
        {
            playerScript.Move(direction);
        }
    }

    private void EndHold()
    {
        isHolding = false;
        if (cameraRig) cameraRig.OnArrowReleased();
    }
}
