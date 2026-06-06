using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Apothecary/Metabolize")]
public class Metabolize : AbilityBase
{
    public override bool CanUse(NetworkEntityStub caster) => true;

    public override void Execute(NetworkEntityStub caster, NetworkEntityStub target)
    {
        Debug.Log("Metabolize reduces all buildups on self by 50%");
    }
}