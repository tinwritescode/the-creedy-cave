using UnityEngine;
using TMPro;

public class CoinUI : MonoBehaviour
{
    [SerializeField] private TMP_Text coinText;

    void Start()
    {
        InventoryController.Instance.OnCoinChanged += UpdateCoin;
        UpdateCoin(InventoryController.Instance.Coin);
    }

    void UpdateCoin(int value)
    {
        coinText.text = value.ToString() + " coin(s)";
    }

    void OnDestroy()
    {
        if (InventoryController.Instance != null)
            InventoryController.Instance.OnCoinChanged -= UpdateCoin;
    }
}
