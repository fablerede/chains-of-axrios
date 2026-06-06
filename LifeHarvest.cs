using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Apothecary/LifeHarvest")]
public class LifeHarvest : AbilityBase
{
    public override bool CanUse(NetworkEntityStub caster) => true;

    public override void Execute(NetworkEntityStub caster, NetworkEntityStub target)
    {
        Debug.Log("Life Harvest summons temporary herbs");
    }
}