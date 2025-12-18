using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private float offsetY = 1.5f; // Height above enemy sprite
    [SerializeField] private Vector2 healthBarSize = new Vector2(1f, 0.15f); // Width and height in world units
    [SerializeField] private Color fullHealthColor = new Color(0f, 1f, 0f, 1f); // Green
    [SerializeField] private Color lowHealthColor = new Color(1f, 0f, 0f, 1f); // Red
    [SerializeField] private float lowHealthThreshold = 0.3f; // 30% health
    [SerializeField] private bool hideWhenFullHealth = true; // Hide bar when at full health
    [SerializeField] private bool hideWhenDead = true; // Hide bar when dead
    
    private EnemyHealth enemyHealth;
    private Canvas healthBarCanvas;
    private Image healthBarFill;
    private Image healthBarBackground;
    private RectTransform canvasRect;
    private Camera mainCamera;
    
    void Start()
    {
        // Get EnemyHealth component
        enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth == null)
        {
            Debug.LogWarning($"EnemyHealthBar on {gameObject.name}: EnemyHealth component not found. Health bar will not work.");
            enabled = false;
            return;
        }
        
        // Get main camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        if (mainCamera == null)
        {
            Debug.LogWarning("EnemyHealthBar: No camera found. Health bar will not work.");
            enabled = false;
            return;
        }
        
        // Create health bar UI
        CreateHealthBar();
        
        // Subscribe to health events
        enemyHealth.OnHealthChanged += UpdateHealthBar;
        enemyHealth.OnDeath += OnEnemyDeath;
        
        // Initial update
        UpdateHealthBar(enemyHealth.CurrentHealth, enemyHealth.MaxHealth);
    }
    
    void CreateHealthBar()
    {
        // Create Canvas for world-space UI
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = new Vector3(0, offsetY, 0);
        
        healthBarCanvas = canvasObj.AddComponent<Canvas>();
        healthBarCanvas.renderMode = RenderMode.WorldSpace;
        healthBarCanvas.worldCamera = mainCamera;
        
        // Add CanvasScaler for proper scaling
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 1f;
        
        // Add GraphicRaycaster (optional, but good practice)
        canvasObj.AddComponent<GraphicRaycaster>();
        
        canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(healthBarSize.x * 100, healthBarSize.y * 100); // Convert to pixels (assuming 100 pixels per unit)
        canvasRect.localScale = Vector3.one * 0.01f; // Scale down to world units
        
        // Create background
        GameObject backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(canvasObj.transform);
        backgroundObj.transform.localPosition = Vector3.zero;
        backgroundObj.transform.localScale = Vector3.one;
        
        healthBarBackground = backgroundObj.AddComponent<Image>();
        healthBarBackground.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Dark gray background
        
        RectTransform bgRect = backgroundObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        // Create fill bar
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(backgroundObj.transform);
        fillObj.transform.localPosition = Vector3.zero;
        fillObj.transform.localScale = Vector3.one;
        
        healthBarFill = fillObj.AddComponent<Image>();
        healthBarFill.color = fullHealthColor;
        healthBarFill.type = Image.Type.Filled;
        healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;
    }
    
    void LateUpdate()
    {
        // Make health bar always face the camera (billboard effect)
        if (healthBarCanvas != null && healthBarCanvas.gameObject.activeInHierarchy && mainCamera != null && mainCamera.gameObject.activeInHierarchy)
        {
            try
            {
                healthBarCanvas.transform.LookAt(healthBarCanvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                    mainCamera.transform.rotation * Vector3.up);
            }
            catch (System.Exception e)
            {
                // Silently handle any transform errors (e.g., if object was destroyed)
                Debug.LogWarning($"EnemyHealthBar: Error updating billboard - {e.Message}");
            }
        }
    }
    
    void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBarFill == null || enemyHealth == null || healthBarCanvas == null) return;
        
        // Calculate fill amount
        float fillAmount = maxHealth > 0 ? currentHealth / maxHealth : 0f;
        
        // Update fill amount safely
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = fillAmount;
            
            // Update color based on health percentage
            if (fillAmount <= lowHealthThreshold)
            {
                healthBarFill.color = Color.Lerp(lowHealthColor, fullHealthColor, fillAmount / lowHealthThreshold);
            }
            else
            {
                healthBarFill.color = fullHealthColor;
            }
        }
        
        // Show/hide based on health
        if (healthBarCanvas != null && healthBarCanvas.gameObject != null)
        {
            bool shouldShow = true;
            
            if (hideWhenFullHealth && fillAmount >= 1f)
            {
                shouldShow = false;
            }
            
            if (hideWhenDead && currentHealth <= 0)
            {
                shouldShow = false;
            }
            
            healthBarCanvas.gameObject.SetActive(shouldShow);
        }
    }
    
    void OnEnemyDeath()
    {
        // Hide health bar when enemy dies
        if (healthBarCanvas != null && hideWhenDead)
        {
            healthBarCanvas.gameObject.SetActive(false);
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (enemyHealth != null)
        {
            enemyHealth.OnHealthChanged -= UpdateHealthBar;
            enemyHealth.OnDeath -= OnEnemyDeath;
        }
    }
}
