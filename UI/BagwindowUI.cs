using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Logic-only bag window. All UI structure lives in the BagWindow prefab.
/// The editor script BagWindowBuilder creates that prefab.
/// At runtime, StatScreenUI instantiates the prefab and calls Initialize().
/// </summary>
public class BagWindowUI : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    [Header("Prefab References — assigned by BagWindowBuilder")]
    [SerializeField] public TextMeshProUGUI bagNameLabel;
    [SerializeField] public Transform slotContainer;
    [SerializeField] public GameObject tooltipPanel;
    [SerializeField] public TextMeshProUGUI tooltipText;
    [SerializeField] public GameObject slotPrefab;

    [Header("World Drop")]
    [SerializeField] public GameObject worldItemPrefab;

    private static readonly Color C_SLOT = new Color(0.14f, 0.14f, 0.18f, 1.00f);
    private static readonly Color C_HELD = new Color(0.50f, 0.42f, 0.20f, 1.00f);
    private static readonly Color C_BORDER = new Color(0.38f, 0.32f, 0.18f, 1.00f);
    private const float SLOT_SIZE = 52f;

    private PlayerInventory inventory;
    private int bagSlotIndex;
    private BagItem bag;
    private Canvas parentCanvas;
    private RectTransform rectTransform;

    private Image[] slotImages;
    private Image[] slotIcons;
    private TextMeshProUGUI[] stackLabels;
    private Vector2 dragOffset;

    private static InventoryItem heldItem = null;
    private static int heldFromBag = -1;
    private static int heldFromSlot = -1;
    private static Image heldCursor = null;

    private EquipmentBlock equipBlock;
    private StatBlock statBlock;

    // ── Initialize ────────────────────────────────────────────────────────────

    public void Initialize(PlayerInventory inv, int slotIndex, BagItem bagItem, Canvas canvas)
    {
        inventory = inv;
        bagSlotIndex = slotIndex;
        bag = bagItem;
        parentCanvas = canvas;
        rectTransform = GetComponent<RectTransform>();

        var player = FindAnyObjectByType<PlayerEntity>();
        if (player != null)
        {
            equipBlock = player.GetComponent<EquipmentBlock>();
            statBlock = player.GetComponent<StatBlock>();
        }

        if (bagNameLabel != null)
            bagNameLabel.text = bag != null ? bag.itemName : "Bag";

        BuildSlots();
        HideTooltip();
        Refresh();
    }

    // ── Slot spawning ─────────────────────────────────────────────────────────

    private void BuildSlots()
    {
        if (slotContainer == null || slotPrefab == null) return;

        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        int count = bag != null ? bag.slotCount : 8;
        slotImages = new Image[count];
        slotIcons = new Image[count];
        stackLabels = new TextMeshProUGUI[count];

        for (int i = 0; i < count; i++)
        {
            var slotGO = Instantiate(slotPrefab, slotContainer);
            slotGO.name = $"Slot{i}";
            slotGO.SetActive(true);

            slotImages[i] = slotGO.GetComponent<Image>();
            slotIcons[i] = slotGO.transform.Find("Icon")?.GetComponent<Image>();

            if (slotImages[i] != null) slotImages[i].color = C_SLOT;
            if (slotIcons[i] != null) { slotIcons[i].color = Color.clear; slotIcons[i].raycastTarget = false; }

            // Stack count label — bottom right corner of slot
            var stackGO = new GameObject("StackCount", typeof(RectTransform));
            stackGO.transform.SetParent(slotGO.transform, false);
            var sr = stackGO.GetComponent<RectTransform>();
            sr.anchorMin = new Vector2(0.5f, 0f);
            sr.anchorMax = new Vector2(1f, 0.4f);
            sr.offsetMin = sr.offsetMax = Vector2.zero;
            var stackTmp = stackGO.AddComponent<TextMeshProUGUI>();
            stackTmp.fontSize = 11;
            stackTmp.color = Color.white;
            stackTmp.alignment = TextAlignmentOptions.BottomRight;
            stackTmp.fontStyle = FontStyles.Bold;
            stackTmp.text = "";
            stackTmp.raycastTarget = false;
            stackLabels[i] = stackTmp;

            int captured = i;
            var trig = slotGO.AddComponent<EventTrigger>();

            var click = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            click.callback.AddListener(e =>
            {
                var pe = (PointerEventData)e;
                if (pe.button == PointerEventData.InputButton.Right)
                    OnSlotRightClicked(captured);
                else
                    OnSlotClicked(captured);
            });
            trig.triggers.Add(click);

            var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener(_ => OnSlotHover(captured));
            trig.triggers.Add(enter);

            var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener(_ => HideTooltip());
            trig.triggers.Add(exit);
        }
    }

    // ── Refresh ───────────────────────────────────────────────────────────────

    public void Refresh()
    {
        var contents = inventory?.GetBagContents(bagSlotIndex);
        if (contents == null || slotIcons == null) return;

        for (int i = 0; i < slotIcons.Length; i++)
        {
            if (i >= contents.Length)
            {
                if (slotIcons[i] != null) slotIcons[i].color = Color.clear;
                if (slotImages[i] != null) slotImages[i].color = C_SLOT;
                if (stackLabels[i] != null) stackLabels[i].text = "";
                continue;
            }

            var item = contents[i];
            bool held = heldFromBag == bagSlotIndex && heldFromSlot == i;

            if (item != null && !item.IsEmpty && !held)
            {
                if (slotIcons[i] != null) { slotIcons[i].sprite = item.Icon; slotIcons[i].color = Color.white; }
                if (slotImages[i] != null) slotImages[i].color = C_SLOT;
                if (stackLabels[i] != null)
                    stackLabels[i].text = item.stackCount > 1 ? item.stackCount.ToString() : "";
            }
            else
            {
                if (slotIcons[i] != null) { slotIcons[i].sprite = null; slotIcons[i].color = Color.clear; }
                if (slotImages[i] != null) slotImages[i].color = held ? C_HELD : C_SLOT;
                if (stackLabels[i] != null) stackLabels[i].text = "";
            }
        }
    }

    // ── Click handlers ────────────────────────────────────────────────────────

    private void OnSlotClicked(int slotIndex)
    {
        var contents = inventory?.GetBagContents(bagSlotIndex);
        if (contents == null || slotIndex >= contents.Length) return;

        if (heldItem == null)
        {
            var item = contents[slotIndex];
            if (item == null || item.IsEmpty) return;
            heldItem = item;
            heldFromBag = bagSlotIndex;
            heldFromSlot = slotIndex;
            ShowCursor(item);
            Refresh();
        }
        else
        {
            bool ok = inventory.MoveItem(heldFromBag, heldFromSlot, bagSlotIndex, slotIndex);
            if (ok)
            {
                DropHeldItem();
                foreach (var bw in FindObjectsByType<BagWindowUI>())
                    bw.Refresh();
            }
            else Debug.Log("Cannot place item here.");
        }
    }

    private void OnSlotRightClicked(int slotIndex)
    {
        var contents = inventory?.GetBagContents(bagSlotIndex);
        if (contents == null || slotIndex >= contents.Length) return;
        var item = contents[slotIndex];
        if (item == null || item.IsEmpty) return;
        ShowContextMenu(slotIndex, item);
    }

    // ── Context Menu ──────────────────────────────────────────────────────────

    private void ShowContextMenu(int slotIndex, InventoryItem item)
    {
        var existingMenu = parentCanvas.transform.Find("ContextMenu");
        if (existingMenu != null) Destroy(existingMenu.gameObject);
        var existingBlocker = parentCanvas.transform.Find("ContextBlocker");
        if (existingBlocker != null) Destroy(existingBlocker.gameObject);

        // Blocker (transparent fullscreen, closes menu on click outside)
        var blockerGO = new GameObject("ContextBlocker", typeof(RectTransform), typeof(Image));
        blockerGO.transform.SetParent(parentCanvas.transform, false);
        blockerGO.GetComponent<Image>().color = Color.clear;
        var blockerR = blockerGO.GetComponent<RectTransform>();
        blockerR.anchorMin = Vector2.zero;
        blockerR.anchorMax = Vector2.one;
        blockerR.offsetMin = blockerR.offsetMax = Vector2.zero;

        // Menu panel
        var menuGO = new GameObject("ContextMenu", typeof(RectTransform), typeof(Image));
        menuGO.transform.SetParent(parentCanvas.transform, false);
        menuGO.GetComponent<Image>().color = new Color(0.07f, 0.07f, 0.10f, 0.97f);
        menuGO.AddComponent<Outline>().effectColor = C_BORDER;

        var menuR = menuGO.GetComponent<RectTransform>();
        menuR.anchorMin = menuR.anchorMax = new Vector2(0.5f, 0.5f);
        menuR.pivot = new Vector2(0f, 1f);

        blockerGO.AddComponent<Button>().onClick.AddListener(() =>
        {
            Destroy(menuGO);
            Destroy(blockerGO);
        });

        var buttons = new System.Collections.Generic.List<(string label, System.Action action)>();

        if (item.type == InventoryItem.ItemType.Weapon || item.type == InventoryItem.ItemType.Armor)
            buttons.Add(("Equip", () =>
            {
                if (equipBlock != null && statBlock != null)
                {
                    bool ok = equipBlock.TryEquipItem(item, statBlock, inventory);
                    if (ok) { inventory.TakeItem(bagSlotIndex, slotIndex); Refresh(); FindAnyObjectByType<StatScreenUI>()?.RefreshEquipSlots(); }
                }
                Destroy(menuGO); Destroy(blockerGO);
            }
            ));

        if (item.type == InventoryItem.ItemType.Consumable)
            buttons.Add(("Use Slot", () =>
            {
                var cm = FindAnyObjectByType<ConsumableManager>();
                if (cm != null)
                    for (int i = 0; i < ConsumableManager.MAX_SLOTS; i++)
                        if (cm.items[i] == null)
                        {
                            if (cm.PlaceConsumable(i, item.consumable, item.stackCount))
                            { inventory.TakeItem(bagSlotIndex, slotIndex); Refresh(); }
                            break;
                        }
                Destroy(menuGO); Destroy(blockerGO);
            }
            ));

        buttons.Add(("Drop", () =>
        {
            var taken = inventory.TakeItem(bagSlotIndex, slotIndex);
            if (taken != null && worldItemPrefab != null)
            {
                var player = FindAnyObjectByType<PlayerEntity>();
                Vector3 dropPos = Vector3.zero;
                if (player != null)
                {
                    Vector3 rayStart = player.transform.position + player.transform.forward * 1.5f + Vector3.up * 2f;
                    if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 10f))
                        dropPos = hit.point + Vector3.up * 0.1f;
                    else
                        dropPos = player.transform.position + player.transform.forward * 1.5f + Vector3.up * 0.1f;
                }
                var go = Instantiate(worldItemPrefab, dropPos, Quaternion.identity);
                var wi = go.GetComponent<WorldItem>();
                if (wi != null) wi.Initialize(taken, null);
            }
            Refresh();
            Destroy(menuGO); Destroy(blockerGO);
        }
        ));

        buttons.Add(("Destroy", () =>
        {
            inventory.TakeItem(bagSlotIndex, slotIndex);
            Refresh();
            Destroy(menuGO); Destroy(blockerGO);
        }
        ));

        float btnH = 24f;
        menuR.sizeDelta = new Vector2(120f, buttons.Count * btnH);

        var canvasRect = parentCanvas.GetComponent<RectTransform>();
        Vector2 scaledMouse = new Vector2(
            (Input.mousePosition.x / Screen.width) * canvasRect.sizeDelta.x,
            (Input.mousePosition.y / Screen.height) * canvasRect.sizeDelta.y);
        scaledMouse -= canvasRect.sizeDelta * 0.5f;
        menuR.anchoredPosition = scaledMouse;

        for (int i = 0; i < buttons.Count; i++)
        {
            var (label, action) = buttons[i];
            var btnGO = new GameObject(label, typeof(RectTransform), typeof(Image));
            btnGO.transform.SetParent(menuGO.transform, false);
            btnGO.GetComponent<Image>().color = new Color(0.10f, 0.10f, 0.14f, 1f);
            var btnR = btnGO.GetComponent<RectTransform>();
            btnR.anchorMin = new Vector2(0, 1);
            btnR.anchorMax = new Vector2(1, 1);
            btnR.offsetMin = new Vector2(1, -(i + 1) * btnH + 1);
            btnR.offsetMax = new Vector2(-1, -i * btnH - 1);
            var btn = btnGO.AddComponent<Button>();
            var captured = action;
            btn.onClick.AddListener(() => captured());
            var txtGO = new GameObject("Txt", typeof(RectTransform));
            txtGO.transform.SetParent(btnGO.transform, false);
            var tr = txtGO.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = tr.offsetMax = Vector2.zero;
            var tmp = txtGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 10;
            tmp.color = new Color(0.95f, 0.95f, 0.95f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;
        }

        blockerGO.transform.SetAsLastSibling();
        menuGO.transform.SetAsLastSibling();
    }

    // ── Tooltip ───────────────────────────────────────────────────────────────

    private void OnSlotHover(int slotIndex)
    {
        var contents = inventory?.GetBagContents(bagSlotIndex);
        if (contents == null || slotIndex >= contents.Length) { HideTooltip(); return; }
        var item = contents[slotIndex];
        if (item == null || item.IsEmpty) { HideTooltip(); return; }
        if (tooltipText != null) tooltipText.text = item.GetTooltip();
        if (tooltipPanel != null) tooltipPanel.SetActive(true);
    }

    private void HideTooltip()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    // ── Held item cursor ──────────────────────────────────────────────────────

    private void ShowCursor(InventoryItem item)
    {
        if (heldCursor != null) Destroy(heldCursor.gameObject);
        var go = new GameObject("HeldCursor", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parentCanvas.transform, false);
        go.transform.SetAsLastSibling();
        var img = go.GetComponent<Image>();
        img.sprite = item.Icon;
        img.color = item.Icon != null ? Color.white : new Color(1, 1, 1, 0.5f);
        img.preserveAspect = true;
        img.raycastTarget = false;
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(SLOT_SIZE * 0.85f, SLOT_SIZE * 0.85f);
        heldCursor = img;
    }

    private void DropHeldItem()
    {
        heldItem = null; heldFromBag = -1; heldFromSlot = -1;
        if (heldCursor != null) { Destroy(heldCursor.gameObject); heldCursor = null; }
    }

    // ── Update ────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (heldCursor != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.GetComponent<RectTransform>(), Input.mousePosition,
                parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera,
                out Vector2 lp);
            heldCursor.rectTransform.anchoredPosition = lp;
        }
        if (heldItem != null && Input.GetMouseButtonDown(1))
            DropHeldItem();
    }

    // ── Drag window ───────────────────────────────────────────────────────────

    public void OnBeginDrag(PointerEventData e)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, e.position, e.pressEventCamera, out dragOffset);
    }

    public void OnDrag(PointerEventData e)
    {
        if (parentCanvas == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.GetComponent<RectTransform>(), e.position,
            e.pressEventCamera, out Vector2 cp);
        rectTransform.anchoredPosition = cp - dragOffset;
    }

    // ── Cleanup ───────────────────────────────────────────────────────────────

    private void OnDestroy()
    {
        if (heldCursor != null) { Destroy(heldCursor.gameObject); heldCursor = null; }
        heldItem = null; heldFromBag = -1; heldFromSlot = -1;
    }
}