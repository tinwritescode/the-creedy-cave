using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Quản lý Death Scene - xử lý các button và navigation từ death screen
/// </summary>
public class DeathSceneManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "Menu"; // Tên scene main menu
    
    [Header("UI Buttons")]
    [SerializeField] private Button returnToMenuButton; // Button quay về menu - GÁN VÀO ĐÂY
    
    void Start()
    {
        Time.timeScale = 1f;
        SetupButtons();
    }
    
    /// <summary>
    /// Tự động setup buttons - gán methods vào buttons
    /// </summary>
    void SetupButtons()
    {
        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.RemoveAllListeners();
            returnToMenuButton.onClick.AddListener(ReturnToMainMenu);
            Debug.Log($"DeathSceneManager: Đã gán ReturnToMainMenu() vào button '{returnToMenuButton.name}'");
        }
        else
        {
            Debug.LogWarning("DeathSceneManager: Return to Menu button chưa được gán! Đang tìm tự động...");
            TryFindButtonByName("ReturnToMenu", "Return to Menu", "Menu", ref returnToMenuButton, ReturnToMainMenu);
        }
    }
    
    /// <summary>
    /// Tự động tìm button theo tên và gán listener
    /// </summary>
    private void TryFindButtonByName(string exactName, string displayName, string alternativeName, ref Button buttonField, UnityEngine.Events.UnityAction action)
    {
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        
        foreach (Button btn in allButtons)
        {
            string btnNameLower = btn.name.ToLowerInvariant();
            string exactNameLower = exactName.ToLowerInvariant();
            string displayNameLower = displayName.ToLowerInvariant();
            string alternativeNameLower = alternativeName.ToLowerInvariant();
            
            bool isMatch = btn.name == exactName || 
                          btnNameLower.Contains(exactNameLower) || 
                          btnNameLower.Contains(displayNameLower) ||
                          btnNameLower.Contains(alternativeNameLower);
            
            if (isMatch)
            {
                buttonField = btn;
                buttonField.onClick.RemoveAllListeners();
                buttonField.onClick.AddListener(action);
                Debug.Log($"DeathSceneManager: Đã tự động tìm và gán {displayName} button: '{btn.name}'");
                return;
            }
        }
        
        Debug.LogError($"DeathSceneManager: Không tìm thấy {displayName} button! Vui lòng kéo button vào field trong Inspector.");
    }
    
    /// <summary>
    /// Quay về main menu
    /// </summary>
    public void ReturnToMainMenu()
    {
        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogError("DeathSceneManager: Main menu scene name chưa được thiết lập!");
            return;
        }
        
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
    /// <summary>
    /// Quit game
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
