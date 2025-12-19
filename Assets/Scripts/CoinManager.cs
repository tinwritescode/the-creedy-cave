using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    public int coinCount { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public void CollectCoin(int amount = 1)
    {
        coinCount += amount;

        // Đẩy coin vào Inventory
        if (InventoryController.Instance != null)
        {
            InventoryController.Instance.AddCoin(amount);
        }

        Debug.Log($"Coin collected. Total: {coinCount}");
    }
}
