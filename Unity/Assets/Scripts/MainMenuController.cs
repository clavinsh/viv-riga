using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField]
    private Button playButton;

    [SerializeField]
    private Button exitButton;

    [Header("Loading Screen")]
    [SerializeField]
    private GameObject loadingScreen;

    [SerializeField]
    private Slider loadingBar;

    [SerializeField]
    private Text loadingText;

    [Header("Scene Settings")]
    [SerializeField]
    private string gameSceneName = "Main";

    void Start()
    {
        // Hide loading screen at start
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }

        // Setup button listeners
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }
    }

    private void OnPlayButtonClicked()
    {
        StartCoroutine(LoadGameScene());
    }

    private void OnExitButtonClicked()
    {
        // This will quit the application
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private IEnumerator LoadGameScene()
    {
        // Disable and hide buttons to prevent multiple clicks
        if (playButton != null)
        {
            playButton.interactable = false;
            playButton.gameObject.SetActive(false);
        }

        if (exitButton != null)
        {
            exitButton.interactable = false;
            exitButton.gameObject.SetActive(false);
        }

        // Show loading screen
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
        }

        // Start loading the scene asynchronously
        AsyncOperation operation = SceneManager.LoadSceneAsync(gameSceneName);
        operation.allowSceneActivation = false;

        // Update loading bar while scene loads
        while (!operation.isDone)
        {
            // Calculate progress (0 to 0.9 is loading, 0.9 to 1.0 is activation)
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            // Update loading bar
            if (loadingBar != null)
            {
                loadingBar.value = progress;
            }

            // Update loading text
            if (loadingText != null)
            {
                loadingText.text = "Lādē... " + (progress * 100f).ToString("F0") + "%";
            }

            // Check if scene is ready to activate
            if (operation.progress >= 0.9f)
            {
                // Wait a bit to show 100% or press any key
                if (loadingBar != null)
                {
                    loadingBar.value = 1f;
                }

                if (loadingText != null)
                {
                    loadingText.text = "Lādē... 100%";
                }

                yield return new WaitForSeconds(0.5f);

                // Activate the scene
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
