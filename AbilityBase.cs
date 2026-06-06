using UnityEngine;

public abstract class AbilityBase : ScriptableObject
{
    [Header("Identity")]
    public string abilityName;
    public Sprite icon;
    public string description;

    [Header("Tags")]
    public AbilityTag[] tags;

    [Header("Costs")]
    public float manaCost;
    public float tpCost;
    public float cooldown;
    public float castTime;

    [Header("Targeting")]
    public TargetType targetType;
    public float range;

    [Header("Grade")]
    public AbilityGrade grade;

    public abstract bool CanUse(NetworkEntityStub caster);
    public abstract void Execute(NetworkEntityStub caster, NetworkEntityStub target);
    public virtual void OnInterrupt(NetworkEntityStub caster) { }
}

public enum AbilityTag
{
    Harmful, Beneficial, Healing, AoE, DoT, Buff, Debuff,
    Summon, Channel, Interrupt, CC, Cleanse, Resurrection
}

public enum TargetType
{
    Self, SingleAlly, SingleEnemy, AoEAlly, AoEEnemy, Ground
}

public enum AbilityGrade
{
    Grey, Green, Blue, Purple, Red, Gold
}