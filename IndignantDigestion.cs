using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Apothecary/IndignantDigestion")]
public class IndignantDigestion : AbilityBase
{
    public float hpCostPercent = 0.15f;

    public override bool CanUse(NetworkEntityStub caster)
        => caster.currentHP > caster.maxHP * hpCostPercent;

    public override void Execute(NetworkEntityStub caster, NetworkEntityStub target)
    {
        Debug.Log("Indignant Digestion extends active Philter duration");
    }
}