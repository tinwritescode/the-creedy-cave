using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float detectionRange = 5f;
    
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;
    private EnemyHealth enemyHealth;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRenderer not found on Enemy. Sprite flipping will not work.");
        }
        
        // Get or add EnemyHealth component
        enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth == null)
        {
            enemyHealth = gameObject.AddComponent<EnemyHealth>();
        }
        
        // Find player by tag
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("Player not found. Make sure Player has 'Player' tag.");
        }
    }

    void Update()
    {
        // Don't update if in combat
        if (CombatManager.Instance != null && CombatManager.Instance.IsInCombat)
        {
            return;
        }
        
        if (playerTransform == null || spriteRenderer == null) return;
        
        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // If player is within detection range, flip enemy to face player
        if (distanceToPlayer <= detectionRange)
        {
            // Determine direction to player
            float directionToPlayer = playerTransform.position.x - transform.position.x;
            
            // Flip sprite based on player position
            if (directionToPlayer < 0)
            {
                // Player is to the left - flip sprite
                spriteRenderer.flipX = true;
            }
            else if (directionToPlayer > 0)
            {
                // Player is to the right - unflip sprite
                spriteRenderer.flipX = false;
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null && enemyHealth != null)
            {
                EnsureCombatManagerExists();
                if (CombatManager.Instance != null)
                {
                    CombatManager.Instance.StartCombat(playerHealth, enemyHealth);
                }
            }
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null && enemyHealth != null)
            {
                EnsureCombatManagerExists();
                if (CombatManager.Instance != null)
                {
                    CombatManager.Instance.StartCombat(playerHealth, enemyHealth);
                }
            }
        }
    }
    
    void EnsureCombatManagerExists()
    {
        if (CombatManager.Instance == null)
        {
            GameObject combatManagerObj = new GameObject("CombatManager");
            combatManagerObj.AddComponent<CombatManager>();
        }
    }
}
