using UnityEngine;
using System.Collections;

public class PlayerDeath : MonoBehaviour
{
    [Header("Death Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string deathStateName = "Death"; // Tên state animation death trong Animator
    [SerializeField] private float fallbackDelay = 2f; // Thời gian chờ dự phòng nếu không tìm thấy animation state
    
    [Header("Components")]
    private Rigidbody2D rb;
    private PlayerController playerController;
    private Collider2D[] colliders;
    
    private bool isDead = false;
    private Coroutine deathSequenceCoroutine;
    
    void Awake()
    {
        // Tự động lấy components nếu chưa được gán
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        colliders = GetComponents<Collider2D>();
    }
    
    /// <summary>
    /// Xử lý khi player chết
    /// </summary>
    public void HandleDeath()
    {
        if (isDead)
        {
            Debug.LogWarning("[PlayerDeath] HandleDeath() đã được gọi rồi, bỏ qua...");
            return;
        }
        
        isDead = true;
        Debug.Log("[PlayerDeath] Player đã chết, bắt đầu death sequence");
        
        // Vô hiệu hóa player
        DisablePlayer();
        
        // Play death animation
        PlayDeathAnimation();
        
        // Bắt đầu death sequence
        deathSequenceCoroutine = StartCoroutine(DeathSequence());
    }
    
    /// <summary>
    /// Vô hiệu hóa player (controller, physics, colliders)
    /// </summary>
    private void DisablePlayer()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }
        
        foreach (Collider2D col in colliders)
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }
    }
    
    /// <summary>
    /// Play death animation
    /// </summary>
    private void PlayDeathAnimation()
    {
        if (animator == null)
        {
            Debug.LogWarning("[PlayerDeath] Animator không tìm thấy!");
            return;
        }
        
        if (string.IsNullOrEmpty(deathStateName))
        {
            Debug.LogWarning("[PlayerDeath] Không có death state name được thiết lập!");
            return;
        }
        
        animator.Play(deathStateName, 0, 0f);
        Debug.Log($"[PlayerDeath] Playing death animation: {deathStateName}");
    }
    
    /// <summary>
    /// Death sequence: chờ animation xong rồi load death scene
    /// </summary>
    private IEnumerator DeathSequence()
    {
        // Chờ animation death hoàn thành
        yield return StartCoroutine(WaitForDeathAnimation());
        
        // Sau khi animation xong, hiển thị death screen
        ShowDeathScreen();
    }
    
    /// <summary>
    /// Hiển thị death screen thông qua DeathManager
    /// </summary>
    private void ShowDeathScreen()
    {
        if (DeathManager.Instance != null)
        {
            DeathManager.Instance.ShowDeathScreen();
        }
        else
        {
            Debug.LogError("[PlayerDeath] DeathManager.Instance is null! Vui lòng đảm bảo DeathManager đã được thêm vào scene.");
        }
    }
    
    /// <summary>
    /// Chờ animation death hoàn thành
    /// </summary>
    private IEnumerator WaitForDeathAnimation()
    {
        if (animator == null || string.IsNullOrEmpty(deathStateName))
        {
            Debug.LogWarning("[PlayerDeath] Animator hoặc death state name null, dùng fallback delay");
            yield return new WaitForSeconds(fallbackDelay);
            yield break;
        }
        
        const int layerIndex = 0;
        const float maxWaitTime = 1f;
        float waitTime = 0f;
        
        // Chờ animation state được kích hoạt
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
        while (!stateInfo.IsName(deathStateName) && waitTime < maxWaitTime)
        {
            waitTime += Time.deltaTime;
            stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            yield return null;
        }
        
        if (!stateInfo.IsName(deathStateName))
        {
            Debug.LogWarning($"[PlayerDeath] Không tìm thấy animation state '{deathStateName}' sau {maxWaitTime} giây, dùng fallback delay");
            yield return new WaitForSeconds(fallbackDelay);
            yield break;
        }
        
        // Chờ animation chạy xong (normalizedTime >= 1)
        while (stateInfo.normalizedTime < 1f)
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            yield return null;
        }
        
        Debug.Log($"[PlayerDeath] Death animation đã hoàn thành (normalizedTime: {stateInfo.normalizedTime})");
        
        // Delay nhỏ để đảm bảo animation hoàn toàn kết thúc
        yield return new WaitForSeconds(0.1f);
    }
    
    void OnDestroy()
    {
        // Stop coroutine nếu GameObject bị destroy
        if (deathSequenceCoroutine != null)
        {
            StopCoroutine(deathSequenceCoroutine);
        }
    }
    
    /// <summary>
    /// Reset death state (dùng khi restart)
    /// </summary>
    public void ResetDeath()
    {
        isDead = false;
        
        // Stop death sequence coroutine nếu đang chạy
        if (deathSequenceCoroutine != null)
        {
            StopCoroutine(deathSequenceCoroutine);
            deathSequenceCoroutine = null;
        }
        
        // Kích hoạt lại player
        EnablePlayer();
    }
    
    /// <summary>
    /// Kích hoạt lại player (controller, physics, colliders)
    /// </summary>
    private void EnablePlayer()
    {
        if (rb != null)
        {
            rb.simulated = true;
        }
        
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        foreach (Collider2D col in colliders)
        {
            if (col != null)
            {
                col.enabled = true;
            }
        }
        
        // Reset animator về idle state
        if (animator != null)
        {
            animator.Play("Idle", 0, 0f);
        }
    }
}

