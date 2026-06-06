using UnityEngine;

public class EquipmentBlock : MonoBehaviour
{
    [Header("Armor Slots")]
    [SerializeField] private ArmorItem head;
    [SerializeField] private ArmorItem shoulders;
    [SerializeField] private ArmorItem body;
    [SerializeField] private ArmorItem gloves;
    [SerializeField] private ArmorItem legs;
    [SerializeField] private ArmorItem feet;

    [Header("Weapon Slots")]
    [SerializeField] private WeaponItem mainHand;
    [SerializeField] private WeaponItem offHand;

    private float eaf;
    private float physMit;
    private float mdefBase;

    private float bonusHeat, bonusCold, bonusEnergy, bonusMatter, bonusMind, bonusBody, bonusSpirit;
    private int gearStrength, gearStamina, gearAgility, gearIntellect, gearEmpathy, gearCharisma, gearHP, gearSP;

    public float EAF => eaf;
    public float PhysMit => physMit;
    public WeaponItem MainHand => mainHand;
    public WeaponItem OffHand => offHand;
    public int GearStrength => gearStrength;
    public int GearStamina => gearStamina;
    public int GearAgility => gearAgility;
    public int GearIntellect => gearIntellect;
    public int GearEmpathy => gearEmpathy;
    public int GearCharisma => gearCharisma;
    public int GearHP => gearHP;
    public int GearSP => gearSP;

    private void Awake() => Recalculate();

    // ── Try Equip (from inventory) ────────────────────────────────────────────

    public bool TryEquipItem(InventoryItem item, StatBlock statBlock, PlayerInventory inventory)
    {
        if (item == null || item.IsEmpty) return false;

        switch (item.type)
        {
            case InventoryItem.ItemType.Weapon:
                return TryEquipWeapon(item.weapon, statBlock, inventory);
            case InventoryItem.ItemType.Armor:
                return TryEquipArmor(item.armor, statBlock, inventory);
            default:
                Debug.Log($"{item.ItemName} cannot be equipped.");
                return false;
        }
    }

    private bool TryEquipWeapon(WeaponItem weapon, StatBlock statBlock, PlayerInventory inventory)
    {
        if (weapon == null) return false;

        if (!CheckClassRestriction(weapon.allowedClasses, statBlock)) return false;

        if (statBlock != null && statBlock.Strength < weapon.strengthRequirement)
        {
            Debug.Log($"Not enough STR to equip {weapon.itemName}. Requires {weapon.strengthRequirement}, have {statBlock.Strength}.");
            return false;
        }

        if (mainHand != null && inventory != null)
        {
            var old = InventoryItem.FromWeapon(mainHand);
            if (!inventory.AddItem(old))
            {
                Debug.Log("No bag space to unequip current weapon.");
                return false;
            }
        }

        mainHand = weapon;
        Recalculate();

        var spawner = GetComponentInChildren<WeaponVisualSpawner>();
        spawner?.Refresh();

        Debug.Log($"Equipped {weapon.itemName} to main hand.");
        GetComponent<PlayerEntity>()?.SaveEquipment();
        return true;
    }

    private bool TryEquipArmor(ArmorItem armor, StatBlock statBlock, PlayerInventory inventory)
    {
        if (armor == null) return false;

        if (statBlock != null && statBlock.Strength < armor.strengthRequirement)
        {
            Debug.Log($"Not enough STR to equip {armor.itemName}. Requires {armor.strengthRequirement}, have {statBlock.Strength}.");
            return false;
        }

        var currentArmor = GetArmorInSlot(armor.slot);
        if (currentArmor != null && inventory != null)
        {
            var old = InventoryItem.FromArmor(currentArmor);
            if (!inventory.AddItem(old))
            {
                Debug.Log("No bag space to unequip current armor.");
                return false;
            }
        }

        EquipArmor(armor);
        GetComponent<CharacterVisuals>()?.RefreshAll();
        Debug.Log($"Equipped {armor.itemName} to {armor.slot}.");
        GetComponent<PlayerEntity>()?.SaveEquipment();
        return true;
    }

    private bool CheckClassRestriction(ClassName[] allowedClasses, StatBlock statBlock)
    {
        if (allowedClasses == null || allowedClasses.Length == 0) return true;
        if (statBlock?.Class == null) return true;

        foreach (var c in allowedClasses)
            if (c == statBlock.Class.className) return true;

        Debug.Log($"Your class ({statBlock.Class.className}) cannot equip this item. Allowed: {string.Join(", ", allowedClasses)}");
        return false;
    }

    // ── Unequip to Inventory ──────────────────────────────────────────────────

    public bool UnequipMainHand(PlayerInventory inventory)
    {
        if (mainHand == null) return false;
        if (inventory != null)
        {
            var old = InventoryItem.FromWeapon(mainHand);
            if (!inventory.AddItem(old))
            {
                Debug.Log("No bag space to unequip weapon.");
                return false;
            }
        }
        mainHand = null;
        Recalculate();
        var spawner = GetComponentInChildren<WeaponVisualSpawner>();
        spawner?.Refresh();
        Debug.Log("Main hand unequipped.");
        GetComponent<PlayerEntity>()?.SaveEquipment();
        return true;
    }

    public bool UnequipOffHand(PlayerInventory inventory)
    {
        if (offHand == null) return false;
        if (inventory != null)
        {
            var old = InventoryItem.FromWeapon(offHand);
            if (!inventory.AddItem(old))
            {
                Debug.Log("No bag space to unequip off hand.");
                return false;
            }
        }
        offHand = null;
        Recalculate();
        Debug.Log("Off hand unequipped.");
        GetComponent<PlayerEntity>()?.SaveEquipment();
        return true;
    }

    public bool UnequipArmorSlot(ArmorSlot slot, PlayerInventory inventory)
    {
        var armor = GetArmorInSlot(slot);
        if (armor == null) return false;
        if (inventory != null)
        {
            var old = InventoryItem.FromArmor(armor);
            if (!inventory.AddItem(old))
            {
                Debug.Log("No bag space to unequip armor.");
                return false;
            }
        }
        UnequipArmor(slot);
        GetComponent<CharacterVisuals>()?.RefreshAll();
        Debug.Log($"{slot} unequipped.");
        GetComponent<PlayerEntity>()?.SaveEquipment();
        return true;
    }

    // ── Existing methods ──────────────────────────────────────────────────────

    public float GetResistance(DamageType type)
    {
        float bonus = type switch
        {
            DamageType.Heat => bonusHeat,
            DamageType.Cold => bonusCold,
            DamageType.Energy => bonusEnergy,
            DamageType.Matter => bonusMatter,
            DamageType.Mind => bonusMind,
            DamageType.Body => bonusBody,
            DamageType.Spirit => bonusSpirit,
            _ => 0f
        };
        return Mathf.Clamp(mdefBase + bonus, 0f, CombatCalculator.MAX_MITIGATION);
    }

    public void EquipArmor(ArmorItem item)
    {
        if (item == null) return;
        switch (item.slot)
        {
            case ArmorSlot.Head: head = item; break;
            case ArmorSlot.Shoulders: shoulders = item; break;
            case ArmorSlot.Body: body = item; break;
            case ArmorSlot.Gloves: gloves = item; break;
            case ArmorSlot.Legs: legs = item; break;
            case ArmorSlot.Feet: feet = item; break;
        }
        Recalculate();
    }

    public void UnequipArmor(ArmorSlot slot)
    {
        switch (slot)
        {
            case ArmorSlot.Head: head = null; break;
            case ArmorSlot.Shoulders: shoulders = null; break;
            case ArmorSlot.Body: body = null; break;
            case ArmorSlot.Gloves: gloves = null; break;
            case ArmorSlot.Legs: legs = null; break;
            case ArmorSlot.Feet: feet = null; break;
        }
        Recalculate();
    }

    public void EquipMainHand(WeaponItem weapon) { mainHand = weapon; Recalculate(); }
    public void EquipOffHand(WeaponItem weapon) { offHand = weapon; Recalculate(); }

    public ArmorItem GetArmorInSlot(ArmorSlot slot) => slot switch
    {
        ArmorSlot.Head => head,
        ArmorSlot.Shoulders => shoulders,
        ArmorSlot.Body => body,
        ArmorSlot.Gloves => gloves,
        ArmorSlot.Legs => legs,
        ArmorSlot.Feet => feet,
        _ => null
    };

    public void Recalculate()
    {
        ArmorItem[] slots = { head, shoulders, body, gloves, legs, feet };

        float totalEAF = 0f, totalPhysMit = 0f, totalMDEF = 0f;
        float tHeat = 0f, tCold = 0f, tEnergy = 0f, tMatter = 0f, tMind = 0f, tBody = 0f, tSpirit = 0f;
        int tStr = 0, tSta = 0, tAgi = 0, tInt = 0, tEmp = 0, tCha = 0, tHP = 0, tSP = 0;

        foreach (var piece in slots)
        {
            if (piece == null) continue;
            totalEAF += piece.GetEAF();
            totalPhysMit += piece.physMit;
            totalMDEF += piece.GetBaseMDEF();
            tHeat += piece.bonusHeatResist;
            tCold += piece.bonusColdResist;
            tEnergy += piece.bonusEnergyResist;
            tMatter += piece.bonusMatterResist;
            tMind += piece.bonusMindResist;
            tBody += piece.bonusBodyResist;
            tSpirit += piece.bonusSpiritResist;
            tStr += piece.bonusStrength; tSta += piece.bonusStamina;
            tAgi += piece.bonusAgility; tInt += piece.bonusIntellect;
            tEmp += piece.bonusEmpathy; tCha += piece.bonusCharisma;
            tHP += piece.bonusHP;
            tSP += piece.bonusSP;
        }

        eaf = totalEAF / 6f;
        physMit = Mathf.Clamp(totalPhysMit / 6f, 0f, CombatCalculator.MAX_MITIGATION);
        mdefBase = Mathf.Clamp(totalMDEF / 6f, 0f, CombatCalculator.MAX_MITIGATION);

        bonusHeat = tHeat; bonusCold = tCold; bonusEnergy = tEnergy;
        bonusMatter = tMatter; bonusMind = tMind; bonusBody = tBody; bonusSpirit = tSpirit;

        gearStrength = tStr; gearStamina = tSta; gearAgility = tAgi;
        gearIntellect = tInt; gearEmpathy = tEmp; gearCharisma = tCha;
        gearHP = tHP; gearSP = tSP;

        if (mainHand != null)
        {
            gearStrength += mainHand.bonusStrength; gearStamina += mainHand.bonusStamina;
            gearAgility += mainHand.bonusAgility; gearIntellect += mainHand.bonusIntellect;
            gearEmpathy += mainHand.bonusEmpathy; gearCharisma += mainHand.bonusCharisma;
        }
        if (offHand != null)
        {
            gearStrength += offHand.bonusStrength; gearStamina += offHand.bonusStamina;
            gearAgility += offHand.bonusAgility; gearIntellect += offHand.bonusIntellect;
            gearEmpathy += offHand.bonusEmpathy; gearCharisma += offHand.bonusCharisma;
        }

        // Notify StatBlock to update derived stats (HP, SP, AP) with new gear values
        GetComponent<StatBlock>()?.RecalculateDerived();
    }
    // ── Save / Load ───────────────────────────────────────────────────────────

    public void SaveToCharacterData(CharacterData data)
    {
        if (data.equipment == null)
            data.equipment = new EquipmentSaveData
            {
                armorItemNames = new string[6],
                bagItemNames = new string[6]
            };

        data.equipment.mainHandItemName = mainHand != null ? mainHand.itemName : "";
        data.equipment.offHandItemName = offHand != null ? offHand.itemName : "";

        ArmorSlot[] slots = { ArmorSlot.Head, ArmorSlot.Shoulders, ArmorSlot.Body,
                              ArmorSlot.Gloves, ArmorSlot.Legs, ArmorSlot.Feet };
        for (int i = 0; i < slots.Length; i++)
        {
            var a = GetArmorInSlot(slots[i]);
            data.equipment.armorItemNames[i] = a != null ? a.itemName : "";
        }
    }

    public void LoadFromCharacterData(CharacterData data, ItemRegistry registry)
    {
        if (data.equipment == null || registry == null) return;

        mainHand = registry.FindWeapon(data.equipment.mainHandItemName);
        offHand = registry.FindWeapon(data.equipment.offHandItemName);

        ArmorSlot[] slots = { ArmorSlot.Head, ArmorSlot.Shoulders, ArmorSlot.Body,
                              ArmorSlot.Gloves, ArmorSlot.Legs, ArmorSlot.Feet };
        for (int i = 0; i < slots.Length; i++)
        {
            var a = registry.FindArmor(data.equipment.armorItemNames[i]);
            if (a != null) EquipArmor(a);
            else UnequipArmor(slots[i]);
        }

        Recalculate();
        GetComponent<CharacterVisuals>()?.RefreshAll();
        GetComponentInChildren<WeaponVisualSpawner>()?.Refresh();
    }

#if UNITY_EDITOR
    private void OnValidate() => Recalculate();
#endif
}