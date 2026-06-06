using UnityEngine;

/// <summary>
/// Toxic Strike — Pathfinder ability.
/// Loads the caster's next auto attack as toxic.
/// That attack deals 110% damage and applies poison stacks equal to damage dealt,
/// checked against the target's Stamina Save.
///
/// To add this ability to the game:
///   1. Right-click in Project → Create → Abilities → Pathfinder → Toxic Strike
///   2. Fill in name, icon, description, costs, grade in the Inspector
///   3. Assign the SO asset to the appropriate slot in ClassAbilityRegistry
/// </summary>
[CreateAssetMenu(menuName = "Abilities/Apothecary/Toxic Strike")]
public class ToxicStrikeAbility : AbilityBase
{
    public override bool CanUse(NetworkEntityStub caster)
    {
        if (caster == null || caster.playerEntity == null) return false;
        if (caster.playerEntity.IsDead) return false;

        if (tpCost > 0f && caster.currentTP < tpCost)
        {
            ChatWindowUI.Combat("Not enough TP.");
            return false;
        }

        if (manaCost > 0f && caster.currentSP < manaCost)
        {
            ChatWindowUI.Combat("Not enough SP.");
            return false;
        }

        return true;
    }

    public override void Execute(NetworkEntityStub caster, NetworkEntityStub target)
    {
        if (!CanUse(caster)) return;

        // Deduct TP cost via VitalsUI
        if (tpCost > 0f && caster.vitalsUI != null)
            caster.vitalsUI.UpdateTP(Mathf.Max(caster.currentTP - tpCost, 0f), caster.maxTP);

        // Load the next auto attack as toxic
        caster.playerEntity.LoadToxicStrike();
    }
}