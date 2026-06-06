using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Floating tooltip that appears when hovering over hotbar ability slots.
/// Add to GameWorldCanvas. Singleton — one instance per scene.
/// </summary>
public class HotbarTooltipUI : MonoBehaviour
{
    public static HotbarTooltipUI Instance { get; private set; }

    private static readonly Color C_BG = new Color(0.04f, 0.04f, 0.06f, 0.96f);
    private static readonly Color C_BORDER = new Color(0.38f, 0.32f, 0.18f, 1.00f);
    private static readonly Color C_GOLD = new Color(0.80f, 0.68f, 0.30f, 1.00f);
    private static readonly Color C_LABEL = new Color(0.65f, 0.65f, 0.65f, 1.00f);
    private static readonly Color C_VALUE = new Color(0.95f, 0.95f, 0.95f, 1.00f);
    private static readonly Color C_BLUE = new Color(0.50f, 0.70f, 1.00f, 1.00f);

    private RectTransform panelRect;
    private TextMeshProUGUI nameTxt;
    private TextMeshProUGUI descTxt;
    private TextMeshProUGUI statsTxt;
    private Canvas parentCanvas;
    private GameObject panel;

    private void Awake()
    {
        Instance = this;
        parentCanvas = GetComponentInParent<Canvas>();
        Build();
        panel.SetActive(false);
    }

    private void Build()
    {
        panel = new GameObject("HotbarTooltip", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(transform, false);
        panel.GetComponent<Image>().color = C_BG;
        var ol = panel.AddComponent<Outline>();
        ol.effectColor = C_BORDER;
        ol.effectDistance = new Vector2(1, -1);

        panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = panelRect.anchorMax = Vector2.zero;
        panelRect.pivot = new Vector2(0f, 0f);
        panelRect.sizeDelta = new Vector2(220f, 100f);

        // Layout
        var vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(8, 8, 6, 6);
        vlg.spacing = 3f;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        var csf = panel.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        nameTxt = AddTMP(panel.transform, "", 12, C_GOLD, true);
        descTxt = AddTMP(panel.transform, "", 10, C_VALUE, false, true);
        statsTxt = AddTMP(panel.transform, "", 10, C_LABEL, false, true);
    }

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
        var le = go.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;
        return t;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public static void Show(AbilityBase ability, Vector2 screenPos)
    {
        if (Instance == null || ability == null) return;
        Instance.ShowInternal(ability, screenPos);
    }

    public static void Hide()
    {
        if (Instance == null) return;
        Instance.panel.SetActive(false);
    }

    private void ShowInternal(AbilityBase ability, Vector2 screenPos)
    {
        // Name
        nameTxt.text = ability.abilityName;

        // Description
        descTxt.text = string.IsNullOrEmpty(ability.description) ? "" : ability.description;
        descTxt.gameObject.SetActive(!string.IsNullOrEmpty(ability.description));

        // Stats line
        var sb = new System.Text.StringBuilder();

        if (ability.cooldown > 0f)
            sb.AppendLine($"Cooldown: {ability.cooldown:F1}s");
        if (ability.castTime > 0f)
            sb.AppendLine($"Cast Time: {ability.castTime:F1}s");
        if (ability.manaCost > 0f)
            sb.AppendLine($"SP Cost: {ability.manaCost:F0}");
        if (ability.tpCost > 0f)
            sb.AppendLine($"TP Cost: {ability.tpCost:F0}");
        if (ability.range > 0f)
            sb.AppendLine($"Range: {ability.range:F0}m");

        sb.Append($"Target: {ability.targetType}");

        statsTxt.text = sb.ToString();

        panel.SetActive(true);
        panel.transform.SetAsLastSibling();

        // Position near mouse, above the hotbar
        var canvasRect = parentCanvas.GetComponent<RectTransform>();
        Vector2 scaledPos = new Vector2(
            (screenPos.x / Screen.width) * canvasRect.sizeDelta.x,
            (screenPos.y / Screen.height) * canvasRect.sizeDelta.y);
        scaledPos -= canvasRect.sizeDelta * 0.5f;

        // Offset upward so it appears above the slot
        scaledPos += new Vector2(10f, 80f);

        panelRect.anchoredPosition = scaledPos;
    }
}