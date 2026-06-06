using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Apothecary/Aspiration")]
public class Aspiration : AbilityBase
{
    public override bool CanUse(NetworkEntityStub caster) => true;

    public override void Execute(NetworkEntityStub caster, NetworkEntityStub target)
    {
        Debug.Log($"Aspiration echoes current poultice on {target.name}");
    }
}