using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private PlayerHealth health;

    [SerializeField] private string deathAnimation = "Death";

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<PlayerHealth>();

        if (health != null)
        {
            health.OnDeath += OnPlayerDeath;
        }
    }

    private void OnPlayerDeath()
    {
        // Stop movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // Play death animation
        if (animator != null)
        {
            animator.Play(deathAnimation, 0, 0f);
        }

        // Disable controller
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        Debug.Log("🪦 Player death sequence started");
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDeath -= OnPlayerDeath;
        }
    }
}
