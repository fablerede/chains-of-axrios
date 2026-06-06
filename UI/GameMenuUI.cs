using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Small expandable menu button that sits next to the vitals panel.
/// Click the hamburger button to expand/collapse options.
/// Add this to the GameWorldCanvas and position it next to VitalsUI.
/// </summary>
public class GameMenuUI : MonoBehaviour
{
    private static readonly Color C_BG = new Color(0.06f, 0.06f, 0.08f, 0.96f);
    private static readonly Color C_BORDER = new Color(0.38f, 0.32f, 0.18f, 1.00f);
    private static readonly Color C_GOLD = new Color(0.80f, 0.68f, 0.30f, 1.00f);
    private static readonly Color C_BTN = new Color(0.10f, 0.10f, 0.14f, 1.00f);
    private static readonly Color C_BTN_HOV = new Color(0.20f, 0.18f, 0.12f, 1.00f);
    private static readonly Color C_VALUE = new Color(0.95f, 0.95f, 0.95f, 1.00f);
    private static readonly Color C_RED = new Color(0.50f, 0.10f, 0.10f, 1.00f);

    private const float BTN_W = 28f;
    private const float BTN_H = 28f;
    private const float MENU_W = 110f;
    private const float MENU_BTN_H = 26f;

    private GameObject menuPanel;
    private bool isOpen = false;

    private void Start()
    {
        Build();
    }

    private void Build()
    {
        var rt = GetComponent<RectTransform>();
        if (rt == null) rt = gameObject.AddComponent<RectTransform>();

        // ── Hamburger toggle button ───────────────────────────────────────────
        var toggleGO = new GameObject("MenuToggle", typeof(RectTransform), typeof(Image));
        toggleGO.transform.SetParent(transform, false);
        toggleGO.GetComponent<Image>().color = C_BG;
        var toggleOl = toggleGO.AddComponent<Outline>();
        toggleOl.effectColor = C_BORDER;
        toggleOl.effectDistance = new Vector2(1, -1);

        var toggleR = toggleGO.GetComponent<RectTransform>();
        toggleR.anchorMin = toggleR.anchorMax = new Vector2(0, 0);
        toggleR.pivot = new Vector2(0, 0);
        toggleR.sizeDelta = new Vector2(BTN_W, BTN_H);
        toggleR.anchoredPosition = Vector2.zero;

        var toggleBtn = toggleGO.AddComponent<Button>();
        toggleBtn.onClick.AddListener(ToggleMenu);

        var toggleTxtGO = new GameObject("Txt", typeof(RectTransform));
        toggleTxtGO.transform.SetParent(toggleGO.transform, false);
        var ttr = toggleTxtGO.GetComponent<RectTransform>();
        ttr.anchorMin = Vector2.zero; ttr.anchorMax = Vector2.one;
        ttr.offsetMin = ttr.offsetMax = Vector2.zero;
        var toggleTxt = toggleTxtGO.AddComponent<TextMeshProUGUI>();
        toggleTxt.text = "≡";
        toggleTxt.fontSize = 14;
        toggleTxt.color = C_GOLD;
        toggleTxt.alignment = TextAlignmentOptions.Center;
        toggleTxt.fontStyle = FontStyles.Bold;

        // ── Expandable menu panel (hidden by default) ─────────────────────────
        menuPanel = new GameObject("MenuPanel", typeof(RectTransform), typeof(Image));
        menuPanel.transform.SetParent(transform, false);
        menuPanel.GetComponent<Image>().color = C_BG;
        var panelOl = menuPanel.AddComponent<Outline>();
        panelOl.effectColor = C_BORDER;
        panelOl.effectDistance = new Vector2(1, -1);

        // Define menu options
        var menuItems = new (string label, bool isRed, System.Action action)[]
        {
            ("Camp", false, OnCampClicked),
            ("Quit", true,  OnQuitClicked),
        };

        var panelR = menuPanel.GetComponent<RectTransform>();
        panelR.anchorMin = panelR.anchorMax = new Vector2(0, 0);
        panelR.pivot = new Vector2(0, 0);
        panelR.sizeDelta = new Vector2(MENU_W, menuItems.Length * MENU_BTN_H);
        panelR.anchoredPosition = new Vector2(0, BTN_H + 2f); // appears above toggle

        for (int i = 0; i < menuItems.Length; i++)
        {
            var (label, isRed, action) = menuItems[i];
            int reversedI = menuItems.Length - 1 - i; // top item = first in list

            var btnGO = new GameObject(label, typeof(RectTransform), typeof(Image));
            btnGO.transform.SetParent(menuPanel.transform, false);
            btnGO.GetComponent<Image>().color = isRed ? C_RED : C_BTN;

            var btnR = btnGO.GetComponent<RectTransform>();
            btnR.anchorMin = new Vector2(0, 1);
            btnR.anchorMax = new Vector2(1, 1);
            btnR.offsetMin = new Vector2(1, -(reversedI + 1) * MENU_BTN_H + 1);
            btnR.offsetMax = new Vector2(-1, -reversedI * MENU_BTN_H - 1);

            var btn = btnGO.AddComponent<Button>();
            var captured = action;
            btn.onClick.AddListener(() => captured());

            // Hover color
            var colors = btn.colors;
            colors.highlightedColor = isRed
                ? new Color(0.7f, 0.15f, 0.15f, 1f)
                : C_BTN_HOV;
            btn.colors = colors;

            var txtGO = new GameObject("Txt", typeof(RectTransform));
            txtGO.transform.SetParent(btnGO.transform, false);
            var tr = txtGO.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(6, 0); tr.offsetMax = new Vector2(-6, 0);
            var tmp = txtGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 10;
            tmp.color = C_VALUE;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.fontStyle = FontStyles.Bold;
        }

        menuPanel.SetActive(false);
    }

    private void ToggleMenu()
    {
        isOpen = !isOpen;
        menuPanel.SetActive(isOpen);
    }

    // ── Camp ─────────────────────────────────────────────────────────────────

    private void OnCampClicked()
    {
        isOpen = false;
        menuPanel.SetActive(false);

        var charData = GameManager.Instance?.SelectedCharacter;
        if (charData == null) { GameManager.Instance?.LoadCharacterSelect(); return; }

        var player = FindAnyObjectByType<PlayerEntity>();

        // Save position
        if (player != null)
        {
            charData.posX = player.transform.position.x;
            charData.posY = player.transform.position.y;
            charData.posZ = player.transform.position.z;
        }

        // Save equipment and inventory
        var equipBlock = player?.GetComponent<EquipmentBlock>();
        if (equipBlock != null) equipBlock.SaveToCharacterData(charData);

        var inventory = player?.GetComponent<PlayerInventory>();
        if (inventory != null) inventory.SaveToCharacterData(charData);

        // Save consumables
        var consumables = FindAnyObjectByType<ConsumableManager>();
        if (consumables != null)
        {
            if (charData.consumableSlots == null || charData.consumableSlots.Length != ConsumableManager.MAX_SLOTS)
                charData.consumableSlots = new ConsumableSlotData[ConsumableManager.MAX_SLOTS];
            for (int i = 0; i < ConsumableManager.MAX_SLOTS; i++)
                charData.consumableSlots[i] = new ConsumableSlotData
                {
                    itemName = consumables.items[i] != null ? consumables.items[i].name : "",
                    stackCount = consumables.stacks[i]
                };
        }

        // Save skills
        var skillSheet = FindAnyObjectByType<MiscSkillSheet>();
        if (skillSheet != null)
        {
            charData.skillData = new SkillSaveData[skillSheet.skills.Count];
            for (int i = 0; i < skillSheet.skills.Count; i++)
                charData.skillData[i] = new SkillSaveData
                {
                    skillName = skillSheet.skills[i].name,
                    value = skillSheet.skills[i].value,
                    cap = skillSheet.skills[i].cap,
                    arrowState = (int)skillSheet.skills[i].arrowState
                };
        }

        // Single file write
        _ = GameManager.Instance.CharacterDataService.SaveCharacter(charData);

        // Return to character select
        GameManager.Instance?.LoadCharacterSelect();
    }

    // ── Quit ─────────────────────────────────────────────────────────────────

    private void OnQuitClicked()
    {
        // Save first then quit
        OnCampClicked();
        GameManager.Instance?.QuitGame();
    }
}