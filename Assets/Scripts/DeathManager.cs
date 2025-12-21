using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Singleton manager that handles player death screen and game over logic.
/// </summary>
public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance { get; private set; }
    
    [Header("Death Screen UI")]
    [SerializeField] private GameObject deathScreenUI; // Canvas hoặc Panel chứa death screen
    [SerializeField] private float fadeInDuration = 1f; // Thời gian fade in death screen
    
    [Header("Options")]
    [SerializeField] private bool pauseGameOnDeath = true; // Có pause game khi chết không
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Tên scene main menu (nếu có)
    [SerializeField] private bool allowRestart = true; // Cho phép restart level
    
    private bool isDeathScreenActive = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("DeathManager: Initialized");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Ẩn death screen khi bắt đầu
        if (deathScreenUI != null)
        {
            deathScreenUI.SetActive(false);
        }
    }
    
    /// <summary>
    /// Hiển thị death screen khi player chết
    /// </summary>
    public void ShowDeathScreen()
    {
        if (isDeathScreenActive) return; // Tránh gọi nhiều lần
        
        isDeathScreenActive = true;
        
        // Pause game nếu cần
        if (pauseGameOnDeath)
        {
            Time.timeScale = 0f;
        }
        
        // Hiển thị death screen UI
        if (deathScreenUI != null)
        {
            deathScreenUI.SetActive(true);
            
            // Nếu có CanvasGroup, có thể làm fade in
            CanvasGroup canvasGroup = deathScreenUI.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                StartCoroutine(FadeInDeathScreen(canvasGroup));
            }
        }
        else
        {
            Debug.LogWarning("DeathManager: Death Screen UI chưa được gán! Vui lòng gán GameObject chứa UI death screen vào DeathManager.");
        }
        
        Debug.Log("DeathManager: Death screen đã được hiển thị");
    }
    
    /// <summary>
    /// Ẩn death screen (khi restart hoặc quay về menu)
    /// </summary>
    public void HideDeathScreen()
    {
        isDeathScreenActive = false;
        Time.timeScale = 1f; // Resume game
        
        if (deathScreenUI != null)
        {
            deathScreenUI.SetActive(false);
        }
    }
    
    /// <summary>
    /// Restart level hiện tại
    /// </summary>
    public void RestartLevel()
    {
        if (!allowRestart) return;
        
        HideDeathScreen();
        Time.timeScale = 1f;
        
        // Reload scene hiện tại
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
        
        Debug.Log("DeathManager: Level đã được restart");
    }
    
    /// <summary>
    /// Quay về main menu
    /// </summary>
    public void ReturnToMainMenu()
    {
        HideDeathScreen();
        Time.timeScale = 1f;
        
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
            Debug.Log($"DeathManager: Đã quay về {mainMenuSceneName}");
        }
        else
        {
            Debug.LogWarning("DeathManager: Main menu scene name chưa được thiết lập!");
        }
    }
    
    /// <summary>
    /// Quit game
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("DeathManager: Quitting game...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    /// <summary>
    /// Fade in death screen với animation
    /// </summary>
    private IEnumerator FadeInDeathScreen(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null) yield break;
        
        canvasGroup.alpha = 0f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Dùng unscaledDeltaTime vì game đã pause
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    void OnDestroy()
    {
        // Đảm bảo time scale được reset khi destroy
        if (Instance == this)
        {
            Time.timeScale = 1f;
        }
    }
}

