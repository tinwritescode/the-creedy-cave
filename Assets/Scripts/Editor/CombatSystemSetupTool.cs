using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;

public class CombatSystemSetupTool : EditorWindow
{
    private float playerMaxHealth = 2000f;
    private float playerAttackDamage = 150f;
    private float enemyMaxHealth = 1000f;
    private float enemyAttackDamage = 100f;
    
    [MenuItem("Tools/Setup Combat System")]
    public static void ShowWindow()
    {
        GetWindow<CombatSystemSetupTool>("Combat System Setup");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Combat System Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Player Settings", EditorStyles.boldLabel);
        playerMaxHealth = EditorGUILayout.FloatField("Player Max Health", playerMaxHealth);
        playerAttackDamage = EditorGUILayout.FloatField("Player Attack Damage", playerAttackDamage);
        
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Enemy Settings (Default)", EditorStyles.boldLabel);
        enemyMaxHealth = EditorGUILayout.FloatField("Enemy Max Health", enemyMaxHealth);
        enemyAttackDamage = EditorGUILayout.FloatField("Enemy Attack Damage", enemyAttackDamage);
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("This will set up the complete combat system including CombatManager, CombatUI, and configure all entities.", MessageType.Info);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Setup Complete Combat System", GUILayout.Height(30)))
        {
            SetupCompleteCombatSystem();
        }
        
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Individual Setup Options", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Setup CombatManager Only", GUILayout.Height(25)))
        {
            SetupCombatManager();
        }
        
        if (GUILayout.Button("Setup CombatUI Only", GUILayout.Height(25)))
        {
            SetupCombatUI();
        }
        
        if (GUILayout.Button("Setup Player Health & Damage", GUILayout.Height(25)))
        {
            SetupPlayerHealth();
        }
        
        if (GUILayout.Button("Setup All Enemies", GUILayout.Height(25)))
        {
            SetupAllEnemies();
        }
    }
    
    void SetupCompleteCombatSystem()
    {
        SetupCombatManager();
        SetupCombatUI();
        SetupPlayerHealth();
        SetupAllEnemies();
        
        EditorUtility.DisplayDialog("Setup Complete", 
            "Combat System has been set up successfully!\n\n" +
            "- CombatManager created\n" +
            "- CombatUI created with panel and buttons\n" +
            "- Player health configured\n" +
            "- All enemies configured\n\n" +
            "Make sure enemies have colliders (trigger or collision) to detect player contact.", "OK");
    }
    
    void SetupCombatManager()
    {
        // Check if CombatManager already exists
        CombatManager existingManager = FindFirstObjectByType<CombatManager>();
        if (existingManager != null)
        {
            if (EditorUtility.DisplayDialog("CombatManager Exists", 
                "A CombatManager already exists. Do you want to recreate it?", "Yes", "No"))
            {
                DestroyImmediate(existingManager.gameObject);
            }
            else
            {
                return;
            }
        }
        
        // Create CombatManager GameObject
        GameObject combatManagerObj = new GameObject("CombatManager");
        combatManagerObj.AddComponent<CombatManager>();
        
        Debug.Log("CombatManager created");
        
        MarkSceneDirty();
    }
    
    void SetupCombatUI()
    {
        // Find or create Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Create EventSystem if it doesn't exist
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
            
            Debug.Log("Created Canvas");
        }
        
        // Check if CombatUI already exists
        CombatUI existingCombatUI = canvas.GetComponentInChildren<CombatUI>();
        if (existingCombatUI != null)
        {
            if (EditorUtility.DisplayDialog("CombatUI Exists", 
                "A CombatUI already exists. Do you want to recreate it?", "Yes", "No"))
            {
                DestroyImmediate(existingCombatUI.gameObject);
            }
            else
            {
                return;
            }
        }
        
        // Create CombatUI GameObject
        GameObject combatUIObj = new GameObject("CombatUI");
        combatUIObj.transform.SetParent(canvas.transform, false);
        CombatUI combatUI = combatUIObj.AddComponent<CombatUI>();
        
        // Create Combat Panel
        GameObject combatPanel = new GameObject("CombatPanel");
        combatPanel.transform.SetParent(combatUIObj.transform, false);
        RectTransform panelRect = combatPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(600, 400);
        panelRect.anchoredPosition = Vector2.zero;
        
        Image panelImage = combatPanel.AddComponent<Image>();
        panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        
        // Player Health Section
        GameObject playerHealthSection = CreateHealthBarSection("PlayerHealthSection", combatPanel.transform, new Vector2(-150, 100));
        Image playerHealthBarFill = CreateHealthBar(playerHealthSection.transform, "PlayerHealthBar");
        Text playerHealthText = CreateHealthText(playerHealthSection.transform, "PlayerHealthText");
        
        // Enemy Health Section
        GameObject enemyHealthSection = CreateHealthBarSection("EnemyHealthSection", combatPanel.transform, new Vector2(150, 100));
        Image enemyHealthBarFill = CreateHealthBar(enemyHealthSection.transform, "EnemyHealthBar");
        Text enemyHealthText = CreateHealthText(enemyHealthSection.transform, "EnemyHealthText");
        
        // Run Away Button
        GameObject runAwayButtonObj = new GameObject("RunAwayButton");
        runAwayButtonObj.transform.SetParent(combatPanel.transform, false);
        RectTransform buttonRect = runAwayButtonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(200, 50);
        buttonRect.anchoredPosition = new Vector2(0, -150);
        
        Image buttonImage = runAwayButtonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        
        Button runAwayButton = runAwayButtonObj.AddComponent<Button>();
        
        // Button Text
        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(runAwayButtonObj.transform, false);
        RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;
        buttonTextRect.anchoredPosition = Vector2.zero;
        
        Text buttonText = buttonTextObj.AddComponent<Text>();
        buttonText.text = "Run Away";
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (buttonText.font == null)
        {
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        buttonText.fontSize = 24;
        buttonText.fontStyle = FontStyle.Bold;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        // Set up CombatUI component references
        SerializedObject serializedCombatUI = new SerializedObject(combatUI);
        serializedCombatUI.FindProperty("combatPanel").objectReferenceValue = combatPanel;
        serializedCombatUI.FindProperty("playerHealthBarFill").objectReferenceValue = playerHealthBarFill;
        serializedCombatUI.FindProperty("enemyHealthBarFill").objectReferenceValue = enemyHealthBarFill;
        serializedCombatUI.FindProperty("playerHealthText").objectReferenceValue = playerHealthText;
        serializedCombatUI.FindProperty("enemyHealthText").objectReferenceValue = enemyHealthText;
        serializedCombatUI.FindProperty("runAwayButton").objectReferenceValue = runAwayButton;
        serializedCombatUI.ApplyModifiedProperties();
        
        // Initially hide the panel
        combatPanel.SetActive(false);
        
        Debug.Log("CombatUI created with all UI elements");
        
        MarkSceneDirty();
    }
    
    GameObject CreateHealthBarSection(string name, Transform parent, Vector2 position)
    {
        GameObject section = new GameObject(name);
        section.transform.SetParent(parent, false);
        
        RectTransform sectionRect = section.AddComponent<RectTransform>();
        sectionRect.anchorMin = new Vector2(0.5f, 0.5f);
        sectionRect.anchorMax = new Vector2(0.5f, 0.5f);
        sectionRect.pivot = new Vector2(0.5f, 0.5f);
        sectionRect.sizeDelta = new Vector2(250, 150);
        sectionRect.anchoredPosition = position;
        
        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(section.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 1f);
        labelRect.anchorMax = new Vector2(0.5f, 1f);
        labelRect.pivot = new Vector2(0.5f, 1f);
        labelRect.sizeDelta = new Vector2(200, 30);
        labelRect.anchoredPosition = new Vector2(0, -10);
        
        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = name.Contains("Player") ? "Player" : "Enemy";
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (labelText.font == null)
        {
            labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        labelText.fontSize = 20;
        labelText.fontStyle = FontStyle.Bold;
        labelText.color = Color.white;
        labelText.alignment = TextAnchor.MiddleCenter;
        
        return section;
    }
    
    Image CreateHealthBar(Transform parent, string name)
    {
        GameObject healthBarObj = new GameObject(name);
        healthBarObj.transform.SetParent(parent, false);
        
        RectTransform healthBarRect = healthBarObj.AddComponent<RectTransform>();
        healthBarRect.anchorMin = new Vector2(0.1f, 0.3f);
        healthBarRect.anchorMax = new Vector2(0.9f, 0.7f);
        healthBarRect.sizeDelta = Vector2.zero;
        healthBarRect.anchoredPosition = Vector2.zero;
        
        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(healthBarObj.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        
        // Fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(healthBarObj.transform, false);
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;
        
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = new Color(0f, 1f, 0f, 1f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillAmount = 1f;
        
        return fillImage;
    }
    
    Text CreateHealthText(Transform parent, string name)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.1f, 0.1f);
        textRect.anchorMax = new Vector2(0.9f, 0.3f);
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        Text healthText = textObj.AddComponent<Text>();
        healthText.text = "1000 / 1000";
        healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (healthText.font == null)
        {
            healthText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        healthText.fontSize = 18;
        healthText.fontStyle = FontStyle.Bold;
        healthText.color = Color.white;
        healthText.alignment = TextAnchor.MiddleCenter;
        
        return healthText;
    }
    
    void SetupPlayerHealth()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            EditorUtility.DisplayDialog("Player Not Found", 
                "Player not found. Make sure there is a GameObject with the 'Player' tag in the scene.", "OK");
            return;
        }
        
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = player.AddComponent<PlayerHealth>();
            Debug.Log("Added PlayerHealth component to Player");
        }
        
        // Set values using SerializedObject
        SerializedObject serializedHealth = new SerializedObject(playerHealth);
        serializedHealth.FindProperty("maxHealth").floatValue = playerMaxHealth;
        serializedHealth.FindProperty("attackDamage").floatValue = playerAttackDamage;
        serializedHealth.ApplyModifiedProperties();
        
        Debug.Log($"Player Health setup complete: Max Health = {playerMaxHealth}, Attack Damage = {playerAttackDamage}");
        
        MarkSceneDirty();
    }
    
    void SetupAllEnemies()
    {
        // Find all GameObjects that might be enemies (you can customize this logic)
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        List<GameObject> enemies = new List<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            // Check if it has EnemyController or if name contains "Enemy"
            if (obj.GetComponent<EnemyController>() != null || 
                obj.name.ToLower().Contains("enemy"))
            {
                enemies.Add(obj);
            }
        }
        
        if (enemies.Count == 0)
        {
            EditorUtility.DisplayDialog("No Enemies Found", 
                "No enemies found in the scene. Make sure enemies have EnemyController component or 'Enemy' in their name.", "OK");
            return;
        }
        
        int setupCount = 0;
        foreach (GameObject enemy in enemies)
        {
            // Add EnemyHealth if missing
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth == null)
            {
                enemyHealth = enemy.AddComponent<EnemyHealth>();
                Debug.Log($"Added EnemyHealth to {enemy.name}");
            }
            
            // Set enemy health values
            SerializedObject serializedHealth = new SerializedObject(enemyHealth);
            serializedHealth.FindProperty("maxHealth").floatValue = enemyMaxHealth;
            serializedHealth.FindProperty("attackDamage").floatValue = enemyAttackDamage;
            serializedHealth.ApplyModifiedProperties();
            
            // Add EnemyController if missing
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController == null)
            {
                enemyController = enemy.AddComponent<EnemyController>();
                Debug.Log($"Added EnemyController to {enemy.name}");
            }
            
            // Ensure enemy has a collider
            Collider2D collider = enemy.GetComponent<Collider2D>();
            if (collider == null)
            {
                BoxCollider2D boxCollider = enemy.AddComponent<BoxCollider2D>();
                boxCollider.isTrigger = true;
                Debug.Log($"Added trigger BoxCollider2D to {enemy.name}");
            }
            
            setupCount++;
        }
        
        Debug.Log($"Setup complete for {setupCount} enemy/enemies");
        EditorUtility.DisplayDialog("Enemy Setup Complete", 
            $"Successfully configured {setupCount} enemy/enemies with:\n" +
            $"- EnemyHealth (Max: {enemyMaxHealth}, Attack: {enemyAttackDamage})\n" +
            $"- EnemyController\n" +
            $"- Collider (if missing)", "OK");
        
        MarkSceneDirty();
    }
    
    void MarkSceneDirty()
    {
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }
}

