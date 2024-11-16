using UnityEngine;
using System.Collections;

public class InventoryArrowScript : MonoBehaviour
{
    public Transform inventoryTransform; // Reference to the Inventory GameObject's transform
    public Transform inventoryAppendixTransform;
    public float rotateDuration = 0.3f; // Duration for rotation smoothing
    public float moveDuration = 0.5f; // Duration for position smoothing

    public float inventoryOffsetX = -500f;
    public float inventoryAppendixOffsetX = -500f;

    [SerializeField] private GameObject InventoryExpanded;

    private bool isOpen = true; // Tracks the state of the inventory
    private float targetRotationZ = 0; // Target rotation in Z-axis for the arrow
    private float rotationVelocity = 0; // Helper for SmoothDamp rotation
    private float xVelocity = 0; // Helper for SmoothDamp position (x-axis only)
    private float xVelocityAppendix = 0;
    private float initialInventoryX;
    private float initialInventoryAppendixX;
    private bool initialized = false; // Tracks if initial position is captured

    void Start()
    {
        // Start coroutine to capture initial position after 1 frame
        StartCoroutine(InitializePositionAfterFrame());
    }

    IEnumerator InitializePositionAfterFrame()
    {
        // Wait for 1 frame
        yield return null;

        // Capture the initial x position of inventory
        initialInventoryX = inventoryTransform.localPosition.x;
        initialInventoryAppendixX = inventoryAppendixTransform.localPosition.x;
        initialized = true; // Mark as initialized
    }

    void Update()
    {
        // Only proceed if initialized
        if (!initialized) return;

        // Rotate the arrow smoothly towards the target rotation
        float currentRotationZ = transform.eulerAngles.z;
        float smoothRotationZ = Mathf.SmoothDampAngle(currentRotationZ, targetRotationZ, ref rotationVelocity, rotateDuration);
        transform.rotation = Quaternion.Euler(0, 0, smoothRotationZ);

        // Move the inventory smoothly only along the x-axis
        float targetX = isOpen ? initialInventoryX : initialInventoryX + inventoryOffsetX;
        float smoothX = Mathf.SmoothDamp(inventoryTransform.localPosition.x, targetX, ref xVelocity, moveDuration);
        inventoryTransform.localPosition = new Vector3(smoothX, inventoryTransform.localPosition.y, inventoryTransform.localPosition.z);

        float targetX2 = isOpen ? initialInventoryAppendixX : initialInventoryAppendixX + inventoryAppendixOffsetX;
        float smoothX2 = Mathf.SmoothDamp(inventoryAppendixTransform.localPosition.x, targetX2, ref xVelocityAppendix, moveDuration);
        inventoryAppendixTransform.localPosition = new Vector3(smoothX2, inventoryAppendixTransform.localPosition.y, inventoryAppendixTransform.localPosition.z);
    }

    public void ToggleInventory()
    {
        // Only toggle if initialized
        if (!initialized) return;

        isOpen = !isOpen;

        // Set target rotation for each toggle
        targetRotationZ += isOpen ? 180 : -180;
        InventoryExpanded.SetActive(!isOpen);
    }
}
