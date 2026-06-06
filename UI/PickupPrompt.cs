using UnityEngine;
using TMPro;

/// <summary>
/// Attach to a TextMeshProUGUI object on the GameWorldCanvas.
/// Shows "Press G to loot" when the player is near a WorldItem.
/// Multiple overlapping items are handled via a counter.
/// </summary>
public class PickupPromptUI : MonoBehaviour
{
    private static PickupPromptUI instance;

    private TextMeshProUGUI promptText;
    private int activeCount = 0;

    private static readonly Color C_GREEN = new Color(0.30f, 0.90f, 0.30f, 1f);

    private void Awake()
    {
        instance = this;
        promptText = GetComponent<TextMeshProUGUI>();

        if (promptText != null)
        {
            promptText.text = "";
            promptText.color = C_GREEN;
            promptText.fontSize = 14;
            promptText.fontStyle = FontStyles.Bold;
            promptText.alignment = TextAlignmentOptions.Center;
        }
    }

    /// <summary>Call when a player enters pickup range of a WorldItem.</summary>
    public static void Show()
    {
        if (instance == null) return;
        instance.activeCount++;
        if (instance.promptText != null)
            instance.promptText.text = "Press G to loot";
    }

    /// <summary>Call when a player exits pickup range of a WorldItem.</summary>
    public static void Hide()
    {
        if (instance == null) return;
        instance.activeCount = Mathf.Max(0, instance.activeCount - 1);
        if (instance.activeCount == 0 && instance.promptText != null)
            instance.promptText.text = "";
    }

    /// <summary>Force hide regardless of count — call on scene cleanup.</summary>
    public static void ForceHide()
    {
        if (instance == null) return;
        instance.activeCount = 0;
        if (instance.promptText != null)
            instance.promptText.text = "";
    }
}