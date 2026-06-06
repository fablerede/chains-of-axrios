using UnityEngine;

/// <summary>
/// Temporary test pickup — click to add any item to the first free bag slot.
/// Attach to any GameObject with a Collider.
/// </summary>
public class TestPickup : MonoBehaviour
{
    [Header("Item — fill in only one")]
    [SerializeField] private WeaponItem weapon;
    [SerializeField] private ArmorItem armor;
    [SerializeField] private ConsumableItem consumable;
    [SerializeField] private int consumableStack = 5;

    private void OnMouseDown()
    {
        var inventory = FindAnyObjectByType<PlayerInventory>();
        if (inventory == null) { Debug.Log("No PlayerInventory found."); return; }

        InventoryItem item = null;

        if (weapon != null)
            item = InventoryItem.FromWeapon(weapon);
        else if (armor != null)
            item = InventoryItem.FromArmor(armor);
        else if (consumable != null)
        {
            var manager = FindAnyObjectByType<ConsumableManager>();
            if (manager == null) { Debug.Log("No ConsumableManager found."); return; }

            for (int i = 0; i < ConsumableManager.MAX_SLOTS; i++)
            {
                if (manager.items[i] == null)
                {
                    manager.PlaceConsumable(i, consumable, consumableStack);
                    Debug.Log($"Picked up {consumableStack}x {consumable.itemName} into slot {i + 1}");
                    return;
                }
            }
            Debug.Log("No empty consumable slots.");
            return;
        }

        if (item == null) { Debug.Log("No item assigned to TestPickup."); return; }

        bool success = inventory.AddItem(item);
        Debug.Log(success
            ? $"Picked up {item.ItemName} into bag"
            : $"No bag space for {item.ItemName}");
        FindAnyObjectByType<BagWindowUI>()?.Refresh();
    }
}