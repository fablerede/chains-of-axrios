using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class VitalsUI : MonoBehaviour, IDragHandler
{
    [Header("HP Bar")]
    [SerializeField] private Image hpBarFill;
    [SerializeField] private TextMeshProUGUI hpTooltip;

    [Header("SP Bar")]
    [SerializeField] private Image spBarFill;
    [SerializeField] private TextMeshProUGUI spTooltip;

    [Header("TP Bar")]
    [SerializeField] private Image tpBarFill;
    [SerializeField] private TextMeshProUGUI tpText;
    [SerializeField] private Gradient tpRainbowGradient;

    [Header("Settings")]
    [SerializeField] private float tooltipDelay = 1f;

    private float currentHP = 450f;
    private float maxHP = 800f;
    private float currentSP = 120f;
    private float maxSP = 200f;
    private float currentTP = 0f;
    private float maxTP = 200f;

    public float CurrentTP => currentTP;

    public bool IsLocked { get; set; } = true;

    private RectTransform rectTransform;
    private Canvas canvas;
    private Coroutine hpTooltipCoroutine;
    private Coroutine spTooltipCoroutine;

    private Color hpGreen = new Color(0.2f, 0.85f, 0.2f);
    private Color hpRed = new Color(0.85f, 0.1f, 0.1f);
    private Color spBlue = new Color(0.2f, 0.5f, 1f);
    private Color tpYellow = new Color(1f, 0.85f, 0f);

    private string SaveKey => "VitalsUI_Position";

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        LoadPosition();
        if (hpTooltip != null) hpTooltip.gameObject.SetActive(false);
        if (spTooltip != null) spTooltip.gameObject.SetActive(false);
        UpdateBars();
    }

    private void Update()
    {
        if (currentTP >= 100f)
            UpdateBars();
    }

    // ── Public API ─────────────────────────────────────────────────────────

    public void UpdateHP(float current, float max)
    {
        currentHP = current;
        maxHP = max;
        UpdateBars();
    }

    public void UpdateSP(float current, float max)
    {
        currentSP = current;
        maxSP = max;
        UpdateBars();
    }

    public void UpdateTP(float current, float max)
    {
        currentTP = current;
        maxTP = max;
        UpdateBars();
    }

    public void OnDamageTaken(float damageAmount)
    {
        float tpGain = maxTP * 0.03f; // 3% of max TP per hit
        currentTP = Mathf.Min(currentTP + tpGain, maxTP);
        UpdateBars();
    }

    // ── Bars ───────────────────────────────────────────────────────────────

    private void UpdateBars()
    {
        if (hpBarFill != null)
        {
            float hpPercent = currentHP / maxHP;
            hpBarFill.fillAmount = hpPercent;
            hpBarFill.color = hpPercent > 0.3f
                ? hpGreen
                : Color.Lerp(hpRed, hpGreen, hpPercent / 0.3f);
        }

        if (spBarFill != null)
        {
            spBarFill.fillAmount = currentSP / maxSP;
            spBarFill.color = spBlue;
        }

        if (tpBarFill != null)
        {
            tpBarFill.fillAmount = currentTP / maxTP;
            tpBarFill.color = currentTP >= 100f
                ? tpRainbowGradient.Evaluate((Time.time * 0.5f) % 1f)
                : tpYellow;
        }

        if (tpText != null)
            tpText.text = $"{Mathf.FloorToInt(currentTP)}%";
    }

    // ── Tooltips ───────────────────────────────────────────────────────────

    public void ShowHPTooltip()
    {
        if (hpTooltipCoroutine != null) StopCoroutine(hpTooltipCoroutine);
        hpTooltipCoroutine = StartCoroutine(ShowTooltipDelayed(hpTooltip,
            $"{Mathf.FloorToInt(currentHP)} / {Mathf.FloorToInt(maxHP)}"));
    }

    public void HideHPTooltip()
    {
        if (hpTooltipCoroutine != null) StopCoroutine(hpTooltipCoroutine);
        if (hpTooltip != null) hpTooltip.gameObject.SetActive(false);
    }

    public void ShowSPTooltip()
    {
        if (spTooltipCoroutine != null) StopCoroutine(spTooltipCoroutine);
        spTooltipCoroutine = StartCoroutine(ShowTooltipDelayed(spTooltip,
            $"{Mathf.FloorToInt(currentSP)} / {Mathf.FloorToInt(maxSP)}"));
    }

    public void HideSPTooltip()
    {
        if (spTooltipCoroutine != null) StopCoroutine(spTooltipCoroutine);
        if (spTooltip != null) spTooltip.gameObject.SetActive(false);
    }

    private IEnumerator ShowTooltipDelayed(TextMeshProUGUI tooltip, string text)
    {
        yield return new WaitForSeconds(tooltipDelay);
        if (tooltip != null)
        {
            tooltip.text = text;
            tooltip.gameObject.SetActive(true);
        }
    }

    // ── Drag & Save ────────────────────────────────────────────────────────

    public void OnDrag(PointerEventData eventData)
    {
        if (IsLocked) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        SavePosition();
    }

    private void SavePosition()
    {
        PlayerPrefs.SetFloat(SaveKey + "_X", rectTransform.anchoredPosition.x);
        PlayerPrefs.SetFloat(SaveKey + "_Y", rectTransform.anchoredPosition.y);
        PlayerPrefs.Save();
    }

    private void LoadPosition()
    {
        if (PlayerPrefs.HasKey(SaveKey + "_X"))
        {
            float x = PlayerPrefs.GetFloat(SaveKey + "_X");
            float y = PlayerPrefs.GetFloat(SaveKey + "_Y");
            rectTransform.anchoredPosition = new Vector2(x, y);
        }
    }
}