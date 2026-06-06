using UnityEngine;

public enum ArmorType
{
    Cloth,
    Light,
    Medium,
    Heavy
}

public enum ArmorSlot
{
    Head,
    Shoulders,
    Body,
    Gloves,
    Legs,
    Feet
}

[CreateAssetMenu(fileName = "ArmorItem_New", menuName = "Chains of Axrios/Items/Armor")]
public class ArmorItem : ScriptableObject
{
    [Header("Identity")]
    public string itemName;
    public ArmorSlot slot;
    public ArmorType armorType;
    public int itemLevel = 1;

    [Header("Display")]
    public Sprite icon;
    [TextArea] public string description;

    [Header("Requirements")]
    public int strengthRequirement = 0;
    // Class restrictions can be added later when ClassName enum is available

    [Header("Armor Stats")]
    [Tooltip("Base armor factor value for this piece. EAF = itemLevel * armorType multiplier.")]
    public float armorFactor = 1f;

    [Tooltip("Physical mitigation as a decimal (e.g. 0.30 = 30%). Hard capped at 0.80 in CombatCalculator.")]
    [Range(0f, 0.80f)]
    public float physMit = 0f;

    [Header("Visual")]
    [Tooltip("The SkinnedMeshRenderer on the player prefab that represents this item.")]
    public string meshObjectName; // e.g. "Steel_cuirass_1"
    public string meshObjectNameAlt;

    [Tooltip("Optional color tint applied to this armor mesh. White = no tint.")]
    public Color meshTint = Color.white;

    [Header("Elemental Resistance Bonuses")]
    [Tooltip("Base MDEF comes from armorType * itemLevel. These are bonus resistances on top of that.")]
    [Range(0f, 0.80f)] public float bonusHeatResist = 0f;
    [Range(0f, 0.80f)] public float bonusColdResist = 0f;
    [Range(0f, 0.80f)] public float bonusEnergyResist = 0f;
    [Range(0f, 0.80f)] public float bonusMatterResist = 0f;
    [Range(0f, 0.80f)] public float bonusMindResist = 0f;
    [Range(0f, 0.80f)] public float bonusBodyResist = 0f;
    [Range(0f, 0.80f)] public float bonusSpiritResist = 0f;

    [Header("Stat Bonuses")]
    public int bonusStrength = 0;
    public int bonusStamina = 0;
    public int bonusAgility = 0;
    public int bonusIntellect = 0;
    public int bonusEmpathy = 0;
    public int bonusCharisma = 0;
    public int bonusHP = 0;
    public int bonusSP = 0;

    /// <summary>
    /// Returns the effective EAF contribution of this piece.
    /// EAF = itemLevel * armorType EAF multiplier * armorFactor
    /// </summary>
    public float GetEAF()
    {
        return itemLevel * CombatCalculator.GetEAFMultiplierForType(armorType) * armorFactor;
    }

    /// <summary>
    /// Returns the base MDEF contribution of this piece (before bonus resistances).
    /// MDEF = itemLevel * armorType MDEF multiplier
    /// </summary>
    public float GetBaseMDEF()
    {
        return itemLevel * CombatCalculator.GetMDEFMultiplierForType(armorType) * 0.01f;
    }
}