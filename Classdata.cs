using UnityEngine;

public enum ClassName
{
    Paladin,
    Mercenary,
    Apothecary,
    Warlord,
    Oracle,
    Merchant,
    Pathfinder,
    Druid,
    Beastmaster
}

public enum SPStat
{
    None,
    Empathy,
    Intellect
}

[CreateAssetMenu(fileName = "ClassData_New", menuName = "Chains of Axrios/Class Data")]
public class ClassData : ScriptableObject
{
    [Header("Identity")]
    public ClassName className;

    [Header("HP")]
    [Tooltip("Multiplied by class level for base HP. Paladin/Mercenary/Warlord/Pathfinder = 10, Beastmaster = 12, others = 8")]
    public int hitDice = 10;

    [Header("SP")]
    [Tooltip("Multiplied by half the SP stat bonus for SP pool. 0 = no SP pool.")]
    public int spMultiplier = 0;

    [Tooltip("Which stat contributes to SP. None for Merchant and Apothecary.")]
    public SPStat spStat = SPStat.None;

    [Header("Attack Power")]
    [Tooltip("Base AP granted per class level. 1.0 for most classes, 0.75 for Apothecary, Oracle, Druid.")]
    public float apPerLevel = 1f;

    [Header("Resource System")]
    [Tooltip("Classes with no SP pool use this description for UI reference.")]
    public string resourceDescription = "SP";

    [Header("Natural Crit")]
    [Tooltip("Can this class land critical hits without a special ability?")]
    public bool naturalCrit = false;
}