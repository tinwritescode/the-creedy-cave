using UnityEngine;
using System.Collections;

public class PlayerDeath : MonoBehaviour
{
    [Header("Death Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string deathTriggerName = "Death"; // Tên trigger animation death
    [SerializeField] private string deathStateName = "Death"; // Tên state animation death trong Animator
    [SerializeField] private float fallbackDelay = 2f; // Thời gian chờ dự phòng nếu không tìm thấy animation state
    
    [Header("Components")]
    private Rigidbody2D rb;
    private PlayerController playerController;
    private Collider2D[] colliders;
    
    private bool isDead = false;
    
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
        if (isDead) return; // Tránh gọi nhiều lần
        
        isDead = true;
        
        // Vô hiệu hóa player controller TRƯỚC để tránh override animation
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Dừng movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false; // Tắt physics simulation
        }
        
        // Vô hiệu hóa colliders
        foreach (Collider2D col in colliders)
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }
        
        // Play death animation - dùng Play() thay vì trigger để đảm bảo chạy
        if (animator != null)
        {
            if (!string.IsNullOrEmpty(deathStateName))
            {
                // Dùng Play() để chắc chắn animation chạy
                animator.Play(deathStateName, 0, 0f);
                Debug.Log($"[PlayerDeath] Playing death animation: {deathStateName}");
            }
            else if (!string.IsNullOrEmpty(deathTriggerName))
            {
                // Fallback: dùng trigger nếu không có state name
                animator.SetTrigger(deathTriggerName);
                Debug.Log($"[PlayerDeath] Triggering death animation: {deathTriggerName}");
            }
            else
            {
                Debug.LogWarning("[PlayerDeath] Không có death trigger name hoặc state name được thiết lập!");
            }
        }
        else
        {
            Debug.LogWarning("[PlayerDeath] Animator không tìm thấy!");
        }
        
        // Bắt đầu death sequence
        StartCoroutine(DeathSequence());
    }
    
    /// <summary>
    /// Death sequence: chờ animation xong rồi mới pause game và hiện death screen
    /// </summary>
    private IEnumerator DeathSequence()
    {
        // Chờ animation death hoàn thành
        yield return StartCoroutine(WaitForDeathAnimation());
        
        // Sau khi animation xong, hiển thị death screen (sẽ pause game)
        if (DeathManager.Instance != null)
        {
            DeathManager.Instance.ShowDeathScreen();
        }
        else
        {
            Debug.LogError("DeathManager.Instance is null! Vui lòng đảm bảo DeathManager đã được thêm vào scene.");
        }
    }
    
    /// <summary>
    /// Chờ animation death hoàn thành
    /// </summary>
    private IEnumerator WaitForDeathAnimation()
    {
        if (animator == null)
        {
            Debug.LogWarning("[PlayerDeath] Animator null, dùng fallback delay");
            yield return new WaitForSeconds(fallbackDelay);
            yield break;
        }
        
        // Tìm layer chứa death animation (thường là layer 0)
        int layerIndex = 0;
        
        // Nếu có state name, chờ animation đó chạy xong
        if (!string.IsNullOrEmpty(deathStateName))
        {
            // Chờ cho đến khi animation state được kích hoạt
            float waitTime = 0f;
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            
            while (!stateInfo.IsName(deathStateName) && waitTime < 1f)
            {
                waitTime += Time.deltaTime;
                stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
                yield return null;
            }
            
            if (!stateInfo.IsName(deathStateName))
            {
                Debug.LogWarning($"[PlayerDeath] Không tìm thấy animation state '{deathStateName}' sau 1 giây, dùng fallback delay");
                yield return new WaitForSeconds(fallbackDelay);
                yield break;
            }
            
            // Chờ animation chạy xong (normalizedTime >= 1)
            stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            while (stateInfo.IsName(deathStateName) && stateInfo.normalizedTime < 1f)
            {
                stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
                yield return null;
            }
            
            Debug.Log($"[PlayerDeath] Death animation đã hoàn thành (normalizedTime: {stateInfo.normalizedTime})");
        }
        else
        {
            // Nếu không có state name, dùng fallback delay
            Debug.LogWarning("[PlayerDeath] Không có death state name, dùng fallback delay");
            yield return new WaitForSeconds(fallbackDelay);
        }
        
        // Thêm một chút delay nhỏ để đảm bảo animation đã hoàn toàn kết thúc
        yield return new WaitForSeconds(0.1f);
    }
    
    /// <summary>
    /// Reset death state (dùng khi restart)
    /// </summary>
    public void ResetDeath()
    {
        isDead = false;
        
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
    }
}
