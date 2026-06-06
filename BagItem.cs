using UnityEngine;

/// <summary>
/// A bag item that can be equipped in a bag slot on the character screen.
/// When equipped, provides a number of inventory slots equal to slotCount.
/// A bag inside another bag is just an item — it only functions when in a bag slot.
/// </summary>
[CreateAssetMenu(fileName = "BagItem_New", menuName = "Chains of Axrios/Items/Bag")]
public class BagItem : ScriptableObject
{
    [Header("Identity")]
    public string itemName;
    public int itemLevel = 1;

    [Header("Display")]
    public Sprite icon;
    [TextArea]
    public string description;

    [Header("Bag Properties")]
    [Tooltip("Number of item slots this bag provides when equipped.")]
    [Range(1, 32)]
    public int slotCount = 8;

    [Tooltip("Visual color tint for this bag's window header.")]
    public Color windowColor = new Color(0.25f, 0.20f, 0.12f, 1f);
}