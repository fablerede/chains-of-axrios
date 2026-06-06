using UnityEngine;

[CreateAssetMenu(menuName = "Classes/ClassAbilityRegistry")]
public class ClassAbilityRegistry : ScriptableObject
{
    public string className;

    [Header("Focus A Abilities (Q E R F)")]
    public AbilityBase slotQ_FocusA;
    public AbilityBase slotE_FocusA;
    public AbilityBase slotR_FocusA;
    public AbilityBase slotF_FocusA;

    [Header("Focus B Abilities (Q E R F)")]
    public AbilityBase slotQ_FocusB;
    public AbilityBase slotE_FocusB;
    public AbilityBase slotR_FocusB;
    public AbilityBase slotF_FocusB;

    [Header("General Abilities (Alt 1 2 3 4)")]
    public AbilityBase slotA1;
    public AbilityBase slotA2;
    public AbilityBase slotA3;
    public AbilityBase slotA4;

    [Header("Extended Slots (5 6 7 8 — eligible classes only)")]
    public bool hasExtendedSlots;
    public AbilityBase slot5;
    public AbilityBase slot6;
    public AbilityBase slot7;
    public AbilityBase slot8;
}