using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class HotbarSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private Image border;
    [SerializeField] private Image icon;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private TextMeshProUGUI keybindLabel;
    [SerializeField] private TextMeshProUGUI stackLabel;

    [Header("Grade Colors")]
    private Color gradeGrey = new Color(0.5f, 0.5f, 0.5f);
    private Color gradeGreen = new Color(0.2f, 0.8f, 0.2f);
    private Color gradeBlue = new Color(0.2f, 0.4f, 1.0f);
    private Color gradePurple = new Color(0.7f, 0.2f, 1.0f);
    private Color gradeRed = new Color(1.0f, 0.2f, 0.2f);
    private Color gradeGold = new Color(1.0f, 0.8f, 0.0f);

    private float cooldownRemaining = 0f;
    private float cooldownTotal = 0f;
    private bool onCooldown = false;
    private AbilityBase assignedAbility;

    public enum AbilityGrade { Grey, Green, Blue, Purple, Red, Gold }

    private void Update()
    {
        if (!onCooldown) return;

        cooldownRemaining -= Time.deltaTime;
        if (cooldownRemaining <= 0f)
        {
            cooldownRemaining = 0f;
            onCooldown = false;
            cooldownOverlay.fillAmount = 0f;
            cooldownText.text = "";
        }
        else
        {
            cooldownOverlay.fillAmount = cooldownRemaining / cooldownTotal;
            cooldownText.text = cooldownRemaining > 1f
                ? Mathf.CeilToInt(cooldownRemaining).ToString()
                : cooldownRemaining.ToString("F1");
        }
    }

    public void SetIcon(Sprite sprite)
    {
        if (icon != null)
        {
            icon.sprite = sprite;
            icon.enabled = sprite != null;
        }
    }
    public void SetStackCount(int count)
    {
        if (stackLabel == null) return;
        stackLabel.text = count > 1 ? count.ToString() : "";
        stackLabel.gameObject.SetActive(count > 0);
    }

    public void SetGrade(AbilityGrade grade)
    {
        if (border == null) return;
        border.color = grade switch
        {
            AbilityGrade.Grey => gradeGrey,
            AbilityGrade.Green => gradeGreen,
            AbilityGrade.Blue => gradeBlue,
            AbilityGrade.Purple => gradePurple,
            AbilityGrade.Red => gradeRed,
            AbilityGrade.Gold => gradeGold,
            _ => gradeGrey
        };
    }

    public void SetKeybind(string key)
    {
        if (keybindLabel != null)
            keybindLabel.text = key;
    }

    public void ShowKeybind(bool show)
    {
        if (keybindLabel != null)
            keybindLabel.gameObject.SetActive(show);
    }

    public void TriggerCooldown(float duration)
    {
        if (duration <= 0f) return;
        cooldownTotal = duration;
        cooldownRemaining = duration;
        onCooldown = true;
        cooldownOverlay.fillAmount = 1f;
    }

    public void ClearSlot()
    {
        SetIcon(null);
        SetGrade(AbilityGrade.Grey);
        if (cooldownText != null) cooldownText.text = "";
        if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0f;
        onCooldown = false;
    }

    public bool IsOnCooldown => onCooldown;
    public void SetAbility(AbilityBase ability)
    {
        assignedAbility = ability;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (assignedAbility != null)
            HotbarTooltipUI.Show(assignedAbility, Input.mousePosition);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HotbarTooltipUI.Hide();
    }
}