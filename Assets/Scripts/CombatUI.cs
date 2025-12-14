using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CombatUI : MonoBehaviour
{
    [SerializeField] private GameObject combatPanel;
    [SerializeField] private Image playerHealthBarFill;
    [SerializeField] private Image enemyHealthBarFill;
    [SerializeField] private Text playerHealthText;
    [SerializeField] private Text enemyHealthText;
    [SerializeField] private Button runAwayButton;
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Transform damageNumberParent;
    [SerializeField] private Transform playerDamageSpawnPoint;
    [SerializeField] private Transform enemyDamageSpawnPoint;
    
    private PlayerHealth currentPlayer;
    private EnemyHealth currentEnemy;
    private List<GameObject> activeDamageNumbers = new List<GameObject>();
    
    void Start()
    {
        // Ensure CombatManager exists
        EnsureCombatManagerExists();
        
        // Subscribe to combat events
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatStarted += OnCombatStarted;
            CombatManager.Instance.OnCombatEnded += OnCombatEnded;
            CombatManager.Instance.OnDamageDealt += OnDamageDealt;
        }
    }
    
    void EnsureCombatManagerExists()
    {
        if (CombatManager.Instance == null)
        {
            GameObject combatManagerObj = new GameObject("CombatManager");
            combatManagerObj.AddComponent<CombatManager>();
        }
        
        // Setup run away button
        if (runAwayButton != null)
        {
            runAwayButton.onClick.AddListener(OnRunAwayClicked);
        }
        
        // Hide combat panel initially
        if (combatPanel != null)
        {
            combatPanel.SetActive(false);
        }
        
        // Find damage number parent if not assigned
        if (damageNumberParent == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
            }
            if (canvas != null)
            {
                damageNumberParent = canvas.transform;
            }
        }
    }
    
    void OnDestroy()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatStarted -= OnCombatStarted;
            CombatManager.Instance.OnCombatEnded -= OnCombatEnded;
            CombatManager.Instance.OnDamageDealt -= OnDamageDealt;
        }
        
        if (runAwayButton != null)
        {
            runAwayButton.onClick.RemoveListener(OnRunAwayClicked);
        }
    }
    
    private void OnCombatStarted(PlayerHealth player, EnemyHealth enemy)
    {
        currentPlayer = player;
        currentEnemy = enemy;
        
        // Show combat panel
        if (combatPanel != null)
        {
            combatPanel.SetActive(true);
        }
        
        // Subscribe to health changes
        if (currentPlayer != null)
        {
            currentPlayer.OnHealthChanged += UpdatePlayerHealthBar;
            UpdatePlayerHealthBar(currentPlayer.CurrentHealth, currentPlayer.MaxHealth);
        }
        
        if (currentEnemy != null)
        {
            currentEnemy.OnHealthChanged += UpdateEnemyHealthBar;
            UpdateEnemyHealthBar(currentEnemy.CurrentHealth, currentEnemy.MaxHealth);
        }
    }
    
    private void OnCombatEnded()
    {
        // Hide combat panel
        if (combatPanel != null)
        {
            combatPanel.SetActive(false);
        }
        
        // Unsubscribe from health changes
        if (currentPlayer != null)
        {
            currentPlayer.OnHealthChanged -= UpdatePlayerHealthBar;
        }
        
        if (currentEnemy != null)
        {
            currentEnemy.OnHealthChanged -= UpdateEnemyHealthBar;
        }
        
        currentPlayer = null;
        currentEnemy = null;
        
        // Clean up damage numbers
        foreach (GameObject damageNum in activeDamageNumbers)
        {
            if (damageNum != null)
            {
                Destroy(damageNum);
            }
        }
        activeDamageNumbers.Clear();
    }
    
    private void OnDamageDealt(float damage, bool isPlayerAttacking)
    {
        // Determine spawn position
        Vector3 spawnPosition;
        if (isPlayerAttacking && enemyDamageSpawnPoint != null)
        {
            spawnPosition = enemyDamageSpawnPoint.position;
        }
        else if (!isPlayerAttacking && playerDamageSpawnPoint != null)
        {
            spawnPosition = playerDamageSpawnPoint.position;
        }
        else
        {
            // Fallback to world positions
            if (isPlayerAttacking && currentEnemy != null)
            {
                spawnPosition = currentEnemy.transform.position + Vector3.up * 1f;
            }
            else if (currentPlayer != null)
            {
                spawnPosition = currentPlayer.transform.position + Vector3.up * 1f;
            }
            else
            {
                spawnPosition = Vector3.zero;
            }
        }
        
        // Create damage number
        GameObject damageNumberObj;
        if (damageNumberPrefab != null)
        {
            damageNumberObj = Instantiate(damageNumberPrefab, damageNumberParent != null ? damageNumberParent : transform);
        }
        else
        {
            // Create basic damage number if no prefab
            damageNumberObj = new GameObject("DamageNumber");
            if (damageNumberParent != null)
            {
                damageNumberObj.transform.SetParent(damageNumberParent);
            }
            else
            {
                damageNumberObj.transform.SetParent(transform);
            }
            DamageNumber damageNumber = damageNumberObj.AddComponent<DamageNumber>();
            damageNumber.SetDamage(damage);
        }
        
        DamageNumber damageNumComponent = damageNumberObj.GetComponent<DamageNumber>();
        if (damageNumComponent == null)
        {
            damageNumComponent = damageNumberObj.AddComponent<DamageNumber>();
        }
        
        damageNumComponent.SetDamage(damage);
        damageNumComponent.SetWorldPosition(spawnPosition);
        
        activeDamageNumbers.Add(damageNumberObj);
    }
    
    private void UpdatePlayerHealthBar(float currentHealth, float maxHealth)
    {
        if (playerHealthBarFill != null)
        {
            playerHealthBarFill.fillAmount = maxHealth > 0 ? currentHealth / maxHealth : 0f;
        }
        
        if (playerHealthText != null)
        {
            playerHealthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }
    
    private void UpdateEnemyHealthBar(float currentHealth, float maxHealth)
    {
        if (enemyHealthBarFill != null)
        {
            enemyHealthBarFill.fillAmount = maxHealth > 0 ? currentHealth / maxHealth : 0f;
        }
        
        if (enemyHealthText != null)
        {
            enemyHealthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }
    
    private void OnRunAwayClicked()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.EndCombat();
        }
    }
}

