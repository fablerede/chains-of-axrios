using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    [Header("Consumable Slots")]
    [SerializeField] private HotbarSlot slot1;
    [SerializeField] private HotbarSlot slot2;
    [SerializeField] private HotbarSlot slot3;
    [SerializeField] private HotbarSlot slot4;

    [Header("Extended Slots (Apothecary/Druid/Merchant)")]
    [SerializeField] private HotbarSlot slot5;
    [SerializeField] private HotbarSlot slot6;
    [SerializeField] private HotbarSlot slot7;
    [SerializeField] private HotbarSlot slot8;
    [SerializeField] private GameObject extendedSlotsContainer;

    [Header("Focus Slots")]
    [SerializeField] private HotbarSlot slotQ;
    [SerializeField] private HotbarSlot slotE;
    [SerializeField] private HotbarSlot slotR;
    [SerializeField] private HotbarSlot slotF;

    [Header("General Ability Slots")]
    [SerializeField] private HotbarSlot slotA1;
    [SerializeField] private HotbarSlot slotA2;
    [SerializeField] private HotbarSlot slotA3;
    [SerializeField] private HotbarSlot slotA4;

    [Header("F Slot Glow")]
    [SerializeField] private Image fSlotGlow;
    [SerializeField] private Gradient tpRainbowGradient;

    [Header("Class Registries")]
    [SerializeField] private ClassAbilityRegistry[] allRegistries;

    [Header("Settings")]
    [SerializeField] private bool showKeybinds = false;

    [Header("Vitals Reference")]
    [SerializeField] private VitalsUI vitalsUI;

    private ClassAbilityRegistry currentRegistry;
    private bool focusA = true;
    private float currentTP = 0f;
    private float maxTP = 200f;

    private void Start()
    {
        SetupKeybinds();
        LoadClassFromSelectedCharacter();
        UpdateFGlow(0f);
    }
    private void Update()
    {
        if (vitalsUI != null)
            currentTP = vitalsUI.CurrentTP;
        UpdateFGlow(currentTP);
    }
    private void LoadClassFromSelectedCharacter()
    {
        if (GameManager.Instance == null || GameManager.Instance.SelectedCharacter == null)
        {
            Debug.LogWarning("No selected character found");
            return;
        }

        string className = GameManager.Instance.SelectedCharacter.className;
        Debug.Log($"Loading hotbar for class: {className}");

        foreach (var registry in allRegistries)
        {
            if (registry.className == className)
            {
                LoadRegistry(registry);
                return;
            }
        }

        Debug.LogWarning($"No registry found for class: {className}");
    }
    public ClassAbilityRegistry GetCurrentRegistry() => currentRegistry;
    public bool IsOnFocusB => !focusA;
    private void LoadRegistry(ClassAbilityRegistry registry)
    {
        currentRegistry = registry;

        // Load Focus A by default
        LoadFocusAbilities(focusA);

        // Load general abilities
        SetSlot(slotA1, registry.slotA1);
        SetSlot(slotA2, registry.slotA2);
        SetSlot(slotA3, registry.slotA3);
        SetSlot(slotA4, registry.slotA4);

        // Extended slots
        SetExtendedSlotsVisible(registry.hasExtendedSlots);
        if (registry.hasExtendedSlots)
        {
            SetSlot(slot5, registry.slot5);
            SetSlot(slot6, registry.slot6);
            SetSlot(slot7, registry.slot7);
            SetSlot(slot8, registry.slot8);
        }
    }

    private void LoadFocusAbilities(bool loadFocusA)
    {
        if (currentRegistry == null) return;
        focusA = loadFocusA;

        if (loadFocusA)
        {
            SetSlot(slotQ, currentRegistry.slotQ_FocusA);
            SetSlot(slotE, currentRegistry.slotE_FocusA);
            SetSlot(slotR, currentRegistry.slotR_FocusA);
            SetSlot(slotF, currentRegistry.slotF_FocusA);
        }
        else
        {
            SetSlot(slotQ, currentRegistry.slotQ_FocusB);
            SetSlot(slotE, currentRegistry.slotE_FocusB);
            SetSlot(slotR, currentRegistry.slotR_FocusB);
            SetSlot(slotF, currentRegistry.slotF_FocusB);
        }
    }

    public void SwapFocus()
    {
        LoadFocusAbilities(!focusA);
        Debug.Log($"Swapped to Focus {(focusA ? "A" : "B")}");
    }

    private void SetSlot(HotbarSlot slot, AbilityBase ability)
    {
        if (slot == null) return;
        if (ability == null)
        {
            slot.ClearSlot();
            return;
        }
        slot.SetIcon(ability.icon);
        slot.SetGrade((HotbarSlot.AbilityGrade)ability.grade);
        slot.SetStackCount(0); // abilities don't show stacks
        slot.SetAbility(ability);
    }

    private void SetupKeybinds()
    {
        slot1?.SetKeybind("1");
        slot2?.SetKeybind("2");
        slot3?.SetKeybind("3");
        slot4?.SetKeybind("4");
        slot5?.SetKeybind("5");
        slot6?.SetKeybind("6");
        slot7?.SetKeybind("7");
        slot8?.SetKeybind("8");
        slotQ?.SetKeybind("Q");
        slotE?.SetKeybind("E");
        slotR?.SetKeybind("R");
        slotF?.SetKeybind("F");
        slotA1?.SetKeybind("A1");
        slotA2?.SetKeybind("A2");
        slotA3?.SetKeybind("A3");
        slotA4?.SetKeybind("A4");

        foreach (var slot in new[] { slot1, slot2, slot3, slot4,
            slot5, slot6, slot7, slot8, slotQ, slotE, slotR, slotF,
            slotA1, slotA2, slotA3, slotA4 })
        {
            slot?.ShowKeybind(showKeybinds);
        }
    }

    public void SetExtendedSlotsVisible(bool visible)
    {
        if (extendedSlotsContainer != null)
            extendedSlotsContainer.SetActive(visible);
    }

    private void UpdateFGlow(float tp)
    {
        if (fSlotGlow == null) return;

        if (tp >= 100f)
        {
            float t = (Time.time * 0.5f) % 1f;
            Color glowColor = tpRainbowGradient.Evaluate(t);
            glowColor.a = 0.8f;
            fSlotGlow.color = glowColor;
        }
        else
        {
            fSlotGlow.color = new Color(0f, 0f, 0f, 0f);
        }
    }
    public void ShowKeybinds(bool show)
    {
        showKeybinds = show;
        SetupKeybinds();
    }

    public void TriggerCooldown(string slotKey, float duration)
    {
        HotbarSlot slot = slotKey switch
        {
            "Q" => slotQ,
            "E" => slotE,
            "R" => slotR,
            "F" => slotF,
            "A1" => slotA1,
            "A2" => slotA2,
            "A3" => slotA3,
            "A4" => slotA4,
            "1" => slot1,
            "2" => slot2,
            "3" => slot3,
            "4" => slot4,
            _ => null
        };
        slot?.TriggerCooldown(duration);
    }
    public void SetConsumableSlot(string key, ConsumableItem item, int stackCount)
    {
        HotbarSlot slot = GetConsumableSlot(key);
        if (slot == null) return;
        slot.SetIcon(item.icon);
        slot.SetStackCount(stackCount);
        slot.SetGrade(HotbarSlot.AbilityGrade.Green);
    }

    public void ClearConsumableSlot(string key)
    {
        HotbarSlot slot = GetConsumableSlot(key);
        if (slot == null) return;
        slot.ClearSlot();
    }

    private HotbarSlot GetConsumableSlot(string key) => key switch
    {
        "1" => slot1,
        "2" => slot2,
        "3" => slot3,
        "4" => slot4,
        "5" => slot5,
        "6" => slot6,
        "7" => slot7,
        "8" => slot8,
        _ => null
    };
}