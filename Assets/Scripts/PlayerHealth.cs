using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 2000f;
    [SerializeField] private float currentHealth;
    [SerializeField] private float attackDamage = 150f;
    
    public event Action<float, float> OnHealthChanged; // currentHealth, maxHealth
    
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float AttackDamage => attackDamage;
    private bool isDead = false;
    public bool IsDead => isDead;
    public event Action OnDeath;

    
    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // trừ nửa ống máu hiện tại
    public void TakeHalfDamage()
    {
        TakeDamage(currentHealth * 0.5f);
    }

    
    public void Heal(float amount)
    {
        if (isDead) return;
        
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void HealHalf()
    {
        Heal(maxHealth * 0.5f); // heal nửa ống máu tối đa
    }
    
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player died!");
        
        OnDeath?.Invoke();
    }
}

