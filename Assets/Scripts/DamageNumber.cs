using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private Text damageText;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float floatSpeed = 50f;
    [SerializeField] private Color damageColor = Color.red;
    
    private RectTransform rectTransform;
    private Canvas canvas;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }
        
        // Find or create canvas
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }
        
        // Setup text if not assigned
        if (damageText == null)
        {
            damageText = GetComponentInChildren<Text>();
            if (damageText == null)
            {
                GameObject textObj = new GameObject("DamageText");
                textObj.transform.SetParent(transform);
                damageText = textObj.AddComponent<Text>();
                rectTransform = textObj.GetComponent<RectTransform>();
                if (rectTransform == null)
                {
                    rectTransform = textObj.AddComponent<RectTransform>();
                }
            }
        }
        
        // Setup text properties
        if (damageText != null)
        {
            damageText.color = damageColor;
            damageText.alignment = TextAnchor.MiddleCenter;
            damageText.fontSize = 36;
            damageText.fontStyle = FontStyle.Bold;
        }
        
        // Start auto-destroy coroutine
        StartCoroutine(DestroyAfterDelay());
        
        // Start float animation
        StartCoroutine(FloatAnimation());
    }
    
    public void SetDamage(float damage)
    {
        if (damageText != null)
        {
            damageText.text = $"-{Mathf.CeilToInt(damage)}";
        }
    }
    
    public void SetWorldPosition(Vector3 worldPos)
    {
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            if (rectTransform != null)
            {
                rectTransform.position = screenPos;
            }
        }
        else if (canvas != null)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 viewportPos = Camera.main.WorldToViewportPoint(worldPos);
            if (rectTransform != null && canvasRect != null)
            {
                rectTransform.anchoredPosition = new Vector2(
                    (viewportPos.x - 0.5f) * canvasRect.sizeDelta.x,
                    (viewportPos.y - 0.5f) * canvasRect.sizeDelta.y
                );
            }
        }
    }
    
    private IEnumerator FloatAnimation()
    {
        float elapsed = 0f;
        Vector3 startPos = rectTransform != null ? rectTransform.position : Vector3.zero;
        
        while (elapsed < displayDuration)
        {
            elapsed += Time.deltaTime;
            if (rectTransform != null)
            {
                rectTransform.position = startPos + Vector3.up * (elapsed * floatSpeed);
                
                // Fade out
                if (damageText != null)
                {
                    Color c = damageText.color;
                    c.a = 1f - (elapsed / displayDuration);
                    damageText.color = c;
                }
            }
            yield return null;
        }
    }
    
    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        Destroy(gameObject);
    }
}

