using UnityEngine;

/// <summary>
/// Wraps any item type so it can live in an inventory slot.
/// </summary>
[System.Serializable]
public class InventoryItem
{
    public enum ItemType { None, Weapon, Armor, Bag, Consumable, Ingredient, CraftingMaterial }

    public ItemType type = ItemType.None;

    public WeaponItem weapon;
    public ArmorItem armor;
    public BagItem bag;
    public ConsumableItem consumable;
    public CraftingMaterial material;

    public int stackCount = 1;

    // ── Convenience constructors ──────────────────────────────────────────────

    public static InventoryItem FromWeapon(WeaponItem w)
        => new InventoryItem { type = ItemType.Weapon, weapon = w };

    public static InventoryItem FromArmor(ArmorItem a)
        => new InventoryItem { type = ItemType.Armor, armor = a };

    public static InventoryItem FromBag(BagItem b)
        => new InventoryItem { type = ItemType.Bag, bag = b };

    public static InventoryItem FromConsumable(ConsumableItem c)
        => new InventoryItem { type = ItemType.Consumable, consumable = c };

    public static InventoryItem FromMaterial(CraftingMaterial m)
        => new InventoryItem { type = ItemType.CraftingMaterial, material = m };

    // ── Shared accessors ──────────────────────────────────────────────────────

    public string ItemName
    {
        get
        {
            switch (type)
            {
                case ItemType.Weapon: return weapon != null ? weapon.itemName : "Unknown Weapon";
                case ItemType.Armor: return armor != null ? armor.itemName : "Unknown Armor";
                case ItemType.Bag: return bag != null ? bag.itemName : "Unknown Bag";
                case ItemType.Consumable: return consumable != null ? consumable.itemName : "Unknown Consumable";
                case ItemType.CraftingMaterial: return material != null ? material.itemName : "Unknown Material";
                default: return "Unknown Item";
            }
        }
    }

    public Sprite Icon
    {
        get
        {
            switch (type)
            {
                case ItemType.Weapon: return weapon?.icon;
                case ItemType.Armor: return armor?.icon;
                case ItemType.Bag: return bag?.icon;
                case ItemType.Consumable: return consumable?.icon;
                case ItemType.CraftingMaterial: return material?.icon;
                default: return null;
            }
        }
    }

    public int ItemLevel
    {
        get
        {
            switch (type)
            {
                case ItemType.Weapon: return weapon?.itemLevel ?? 0;
                case ItemType.Armor: return armor?.itemLevel ?? 0;
                case ItemType.Bag: return bag?.itemLevel ?? 0;
                case ItemType.Consumable: return consumable?.itemLevel ?? 0;
                case ItemType.CraftingMaterial: return material?.itemLevel ?? 0;
                default: return 0;
            }
        }
    }

    public string GetTooltip()
    {
        switch (type)
        {
            case ItemType.Weapon:
                if (weapon == null) return "No weapon data.";
                return $"<b>{weapon.itemName}</b>  <size=80%>{weapon.weaponClass}</size>\n" +
                       $"Item Level {weapon.itemLevel}\n\n" +
                       $"Damage  {weapon.baseDamage:F0}–{weapon.MaxDamage:F0}  (Glance: {weapon.GlanceDamage:F0})\n" +
                       $"Speed   {weapon.attackSpeed}s\n" +
                       $"Range   {weapon.attackRange}m\n" +
                       (weapon.hasElementalDamage ? $"Elemental +{weapon.elementalDamage:F0} {weapon.elementalDamageType}\n" : "") +
                       (string.IsNullOrEmpty(weapon.description) ? "" : $"\n<i>{weapon.description}</i>");

            case ItemType.Armor:
                if (armor == null) return "No armor data.";
                return $"<b>{armor.itemName}</b>  <size=80%>{armor.armorType} — {armor.slot}</size>\n" +
                       $"Item Level {armor.itemLevel}\n\n" +
                       $"EAF      {armor.GetEAF():F1}\n" +
                       $"Phys Mit {armor.physMit * 100f:F0}%\n" +
                       $"MDEF     {armor.GetBaseMDEF() * 100f:F0}%\n" +
                       (armor.bonusHP != 0 ? $"HP  +{armor.bonusHP}\n" : "") +
                       (armor.bonusSP != 0 ? $"SP  +{armor.bonusSP}\n" : "") +
                       (armor.bonusStrength != 0 ? $"STR +{armor.bonusStrength}\n" : "") +
                       (armor.bonusStamina != 0 ? $"STA +{armor.bonusStamina}\n" : "") +
                       (armor.bonusAgility != 0 ? $"AGI +{armor.bonusAgility}\n" : "") +
                       (armor.bonusIntellect != 0 ? $"INT +{armor.bonusIntellect}\n" : "") +
                       (armor.bonusEmpathy != 0 ? $"EMP +{armor.bonusEmpathy}\n" : "") +
                       (armor.bonusCharisma != 0 ? $"CHA +{armor.bonusCharisma}\n" : "") +
                       (string.IsNullOrEmpty(armor.description) ? "" : $"\n<i>{armor.description}</i>");

            case ItemType.Bag:
                if (bag == null) return "No bag data.";
                return $"<b>{bag.itemName}</b>\n" +
                       $"Item Level {bag.itemLevel}\n\n" +
                       $"Slots: {bag.slotCount}\n" +
                       (string.IsNullOrEmpty(bag.description) ? "" : $"\n<i>{bag.description}</i>");

            case ItemType.Consumable:
                if (consumable == null) return "No consumable data.";
                return $"<b>{consumable.itemName}</b>  <size=80%>Consumable</size>\n" +
                       $"Item Level {consumable.itemLevel}\n\n" +
                       (consumable.hpRestore > 0 ? $"Restores {consumable.hpRestore:F0} HP\n" : "") +
                       (consumable.spRestore > 0 ? $"Restores {consumable.spRestore:F0} SP\n" : "") +
                       (consumable.cooldown > 0 ? $"Cooldown {consumable.cooldown:F0}s\n" : "") +
                       $"Classes: {consumable.ClassListShort()}\n" +
                       (string.IsNullOrEmpty(consumable.description) ? "" : $"\n<i>{consumable.description}</i>");

            case ItemType.CraftingMaterial:
                if (material == null) return "No material data.";
                return $"<b>{material.itemName}</b>  <size=80%>Crafting Material</size>\n" +
                       $"Item Level {material.itemLevel}\n\n" +
                       (!string.IsNullOrEmpty(material.category) ? $"Category: {material.category}\n" : "") +
                       (string.IsNullOrEmpty(material.description) ? "" : $"\n<i>{material.description}</i>");

            default:
                return "Unknown item.";
        }
    }

    public bool IsEmpty => type == ItemType.None;

    public bool CanPlaceInBag(PlayerInventory inventory)
    {
        if (type != ItemType.Bag) return true;
        if (bag == null) return true;
        return inventory == null || !inventory.BagHasItems(bag);
    }
}