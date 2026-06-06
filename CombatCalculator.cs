using UnityEngine;

/// <summary>
/// Pure static math utility for the Chains of Axrios combat system.
/// Handles hit quality, damage rolls, mitigation, and NPC archetype stats.
/// No MonoBehaviour — safe to call from anywhere.
/// </summary>
public static class CombatCalculator
{
    public const float MAX_MITIGATION = 0.80f;

    public const float EAF_CLOTH = 1.0f;
    public const float EAF_LIGHT = 1.0f;
    public const float EAF_MEDIUM = 1.6f;
    public const float EAF_HEAVY = 2.0f;

    public const float MDEF_CLOTH = 2.0f;
    public const float MDEF_LIGHT = 1.0f;
    public const float MDEF_MEDIUM = 0.6f;
    public const float MDEF_HEAVY = 0.2f;

    public enum HitQuality { Glance, Normal, Solid }

    /// <summary>
    /// Rolls 1d4 and adds to base AP for a single attack swing.
    /// Use this for every attack roll instead of raw AP.
    /// </summary>
    public static int RollAttackAP(int baseAP)
    {
        return baseAP + Random.Range(1, 5); // 1–4
    }

    /// <summary>
    /// Determines hit quality based on attacker AP vs target EAF.
    /// AP >= EAF*2 = always solid. EAF >= AP*2 = always glance. Otherwise normal.
    /// </summary>
    public static HitQuality GetHitQuality(int attackerAP, float targetEAF)
    {
        if (attackerAP >= targetEAF * 2f) return HitQuality.Solid;
        if (targetEAF >= attackerAP * 2f) return HitQuality.Glance;
        return HitQuality.Normal;
    }

    /// <summary>
    /// Rolls damage based on base damage stat and hit quality.
    /// Glance = base/2, Normal = random base to base*2, Solid = base*2.
    /// </summary>
    public static float RollDamage(float baseDamage, HitQuality quality)
    {
        switch (quality)
        {
            case HitQuality.Glance: return Mathf.Floor(baseDamage / 2f);
            case HitQuality.Solid: return Mathf.Floor(baseDamage * 2f);
            default: return Mathf.Floor(Random.Range(baseDamage, baseDamage * 2f + 1f));
        }
    }

    public static float ApplyDamageMultiplier(float damage, float multiplier)
        => Mathf.Floor(damage * multiplier);

    public static float ApplyPhysMit(float damage, float physMitPercent)
    {
        float mit = Mathf.Clamp(physMitPercent, 0f, MAX_MITIGATION);
        return Mathf.Floor(damage * (1f - mit));
    }

    public static float ApplyMagicResist(float damage, float resistPercent)
    {
        float resist = Mathf.Clamp(resistPercent, 0f, MAX_MITIGATION);
        return Mathf.Floor(damage * (1f - resist));
    }

    /// <summary>
    /// Full physical damage pipeline: Roll → multiplier → phys mit.
    /// Pass a pre-rolled swingAP (use RollAttackAP before calling).
    /// </summary>
    public static float CalculatePhysicalDamage(
        float baseDamage, int swingAP, float targetEAF,
        float damageMultiplier, float targetPhysMit)
    {
        HitQuality quality = GetHitQuality(swingAP, targetEAF);
        float rolled = RollDamage(baseDamage, quality);
        float amplified = ApplyDamageMultiplier(rolled, damageMultiplier);
        return ApplyPhysMit(amplified, targetPhysMit);
    }

    /// <summary>
    /// Full magic damage pipeline: Roll → multiplier → magic resist.
    /// Pass a pre-rolled swingAP (use RollAttackAP before calling).
    /// </summary>
    public static float CalculateMagicDamage(
        float baseDamage, int swingMAP, float targetEAF,
        float damageMultiplier, float targetResist)
    {
        HitQuality quality = GetHitQuality(swingMAP, targetEAF);
        float rolled = RollDamage(baseDamage, quality);
        float amplified = ApplyDamageMultiplier(rolled, damageMultiplier);
        return ApplyMagicResist(amplified, targetResist);
    }

    /// <summary>
    /// Rolls a critical hit check. Chance = class level as a percentage.
    /// </summary>
    public static bool RollCrit(int classLevel)
        => Random.value < classLevel / 100f;

    public static float ApplyCrit(float damage, float critMultiplier)
        => Mathf.Floor(damage * critMultiplier);

    public static float ApplyCritMitigation(float baseDamage, float critDamage, float mitigationChance)
    {
        float critBonus = critDamage - baseDamage;
        if (Random.value < mitigationChance) critBonus *= 0.5f;
        return Mathf.Floor(baseDamage + critBonus);
    }

    /// <summary>
    /// Rolls evasion check. Each rank = 6% dodge chance.
    /// Returns true if the attack is dodged.
    /// </summary>
    public static bool RollEvasion(int evasionRanks)
        => Random.value < evasionRanks * 0.06f;

    // ── NPC Archetype stat calculations (no variance — static on spawn) ───────

    public enum NPCArchetype
    {
        Guardian, Sentinel, Vanguard, Scout,
        Brawler, Corrupter, Violator, Evoker
    }

    public static int CalculateNPCAP(int level, NPCArchetype archetype)
        => Mathf.Max(Mathf.RoundToInt(level * GetAPMultiplier(archetype)), 1);

    public static int CalculateNPCMAP(int level, NPCArchetype archetype)
    {
        float mult = GetMAPMultiplier(archetype);
        if (mult <= 0f) return 0;
        return Mathf.Max(Mathf.RoundToInt(level * mult), 0);
    }

    public static float CalculateNPCEAF(int level, NPCArchetype archetype)
        => level * GetEAFMultiplier(archetype);

    public static float CalculateNPCMDEF(int level, NPCArchetype archetype)
        => level * GetMDEFMultiplier(archetype);

    public static int GetArchetypeEvasion(NPCArchetype archetype)
    {
        switch (archetype)
        {
            case NPCArchetype.Scout: return 2;
            case NPCArchetype.Brawler: return 3;
            case NPCArchetype.Violator: return 2;
            default: return 0;
        }
    }

    private static float GetAPMultiplier(NPCArchetype archetype)
    {
        switch (archetype)
        {
            case NPCArchetype.Guardian: return 1.8f;
            case NPCArchetype.Sentinel: return 1.8f;
            case NPCArchetype.Vanguard: return 2.0f;
            case NPCArchetype.Scout: return 1.0f;
            case NPCArchetype.Brawler: return 1.8f;
            case NPCArchetype.Corrupter: return 1.5f;
            case NPCArchetype.Violator: return 2.0f;
            case NPCArchetype.Evoker: return 1.5f;
            default: return 1.0f;
        }
    }

    private static float GetMAPMultiplier(NPCArchetype archetype)
    {
        switch (archetype)
        {
            case NPCArchetype.Corrupter: return 1.5f;
            case NPCArchetype.Violator: return 1.0f;
            case NPCArchetype.Evoker: return 2.0f;
            default: return 0f;
        }
    }

    private static float GetEAFMultiplier(NPCArchetype archetype)
    {
        switch (archetype)
        {
            case NPCArchetype.Guardian: return 2.0f;
            case NPCArchetype.Sentinel: return 1.6f;
            case NPCArchetype.Vanguard: return 1.5f;
            case NPCArchetype.Scout: return 1.0f;
            case NPCArchetype.Brawler: return 1.2f;
            case NPCArchetype.Corrupter: return 1.0f;
            case NPCArchetype.Violator: return 1.2f;
            case NPCArchetype.Evoker: return 1.0f;
            default: return 1.0f;
        }
    }

    private static float GetMDEFMultiplier(NPCArchetype archetype)
    {
        switch (archetype)
        {
            case NPCArchetype.Guardian: return 0.8f;
            case NPCArchetype.Sentinel: return 1.8f;
            case NPCArchetype.Vanguard: return 0.5f;
            case NPCArchetype.Scout: return 1.0f;
            case NPCArchetype.Brawler: return 1.5f;
            case NPCArchetype.Corrupter: return 1.5f;
            case NPCArchetype.Violator: return 1.4f;
            case NPCArchetype.Evoker: return 1.6f;
            default: return 1.0f;
        }
    }

    public static float GetEAFMultiplierForType(ArmorType type)
    {
        switch (type)
        {
            case ArmorType.Heavy: return EAF_HEAVY;
            case ArmorType.Medium: return EAF_MEDIUM;
            default: return EAF_LIGHT;
        }
    }

    public static float GetMDEFMultiplierForType(ArmorType type)
    {
        switch (type)
        {
            case ArmorType.Cloth: return MDEF_CLOTH;
            case ArmorType.Light: return MDEF_LIGHT;
            case ArmorType.Medium: return MDEF_MEDIUM;
            case ArmorType.Heavy: return MDEF_HEAVY;
            default: return MDEF_LIGHT;
        }
    }
}