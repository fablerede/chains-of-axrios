using UnityEngine;

/// <summary>
/// Handles XP thresholds, level up logic, and trait unlocks for Chains of Axrios.
/// Pure static — no MonoBehaviour needed.
/// </summary>
public static class ExperienceSystem
{
    // XP required to reach each level (index = level, value = total XP needed)
    // Level 1 = 0 (starting level), Level 2 = 400, etc.
    private static readonly int[] XP_THRESHOLDS = new int[]
    {
        0,       // Level 1  (starting)
        400,     // Level 2
        600,     // Level 3
        900,     // Level 4
        1350,    // Level 5
        2025,    // Level 6
        3038,    // Level 7
        4556,    // Level 8
        6834,    // Level 9
        10251,   // Level 10
        15377,   // Level 11
        23066,   // Level 12
        34599,   // Level 13
        51898,   // Level 14
        77847,   // Level 15
        116771,  // Level 16
        175156,  // Level 17
        262734,  // Level 18
        394101,  // Level 19
        591152,  // Level 20 (max)
    };

    public const int MAX_LEVEL = 20;

    /// <summary>
    /// Returns total XP needed to reach the next level from currentLevel.
    /// Returns -1 if already at max level.
    /// </summary>
    public static int GetXPForNextLevel(int currentLevel)
    {
        if (currentLevel >= MAX_LEVEL) return -1;
        if (currentLevel < 1 || currentLevel >= XP_THRESHOLDS.Length) return -1;
        return XP_THRESHOLDS[currentLevel]; // index = next level - 1
    }

    /// <summary>
    /// Returns total XP needed to reach a specific level.
    /// </summary>
    public static int GetXPThreshold(int level)
    {
        if (level < 1 || level > MAX_LEVEL) return 0;
        return XP_THRESHOLDS[level - 1];
    }

    /// <summary>
    /// Adds XP to the character and handles level ups.
    /// Returns true if the character levelled up.
    /// Saves automatically on level up.
    /// </summary>
    public static bool AddXP(CharacterData charData, StatBlock statBlock, int amount)
    {
        if (charData == null || statBlock == null) return false;
        if (charData.level >= MAX_LEVEL) return false;

        charData.experience += amount;
        Debug.Log($"{charData.characterName} gains {amount} XP. Total: {charData.experience}");

        bool levelledUp = false;

        // Check for level up (may level multiple times if XP gain is large)
        while (charData.level < MAX_LEVEL)
        {
            int xpNeeded = GetXPForNextLevel(charData.level);
            if (xpNeeded < 0 || charData.experience < xpNeeded) break;

            charData.level++;
            statBlock.SetClassLevel(charData.level);
            levelledUp = true;

            Debug.Log($"LEVEL UP! {charData.characterName} is now level {charData.level}!");

            // Apply trait unlocks for this level
            ApplyTraitUnlocks(charData, statBlock);
        }

        // Save to JSON
        if (GameManager.Instance != null)
            _ = GameManager.Instance.CharacterDataService.SaveCharacter(charData);

        return levelledUp;
    }

    /// <summary>
    /// Applies any traits unlocked at the character's current level.
    /// Currently logs — hook to a trait system when built.
    /// </summary>
    private static void ApplyTraitUnlocks(CharacterData charData, StatBlock statBlock)
    {
        if (statBlock.Class == null) return;

        string className = statBlock.Class.className.ToString();
        int level = charData.level;

        // Apothecary trait unlocks
        if (className == "Apothecary")
        {
            switch (level)
            {
                case 1:
                    Debug.Log("Trait unlocked: Herbalist");
                    Debug.Log("Trait unlocked: Light Armor Proficiency");
                    break;
                case 2:
                    Debug.Log("Trait unlocked: Toxicology I");
                    break;
                case 4:
                    Debug.Log("Trait unlocked: Field Medic");
                    break;
                case 6:
                    Debug.Log("Trait unlocked: Toxicology II");
                    break;
                case 8:
                    Debug.Log("Trait unlocked: Compound Mixing");
                    Debug.Log("Trait unlocked: Evasion I");
                    break;
                case 10:
                    Debug.Log("Trait unlocked: Resilience");
                    break;
                case 12:
                    Debug.Log("Trait unlocked: Toxicology III");
                    break;
                case 14:
                    Debug.Log("Trait unlocked: Practiced Hand");
                    break;
                case 16:
                    Debug.Log("Trait unlocked: Antidote Mastery");
                    Debug.Log("Trait unlocked: Evasion II");
                    break;
                case 18:
                    Debug.Log("Trait unlocked: Virulent Strain");
                    break;
                case 20:
                    Debug.Log("Trait unlocked: Master Apothecary");
                    break;
            }
        }
        // Add other classes here as they get designed
    }
}