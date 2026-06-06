using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Apothecary/CorpseSeed")]
public class CorpseSeed : AbilityBase
{
    public override bool CanUse(NetworkEntityStub caster) => true;

    public override void Execute(NetworkEntityStub caster, NetworkEntityStub target)
    {
        Debug.Log($"Corpse Seed hits {target.name} for Matter damage, builds Tangle");
    }
}