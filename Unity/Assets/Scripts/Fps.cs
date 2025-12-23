using UnityEngine;

public class Fps : MonoBehaviour
{
    private float fps;
    private float updateInterval = 0.5f;
    
    void Start()
    {
        InvokeRepeating("UpdateFPS", 0f, updateInterval);
    }
    
    void UpdateFPS()
    {
        fps = 1f / Time.unscaledDeltaTime;
    }
    
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 400, 100), "FPS: " + Mathf.Round(fps));
    }
}
