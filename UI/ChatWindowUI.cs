using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Chat window with two tabs — Combat and Chat.
/// Press Enter to activate the input field. Press Enter again to send. Press Escape to cancel.
/// Combat tab: damage, misses, skill gains, spell casting, death messages.
/// Chat tab: NPC dialogue, quest updates, player chat, system messages.
/// </summary>
public class ChatWindowUI : MonoBehaviour
{
    public static ChatWindowUI Instance { get; private set; }

    // ── Colors ────────────────────────────────────────────────────────────────
    private static readonly Color C_BG = new Color(0.04f, 0.04f, 0.06f, 0.85f);
    private static readonly Color C_TAB_ACT = new Color(0.20f, 0.16f, 0.08f, 1.00f);
    private static readonly Color C_TAB_INACT = new Color(0.08f, 0.08f, 0.10f, 1.00f);
    private static readonly Color C_BORDER = new Color(0.38f, 0.32f, 0.18f, 1.00f);
    private static readonly Color C_INPUT_BG = new Color(0.06f, 0.06f, 0.09f, 1.00f);
    private static readonly Color C_LABEL = new Color(0.65f, 0.65f, 0.65f, 1f);

    // Message colors
    public static readonly Color COL_DAMAGE_DEALT = new Color(1.00f, 1.00f, 1.00f, 1f);
    public static readonly Color COL_DAMAGE_TAKEN = new Color(0.90f, 0.25f, 0.25f, 1f);
    public static readonly Color COL_MISS = new Color(0.60f, 0.60f, 0.60f, 1f);
    public static readonly Color COL_GLANCE = new Color(0.80f, 0.65f, 0.30f, 1f);
    public static readonly Color COL_SKILL_GAIN = new Color(0.40f, 0.90f, 0.40f, 1f);
    public static readonly Color COL_SPELL = new Color(0.50f, 0.70f, 1.00f, 1f);
    public static readonly Color COL_DEATH = new Color(0.80f, 0.10f, 0.10f, 1f);
    public static readonly Color COL_NPC = new Color(0.80f, 0.68f, 0.30f, 1f);
    public static readonly Color COL_QUEST = new Color(0.40f, 0.90f, 0.40f, 1f);
    public static readonly Color COL_PLAYER_CHAT = new Color(0.95f, 0.95f, 0.95f, 1f);
    public static readonly Color COL_SYSTEM = new Color(0.70f, 0.70f, 0.40f, 1f);

    // ── Layout ────────────────────────────────────────────────────────────────
    private const float WIN_W = 520f;
    private const float WIN_H = 220f;
    private const float TAB_H = 26f;
    private const float INPUT_H = 28f;
    private const int MAX_LINES = 200;

    // ── State ─────────────────────────────────────────────────────────────────
    private enum Tab { Combat, Chat }
    private Tab activeTab = Tab.Combat;

    private List<(string text, Color color)> combatMessages = new List<(string, Color)>();
    private List<(string text, Color color)> chatMessages = new List<(string, Color)>();

    // ── UI refs ───────────────────────────────────────────────────────────────
    private Image combatTabImg, chatTabImg;
    private TextMeshProUGUI combatTabTxt, chatTabTxt;
    private ScrollRect combatScroll, chatScroll;
    private TextMeshProUGUI combatLog, chatLog;
    private TMP_InputField inputField;

    // ── Init ──────────────────────────────────────────────────────────────────

    private void Awake() => Instance = this;

    private void Start() => Build();

    private void Update()
    {
        // Enter to activate input, Escape to deactivate
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (inputField != null && !inputField.isFocused)
                inputField.ActivateInputField();
        }
        if (Input.GetKeyDown(KeyCode.Escape) && inputField != null && inputField.isFocused)
        {
            inputField.text = "";
            inputField.DeactivateInputField();
        }
    }

    private void Build()
    {
        var rt = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();

        gameObject.AddComponent<Image>().color = C_BG;
        AddOutline(gameObject, C_BORDER);

        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        rt.sizeDelta = new Vector2(WIN_W, WIN_H);
        rt.anchoredPosition = new Vector2(-8f, 8f);

        // ── Tab bar ───────────────────────────────────────────────────────────
        var tabBar = MakeRect("TabBar", transform, C_TAB_INACT,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -TAB_H), Vector2.zero);

        float tabW = WIN_W / 2f;

        var combatTab = MakeRect("CombatTab", tabBar.transform, C_TAB_ACT,
            new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0), new Vector2(tabW, 0));
        combatTabImg = combatTab.GetComponent<Image>();
        combatTabTxt = AddCenteredText(combatTab.transform, "Combat", 12, Color.white);
        combatTab.AddComponent<Button>().onClick.AddListener(() => SwitchTab(Tab.Combat));

        var chatTab = MakeRect("ChatTab", tabBar.transform, C_TAB_INACT,
            new Vector2(0, 0), new Vector2(0, 1), new Vector2(tabW, 0), new Vector2(WIN_W, 0));
        chatTabImg = chatTab.GetComponent<Image>();
        chatTabTxt = AddCenteredText(chatTab.transform, "Chat", 12, C_LABEL);
        chatTab.AddComponent<Button>().onClick.AddListener(() => SwitchTab(Tab.Chat));

        // ── Input field (always visible, bottom) ──────────────────────────────
        var inputArea = MakeRect("InputArea", transform, C_INPUT_BG,
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, INPUT_H));
        AddOutline(inputArea, C_BORDER);

        BuildInputField(inputArea.transform);

        // ── Scroll views ──────────────────────────────────────────────────────
        combatScroll = BuildScrollView("CombatScroll", transform, out combatLog);
        chatScroll = BuildScrollView("ChatScroll", transform, out chatLog);
        chatScroll.gameObject.SetActive(false);

        SwitchTab(Tab.Combat);
    }

    private void BuildInputField(Transform parent)
    {
        var go = new GameObject("InputField", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = new Vector2(4, 2); r.offsetMax = new Vector2(-4, -2);

        inputField = go.AddComponent<TMP_InputField>();

        // Viewport
        var vpGO = new GameObject("TextArea", typeof(RectTransform));
        vpGO.transform.SetParent(go.transform, false);
        var vpr = vpGO.GetComponent<RectTransform>();
        vpr.anchorMin = Vector2.zero; vpr.anchorMax = Vector2.one;
        vpr.offsetMin = vpr.offsetMax = Vector2.zero;
        vpGO.AddComponent<RectMask2D>();

        // Text
        var textGO = new GameObject("Text", typeof(RectTransform));
        textGO.transform.SetParent(vpGO.transform, false);
        var tr = textGO.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = tr.offsetMax = Vector2.zero;
        var textComp = textGO.AddComponent<TextMeshProUGUI>();
        textComp.fontSize = 13;
        textComp.color = COL_PLAYER_CHAT;

        // Placeholder
        var phGO = new GameObject("Placeholder", typeof(RectTransform));
        phGO.transform.SetParent(vpGO.transform, false);
        var phr = phGO.GetComponent<RectTransform>();
        phr.anchorMin = Vector2.zero; phr.anchorMax = Vector2.one;
        phr.offsetMin = phr.offsetMax = Vector2.zero;
        var phTxt = phGO.AddComponent<TextMeshProUGUI>();
        phTxt.text = "Press Enter to chat...";
        phTxt.fontSize = 13;
        phTxt.color = new Color(0.4f, 0.4f, 0.4f, 1f);
        phTxt.fontStyle = FontStyles.Italic;

        inputField.textComponent = textComp;
        inputField.placeholder = phTxt;
        inputField.textViewport = vpr;
        inputField.onSubmit.AddListener(OnInputSubmit);

        // Start deactivated so it doesn't steal keypresses
        inputField.DeactivateInputField();
    }

    private ScrollRect BuildScrollView(string name, Transform parent, out TextMeshProUGUI logText)
    {
        var scrollGO = new GameObject(name, typeof(RectTransform));
        scrollGO.transform.SetParent(parent, false);
        var sr = scrollGO.AddComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.scrollSensitivity = 20f;

        var scrollR = scrollGO.GetComponent<RectTransform>();
        scrollR.anchorMin = new Vector2(0, 0);
        scrollR.anchorMax = new Vector2(1, 1);
        scrollR.offsetMin = new Vector2(0, INPUT_H + 2f);
        scrollR.offsetMax = new Vector2(0, -TAB_H);

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
        contentR.anchorMin = new Vector2(0, 1);
        contentR.anchorMax = new Vector2(1, 1);
        contentR.pivot = new Vector2(0.5f, 1f);
        contentR.sizeDelta = Vector2.zero;
        sr.content = contentR;

        // ContentSizeFitter on content so it grows with text
        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // VerticalLayoutGroup to stack children
        var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.padding = new RectOffset(4, 4, 4, 4);

        // Log text as child of content
        var logGO = new GameObject("Log", typeof(RectTransform));
        logGO.transform.SetParent(contentGO.transform, false);
        var lr = logGO.GetComponent<RectTransform>();
        lr.anchorMin = new Vector2(0, 1);
        lr.anchorMax = new Vector2(1, 1);
        lr.pivot = new Vector2(0.5f, 1f);
        lr.offsetMin = lr.offsetMax = Vector2.zero;

        logText = logGO.AddComponent<TextMeshProUGUI>();
        logText.fontSize = 13;
        logText.color = Color.white;
        logText.textWrappingMode = TextWrappingModes.Normal;
        logText.richText = true;
        logText.text = "";

        // Let the log text size itself
        var logCsf = logGO.AddComponent<ContentSizeFitter>();
        logCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var le = logGO.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;

        return sr;
    }
    // ── Tab switching ─────────────────────────────────────────────────────────

    private void SwitchTab(Tab tab)
    {
        activeTab = tab;

        combatTabImg.color = tab == Tab.Combat ? C_TAB_ACT : C_TAB_INACT;
        chatTabImg.color = tab == Tab.Chat ? C_TAB_ACT : C_TAB_INACT;
        combatTabTxt.color = tab == Tab.Combat ? Color.white : C_LABEL;
        chatTabTxt.color = tab == Tab.Chat ? Color.white : C_LABEL;

        combatScroll.gameObject.SetActive(tab == Tab.Combat);
        chatScroll.gameObject.SetActive(tab == Tab.Chat);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public static void PostCombat(string message, Color color)
    {
        if (Instance == null) return;
        Instance.combatMessages.Add((message, color));
        if (Instance.combatMessages.Count > MAX_LINES)
            Instance.combatMessages.RemoveAt(0);
        Instance.RebuildLog(Instance.combatLog, Instance.combatMessages);
        Instance.ScrollToBottom(Instance.combatScroll);
    }

    public static void PostChat(string message, Color color)
    {
        if (Instance == null) return;
        Instance.chatMessages.Add((message, color));
        if (Instance.chatMessages.Count > MAX_LINES)
            Instance.chatMessages.RemoveAt(0);
        Instance.RebuildLog(Instance.chatLog, Instance.chatMessages);
        Instance.ScrollToBottom(Instance.chatScroll);
    }

    public static void Combat(string msg) => PostCombat(msg, COL_DAMAGE_DEALT);
    public static void DamageTaken(string msg) => PostCombat(msg, COL_DAMAGE_TAKEN);
    public static void Miss(string msg) => PostCombat(msg, COL_MISS);
    public static void Glance(string msg) => PostCombat(msg, COL_GLANCE);
    public static void SkillGain(string msg) => PostCombat(msg, COL_SKILL_GAIN);
    public static void Spell(string msg) => PostCombat(msg, COL_SPELL);
    public static void Death(string msg) => PostCombat(msg, COL_DEATH);
    public static void NPC(string msg) => PostChat(msg, COL_NPC);
    public static void Quest(string msg) => PostChat(msg, COL_QUEST);
    public static void System(string msg) => PostChat(msg, COL_SYSTEM);
    public static void PlayerChat(string msg) => PostChat(msg, COL_PLAYER_CHAT);

    // ── Input ─────────────────────────────────────────────────────────────────

    private void OnInputSubmit(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        PostChat($"You say, \"{text}\"", COL_PLAYER_CHAT);

        var npc = FindNearestNPC();
        if (npc != null)
            npc.ReceiveKeyword(text.Trim().ToLower());
        inputField.text = "";
        inputField.DeactivateInputField(); // release keyboard back to game
        SwitchTab(Tab.Chat);
    }
    private NPCEntity FindNearestNPC()
    {
        NPCEntity nearest = null;
        float nearestDist = float.MaxValue;
        foreach (var npc in FindObjectsByType<NPCEntity>())
        {
            float d = Vector3.Distance(
                Camera.main?.transform.position ?? Vector3.zero,
                npc.transform.position);
            if (d < 10f && d < nearestDist) { nearestDist = d; nearest = npc; }
        }
        return nearest;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void RebuildLog(TextMeshProUGUI log, List<(string text, Color color)> messages)
    {
        if (log == null) return;
        var sb = new System.Text.StringBuilder();
        foreach (var (text, color) in messages)
        {
            string hex = ColorUtility.ToHtmlStringRGB(color);
            sb.AppendLine($"<color=#{hex}>{text}</color>");
        }
        log.text = sb.ToString();
    }

    private void ScrollToBottom(ScrollRect scroll)
    {
        StartCoroutine(ScrollToBottomNextFrame(scroll));
    }

    private System.Collections.IEnumerator ScrollToBottomNextFrame(ScrollRect scroll)
    {
        yield return null; // wait one frame for layout to rebuild
        yield return null;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            scroll.content.GetComponent<RectTransform>());
        scroll.verticalNormalizedPosition = 0f;
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

    private TextMeshProUGUI AddCenteredText(Transform parent, string text, int size, Color color)
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
        t.alignment = TextAlignmentOptions.Center;
        t.fontStyle = FontStyles.Bold;
        return t;
    }

    private void AddOutline(GameObject go, Color col)
    {
        var ol = go.AddComponent<Outline>();
        ol.effectColor = col;
        ol.effectDistance = new Vector2(1, -1);
    }

    /// <summary>Returns true if the chat input field is currently focused — use to block game hotkeys.</summary>
    public static bool IsTyping => Instance != null && Instance.inputField != null && Instance.inputField.isFocused;
}