using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 2000f;
    [SerializeField] private float attackDamage = 150f;
    
    public event Action<float, float> OnHealthChanged; // currentHealth, maxHealth
    public event Action<PlayerHealth, float> OnDamageTaken; // player, damage amount
    
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float AttackDamage => attackDamage;
    
    private float currentHealth;
    private bool isDead = false;
    
    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Auto-subscribe to DamageNumberManager if it exists
        if (DamageNumberManager.Instance != null)
        {
            DamageNumberManager.Instance.SubscribeToPlayer(this);
        }
    }
    
    void OnEnable()
    {
        // Subscribe when enabled (in case manager wasn't ready at Start)
        if (DamageNumberManager.Instance != null)
        {
            DamageNumberManager.Instance.SubscribeToPlayer(this);
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe when destroyed
        if (DamageNumberManager.Instance != null)
        {
            DamageNumberManager.Instance.UnsubscribeFromPlayer(this);
        }
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnDamageTaken?.Invoke(this, damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void TakeHalfDamage()
    {
        TakeDamage(0.5f);
    }
    
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void HealHalf()
    {
        Heal(0.5f);
    }
    
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void SetAttackDamage(float newAttackDamage)
    {
        attackDamage = newAttackDamage;
    }
    
    private void Die()
    {
        if (isDead) return; // Tránh gọi nhiều lần
        
        isDead = true;
        Debug.Log("Player died!");
        
        // Gọi PlayerDeath để xử lý death sequence
        PlayerDeath playerDeath = GetComponent<PlayerDeath>();
        if (playerDeath != null)
        {
            playerDeath.HandleDeath();
        }
        else
        {
            Debug.LogWarning("PlayerHealth: Không tìm thấy PlayerDeath component! Vui lòng thêm PlayerDeath vào player GameObject.");
        }
    }
    
    /// <summary>
    /// Reset death state (dùng khi restart)
    /// </summary>
    public void ResetDeath()
    {
        isDead = false;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}

