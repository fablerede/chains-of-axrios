using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Apothecary/Catalyst")]
public class Catalyst : AbilityBase
{
    public override bool CanUse(NetworkEntityStub caster) => true;

    public override void Execute(NetworkEntityStub caster, NetworkEntityStub target)
    {
        Debug.Log($"Catalyst detonates Poison on {target.name}");
    }
}