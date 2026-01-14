using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField]
    private Camera[] cameras;

    private int currentCameraIndex = 0;

    void Start()
    {
        // Enable only the first camera at start
        if (cameras.Length > 0)
        {
            SwitchToCamera(0);
        }
    }

    void Update()
    {
        // Check for number key presses 1-6 using new Input System
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.digit1Key.wasPressedThisFrame && cameras.Length > 0)
        {
            SwitchToCamera(0);
        }
        else if (keyboard.digit2Key.wasPressedThisFrame && cameras.Length > 1)
        {
            SwitchToCamera(1);
        }
        else if (keyboard.digit3Key.wasPressedThisFrame && cameras.Length > 2)
        {
            SwitchToCamera(2);
        }
        else if (keyboard.digit4Key.wasPressedThisFrame && cameras.Length > 3)
        {
            SwitchToCamera(3);
        }
        else if (keyboard.digit5Key.wasPressedThisFrame && cameras.Length > 4)
        {
            SwitchToCamera(4);
        }
        else if (keyboard.digit6Key.wasPressedThisFrame && cameras.Length > 5)
        {
            SwitchToCamera(5);
        }
    }

    private void SwitchToCamera(int index)
    {
        // Disable all cameras and their audio listeners
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null)
            {
                cameras[i].enabled = false;

                // Disable audio listener if present
                AudioListener listener = cameras[i].GetComponent<AudioListener>();
                if (listener != null)
                {
                    listener.enabled = false;
                }
            }
        }

        // Enable the selected camera and its audio listener
        if (index >= 0 && index < cameras.Length && cameras[index] != null)
        {
            cameras[index].enabled = true;

            // Enable audio listener if present
            AudioListener listener = cameras[index].GetComponent<AudioListener>();
            if (listener != null)
            {
                listener.enabled = true;
            }

            currentCameraIndex = index;
        }
    }
}