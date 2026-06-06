using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Apothecary/Exhale")]
public class Exhale : AbilityBase
{
    public override bool CanUse(NetworkEntityStub caster)
        => caster.currentTP >= tpCost;

    public override void Execute(NetworkEntityStub caster, NetworkEntityStub target)
    {
        Debug.Log("Exhale triggers active Philter effect");
    }
}