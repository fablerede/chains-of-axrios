using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Apothecary/PhilosophersStone")]
public class PhilosophersStone : AbilityBase
{
    public override bool CanUse(NetworkEntityStub caster)
        => caster.currentTP >= 100f;

    public override void Execute(NetworkEntityStub caster, NetworkEntityStub target)
    {
        Debug.Log($"Philosopher's Stone activated with {caster.currentTP}% TP");
    }
}