using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float detectionRange = 10f; // Increased default range
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float attackRange = 1.5f; // Distance to attack player
    [SerializeField] private float attackCooldown = 1.5f; // Time between attacks
    [SerializeField] private string idleAnimationName = "Idle";
    [SerializeField] private string walkAnimationName = "Walk";
    [SerializeField] private string attack01AnimationName = "attack01";
    [SerializeField] private string attack02AnimationName = "attack02";
    [SerializeField] private string hurtAnimationName = "hurt";
    [SerializeField] private string deathAnimationName = "death";
    [SerializeField] private bool enableDebugLogs = true; // Toggle debug logging
    [SerializeField] private bool usePathfinding = true; // Use pathfinding or direct chase
    [SerializeField] private bool showDebugGizmos = true; // Show visual debug info in Scene view
    [SerializeField] private bool showOnScreenDebug = true; // Show on-screen debug text
    [SerializeField] private bool showDetailedGizmos = true; // Show detailed gizmo information
    [SerializeField] private KeyCode toggleDebugKey = KeyCode.F1; // Key to toggle debug display
    [SerializeField] private Vector2 debugTextOffset = new Vector2(0, 50); // Offset for on-screen debug text
    
    private float lastAttackTime = 0f;
    private bool isAttacking = false;
    private bool isHurt = false;
    private bool isDead = false;
    private float previousHealth = 0f;
    private bool isInitialized = false;
    private float hurtAnimationStartTime = 0f;
    
    private enum EnemyState
    {
        Idle,
        Chasing
    }
    
    private EnemyState currentState = EnemyState.Idle;
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;
    private EnemyHealth enemyHealth;
    private Rigidbody2D rb;
    private Animator animator;
    private SimplePathfinding2D pathfinding;
    private string currentAnimationState = "";
    private Vector2 lastMovementDirection = Vector2.zero;
    private Vector2 movement = Vector2.zero; // Movement vector calculated in Update, applied in FixedUpdate
    private bool isChasing = false;
    private float distanceToPlayer = 0f; // Cached distance for debug display
    private bool debugDisplayEnabled = true; // Current state of debug display
    
    void Start()
    {
        // Get or add Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0; // No gravity for top-down movement
        rb.freezeRotation = true; // Prevent rotation
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Better collision detection
        rb.bodyType = RigidbodyType2D.Dynamic; // Ensure it's dynamic for collisions
        
        if (enableDebugLogs)
        {
            Debug.Log($"[Enemy {gameObject.name}] Rigidbody2D setup: gravityScale={rb.gravityScale}, bodyType={rb.bodyType}, freezeRotation={rb.freezeRotation}");
        }
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRenderer not found on Enemy. Sprite flipping will not work.");
        }
        
        // Get Animator component
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("Animator not found on Enemy. Animation will not work.");
        }
        
        // Get or add EnemyHealth component
        enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth == null)
        {
            enemyHealth = gameObject.AddComponent<EnemyHealth>();
        }
        
        // Get or add EnemyHealthBar component for health bar above enemy
        EnemyHealthBar healthBar = GetComponent<EnemyHealthBar>();
        if (healthBar == null)
        {
            healthBar = gameObject.AddComponent<EnemyHealthBar>();
        }
        
        // Get or add SimplePathfinding2D component for 2D pathfinding
        pathfinding = GetComponent<SimplePathfinding2D>();
        if (pathfinding == null)
        {
            pathfinding = gameObject.AddComponent<SimplePathfinding2D>();
        }
        
        // Find player by tag (try multiple methods)
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            // Try finding by name as fallback
            player = GameObject.Find("Player");
        }
        if (player == null)
        {
            // Try finding PlayerController component
            PlayerController playerController = FindFirstObjectByType<PlayerController>();
            if (playerController != null)
            {
                player = playerController.gameObject;
            }
        }
        
        if (player != null)
        {
            playerTransform = player.transform;
            if (enableDebugLogs)
            {
                Debug.Log($"[Enemy {gameObject.name}] Found player at {playerTransform.position}");
            }
        }
        else
        {
            Debug.LogError($"[Enemy {gameObject.name}] Player not found! Make sure Player has 'Player' tag or is named 'Player'.");
        }
        
        // Subscribe to health events
        if (enemyHealth != null)
        {
            previousHealth = enemyHealth.CurrentHealth;
            enemyHealth.OnHealthChanged += OnHealthChanged;
            enemyHealth.OnDeath += OnDeath;
            isInitialized = true;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from health events
        if (enemyHealth != null)
        {
            enemyHealth.OnHealthChanged -= OnHealthChanged;
            enemyHealth.OnDeath -= OnDeath;
        }
    }
    
    
    void Update()
    {
        if (playerTransform == null || spriteRenderer == null) return;
        
        // Don't process if dead
        if (isDead)
        {
            movement = Vector2.zero;
            UpdateAnimation();
            return;
        }
        
        // Don't move or attack if currently attacking
        if (isAttacking)
        {
            movement = Vector2.zero;
            UpdateAnimation();
            return;
        }
        
        // Handle hurt animation completion
        if (isHurt)
        {
            if (animator != null)
            {
                // Wait a small amount of time for animation to start (at least 0.05 seconds)
                float timeSinceHurtStart = Time.time - hurtAnimationStartTime;
                if (timeSinceHurtStart < 0.05f)
                {
                    // Animation just started, wait a bit before checking
                    movement = Vector2.zero;
                    UpdateAnimation();
                    return;
                }
                
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                // Check if hurt animation exists and has finished, or if animation doesn't exist
                bool hurtAnimExists = !string.IsNullOrEmpty(hurtAnimationName);
                bool hurtAnimPlaying = hurtAnimExists && stateInfo.IsName(hurtAnimationName);
                bool hurtAnimFinished = hurtAnimPlaying && stateInfo.normalizedTime >= 1.0f;
                
                if (hurtAnimFinished)
                {
                    // Hurt animation finished
                    isHurt = false;
                    currentAnimationState = ""; // Reset to allow animation change
                    if (enableDebugLogs)
                    {
                        Debug.Log($"[Enemy {gameObject.name}] Hurt animation finished");
                    }
                }
                else if (hurtAnimPlaying)
                {
                    // Still playing hurt animation
                    movement = Vector2.zero;
                    UpdateAnimation();
                    return; // Still playing hurt animation, don't process other logic
                }
                else if (hurtAnimExists)
                {
                    // Animation name exists but not playing - might be an issue, but wait a bit more
                    if (timeSinceHurtStart > 0.2f)
                    {
                        // Been waiting too long, animation might not exist in animator
                        if (enableDebugLogs)
                        {
                            Debug.LogWarning($"[Enemy {gameObject.name}] Hurt animation '{hurtAnimationName}' not found in Animator Controller");
                        }
                        isHurt = false;
                        currentAnimationState = ""; // Reset to allow animation change
                    }
                    else
                    {
                        // Still waiting for animation to start
                        movement = Vector2.zero;
                        UpdateAnimation();
                        return;
                    }
                }
                else
                {
                    // Hurt animation doesn't exist, just clear the hurt state
                    isHurt = false;
                }
            }
            else
            {
                // No animator, just clear hurt state
                isHurt = false;
            }
        }
        
        // Calculate distance to player
        distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // Toggle debug display with key
        if (Input.GetKeyDown(toggleDebugKey))
        {
            debugDisplayEnabled = !debugDisplayEnabled;
        }
        
        // Debug logging
        if (enableDebugLogs && Time.frameCount % 60 == 0) // Log every 60 frames to avoid spam
        {
            Debug.Log($"[Enemy {gameObject.name}] Distance={distanceToPlayer:F2}, Range={detectionRange}, State={currentState}, isChasing={isChasing}, movement={movement}, RB.velocity={rb.linearVelocity}");
        }
        
        // Check if player is in attack range
        if (distanceToPlayer <= attackRange)
        {
            // Stop moving and attack
            movement = Vector2.zero;
            TryAttack(distanceToPlayer);
        }
        else if (distanceToPlayer <= detectionRange)
        {
            // Chase player
            isChasing = true;
            currentState = EnemyState.Chasing;
            
            // Use 2D pathfinding to get direction toward player (avoids obstacles)
            Vector2 directionToPlayer;
            if (usePathfinding && pathfinding != null)
            {
                directionToPlayer = pathfinding.GetDirectionToTarget(transform.position, playerTransform.position);
                // Fallback to direct if pathfinding returns zero
                if (directionToPlayer.magnitude < 0.1f)
                {
                    directionToPlayer = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
                    if (enableDebugLogs && Time.frameCount % 60 == 0)
                    {
                        Debug.LogWarning($"[Enemy {gameObject.name}] Pathfinding returned zero vector, using direct direction");
                    }
                }
            }
            else
            {
                // Direct chase (simpler, like reference code)
                directionToPlayer = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            }
            
            movement = directionToPlayer;
            lastMovementDirection = directionToPlayer;
            
            if (enableDebugLogs && Time.frameCount % 60 == 0)
            {
                Debug.Log($"[Enemy {gameObject.name}] Chasing: direction={directionToPlayer}, magnitude={directionToPlayer.magnitude}");
            }
        }
        else
        {
            // Player out of range - idle
            isChasing = false;
            currentState = EnemyState.Idle;
            movement = Vector2.zero;
        }
        
        // Update animation based on movement
        UpdateAnimation();
        
        // Flip sprite based on movement direction
        if (movement.magnitude > 0.1f)
        {
            if (movement.x < -0.1f)
            {
                spriteRenderer.flipX = true;
            }
            else if (movement.x > 0.1f)
            {
                spriteRenderer.flipX = false;
            }
        }
    }
    
    void FixedUpdate()
    {
        // Apply movement in FixedUpdate (like reference code)
        if (rb == null) return;
        
        if (movement.magnitude > 0.1f)
        {
            Vector2 newVelocity = movement * chaseSpeed;
            rb.linearVelocity = newVelocity;
            
            if (enableDebugLogs && Time.frameCount % 60 == 0)
            {
                Debug.Log($"[Enemy {gameObject.name}] FixedUpdate: movement={movement}, speed={chaseSpeed}, velocity={rb.linearVelocity}");
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
    
    private void UpdateAnimation()
    {
        if (animator == null) return;
        
        // Don't change animation if dead
        if (isDead)
        {
            return;
        }
        
        // Don't change animation if currently hurt
        if (isHurt)
        {
            return;
        }
        
        // Don't change animation if currently attacking
        if (isAttacking)
        {
            // Check if attack animation has finished
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName(attack01AnimationName) || stateInfo.IsName(attack02AnimationName))
            {
                if (stateInfo.normalizedTime >= 1.0f)
                {
                    // Attack animation finished, return to normal animations
                    isAttacking = false;
                    currentAnimationState = ""; // Reset to allow animation change
                }
            }
            return;
        }
        
        // Determine if moving based on movement vector
        bool isMoving = movement.magnitude > 0.1f;
        string targetAnimation = isMoving ? walkAnimationName : idleAnimationName;
        
        // Check if we need to switch animations
        if (currentAnimationState != targetAnimation)
        {
            animator.Play(targetAnimation, 0, 0f);
            currentAnimationState = targetAnimation;
        }
        // Check if current animation has finished and restart it to create loop effect
        else if (currentAnimationState != "")
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            // If animation has finished (normalizedTime >= 1.0) and it's still the same state, restart it
            if (stateInfo.IsName(currentAnimationState) && stateInfo.normalizedTime >= 1.0f)
            {
                animator.Play(currentAnimationState, 0, 0f); // Restart from beginning to create loop
            }
        }
    }
    
    private void TryAttack(float distanceToPlayer)
    {
        // Check if enough time has passed since last attack
        if (Time.time - lastAttackTime < attackCooldown) return;
        
        // Don't attack if already attacking
        if (isAttacking) return;
        
        // Check if player is still in range and has health component
        if (playerTransform == null) return;
        
        PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
        if (playerHealth != null && enemyHealth != null)
        {
            if (distanceToPlayer <= attackRange)
            {
                // Perform attack
                isAttacking = true;
                
                // Play attack animation (randomly choose between attack01 and attack02)
                string attackAnimation = Random.Range(0, 2) == 0 ? attack01AnimationName : attack02AnimationName;
                if (animator != null)
                {
                    animator.Play(attackAnimation, 0, 0f);
                    currentAnimationState = attackAnimation;
                }
                
                // Deal damage to player
                playerHealth.TakeDamage(enemyHealth.AttackDamage);
                lastAttackTime = Time.time;
                
                if (enableDebugLogs)
                {
                    Debug.Log($"[Enemy {gameObject.name}] Attacked player for {enemyHealth.AttackDamage} damage!");
                }
                
                // Reset attack flag after animation (handled in UpdateAnimation)
            }
        }
    }
    
    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        // Play hurt animation when taking damage (but not if dead or dying)
        if (!isDead && currentHealth > 0 && animator != null && isInitialized)
        {
            // Check if health actually decreased (damage was taken)
            if (currentHealth < previousHealth && !isHurt && !isAttacking)
            {
                PlayHurtAnimation();
            }
        }
        
        // Update previous health for next comparison
        previousHealth = currentHealth;
    }
    
    private void OnDeath()
    {
        if (isDead) return; // Already dead
        
        isDead = true;
        
        // Stop all movement
        movement = Vector2.zero;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        // Disable colliders (optional - you may want to keep them for cleanup)
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
        
        // Play death animation
        if (animator != null)
        {
            animator.Play(deathAnimationName, 0, 0f);
            currentAnimationState = deathAnimationName;
        }
        
        Debug.Log($"Enemy {gameObject.name} died!");
    }
    
    private void PlayHurtAnimation()
    {
        if (animator == null || isDead) return;
        
        isHurt = true;
        hurtAnimationStartTime = Time.time;
        movement = Vector2.zero; // Stop movement during hurt animation
        
        if (animator != null)
        {
            animator.Play(hurtAnimationName, 0, 0f);
            currentAnimationState = hurtAnimationName;
            
            if (enableDebugLogs)
            {
                Debug.Log($"[Enemy {gameObject.name}] Playing hurt animation: {hurtAnimationName}");
            }
        }
    }
    
    // Visual debug in Scene view
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        // Draw detection range
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // Yellow with transparency
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // Red with transparency
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw direction to player if found
        if (playerTransform != null)
        {
            Vector3 direction = (playerTransform.position - transform.position);
            float distance = direction.magnitude;
            direction.Normalize();
            
            // Draw line to player
            Gizmos.color = distance <= detectionRange ? Color.green : Color.gray;
            Gizmos.DrawRay(transform.position, direction * Mathf.Min(distance, detectionRange));
            
            // Draw current velocity
            if (rb != null && rb.linearVelocity.magnitude > 0.1f)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(transform.position, rb.linearVelocity * 0.5f);
            }
            
            // Draw movement vector
            if (movement.magnitude > 0.1f)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawRay(transform.position, movement * 2f);
            }
            
            // Detailed gizmos
            if (showDetailedGizmos)
            {
                // Draw player position marker
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(playerTransform.position, 0.3f);
                
                // Draw pathfinding direction if using pathfinding
                if (usePathfinding && pathfinding != null && distance <= detectionRange)
                {
                    Vector2 pathDirection = pathfinding.GetDirectionToTarget(transform.position, playerTransform.position);
                    if (pathDirection.magnitude > 0.1f)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawRay(transform.position, pathDirection * 3f);
                    }
                }
            }
        }
    }
    
    // On-screen debug text
    void OnGUI()
    {
        if (!showOnScreenDebug || !debugDisplayEnabled) return;
        if (playerTransform == null) return;
        
        // Get screen position of enemy
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + (Vector3)debugTextOffset);
        
        // Only show if enemy is on screen
        if (screenPos.z < 0) return;
        
        // Convert to GUI coordinates (Y is flipped)
        float guiY = Screen.height - screenPos.y;
        
        // Create debug text
        string debugText = $"Enemy: {gameObject.name}\n";
        debugText += $"State: {currentState}\n";
        debugText += $"Distance: {distanceToPlayer:F2}m\n";
        debugText += $"Detection Range: {detectionRange}m\n";
        debugText += $"Attack Range: {attackRange}m\n";
        debugText += $"Is Chasing: {isChasing}\n";
        debugText += $"Is Attacking: {isAttacking}\n";
        debugText += $"Is Hurt: {isHurt}\n";
        debugText += $"Is Dead: {isDead}\n";
        
        if (rb != null)
        {
            debugText += $"Velocity: {rb.linearVelocity.magnitude:F2} m/s\n";
            debugText += $"Velocity: ({rb.linearVelocity.x:F2}, {rb.linearVelocity.y:F2})\n";
        }
        
        debugText += $"Movement: ({movement.x:F2}, {movement.y:F2})\n";
        debugText += $"Chase Speed: {chaseSpeed}\n";
        debugText += $"Use Pathfinding: {usePathfinding}\n";
        
        if (enemyHealth != null)
        {
            debugText += $"Health: {enemyHealth.CurrentHealth:F0}/{enemyHealth.MaxHealth:F0}\n";
        }
        
        // Calculate text size
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 12;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.UpperLeft;
        style.normal.background = MakeTex(2, 2, new Color(0, 0, 0, 0.7f)); // Semi-transparent black background
        
        Vector2 textSize = style.CalcSize(new GUIContent(debugText));
        
        // Draw background box
        GUI.Box(new Rect(screenPos.x, guiY, textSize.x + 10, textSize.y + 10), "", style);
        
        // Draw text
        GUI.Label(new Rect(screenPos.x + 5, guiY + 5, textSize.x, textSize.y), debugText, style);
        
    }
    
    // Helper to create texture for background
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
