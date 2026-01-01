using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance;
    public GameObject cellPrefab;
    public Transform gridContainer;
    public int rows = 5;
    public int columns = 10;
    public float cellSize = 64f;
    public float spacing = 4f;
    
    [Header("Equipment Slots")]
    [Tooltip("Weapon Cell GameObject (should have CellController component, like inventory cells)")]
    public GameObject weaponCellGameObject;
    [Tooltip("Armor Cell GameObject (should have CellController component)")]
    public GameObject armorCellGameObject;
    [Tooltip("Hat Cell GameObject (should have CellController component)")]
    public GameObject hatCellGameObject;
    [Tooltip("Gloves Cell GameObject (should have CellController component)")]
    public GameObject glovesCellGameObject;
    [Tooltip("Shoes Cell GameObject (should have CellController component)")]
    public GameObject shoesCellGameObject;
    [Tooltip("Use button GameObject (will get Button component from this)")]
    public GameObject useButtonGameObject;
    [Tooltip("Drop button GameObject (will get Button component from this)")]
    public GameObject dropButtonGameObject;
    
    private CellController weaponCell;
    private CellController armorCell;
    private CellController hatCell;
    private CellController glovesCell;
    private CellController shoesCell;
    private Button useButton;
    private Button dropButton;

    private CellController selectedCell;
    private ItemData[] items;
    
    // Public property to access weapon cell
    public CellController WeaponCell => weaponCell;
    
    void Awake()
    {
        Instance = this;
        items = new ItemData[rows * columns];
        GenerateGrid();
        
        // Get CellController component from weaponCellGameObject (same as inventory cells)
        if (weaponCellGameObject != null)
        {
            Debug.Log($"[InventoryController] weaponCellGameObject is assigned: {weaponCellGameObject.name}");
            weaponCell = weaponCellGameObject.GetComponent<CellController>();
            if (weaponCell != null)
            {
                Debug.Log($"[InventoryController] ✓ Found CellController component on '{weaponCellGameObject.name}'");
            }
            else
            {
                Debug.LogError($"[InventoryController] ✗ weaponCellGameObject '{weaponCellGameObject.name}' does not have a CellController component! " +
                    $"Please add a CellController component to this GameObject (same as inventory cells).");
            }
        }
        else
        {
            Debug.LogWarning("[InventoryController] weaponCellGameObject is not assigned in Inspector.");
            // Auto-find as fallback (try to find by name or tag)
            GameObject weaponCellObj = GameObject.Find("WeaponCell");
            if (weaponCellObj != null)
            {
                weaponCell = weaponCellObj.GetComponent<CellController>();
                if (weaponCell != null)
                {
                    Debug.Log($"Auto-found WeaponCell by name: {weaponCellObj.name}");
                }
            }
            
            if (weaponCell == null)
            {
                Debug.LogWarning("WeaponCell not found. Please drag a Cell GameObject (with CellController component) from the scene into the 'weaponCellGameObject' field.");
            }
        }
        
        // Initialize armor cell
        if (armorCellGameObject != null)
        {
            armorCell = armorCellGameObject.GetComponent<CellController>();
            if (armorCell == null)
            {
                Debug.LogWarning($"[InventoryController] armorCellGameObject '{armorCellGameObject.name}' does not have a CellController component!");
            }
        }
        else
        {
            GameObject armorCellObj = GameObject.Find("ArmorCell");
            if (armorCellObj != null)
            {
                armorCell = armorCellObj.GetComponent<CellController>();
            }
        }
        
        // Initialize hat cell
        if (hatCellGameObject != null)
        {
            hatCell = hatCellGameObject.GetComponent<CellController>();
            if (hatCell == null)
            {
                Debug.LogWarning($"[InventoryController] hatCellGameObject '{hatCellGameObject.name}' does not have a CellController component!");
            }
        }
        else
        {
            GameObject hatCellObj = GameObject.Find("HatCell");
            if (hatCellObj != null)
            {
                hatCell = hatCellObj.GetComponent<CellController>();
            }
        }
        
        // Initialize gloves cell
        if (glovesCellGameObject != null)
        {
            glovesCell = glovesCellGameObject.GetComponent<CellController>();
            if (glovesCell == null)
            {
                Debug.LogWarning($"[InventoryController] glovesCellGameObject '{glovesCellGameObject.name}' does not have a CellController component!");
            }
        }
        else
        {
            GameObject glovesCellObj = GameObject.Find("GlovesCell");
            if (glovesCellObj != null)
            {
                glovesCell = glovesCellObj.GetComponent<CellController>();
            }
        }
        
        // Initialize shoes cell
        if (shoesCellGameObject != null)
        {
            shoesCell = shoesCellGameObject.GetComponent<CellController>();
            if (shoesCell == null)
            {
                Debug.LogWarning($"[InventoryController] shoesCellGameObject '{shoesCellGameObject.name}' does not have a CellController component!");
            }
        }
        else
        {
            GameObject shoesCellObj = GameObject.Find("ShoesCell");
            if (shoesCellObj != null)
            {
                shoesCell = shoesCellObj.GetComponent<CellController>();
            }
        }
        
        // Get Button component from useButtonGameObject
        if (useButtonGameObject != null)
        {
            useButton = useButtonGameObject.GetComponent<Button>();
            if (useButton != null)
            {
                Debug.Log($"Found Use button from useButtonGameObject: {useButtonGameObject.name}");
            }
            else
            {
                Debug.LogWarning($"useButtonGameObject '{useButtonGameObject.name}' does not have a Button component!");
            }
        }
        else
        {
            // Auto-find as fallback
            Button[] buttons = GetComponentsInChildren<Button>();
            foreach (Button btn in buttons)
            {
                if (btn.name.ToLower().Contains("use"))
                {
                    useButton = btn;
                    Debug.Log($"Auto-found Use button in children: {btn.name}");
                    break;
                }
            }
            
            if (useButton == null)
            {
                Debug.LogWarning("Use button not found. Please drag the Use button GameObject from the scene into the 'useButtonGameObject' field.");
            }
        }
        
        // Setup Use button
        if (useButton != null)
        {
            useButton.onClick.AddListener(OnUseButtonClicked);
            UpdateUseButtonState();
        }
        
        // Get Button component from dropButtonGameObject
        if (dropButtonGameObject != null)
        {
            dropButton = dropButtonGameObject.GetComponent<Button>();
            if (dropButton != null)
            {
                Debug.Log($"Found Drop button from dropButtonGameObject: {dropButtonGameObject.name}");
            }
            else
            {
                Debug.LogWarning($"dropButtonGameObject '{dropButtonGameObject.name}' does not have a Button component!");
            }
        }
        else
        {
            // Auto-find as fallback
            Button[] buttons = GetComponentsInChildren<Button>();
            foreach (Button btn in buttons)
            {
                if (btn.name.ToLower().Contains("drop"))
                {
                    dropButton = btn;
                    Debug.Log($"Auto-found Drop button in children: {btn.name}");
                    break;
                }
            }
            
            if (dropButton == null)
            {
                Debug.LogWarning("Drop button not found. Please drag the Drop button GameObject from the scene into the 'dropButtonGameObject' field.");
            }
        }
        
        // Setup Drop button
        if (dropButton != null)
        {
            dropButton.onClick.AddListener(OnDropButtonClicked);
            UpdateDropButtonState();
        }
    }

    void GenerateGrid()
    {
        // Prevent duplicate cells if grid already exists
        if (gridContainer != null && gridContainer.childCount > 0)
        {
            return;
        }
        
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
        
        // Update Use and Drop button states when selection changes
        UpdateUseButtonState();
        UpdateDropButtonState();
    }

    public void DeselectCell()
    {
        if (selectedCell != null)
            selectedCell.SetHighlight(false);
        selectedCell = null;
        
        // Update Use and Drop button states when deselected
        UpdateUseButtonState();
        UpdateDropButtonState();
    }

    public void CloseInventory()
    {
        Debug.Log("CloseInventory called");
        DeselectCell();
        gameObject.SetActive(false);
    }

    public bool AddItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("InventoryController: Cannot add null item!");
            return false;
        }
        
        if (gridContainer == null)
        {
            Debug.LogError("InventoryController: gridContainer is null! Make sure gridContainer is assigned.");
            return false;
        }
        
        // Ensure grid is generated if it hasn't been yet (in case Awake didn't run)
        if (items == null || items.Length == 0)
        {
            if (gridContainer.childCount == 0)
            {
                Debug.LogWarning("InventoryController: Grid not generated yet. Generating now...");
                items = new ItemData[rows * columns];
                GenerateGrid();
            }
            else
            {
                // Grid exists but items array wasn't initialized
                items = new ItemData[rows * columns];
            }
        }
        
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                
                // Check if cell exists
                if (i < gridContainer.childCount)
                {
                    var cell = gridContainer.GetChild(i).GetComponent<CellController>();
                    if (cell != null)
                    {
                        cell.SetItem(item);
                        return true;
                    }
                    else
                    {
                        Debug.LogWarning($"InventoryController: Cell at index {i} doesn't have CellController component!");
                    }
                }
                else
                {
                    Debug.LogWarning($"InventoryController: Cell at index {i} doesn't exist in gridContainer!");
                }
                
                // If we got here, something went wrong but we already set items[i]
                return true;
            }
        }
        return false; // Inventory full
    }
    
    /// <summary>
    /// Called when the Use button is clicked.
    /// Equips the selected weapon to the WeaponCell, or uses consumables like health flasks.
    /// </summary>
    public void OnUseButtonClicked()
    {
        if (selectedCell == null || selectedCell.currentItem == null)
        {
            Debug.LogWarning("No item selected to equip!");
            return;
        }
        
        ItemData itemToUse = selectedCell.currentItem;
        
        // Handle consumable items (like health flasks)
        if (itemToUse.itemType == ItemData.ItemType.Consumable)
        {
            // Check if this is a health flask
            if (itemToUse.itemName == "Health Flask")
            {
                UseHealthFlask(itemToUse);
                return;
            }
            // Add other consumable types here in the future
            Debug.LogWarning($"Unknown consumable item: {itemToUse.itemName}");
            return;
        }
        
        // Route item to correct equipment slot based on item type
        CellController targetCell = null;
        
        switch (itemToUse.itemType)
        {
            case ItemData.ItemType.Weapon:
                targetCell = weaponCell;
                break;
            case ItemData.ItemType.Armor:
                targetCell = armorCell;
                break;
            case ItemData.ItemType.Hat:
                targetCell = hatCell;
                break;
            case ItemData.ItemType.Gloves:
                targetCell = glovesCell;
                break;
            case ItemData.ItemType.Shoes:
                targetCell = shoesCell;
                break;
            default:
                Debug.LogWarning($"Cannot equip item type: {itemToUse.itemType}");
                return;
        }
        
        if (targetCell == null)
        {
            // Try to get cell if it's null (in case it wasn't set in Awake)
            switch (itemToUse.itemType)
            {
                case ItemData.ItemType.Weapon:
                    if (weaponCellGameObject != null)
                    {
                        targetCell = weaponCellGameObject.GetComponent<CellController>();
                    }
                    else
                    {
                        GameObject weaponCellObj = GameObject.Find("WeaponCell");
                        if (weaponCellObj != null)
                        {
                            targetCell = weaponCellObj.GetComponent<CellController>();
                        }
                    }
                    break;
                case ItemData.ItemType.Armor:
                    if (armorCellGameObject != null)
                    {
                        targetCell = armorCellGameObject.GetComponent<CellController>();
                    }
                    else
                    {
                        GameObject armorCellObj = GameObject.Find("ArmorCell");
                        if (armorCellObj != null)
                        {
                            targetCell = armorCellObj.GetComponent<CellController>();
                        }
                    }
                    break;
                case ItemData.ItemType.Hat:
                    if (hatCellGameObject != null)
                    {
                        targetCell = hatCellGameObject.GetComponent<CellController>();
                    }
                    else
                    {
                        GameObject hatCellObj = GameObject.Find("HatCell");
                        if (hatCellObj != null)
                        {
                            targetCell = hatCellObj.GetComponent<CellController>();
                        }
                    }
                    break;
                case ItemData.ItemType.Gloves:
                    if (glovesCellGameObject != null)
                    {
                        targetCell = glovesCellGameObject.GetComponent<CellController>();
                    }
                    else
                    {
                        GameObject glovesCellObj = GameObject.Find("GlovesCell");
                        if (glovesCellObj != null)
                        {
                            targetCell = glovesCellObj.GetComponent<CellController>();
                        }
                    }
                    break;
                case ItemData.ItemType.Shoes:
                    if (shoesCellGameObject != null)
                    {
                        targetCell = shoesCellGameObject.GetComponent<CellController>();
                    }
                    else
                    {
                        GameObject shoesCellObj = GameObject.Find("ShoesCell");
                        if (shoesCellObj != null)
                        {
                            targetCell = shoesCellObj.GetComponent<CellController>();
                        }
                    }
                    break;
            }
            
            if (targetCell == null)
            {
                Debug.LogError($"Equipment cell for {itemToUse.itemType} is not assigned! Please assign the cell GameObject in the Inspector.");
                return;
            }
        }
        
        // Equip the item to the appropriate slot
        // Try to get weaponCell if it's null (in case it wasn't set in Awake)
        if (weaponCell == null)
        {
            if (weaponCellGameObject != null)
            {
                weaponCell = weaponCellGameObject.GetComponent<CellController>();
                if (weaponCell == null)
                {
                    Debug.LogError($"CellController component not found on GameObject '{weaponCellGameObject.name}'. " +
                        $"Make sure the GameObject has a CellController component attached (same as inventory cells)!");
                    return;
                }
            }
            else
            {
                // Try auto-find as fallback
                GameObject weaponCellObj = GameObject.Find("WeaponCell");
                if (weaponCellObj != null)
                {
                    weaponCell = weaponCellObj.GetComponent<CellController>();
                }
                
                if (weaponCell == null)
                {
                    Debug.LogError("WeaponCell is not assigned! Please assign 'weaponCellGameObject' in the Inspector.");
                    return;
                }
                Debug.LogWarning($"Auto-found WeaponCell at runtime: {weaponCell.name}. Consider assigning weaponCellGameObject in Inspector.");
            }
        }
        
        // Equip the item (using SetItem like inventory cells)
        targetCell.SetItem(itemToUse);
        
        // Update player stats if PlayerHealth exists
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            // Update player stats based on item type
            if (itemToUse.itemType == ItemData.ItemType.Weapon)
            {
                UpdatePlayerWeaponStats(playerHealth, itemToUse);
            }
            // TODO: Add defense stat updates for armor items
            // else if (itemToUse.itemType == ItemData.ItemType.Armor || 
            //          itemToUse.itemType == ItemData.ItemType.Hat ||
            //          itemToUse.itemType == ItemData.ItemType.Gloves ||
            //          itemToUse.itemType == ItemData.ItemType.Shoes)
            // {
            //     UpdatePlayerDefenseStats(playerHealth, itemToUse);
            // }
        }
        
        Debug.Log($"Equipped {itemToUse.itemType}: {itemToUse.itemName}");
    }
    
    /// <summary>
    /// Uses a health flask: heals the player, shows message, and removes flask from inventory.
    /// </summary>
    private void UseHealthFlask(ItemData flaskData)
    {
        const float healAmount = 500f; // Default heal amount for health flask
        
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth == null)
        {
            if (MessageDisplay.Instance != null)
            {
                MessageDisplay.Instance.ShowError("Cannot use health flask: PlayerHealth not found!");
            }
            Debug.LogWarning("Cannot use health flask: PlayerHealth not found!");
            return;
        }
        
        // Get health before healing
        float healthBefore = playerHealth.CurrentHealth;
        
        // Heal the player
        playerHealth.Heal(healAmount);
        
        // Calculate actual heal amount (may be less if at max health)
        float actualHealAmount = playerHealth.CurrentHealth - healthBefore;
        
        // Show message
        if (MessageDisplay.Instance == null)
        {
            // Create MessageDisplay if it doesn't exist
            GameObject messageDisplayObj = new GameObject("MessageDisplay");
            MessageDisplay messageDisplay = messageDisplayObj.AddComponent<MessageDisplay>();
        }
        
        if (MessageDisplay.Instance != null)
        {
            string message = $"Player has used a healthflask (HP +{actualHealAmount:F0})";
            MessageDisplay.Instance.ShowMessage(message, 5f);
        }
        
        // Remove flask from inventory
        int itemIndex = -1;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == flaskData)
            {
                itemIndex = i;
                break;
            }
        }
        
        if (itemIndex >= 0)
        {
            RemoveItemAt(itemIndex);
        }
        else
        {
            // Fallback: try to remove by reference
            RemoveItem(flaskData);
        }
        
        Debug.Log($"Used health flask: Healed {actualHealAmount:F0} HP");
    }
    
    /// <summary>
    /// Updates the player's weapon stats based on the equipped weapon.
    /// </summary>
    private void UpdatePlayerWeaponStats(PlayerHealth playerHealth, ItemData weapon)
    {
        if (playerHealth != null && weapon != null)
        {
            playerHealth.SetAttackDamage(weapon.damage);
            Debug.Log($"Updated player attack damage to {weapon.damage} from weapon {weapon.itemName}");
        }
    }
    
    /// <summary>
    /// Updates the Use button's interactable state based on selection.
    /// </summary>
    private void UpdateUseButtonState()
    {
        if (useButton == null) return;
        
        // Enable button only if a weapon is selected
        bool hasSelectedWeapon = selectedCell != null && selectedCell.currentItem != null;
        useButton.interactable = hasSelectedWeapon;
    }
    
    /// <summary>
    /// Updates the Drop button's interactable state based on selection.
    /// </summary>
    private void UpdateDropButtonState()
    {
        if (dropButton == null) return;
        
        // Enable button only if a weapon is selected
        bool hasSelectedWeapon = selectedCell != null && selectedCell.currentItem != null;
        dropButton.interactable = hasSelectedWeapon;
    }
    
    /// <summary>
    /// Called when the Drop button is clicked.
    /// Drops the selected item from inventory (item disappears forever).
    /// </summary>
    public void OnDropButtonClicked()
    {
        if (selectedCell == null || selectedCell.currentItem == null)
        {
            Debug.LogWarning("No item selected to drop!");
            return;
        }
        
        ItemData itemToDrop = selectedCell.currentItem;
        
        // Remove item from inventory (item disappears forever, not spawned as pickupable)
        if (RemoveItem(itemToDrop))
        {
            Debug.Log($"Dropped item: {itemToDrop.itemName} (removed from inventory)");
        }
        else
        {
            Debug.LogWarning($"Failed to remove item {itemToDrop.itemName} from inventory!");
        }
    }
    
    /// <summary>
    /// Gets all items in the inventory with their indices.
    /// Returns a list of tuples containing (index, ItemData).
    /// </summary>
    public System.Collections.Generic.List<System.Tuple<int, ItemData>> GetAllItems()
    {
        System.Collections.Generic.List<System.Tuple<int, ItemData>> itemList = 
            new System.Collections.Generic.List<System.Tuple<int, ItemData>>();
        
        if (items == null) return itemList;
        
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
            {
                itemList.Add(new System.Tuple<int, ItemData>(i, items[i]));
            }
        }
        
        return itemList;
    }
    
    /// <summary>
    /// Removes an item from the inventory at the specified index.
    /// </summary>
    /// <param name="index">Index of the item to remove</param>
    /// <returns>True if item was removed successfully, false otherwise</returns>
    public bool RemoveItemAt(int index)
    {
        if (items == null || index < 0 || index >= items.Length)
        {
            return false;
        }
        
        if (items[index] == null)
        {
            return false;
        }
        
        // Clear the item from the array
        items[index] = null;
        
        // Clear the corresponding cell
        if (gridContainer != null && index < gridContainer.childCount)
        {
            CellController cell = gridContainer.GetChild(index).GetComponent<CellController>();
            if (cell != null)
            {
                cell.ClearItem();
            }
        }
        
        // If the removed item was selected, deselect it
        if (selectedCell != null && selectedCell.currentItem == null)
        {
            DeselectCell();
        }
        
        return true;
    }
    
    /// <summary>
    /// Removes a specific item from the inventory.
    /// </summary>
    /// <param name="item">The item to remove</param>
    /// <returns>True if item was removed successfully, false otherwise</returns>
    public bool RemoveItem(ItemData item)
    {
        if (item == null || items == null) return false;
        
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == item)
            {
                return RemoveItemAt(i);
            }
        }
        
        return false;
    }
}

