using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class SkillScreenUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas parentCanvas;

    private static readonly Color C_BG = new Color(0.06f, 0.06f, 0.08f, 0.97f);
    private static readonly Color C_ROW_A = new Color(0.09f, 0.09f, 0.12f, 1.00f);
    private static readonly Color C_ROW_B = new Color(0.07f, 0.07f, 0.10f, 1.00f);
    private static readonly Color C_BORDER = new Color(0.38f, 0.32f, 0.18f, 1.00f);
    private static readonly Color C_GOLD = new Color(0.80f, 0.68f, 0.30f, 1.00f);
    private static readonly Color C_LABEL = new Color(0.65f, 0.65f, 0.65f, 1.00f);
    private static readonly Color C_VALUE = new Color(0.95f, 0.95f, 0.95f, 1.00f);
    private static readonly Color C_UP = new Color(0.30f, 0.85f, 0.30f, 1.00f);
    private static readonly Color C_DOWN = new Color(0.85f, 0.30f, 0.30f, 1.00f);
    private static readonly Color C_LOCK = new Color(0.60f, 0.60f, 0.60f, 1.00f);

    private const float W = 420f;
    private const float H = 520f;
    private const float TITLE_H = 24f;
    private const float FOOTER_H = 28f;
    private const float ROW_H = 30f;

    private GameObject screenRoot;
    private bool isOpen;
    private MiscSkillSheet skillSheet;
    private TextMeshProUGUI totalText;

    private Dictionary<string, TextMeshProUGUI> valueLabels = new Dictionary<string, TextMeshProUGUI>();
    private Dictionary<string, TextMeshProUGUI> arrowLabels = new Dictionary<string, TextMeshProUGUI>();
    private Dictionary<string, GameObject> openPopups = new Dictionary<string, GameObject>();

    private static readonly (string name, bool isPassive)[] SKILLS = new[]
    {
        ("Anatomy",          false),
        ("Arms Lore",        false),
        ("Camping",          false),
        ("Cartography",      false),
        ("Detect Hidden",    false),
        ("First Aid",        false),
        ("Fishing",          false),
        ("Fitness",          true),
        ("Focus",            true),
        ("Gathering",        true),
        ("Lockpicking",      false),
        ("Meditation",       true),
        ("Parry",            true),
        ("Poisoning",        true),
        ("Psycho Analyze",   false),
        ("Remove Trap",      false),
        ("Repair",           false),
        ("Resisting Spells", true),
        ("Stealth",          false),
        ("Taming",           false),
        ("Taste ID",         false),
        ("Theft",            false),
        ("Tracking",         false),
        ("Wild Lore",        false),
    };

    void Start()
    {
        if (parentCanvas == null) parentCanvas = GetComponentInParent<Canvas>();
        Build();
        screenRoot.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) Toggle();
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        screenRoot.SetActive(isOpen);
        if (isOpen)
        {
            FindSkillSheet();
            Refresh();
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(screenRoot.GetComponent<RectTransform>());
            // Force scroll to top
            var scroll = screenRoot.GetComponentInChildren<ScrollRect>();
            if (scroll != null) scroll.verticalNormalizedPosition = 1f;
        }
    }

    void FindSkillSheet()
    {
        if (skillSheet == null)
            skillSheet = FindAnyObjectByType<MiscSkillSheet>();
    }

    void Refresh()
    {
        if (skillSheet == null) return;
        foreach (var skill in skillSheet.skills)
        {
            if (valueLabels.TryGetValue(skill.name, out var lbl))
                lbl.text = skill.value.ToString("F1");
            if (arrowLabels.TryGetValue(skill.name, out var arrow))
            {
                arrow.text = ArrowText(skill.arrowState);
                arrow.color = ArrowColor(skill.arrowState);
            }
        }
        if (totalText != null)
            totalText.text = $"Total: {skillSheet.GlobalTotal:F1} / {MiscSkillSheet.GLOBAL_CAP}";
    }

    string ArrowText(SkillArrowState s) => s == SkillArrowState.Up ? "UP" : s == SkillArrowState.Down ? "DN" : "LK";
    Color ArrowColor(SkillArrowState s) => s == SkillArrowState.Up ? C_UP : s == SkillArrowState.Down ? C_DOWN : C_LOCK;

    void Build()
    {
        // Root window
        screenRoot = new GameObject("SkillScreen", typeof(RectTransform), typeof(Image));
        screenRoot.transform.SetParent(parentCanvas.transform, false);
        screenRoot.GetComponent<Image>().color = C_BG;
        screenRoot.AddComponent<Outline>().effectColor = C_BORDER;
        var rootR = screenRoot.GetComponent<RectTransform>();
        rootR.anchorMin = rootR.anchorMax = new Vector2(0.5f, 0.5f);
        rootR.sizeDelta = new Vector2(W, H);
        rootR.anchoredPosition = new Vector2(250f, 0f);

        // Title bar
        var titleGO = new GameObject("TitleBar", typeof(RectTransform), typeof(Image));
        titleGO.transform.SetParent(screenRoot.transform, false);
        titleGO.GetComponent<Image>().color = new Color(0.04f, 0.04f, 0.07f, 1f);
        var tr = titleGO.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(0, 1); tr.anchorMax = new Vector2(1, 1);
        tr.offsetMin = new Vector2(0, -TITLE_H); tr.offsetMax = Vector2.zero;

        var titleTxtGO = new GameObject("TitleTxt", typeof(RectTransform));
        titleTxtGO.transform.SetParent(titleGO.transform, false);
        var ttr = titleTxtGO.GetComponent<RectTransform>();
        ttr.anchorMin = Vector2.zero; ttr.anchorMax = Vector2.one;
        ttr.offsetMin = ttr.offsetMax = Vector2.zero;
        var ttmp = titleTxtGO.AddComponent<TextMeshProUGUI>();
        ttmp.text = "SKILLS"; ttmp.fontSize = 12; ttmp.color = C_GOLD;
        ttmp.fontStyle = FontStyles.Bold; ttmp.alignment = TextAlignmentOptions.Center;

        // Close button
        var closeGO = new GameObject("Close", typeof(RectTransform), typeof(Image));
        closeGO.transform.SetParent(titleGO.transform, false);
        closeGO.GetComponent<Image>().color = new Color(0.4f, 0.08f, 0.08f, 1f);
        var cr = closeGO.GetComponent<RectTransform>();
        cr.anchorMin = cr.anchorMax = new Vector2(1, 0.5f);
        cr.sizeDelta = new Vector2(20, 20); cr.anchoredPosition = new Vector2(-12, 0);
        closeGO.AddComponent<Button>().onClick.AddListener(Toggle);
        AddCenteredText(closeGO.transform, "X", 10, C_VALUE);

        // Footer
        var footerGO = new GameObject("Footer", typeof(RectTransform), typeof(Image));
        footerGO.transform.SetParent(screenRoot.transform, false);
        footerGO.GetComponent<Image>().color = new Color(0.04f, 0.04f, 0.07f, 1f);
        var fr = footerGO.GetComponent<RectTransform>();
        fr.anchorMin = new Vector2(0, 0); fr.anchorMax = new Vector2(1, 0);
        fr.offsetMin = Vector2.zero; fr.offsetMax = new Vector2(0, FOOTER_H);

        var ftGO = new GameObject("Total", typeof(RectTransform));
        ftGO.transform.SetParent(footerGO.transform, false);
        var ftr = ftGO.GetComponent<RectTransform>();
        ftr.anchorMin = Vector2.zero; ftr.anchorMax = Vector2.one;
        ftr.offsetMin = new Vector2(8, 0); ftr.offsetMax = Vector2.zero;
        totalText = ftGO.AddComponent<TextMeshProUGUI>();
        totalText.text = $"Total: 0.0 / {MiscSkillSheet.GLOBAL_CAP}";
        totalText.fontSize = 10; totalText.color = C_LABEL;
        totalText.alignment = TextAlignmentOptions.Left;

        // Scrollbar
        var sbGO = new GameObject("SB", typeof(RectTransform), typeof(Image));
        sbGO.transform.SetParent(screenRoot.transform, false);
        sbGO.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 1f);
        var sbR = sbGO.GetComponent<RectTransform>();
        sbR.anchorMin = new Vector2(1, 0); sbR.anchorMax = new Vector2(1, 1);
        sbR.offsetMin = new Vector2(-12, FOOTER_H); sbR.offsetMax = new Vector2(0, -TITLE_H);
        var sb = sbGO.AddComponent<Scrollbar>();
        sb.direction = Scrollbar.Direction.BottomToTop;
        var handleGO = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleGO.transform.SetParent(sbGO.transform, false);
        handleGO.GetComponent<Image>().color = C_BORDER;
        var hR = handleGO.GetComponent<RectTransform>();
        hR.anchorMin = Vector2.zero; hR.anchorMax = Vector2.one;
        hR.offsetMin = hR.offsetMax = Vector2.zero;
        sb.handleRect = hR;

        // Scroll view
        var scrollGO = new GameObject("Scroll", typeof(RectTransform));
        scrollGO.transform.SetParent(screenRoot.transform, false);
        var sr = scrollGO.AddComponent<ScrollRect>();
        sr.horizontal = false; sr.vertical = true;
        sr.verticalScrollbar = sb;
        sr.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        var scrollR = scrollGO.GetComponent<RectTransform>();
        scrollR.anchorMin = new Vector2(0, 0); scrollR.anchorMax = new Vector2(1, 1);
        scrollR.offsetMin = new Vector2(0, FOOTER_H);
        scrollR.offsetMax = new Vector2(-12, -TITLE_H);

        // Viewport
        var vpGO = new GameObject("Viewport", typeof(RectTransform), typeof(Image));
        vpGO.transform.SetParent(scrollGO.transform, false);
        vpGO.GetComponent<Image>().color = Color.white;
        vpGO.AddComponent<Mask>().showMaskGraphic = false;
        var vpR = vpGO.GetComponent<RectTransform>();
        vpR.anchorMin = Vector2.zero; vpR.anchorMax = Vector2.one;
        vpR.offsetMin = vpR.offsetMax = Vector2.zero;
        sr.viewport = vpR;

        // Content
        var contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(vpGO.transform, false);
        var contentR = contentGO.GetComponent<RectTransform>();
        contentR.anchorMin = new Vector2(0, 1); contentR.anchorMax = new Vector2(1, 1);
        contentR.pivot = new Vector2(0.5f, 1f);
        contentR.offsetMin = new Vector2(0, 0);
        contentR.offsetMax = new Vector2(0, 0);
        contentR.sizeDelta = new Vector2(0, 0); sr.content = contentR;

        var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 1f;
        vlg.padding = new RectOffset(0, 0, 0, 0);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Build skill rows
        for (int i = 0; i < SKILLS.Length; i++)
        {
            var (name, isPassive) = SKILLS[i];
            BuildRow(contentGO.transform, name, isPassive, i % 2 == 0);
        }
    }

    void BuildRow(Transform parent, string skillName, bool isPassive, bool altColor)
    {
        var rowGO = new GameObject($"Row_{skillName}", typeof(RectTransform), typeof(Image));
        rowGO.transform.SetParent(parent, false);
        rowGO.GetComponent<Image>().color = altColor ? C_ROW_A : C_ROW_B;

        var le = rowGO.AddComponent<LayoutElement>();
        le.preferredHeight = ROW_H;
        le.flexibleWidth = 1f;

        // Panel toggle button (active skills only)
        float nameXOffset = 6f;
        if (!isPassive)
        {
            var pbGO = new GameObject("PanelBtn", typeof(RectTransform), typeof(Image));
            pbGO.transform.SetParent(rowGO.transform, false);
            pbGO.GetComponent<Image>().color = new Color(0.25f, 0.20f, 0.12f, 1f);
            var pbR = pbGO.GetComponent<RectTransform>();
            pbR.anchorMin = new Vector2(0, 0); pbR.anchorMax = new Vector2(0, 1);
            pbR.offsetMin = new Vector2(4, 3); pbR.offsetMax = new Vector2(26, -3);
            string cap = skillName;
            pbGO.AddComponent<Button>().onClick.AddListener(() => ToggleSkillPopup(cap));
            AddCenteredText(pbGO.transform, ">", 10, C_GOLD);
            nameXOffset = 30f;
        }

        // Skill name
        var nameGO = new GameObject("Name", typeof(RectTransform));
        nameGO.transform.SetParent(rowGO.transform, false);
        var nr = nameGO.GetComponent<RectTransform>();
        nr.anchorMin = new Vector2(0, 0); nr.anchorMax = new Vector2(1, 1);
        nr.offsetMin = new Vector2(nameXOffset, 0); nr.offsetMax = new Vector2(-90, 0);
        var nameTxt = nameGO.AddComponent<TextMeshProUGUI>();
        nameTxt.text = skillName; nameTxt.fontSize = 12;
        nameTxt.color = C_VALUE; nameTxt.alignment = TextAlignmentOptions.Left;
        nameTxt.textWrappingMode = TextWrappingModes.Normal;

        // Value label
        var valGO = new GameObject("Val", typeof(RectTransform));
        valGO.transform.SetParent(rowGO.transform, false);
        var vr = valGO.GetComponent<RectTransform>();
        vr.anchorMin = new Vector2(1, 0); vr.anchorMax = new Vector2(1, 1);
        vr.offsetMin = new Vector2(-88, 0); vr.offsetMax = new Vector2(-44, 0);
        var valTxt = valGO.AddComponent<TextMeshProUGUI>();
        valTxt.text = "0.0"; valTxt.fontSize = 12;
        valTxt.color = C_LABEL; valTxt.alignment = TextAlignmentOptions.Right;
        valueLabels[skillName] = valTxt;

        // Arrow toggle
        var arrowGO = new GameObject("Arrow", typeof(RectTransform), typeof(Image));
        arrowGO.transform.SetParent(rowGO.transform, false);
        arrowGO.GetComponent<Image>().color = new Color(0.12f, 0.12f, 0.15f, 1f);
        var ar = arrowGO.GetComponent<RectTransform>();
        ar.anchorMin = new Vector2(1, 0); ar.anchorMax = new Vector2(1, 1);
        ar.offsetMin = new Vector2(-40, 3); ar.offsetMax = new Vector2(-4, -3);
        string capArrow = skillName;
        arrowGO.AddComponent<Button>().onClick.AddListener(() => CycleArrow(capArrow));

        var arTxtGO = new GameObject("ArTxt", typeof(RectTransform));
        arTxtGO.transform.SetParent(arrowGO.transform, false);
        var artr = arTxtGO.GetComponent<RectTransform>();
        artr.anchorMin = Vector2.zero; artr.anchorMax = Vector2.one;
        artr.offsetMin = artr.offsetMax = Vector2.zero;
        var arTxt = arTxtGO.AddComponent<TextMeshProUGUI>();
        arTxt.text = "UP"; arTxt.fontSize = 9; arTxt.color = C_UP;
        arTxt.fontStyle = FontStyles.Bold;
        arTxt.alignment = TextAlignmentOptions.Center;
        arrowLabels[skillName] = arTxt;
    }

    void CycleArrow(string skillName)
    {
        FindSkillSheet();
        var skill = skillSheet?.Get(skillName);
        if (skill == null) return;

        skill.arrowState = skill.arrowState switch
        {
            SkillArrowState.Up => SkillArrowState.Down,
            SkillArrowState.Down => SkillArrowState.Lock,
            SkillArrowState.Lock => SkillArrowState.Up,
            _ => SkillArrowState.Up
        };

        if (arrowLabels.TryGetValue(skillName, out var lbl))
        {
            lbl.text = ArrowText(skill.arrowState);
            lbl.color = ArrowColor(skill.arrowState);
        }

        skillSheet.SaveToCharacterData();

        if (totalText != null)
            totalText.text = $"Total: {skillSheet.GlobalTotal:F1} / {MiscSkillSheet.GLOBAL_CAP}";
    }

    void ToggleSkillPopup(string skillName)
    {
        if (openPopups.TryGetValue(skillName, out var existing) && existing != null)
        {
            Destroy(existing);
            openPopups.Remove(skillName);
            return;
        }
        openPopups[skillName] = BuildSkillPopup(skillName);
    }

    GameObject BuildSkillPopup(string skillName)
    {
        var go = new GameObject($"Popup_{skillName}", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parentCanvas.transform, false);
        go.GetComponent<Image>().color = C_BG;
        go.AddComponent<Outline>().effectColor = C_BORDER;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(180f, 90f);
        rt.anchoredPosition = new Vector2(Random.Range(-200f, 200f), Random.Range(-100f, 100f));

        // Header
        var hdrGO = new GameObject("Hdr", typeof(RectTransform), typeof(Image));
        hdrGO.transform.SetParent(go.transform, false);
        hdrGO.GetComponent<Image>().color = new Color(0.25f, 0.20f, 0.12f, 1f);
        var hr = hdrGO.GetComponent<RectTransform>();
        hr.anchorMin = new Vector2(0, 1); hr.anchorMax = new Vector2(1, 1);
        hr.offsetMin = new Vector2(0, -24f); hr.offsetMax = Vector2.zero;

        var hTxtGO = new GameObject("HdrTxt", typeof(RectTransform));
        hTxtGO.transform.SetParent(hdrGO.transform, false);
        var htr = hTxtGO.GetComponent<RectTransform>();
        htr.anchorMin = Vector2.zero; htr.anchorMax = Vector2.one;
        htr.offsetMin = new Vector2(6, 0); htr.offsetMax = new Vector2(-26, 0);
        var hTxt = hTxtGO.AddComponent<TextMeshProUGUI>();
        hTxt.text = skillName; hTxt.fontSize = 10; hTxt.color = C_VALUE;
        hTxt.fontStyle = FontStyles.Bold; hTxt.alignment = TextAlignmentOptions.Left;

        // Close button on popup
        var clGO = new GameObject("Close", typeof(RectTransform), typeof(Image));
        clGO.transform.SetParent(hdrGO.transform, false);
        clGO.GetComponent<Image>().color = new Color(0.5f, 0.1f, 0.1f, 1f);
        var clR = clGO.GetComponent<RectTransform>();
        clR.anchorMin = clR.anchorMax = new Vector2(1, 0.5f);
        clR.sizeDelta = new Vector2(18, 18); clR.anchoredPosition = new Vector2(-12, 0);
        string cap = skillName;
        clGO.AddComponent<Button>().onClick.AddListener(() => { openPopups.Remove(cap); Destroy(go); });
        AddCenteredText(clGO.transform, "X", 9, C_VALUE);

        // Value display
        var valGO = new GameObject("Val", typeof(RectTransform));
        valGO.transform.SetParent(go.transform, false);
        var vr = valGO.GetComponent<RectTransform>();
        vr.anchorMin = new Vector2(0, 1); vr.anchorMax = new Vector2(1, 1);
        vr.offsetMin = new Vector2(8, -50f); vr.offsetMax = new Vector2(-8, -28f);
        var vTxt = valGO.AddComponent<TextMeshProUGUI>();
        var skill = skillSheet?.Get(skillName);
        vTxt.text = skill != null ? $"{skill.value:F1} / {skill.cap}" : "0.0 / 100";
        vTxt.fontSize = 11; vTxt.color = C_LABEL;
        vTxt.alignment = TextAlignmentOptions.Center;

        // Use button
        var ubGO = new GameObject("UseBtn", typeof(RectTransform), typeof(Image));
        ubGO.transform.SetParent(go.transform, false);
        ubGO.GetComponent<Image>().color = new Color(0.25f, 0.20f, 0.12f, 1f);
        ubGO.AddComponent<Outline>().effectColor = C_BORDER;
        var ubR = ubGO.GetComponent<RectTransform>();
        ubR.anchorMin = new Vector2(0.1f, 0); ubR.anchorMax = new Vector2(0.9f, 0);
        ubR.offsetMin = new Vector2(0, 8f); ubR.offsetMax = new Vector2(0, 34f);
        string capSkill = skillName;
        TextMeshProUGUI capturedValTxt = vTxt;
        ubGO.AddComponent<Button>().onClick.AddListener(() => UseSkill(capSkill, capturedValTxt));
        AddCenteredText(ubGO.transform, "Use", 10, C_GOLD, true);

        // Draggable
        var dragger = go.AddComponent<SkillPopupDragger>();
        dragger.canvas = parentCanvas;

        return go;
    }

    void UseSkill(string skillName, TextMeshProUGUI valueDisplay)
    {
        FindSkillSheet();
        if (skillSheet == null) return;

        bool gained = skillSheet.TryGain(skillName);
        var skill = skillSheet.Get(skillName);

        if (skill != null && valueDisplay != null)
            valueDisplay.text = $"{skill.value:F1} / {skill.cap}";

        if (valueLabels.TryGetValue(skillName, out var rowLbl) && skill != null)
            rowLbl.text = skill.value.ToString("F1");

        if (totalText != null)
            totalText.text = $"Total: {skillSheet.GlobalTotal:F1} / {MiscSkillSheet.GLOBAL_CAP}";

        Debug.Log($"Used skill: {skillName} — gained: {gained}");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    void AddCenteredText(Transform parent, string text, int size, Color color, bool bold = false)
    {
        var go = new GameObject("Txt", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        if (bold) t.fontStyle = FontStyles.Bold;
    }
}

public class SkillPopupDragger : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    public Canvas canvas;
    private Vector2 dragOffset;
    private RectTransform rt;
    private void Awake() => rt = GetComponent<RectTransform>();

    public void OnBeginDrag(PointerEventData e)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt, e.position, e.pressEventCamera, out dragOffset);
    }

    public void OnDrag(PointerEventData e)
    {
        if (canvas == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(), e.position,
            e.pressEventCamera, out Vector2 pos);
        rt.anchoredPosition = pos - dragOffset;
    }
}