using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance;
    public GameObject cellPrefab;
    public Transform gridContainer;
    public int rows = 5;
    public int columns = 10;
    public float cellSize = 64f;
    public float spacing = 4f;

    private CellController selectedCell;
    private WeaponData[] items;

    void Awake()
    {
        Instance = this;
        items = new WeaponData[rows * columns];
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                GameObject cell = Instantiate(cellPrefab, gridContainer);
                float x = col * (cellSize + spacing);
                float y = -row * (cellSize + spacing);
                cell.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
            }
        }
    }

    public void SelectCell(CellController cell)
    {
        if (selectedCell == cell)
        {
            DeselectCell();
            return;
        }

        if (selectedCell != null)
            selectedCell.SetHighlight(false);

        selectedCell = cell;
        selectedCell.SetHighlight(true);
    }

    public void DeselectCell()
    {
        if (selectedCell != null)
            selectedCell.SetHighlight(false);
        selectedCell = null;
    }

    public void CloseInventory()
    {
        Debug.Log("CloseInventory called");
        DeselectCell();
        gameObject.SetActive(false);
    }

    public bool AddItem(WeaponData weapon)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = weapon;
                var cell = gridContainer.GetChild(i).GetComponent<CellController>();
                cell.SetItem(weapon);
                return true;
            }
        }
        return false; // Inventory full
    }
}

