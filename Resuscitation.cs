using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Apothecary/Resuscitation")]
public class Resuscitation : AbilityBase
{
    public override bool CanUse(NetworkEntityStub caster) => true;

    public override void Execute(NetworkEntityStub caster, NetworkEntityStub target)
    {
        Debug.Log($"Resuscitation revives {target.name}");
    }
}