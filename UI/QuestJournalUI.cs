using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Quest Journal — press J to open/close.
/// Shows active quests with current stage journal entry and hint.
/// Toggle to show completed quests too.
/// Attach to GameWorldCanvas.
/// </summary>
public class QuestJournalUI : MonoBehaviour
{
    [Header("Quest Data — drag all quest assets here")]
    [SerializeField] private QuestData[] allQuests;

    private static readonly Color C_BG = new Color(0.06f, 0.06f, 0.08f, 0.97f);
    private static readonly Color C_HEADER = new Color(0.04f, 0.04f, 0.07f, 1.00f);
    private static readonly Color C_ENTRY_A = new Color(0.09f, 0.09f, 0.12f, 1.00f);
    private static readonly Color C_ENTRY_B = new Color(0.07f, 0.07f, 0.10f, 1.00f);
    private static readonly Color C_BORDER = new Color(0.38f, 0.32f, 0.18f, 1.00f);
    private static readonly Color C_GOLD = new Color(0.80f, 0.68f, 0.30f, 1.00f);
    private static readonly Color C_LABEL = new Color(0.65f, 0.65f, 0.65f, 1.00f);
    private static readonly Color C_VALUE = new Color(0.95f, 0.95f, 0.95f, 1.00f);
    private static readonly Color C_HINT = new Color(0.55f, 0.75f, 0.55f, 1.00f);
    private static readonly Color C_COMPLETE = new Color(0.40f, 0.40f, 0.40f, 1.00f);

    private const float W = 440f;
    private const float H = 500f;
    private const float TITLE_H = 26f;
    private const float FOOTER_H = 32f;

    private GameObject root;
    private bool isOpen = false;
    private bool showCompleted = false;

    private ScrollRect scrollRect;
    private Transform contentParent;
    private TextMeshProUGUI toggleBtnText;

    private void Start()
    {
        Build();
        root.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J) && !ChatWindowUI.IsTyping)
            Toggle();
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        root.SetActive(isOpen);
        if (isOpen) Refresh();
    }

    // ── Build ─────────────────────────────────────────────────────────────────

    private void Build()
    {
        var canvas = GetComponentInParent<Canvas>();

        root = new GameObject("QuestJournal", typeof(RectTransform), typeof(Image));
        root.transform.SetParent(canvas.transform, false);
        root.GetComponent<Image>().color = C_BG;
        AddOutline(root, C_BORDER);

        var rootR = root.GetComponent<RectTransform>();
        rootR.anchorMin = rootR.anchorMax = new Vector2(0.5f, 0.5f);
        rootR.sizeDelta = new Vector2(W, H);
        rootR.anchoredPosition = new Vector2(-100f, 0f);

        // Title bar
        var titleGO = MakeRect("TitleBar", root.transform, C_HEADER,
            new Vector2(0, 1), new Vector2(1, 1),
            new Vector2(0, -TITLE_H), Vector2.zero);

        var titleTxt = titleGO.AddComponent<TextMeshProUGUI>() != null
            ? AddCenteredTMP(titleGO.transform, "QUEST JOURNAL", 12, C_GOLD, true)
            : null;

        // Close button
        var closeGO = MakeRect("Close", titleGO.transform, new Color(0.4f, 0.08f, 0.08f, 1f),
            new Vector2(1, 0), new Vector2(1, 1),
            new Vector2(-22, 0), Vector2.zero);
        closeGO.AddComponent<Button>().onClick.AddListener(Toggle);
        AddCenteredTMP(closeGO.transform, "X", 10, C_VALUE, false);

        // Footer with toggle
        var footerGO = MakeRect("Footer", root.transform, C_HEADER,
            new Vector2(0, 0), new Vector2(1, 0),
            Vector2.zero, new Vector2(0, FOOTER_H));

        var toggleGO = MakeRect("Toggle", footerGO.transform, new Color(0.12f, 0.12f, 0.16f, 1f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-80f, -11f), new Vector2(80f, 11f));
        AddOutline(toggleGO, C_BORDER);
        var toggleBtn = toggleGO.AddComponent<Button>();
        toggleBtn.onClick.AddListener(ToggleShowCompleted);
        toggleBtnText = AddCenteredTMP(toggleGO.transform, "Show Completed: OFF", 9, C_LABEL, false);

        // Scroll view
        var scrollGO = new GameObject("Scroll", typeof(RectTransform));
        scrollGO.transform.SetParent(root.transform, false);
        scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 25f;

        var scrollR = scrollGO.GetComponent<RectTransform>();
        scrollR.anchorMin = new Vector2(0, 0);
        scrollR.anchorMax = new Vector2(1, 1);
        scrollR.offsetMin = new Vector2(0, FOOTER_H);
        scrollR.offsetMax = new Vector2(0, -TITLE_H);

        // Viewport
        var vpGO = new GameObject("Viewport", typeof(RectTransform), typeof(Image));
        vpGO.transform.SetParent(scrollGO.transform, false);
        vpGO.GetComponent<Image>().color = Color.white;
        vpGO.AddComponent<Mask>().showMaskGraphic = false;
        var vpR = vpGO.GetComponent<RectTransform>();
        vpR.anchorMin = Vector2.zero; vpR.anchorMax = Vector2.one;
        vpR.offsetMin = vpR.offsetMax = Vector2.zero;
        scrollRect.viewport = vpR;

        // Content
        var contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(vpGO.transform, false);
        var contentR = contentGO.GetComponent<RectTransform>();
        contentR.anchorMin = new Vector2(0, 1);
        contentR.anchorMax = new Vector2(1, 1);
        contentR.pivot = new Vector2(0.5f, 1f);
        contentR.sizeDelta = Vector2.zero;
        scrollRect.content = contentR;
        contentParent = contentR;

        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.spacing = 2f;
        vlg.padding = new RectOffset(4, 4, 4, 4);
    }

    // ── Refresh ───────────────────────────────────────────────────────────────

    private void Refresh()
    {
        // Clear old entries
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        var charData = GameManager.Instance?.SelectedCharacter;
        if (charData == null || allQuests == null) return;

        bool anyShown = false;
        int entryIndex = 0;

        foreach (var quest in allQuests)
        {
            if (quest == null) continue;

            int stage = charData.GetQuestStage(quest.questId);
            if (stage == 0) continue; // not started

            // Find current stage data
            QuestData.QuestStage stageData = null;
            QuestData.QuestStage lastStage = null;
            foreach (var s in quest.stages)
            {
                if (s.stageNumber <= stage) lastStage = s;
                if (s.stageNumber == stage) stageData = s;
            }
            if (stageData == null) stageData = lastStage;
            if (stageData == null) continue;

            // Determine if complete (last stage)
            bool isComplete = false;
            int maxStage = 0;
            foreach (var s in quest.stages)
                if (s.stageNumber > maxStage) maxStage = s.stageNumber;
            isComplete = stage >= maxStage;

            // Skip completed if toggle is off
            if (isComplete && !showCompleted) continue;

            BuildQuestEntry(quest, stageData, isComplete, entryIndex % 2 == 0);
            entryIndex++;
            anyShown = true;
        }

        if (!anyShown)
        {
            var emptyGO = new GameObject("Empty", typeof(RectTransform));
            emptyGO.transform.SetParent(contentParent, false);
            var txt = emptyGO.AddComponent<TextMeshProUGUI>();
            txt.text = "No active quests.";
            txt.fontSize = 11;
            txt.color = C_LABEL;
            txt.alignment = TextAlignmentOptions.Center;
            txt.textWrappingMode = TextWrappingModes.Normal;
            var le = emptyGO.AddComponent<LayoutElement>();
            le.preferredHeight = 40f;
        }
    }

    private void BuildQuestEntry(QuestData quest, QuestData.QuestStage stageData,
        bool isComplete, bool altColor)
    {
        var entryGO = new GameObject($"Quest_{quest.questId}", typeof(RectTransform), typeof(Image));
        entryGO.transform.SetParent(contentParent, false);
        entryGO.GetComponent<Image>().color = altColor ? C_ENTRY_A : C_ENTRY_B;
        AddOutline(entryGO, new Color(0.2f, 0.2f, 0.2f, 0.5f));

        var vlg = entryGO.AddComponent<VerticalLayoutGroup>();
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.padding = new RectOffset(8, 8, 6, 6);
        vlg.spacing = 4f;

        var csf = entryGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var le = entryGO.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;

        // Quest name
        Color nameColor = isComplete ? C_COMPLETE : C_GOLD;
        string nameText = isComplete ? $"✓ {quest.questName}" : quest.questName;
        AddTMP(entryGO.transform, nameText, 12, nameColor, true);

        // Stage number
        AddTMP(entryGO.transform, $"Stage {stageData.stageNumber}", 9, C_LABEL, false);

        // Separator line
        var sepGO = new GameObject("Sep", typeof(RectTransform), typeof(Image));
        sepGO.transform.SetParent(entryGO.transform, false);
        sepGO.GetComponent<Image>().color = C_BORDER;
        var sepLE = sepGO.AddComponent<LayoutElement>();
        sepLE.preferredHeight = 1f;
        sepLE.flexibleWidth = 1f;

        // Journal entry
        AddTMP(entryGO.transform, stageData.journalEntry, 11, C_VALUE, false, true);

        // Hint (only for active quests)
        if (!isComplete && !string.IsNullOrEmpty(stageData.stageHint))
        {
            AddTMP(entryGO.transform, $"Hint: {stageData.stageHint}", 10, C_HINT, false, true);
        }
    }

    // ── Toggle ────────────────────────────────────────────────────────────────

    private void ToggleShowCompleted()
    {
        showCompleted = !showCompleted;
        if (toggleBtnText != null)
            toggleBtnText.text = showCompleted ? "Show Completed: ON" : "Show Completed: OFF";
        Refresh();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private TextMeshProUGUI AddTMP(Transform parent, string text, int size,
        Color color, bool bold, bool wrap = false)
    {
        var go = new GameObject("Txt", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        t.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
        t.alignment = TextAlignmentOptions.Left;
        var le = go.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;
        return t;
    }

    private TextMeshProUGUI AddCenteredTMP(Transform parent, string text, int size,
        Color color, bool bold)
    {
        var go = new GameObject("Txt", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        t.alignment = TextAlignmentOptions.Center;
        return t;
    }

    private GameObject MakeRect(string name, Transform parent, Color col,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = col;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = anchorMin; r.anchorMax = anchorMax;
        r.offsetMin = offsetMin; r.offsetMax = offsetMax;
        return go;
    }

    private void AddOutline(GameObject go, Color col)
    {
        var ol = go.AddComponent<Outline>();
        ol.effectColor = col;
        ol.effectDistance = new Vector2(1, -1);
    }
}