using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameSceneManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField]
    private string mainMenuSceneName = "MainMenu";

    void Update()
    {
        // Check for ESC key press using new Input System
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            ReturnToMainMenu();
        }
    }

    private void ReturnToMainMenu()
    {
        // Clean up before returning to main menu
        CleanupScene();

        // Load the main menu scene (Single mode unloads all other scenes)
        SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
    }

    private void CleanupScene()
    {
        // Reset time scale in case game was paused
        Time.timeScale = 1f;

        // Destroy Graphy if it exists
        GameObject graphyObject = GameObject.Find("[Graphy]");
        if (graphyObject != null)
        {
            Destroy(graphyObject);
        }

        // Stop all audio
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audio in audioSources)
        {
            audio.Stop();
        }

        // Stop all coroutines in the scene
        StopAllCoroutines();
    }

    void OnDestroy()
    {
        // Ensure time scale is reset when this object is destroyed
        Time.timeScale = 1f;
    }
}
