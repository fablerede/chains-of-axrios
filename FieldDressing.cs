using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Apothecary/FieldDressing")]
public class FieldDressing : AbilityBase
{
    public float healPerTick = 8f;
    public float healScaling = 0.3f;

    public override bool CanUse(NetworkEntityStub caster) => true;

    public override void Execute(NetworkEntityStub caster, NetworkEntityStub target)
    {
        Debug.Log($"Field Dressing channels HOT on {target.name}");
    }
}