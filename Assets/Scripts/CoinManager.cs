using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public int coinCount = 0;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Called when a coin is collected. Increments the coin count.
    /// </summary>
    public void CollectCoin()
    {
        coinCount++;
    }
}
