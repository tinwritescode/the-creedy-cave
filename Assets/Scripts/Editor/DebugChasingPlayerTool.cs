using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class DebugChasingPlayerTool : EditorWindow
{
    private Vector2 scrollPosition;
    private bool showPlayerDetails = true;
    private bool showEnemyDetails = true;
    
    [MenuItem("Tools/Debug Chasing Player")]
    public static void ShowWindow()
    {
        GetWindow<DebugChasingPlayerTool>("Debug Chasing Player");
    }
    
    void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        GUILayout.Label("Debug Chasing Player Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("This tool checks and fixes Player setup to ensure enemies can chase the player properly.", MessageType.Info);
        EditorGUILayout.Space();
        
        // Find Player button
        if (GUILayout.Button("Find and Fix Player Setup", GUILayout.Height(30)))
        {
            FindAndFixPlayer();
        }
        
        EditorGUILayout.Space();
        
        // Check Player button
        if (GUILayout.Button("Check Player Setup (No Changes)", GUILayout.Height(25)))
        {
            CheckPlayerSetup();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();
        
        // Player Details
        showPlayerDetails = EditorGUILayout.Foldout(showPlayerDetails, "Player Setup Details", true);
        if (showPlayerDetails)
        {
            DisplayPlayerDetails();
        }
        
        EditorGUILayout.Space();
        
        // Enemy Details
        showEnemyDetails = EditorGUILayout.Foldout(showEnemyDetails, "Enemy Setup Details", true);
        if (showEnemyDetails)
        {
            DisplayEnemyDetails();
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    void FindAndFixPlayer()
    {
        GameObject player = FindPlayer();
        
        if (player == null)
        {
            EditorUtility.DisplayDialog("Player Not Found", 
                "Could not find Player GameObject!\n\n" +
                "Please ensure:\n" +
                "1. A GameObject exists with the 'Player' tag, OR\n" +
                "2. A GameObject is named 'Player', OR\n" +
                "3. A GameObject has a PlayerController component\n\n" +
                "You can create a new Player GameObject and this tool will set it up.", "OK");
            return;
        }
        
        List<string> fixes = new List<string>();
        bool needsTag = false;
        
        // Check and fix tag
        if (!player.CompareTag("Player"))
        {
            needsTag = true;
            fixes.Add("Missing 'Player' tag");
        }
        
        // Check and add PlayerController
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController == null)
        {
            playerController = player.AddComponent<PlayerController>();
            fixes.Add("Added PlayerController component");
        }
        
        // Check and add PlayerHealth
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = player.AddComponent<PlayerHealth>();
            fixes.Add("Added PlayerHealth component");
        }
        
        // Check and configure Rigidbody2D
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = player.AddComponent<Rigidbody2D>();
            fixes.Add("Added Rigidbody2D component");
        }
        
        // Configure Rigidbody2D settings
        bool rbChanged = false;
        if (rb.gravityScale != 0)
        {
            rb.gravityScale = 0;
            rbChanged = true;
        }
        if (!rb.freezeRotation)
        {
            rb.freezeRotation = true;
            rbChanged = true;
        }
        if (rb.collisionDetectionMode != CollisionDetectionMode2D.Continuous)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rbChanged = true;
        }
        if (rbChanged)
        {
            fixes.Add("Configured Rigidbody2D settings (gravity=0, freezeRotation=true, continuous detection)");
        }
        
        // Check SpriteRenderer (optional but recommended)
        SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            fixes.Add("Warning: No SpriteRenderer found (optional but recommended)");
        }
        
        // Check BoxCollider2D (optional but recommended)
        BoxCollider2D boxCollider = player.GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            fixes.Add("Warning: No BoxCollider2D found (optional but recommended)");
        }
        
        // Check Animator (optional)
        Animator animator = player.GetComponent<Animator>();
        if (animator == null)
        {
            fixes.Add("Info: No Animator found (optional, for animations)");
        }
        
        // Fix tag if needed (must be done after all component checks)
        if (needsTag)
        {
            // Check if Player tag exists
            if (!TagExists("Player"))
            {
                // Create Player tag
                SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty tagsProp = tagManager.FindProperty("tags");
                
                // Add Player tag
                tagsProp.arraySize++;
                SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
                newTagProp.stringValue = "Player";
                tagManager.ApplyModifiedProperties();
            }
            
            player.tag = "Player";
            fixes.Add("Set 'Player' tag");
        }
        
        // Mark scene as dirty
        EditorUtility.SetDirty(player);
        if (Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(player.scene);
        }
        
        // Show results
        string message = "Player setup completed!\n\n";
        if (fixes.Count > 0)
        {
            message += "Changes made:\n";
            foreach (string fix in fixes)
            {
                message += "• " + fix + "\n";
            }
        }
        else
        {
            message += "No changes needed - Player is already properly configured!";
        }
        
        EditorUtility.DisplayDialog("Player Setup Complete", message, "OK");
        
        // Select player in hierarchy
        Selection.activeGameObject = player;
    }
    
    void CheckPlayerSetup()
    {
        GameObject player = FindPlayer();
        
        if (player == null)
        {
            EditorUtility.DisplayDialog("Player Not Found", 
                "Could not find Player GameObject!", "OK");
            return;
        }
        
        List<string> issues = new List<string>();
        List<string> warnings = new List<string>();
        List<string> info = new List<string>();
        
        // Check tag
        if (!player.CompareTag("Player"))
        {
            issues.Add("Missing 'Player' tag");
        }
        
        // Check components
        if (player.GetComponent<PlayerController>() == null)
        {
            issues.Add("Missing PlayerController component");
        }
        
        if (player.GetComponent<PlayerHealth>() == null)
        {
            issues.Add("Missing PlayerHealth component");
        }
        
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            issues.Add("Missing Rigidbody2D component");
        }
        else
        {
            if (rb.gravityScale != 0)
                issues.Add("Rigidbody2D gravityScale should be 0");
            if (!rb.freezeRotation)
                issues.Add("Rigidbody2D freezeRotation should be true");
            if (rb.collisionDetectionMode != CollisionDetectionMode2D.Continuous)
                warnings.Add("Rigidbody2D collisionDetectionMode should be Continuous (recommended)");
        }
        
        if (player.GetComponent<SpriteRenderer>() == null)
        {
            warnings.Add("No SpriteRenderer (optional but recommended)");
        }
        
        if (player.GetComponent<BoxCollider2D>() == null)
        {
            warnings.Add("No BoxCollider2D (optional but recommended)");
        }
        
        if (player.GetComponent<Animator>() == null)
        {
            info.Add("No Animator (optional, for animations)");
        }
        
        // Show results
        string message = $"Player: {player.name}\n\n";
        
        if (issues.Count == 0 && warnings.Count == 0 && info.Count == 0)
        {
            message += "✓ Player is properly configured!";
        }
        else
        {
            if (issues.Count > 0)
            {
                message += "ISSUES (must fix):\n";
                foreach (string issue in issues)
                {
                    message += "✗ " + issue + "\n";
                }
                message += "\n";
            }
            
            if (warnings.Count > 0)
            {
                message += "WARNINGS (recommended):\n";
                foreach (string warning in warnings)
                {
                    message += "⚠ " + warning + "\n";
                }
                message += "\n";
            }
            
            if (info.Count > 0)
            {
                message += "INFO:\n";
                foreach (string i in info)
                {
                    message += "ℹ " + i + "\n";
                }
            }
        }
        
        EditorUtility.DisplayDialog("Player Setup Check", message, "OK");
        
        // Select player in hierarchy
        Selection.activeGameObject = player;
    }
    
    void DisplayPlayerDetails()
    {
        GameObject player = FindPlayer();
        
        if (player == null)
        {
            EditorGUILayout.HelpBox("Player not found. Click 'Find and Fix Player Setup' to create one.", MessageType.Warning);
            return;
        }
        
        EditorGUILayout.LabelField("Player GameObject:", player.name);
        EditorGUILayout.LabelField("Tag:", player.CompareTag("Player") ? "Player ✓" : "Missing ✗");
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Components:", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        
        EditorGUILayout.LabelField("PlayerController:", player.GetComponent<PlayerController>() != null ? "✓" : "✗ Missing");
        EditorGUILayout.LabelField("PlayerHealth:", player.GetComponent<PlayerHealth>() != null ? "✓" : "✗ Missing");
        
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            EditorGUILayout.LabelField("Rigidbody2D:", "✓");
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Gravity Scale:", rb.gravityScale.ToString() + (rb.gravityScale == 0 ? " ✓" : " ✗"));
            EditorGUILayout.LabelField("Freeze Rotation:", rb.freezeRotation.ToString() + (rb.freezeRotation ? " ✓" : " ✗"));
            EditorGUILayout.LabelField("Collision Detection:", rb.collisionDetectionMode.ToString());
            EditorGUI.indentLevel--;
        }
        else
        {
            EditorGUILayout.LabelField("Rigidbody2D:", "✗ Missing");
        }
        
        EditorGUILayout.LabelField("SpriteRenderer:", player.GetComponent<SpriteRenderer>() != null ? "✓" : "⚠ Optional");
        EditorGUILayout.LabelField("BoxCollider2D:", player.GetComponent<BoxCollider2D>() != null ? "✓" : "⚠ Optional");
        EditorGUILayout.LabelField("Animator:", player.GetComponent<Animator>() != null ? "✓" : "ℹ Optional");
        
        EditorGUI.indentLevel--;
    }
    
    void DisplayEnemyDetails()
    {
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        
        if (enemies.Length == 0)
        {
            EditorGUILayout.HelpBox("No enemies found in scene.", MessageType.Info);
            return;
        }
        
        EditorGUILayout.LabelField($"Found {enemies.Length} enemy/enemies:", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        
        foreach (EnemyController enemy in enemies)
        {
            EditorGUILayout.LabelField(enemy.gameObject.name, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            EditorGUILayout.LabelField("EnemyController:", "✓");
            EditorGUILayout.LabelField("EnemyHealth:", enemy.GetComponent<EnemyHealth>() != null ? "✓" : "✗");
            EditorGUILayout.LabelField("Rigidbody2D:", enemy.GetComponent<Rigidbody2D>() != null ? "✓" : "✗");
            EditorGUILayout.LabelField("SimplePathfinding2D:", enemy.GetComponent<SimplePathfinding2D>() != null ? "✓" : "✗");
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }
        
        EditorGUI.indentLevel--;
    }
    
    GameObject FindPlayer()
    {
        // Try by tag first
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) return player;
        
        // Try by name
        player = GameObject.Find("Player");
        if (player != null) return player;
        
        // Try by component
        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null) return playerController.gameObject;
        
        return null;
    }
    
    bool TagExists(string tag)
    {
        try
        {
            GameObject testObj = new GameObject("TagTest");
            testObj.tag = tag;
            bool exists = testObj.CompareTag(tag);
            DestroyImmediate(testObj);
            return exists;
        }
        catch
        {
            return false;
        }
    }
}
