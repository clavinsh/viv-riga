using UnityEngine;
using UnityEngine.InputSystem;

public class cameraController : MonoBehaviour
{
    [Header("Mouse Settings")]
    public float mouseSensitivity = 100f;
    public bool invertY = false;

    [Header("Camera Limits")]
    public float maxLookUp = 90f;
    public float maxLookDown = -90f;

    private Transform playerBody;
    private float xRotation = 0f;
    private Vector2 mouseDelta;

    void Start()
    {
        // Get parent transform (should be the player capsule)
        playerBody = transform.parent;
        
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Get mouse input from new Input System
        mouseDelta = Vector2.zero;
        if (Mouse.current != null)
        {
            mouseDelta = Mouse.current.delta.ReadValue();
        }

        float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;

        // Apply Y inversion if enabled
        if (invertY)
            mouseY = -mouseY;

        // Rotate camera up/down (pitch)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, maxLookDown, maxLookUp);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate player body left/right (yaw)
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }

        // Toggle cursor lock with Escape
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}