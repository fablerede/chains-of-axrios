using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Apothecary/MiasmicCloud")]
public class MiasmicCloud : AbilityBase
{
    public override bool CanUse(NetworkEntityStub caster)
        => caster.currentTP >= 100f;

    public override void Execute(NetworkEntityStub caster, NetworkEntityStub target)
    {
        Debug.Log($"Miasmic Cloud activated with {caster.currentTP}% TP");
    }
}