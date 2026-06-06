using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class StatScreenUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas parentCanvas;
    [Header("Font Scale")]
    [SerializeField] private float fontScale = 1f;
    [Header("Prefabs")]
    [SerializeField] private GameObject bagWindowPrefab;

    private static readonly Color C_BG = new Color(0.06f, 0.06f, 0.08f, 0.96f);
    private static readonly Color C_CELL = new Color(0.09f, 0.09f, 0.12f, 0.98f);
    private static readonly Color C_SLOT = new Color(0.22f, 0.22f, 0.28f, 1.00f);
    private static readonly Color C_TIP = new Color(0.04f, 0.04f, 0.06f, 1.00f);
    private static readonly Color C_GOLD = new Color(0.80f, 0.68f, 0.30f, 1.00f);
    private static readonly Color C_LABEL = new Color(0.65f, 0.65f, 0.65f, 1.00f);
    private static readonly Color C_VALUE = new Color(0.95f, 0.95f, 0.95f, 1.00f);
    private static readonly Color C_BONUS = new Color(0.35f, 0.85f, 0.35f, 1.00f);
    private static readonly Color C_PENALTY = new Color(0.90f, 0.32f, 0.32f, 1.00f);
    private static readonly Color C_BORDER = new Color(0.38f, 0.32f, 0.18f, 1.00f);
    private static readonly Color C_DISABLED = new Color(0.18f, 0.18f, 0.18f, 0.55f);

    const float W = 920f;
    const float H = 580f;
    const float TITLE_H = 24f;
    const float PAD = 4f;
    const float LEFT_W = 195f;
    const float RIGHT_W = 195f;
    const float COL_H = (H - TITLE_H - PAD * 2f) / 3f;

    float MID_W => W - LEFT_W - RIGHT_W - PAD * 4f;
    float LEFT_X0 => PAD;
    float LEFT_X1 => PAD + LEFT_W;
    float MID_X0 => LEFT_X1 + PAD;
    float MID_X1 => MID_X0 + MID_W;
    float RIGHT_X0 => MID_X1 + PAD;
    float RIGHT_X1 => W - PAD;

    private GameObject screenRoot;
    private bool isOpen;
    private StatBlock statBlock;
    private EquipmentBlock equipBlock;
    private PlayerInventory playerInventory;

    // Text refs
    private TextMeshProUGUI tName, tClass, tLevel, tHP, tSP, tAP, tMAP, tEAF, tExp, tWeight;
    private TextMeshProUGUI tSTR, tSTA, tAGI, tINT, tEMP, tCHA;
    private TextMeshProUGUI tPhys, tHeat, tCold, tEnergy, tMatter, tMind, tBody, tSpirit;
    private TextMeshProUGUI tDHP, tDSP, tMAP2, tRAP, tMP, tCrit, tDodge, tCarry, tHeal, tSkill;
    private TextMeshProUGUI tooltipTxt;

    // Equip slot icons
    private Image slotMainHand, slotOffHand;
    private Image[] slotArmor = new Image[6];
    private Image[] bagIcons = new Image[6];
    private TextMeshProUGUI[] bagLabels = new TextMeshProUGUI[6];

    // Bag windows
    private BagWindowUI[] openBagWindows = new BagWindowUI[6];

    void Start()
    {
        if (parentCanvas == null) parentCanvas = GetComponentInParent<Canvas>();
        Build();
        screenRoot.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)) Toggle();
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        screenRoot.SetActive(isOpen);
        if (isOpen) Refresh();
    }

    void FindPlayer()
    {
        var p = FindAnyObjectByType<PlayerEntity>();
        if (p == null) return;
        statBlock = p.GetComponent<StatBlock>();
        equipBlock = p.GetComponent<EquipmentBlock>();
        playerInventory = p.GetComponent<PlayerInventory>();
    }

    void Refresh()
    {
        if (statBlock == null || playerInventory == null) FindPlayer();
        if (statBlock == null) return;

        var charData = GameManager.Instance?.SelectedCharacter;
        tName.text = charData != null ? charData.characterName : "Player";
        tClass.text = statBlock.Class != null ? statBlock.Class.className.ToString() : "—";
        tLevel.text = $"Level {statBlock.ClassLevel}";
        tHP.text = $"HP   {statBlock.MaxHP} / {statBlock.MaxHP}";
        tSP.text = statBlock.MaxSP > 0 ? $"SP   {statBlock.MaxSP} / {statBlock.MaxSP}" : "SP   —";

        float eaf = equipBlock != null ? equipBlock.EAF : 0f;
        tAP.text = $"AP     {statBlock.MeleeAP}";
        tMAP.text = $"MAP   {statBlock.MagicPower}";
        tEAF.text = $"EAF    {eaf:F1}";

        if (tExp != null && charData != null)
        {
            int nextXP = ExperienceSystem.GetXPForNextLevel(statBlock.ClassLevel);
            tExp.text = nextXP > 0
                ? $"EXP  {charData.experience} / {nextXP}"
                : $"EXP  {charData.experience} / MAX";
        }

        SetStat(tSTR, "STR", statBlock.Strength, statBlock.StrBonus);
        SetStat(tSTA, "STA", statBlock.Stamina, statBlock.StaBonus);
        SetStat(tAGI, "AGI", statBlock.Agility, statBlock.AgiBonus);
        SetStat(tINT, "INT", statBlock.Intellect, statBlock.IntBonus);
        SetStat(tEMP, "EMP", statBlock.Empathy, statBlock.EmpBonus);
        SetStat(tCHA, "CHA", statBlock.Charisma, statBlock.ChaBonus);

        float pm = equipBlock != null ? equipBlock.PhysMit : 0f;
        tPhys.text = $"Physical   {pm * 100f:F0}%";
        tHeat.text = $"Heat       {R(DamageType.Heat)}%";
        tCold.text = $"Cold       {R(DamageType.Cold)}%";
        tEnergy.text = $"Energy     {R(DamageType.Energy)}%";
        tMatter.text = $"Matter     {R(DamageType.Matter)}%";
        tMind.text = $"Mind       {R(DamageType.Mind)}%";
        tBody.text = $"Body       {R(DamageType.Body)}%";
        tSpirit.text = $"Spirit     {R(DamageType.Spirit)}%";

        tDHP.text = $"Max HP       {statBlock.MaxHP}";
        tDSP.text = statBlock.MaxSP > 0 ? $"Max SP       {statBlock.MaxSP}" : "Max SP       —";
        tMAP2.text = $"Melee AP     {statBlock.MeleeAP}";
        tRAP.text = $"Ranged AP    {statBlock.RangedAP}";
        tMP.text = $"Magic Pwr    {statBlock.MagicPower}";
        tCrit.text = $"Crit         {statBlock.ClassLevel}%";
        tDodge.text = $"Dodge        0%";
        tCarry.text = $"Carry        {statBlock.CarryWeight}";
        tHeal.text = $"Heal Bonus   {(statBlock.HealingMultiplier - 1f) * 100f:F1}%";
        tSkill.text = $"Skill Gain   x{statBlock.SkillGainMultiplier:F3}";
        tWeight.text = $"Weight   0 / {statBlock.CarryWeight}";

        // Equip slot icons
        if (slotMainHand != null)
        {
            var wp = equipBlock?.MainHand;
            slotMainHand.sprite = wp?.icon;
            slotMainHand.color = wp?.icon != null ? Color.white : Color.clear;
        }
        if (slotOffHand != null)
        {
            var wp = equipBlock?.OffHand;
            slotOffHand.sprite = wp?.icon;
            slotOffHand.color = wp?.icon != null ? Color.white : Color.clear;
        }
        for (int i = 0; i < slotArmor.Length; i++)
        {
            if (slotArmor[i] == null) continue;
            var armor = equipBlock?.GetArmorInSlot((ArmorSlot)i);
            slotArmor[i].sprite = armor?.icon;
            slotArmor[i].color = armor?.icon != null ? Color.white : Color.clear;
        }
        // Bag slot icons
        if (playerInventory != null)
        {
            for (int i = 0; i < 6; i++)
            {
                if (bagIcons[i] == null) continue;
                var bag = playerInventory.GetEquippedBag(i);
                bagIcons[i].sprite = bag?.icon;
                bagIcons[i].color = (bag?.icon != null) ? Color.white : Color.clear;
                if (bagLabels[i] != null)
                    bagLabels[i].text = bag != null ? "" : $"Bag {i + 1}";
            }
        }
    }

    void SetStat(TextMeshProUGUI t, string lbl, int raw, int bonus)
    {
        string b = bonus > 0
            ? $" <color=#{ColorUtility.ToHtmlStringRGB(C_BONUS)}>(+{bonus})</color>"
            : bonus < 0
            ? $" <color=#{ColorUtility.ToHtmlStringRGB(C_PENALTY)}>({bonus})</color>"
            : "";
        t.text = $"{lbl}   {raw}{b}";
    }

    float R(DamageType dt) => equipBlock != null ? equipBlock.GetResistance(dt) * 100f : 0f;

    void Build()
    {
        screenRoot = MakeRect("StatScreen", parentCanvas.transform, C_BG,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-W / 2f, -H / 2f), new Vector2(W / 2f, H / 2f));
        AddBorder(screenRoot);

        var titleBar = MakeRect("TitleBar", screenRoot.transform, new Color(0.04f, 0.04f, 0.07f, 1f),
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -TITLE_H), new Vector2(0, 0));
        MakeTxt("Title", titleBar.transform, "CHARACTER", 12, C_GOLD,
            TextAlignmentOptions.Center, true,
            new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);

        var closeBtn = MakeRect("CloseBtn", screenRoot.transform, new Color(0.4f, 0.08f, 0.08f, 1f),
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-22, -22), new Vector2(0, 0));
        closeBtn.AddComponent<Button>().onClick.AddListener(Toggle);
        MakeTxt("X", closeBtn.transform, "X", 11, C_VALUE,
            TextAlignmentOptions.Center, false,
            new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);

        float accRowH = 70f;
        float equipH = H - TITLE_H - PAD * 2f - accRowH - PAD;

        var midEquip = MakeCell("MidEquip", screenRoot.transform,
            MID_X0, -(TITLE_H + PAD), MID_X1, -(TITLE_H + PAD + equipH));
        midEquip.GetComponent<Image>().color = new Color(0.06f, 0.06f, 0.09f, 0.98f);
        BuildEquipArea(midEquip.transform, MID_W, equipH);

        var accCell = MakeCell("AccRow", screenRoot.transform,
            MID_X0, -(TITLE_H + PAD + equipH + PAD), MID_X1, -(H - PAD));
        accCell.GetComponent<Image>().color = new Color(0.07f, 0.07f, 0.10f, 0.98f);
        BuildAccessoryRow(accCell.transform, MID_W, accRowH);

        float y = 8f;

        // LEFT COLUMN
        var lt = MakeCell("LeftTop", screenRoot.transform,
            LEFT_X0, -(TITLE_H + PAD), LEFT_X1, -(TITLE_H + PAD + COL_H - PAD));
        y = 8f;
        tName = TL(lt, "Name", "Player", 13, C_GOLD, true, ref y, 16f);
        tClass = TL(lt, "Class", "Class", 11, C_LABEL, false, ref y, 14f);
        tLevel = TL(lt, "Level", "Level 1", 11, C_LABEL, false, ref y, 14f);
        SepL(lt, ref y);
        tHP = TL(lt, "HP", "HP  — / —", 11, C_VALUE, false, ref y, 14f);
        tSP = TL(lt, "SP", "SP  — / —", 11, C_VALUE, false, ref y, 14f);
        SepL(lt, ref y);
        tAP = TL(lt, "AP", "AP   —", 11, C_VALUE, false, ref y, 14f);
        tMAP = TL(lt, "MAP", "MAP —", 11, C_VALUE, false, ref y, 14f);
        tEAF = TL(lt, "EAF", "EAF  —", 11, C_VALUE, false, ref y, 14f);
        tExp = TL(lt, "EXP", "EXP  0 / 400", 11, C_LABEL, false, ref y, 14f);

        var lm = MakeCell("LeftMid", screenRoot.transform,
            LEFT_X0, -(TITLE_H + PAD + COL_H), LEFT_X1, -(TITLE_H + PAD + COL_H * 2f - PAD));
        y = 8f;
        HeaderL(lm, "ATTRIBUTES", ref y);
        tSTR = TL(lm, "STR", "STR  50", 11, C_VALUE, false, ref y, 14f);
        tSTA = TL(lm, "STA", "STA  50", 11, C_VALUE, false, ref y, 14f);
        tAGI = TL(lm, "AGI", "AGI  50", 11, C_VALUE, false, ref y, 14f);
        tINT = TL(lm, "INT", "INT  50", 11, C_VALUE, false, ref y, 14f);
        tEMP = TL(lm, "EMP", "EMP  50", 11, C_VALUE, false, ref y, 14f);
        tCHA = TL(lm, "CHA", "CHA  50", 11, C_VALUE, false, ref y, 14f);

        var lb = MakeCell("LeftBot", screenRoot.transform,
            LEFT_X0, -(TITLE_H + PAD + COL_H * 2f), LEFT_X1, -(H - PAD));
        y = 8f;
        HeaderL(lb, "RESISTANCES", ref y);
        tPhys = TL(lb, "Phys", "Physical  0%", 10, C_VALUE, false, ref y, 13f);
        tHeat = TL(lb, "Heat", "Heat      0%", 10, C_VALUE, false, ref y, 13f);
        tCold = TL(lb, "Cold", "Cold      0%", 10, C_VALUE, false, ref y, 13f);
        tEnergy = TL(lb, "Energy", "Energy    0%", 10, C_VALUE, false, ref y, 13f);
        tMatter = TL(lb, "Matter", "Matter    0%", 10, C_VALUE, false, ref y, 13f);
        tMind = TL(lb, "Mind", "Mind      0%", 10, C_VALUE, false, ref y, 13f);
        tBody = TL(lb, "Body", "Body      0%", 10, C_VALUE, false, ref y, 13f);
        tSpirit = TL(lb, "Spirit", "Spirit    0%", 10, C_VALUE, false, ref y, 13f);

        // RIGHT COLUMN
        var rt = MakeCell("RightTop", screenRoot.transform,
            RIGHT_X0, -(TITLE_H + PAD), RIGHT_X1, -(TITLE_H + PAD + COL_H - PAD));
        y = 8f;
        HeaderL(rt, "DERIVED STATS", ref y);
        tDHP = TL(rt, "dHP", "Max HP", 10, C_VALUE, false, ref y, 12f);
        tDSP = TL(rt, "dSP", "Max SP", 10, C_VALUE, false, ref y, 12f);
        tMAP2 = TL(rt, "mAP", "Melee AP", 10, C_VALUE, false, ref y, 12f);
        tRAP = TL(rt, "rAP", "Ranged AP", 10, C_VALUE, false, ref y, 12f);
        tMP = TL(rt, "MP", "Magic Pwr", 10, C_VALUE, false, ref y, 12f);
        y += 3f;
        tCrit = TL(rt, "Crit", "Crit", 10, C_VALUE, false, ref y, 12f);
        tDodge = TL(rt, "Dodge", "Dodge", 10, C_VALUE, false, ref y, 12f);
        tCarry = TL(rt, "Carry", "Carry", 10, C_VALUE, false, ref y, 12f);
        tHeal = TL(rt, "Heal", "Heal Bonus", 10, C_VALUE, false, ref y, 12f);
        tSkill = TL(rt, "Skill", "Skill Gain", 10, C_VALUE, false, ref y, 12f);

        var rm = MakeCell("RightMid", screenRoot.transform,
            RIGHT_X0, -(TITLE_H + PAD + COL_H), RIGHT_X1, -(TITLE_H + PAD + COL_H * 2f - PAD));
        y = 8f;
        HeaderL(rm, "CLASS TRAITS", ref y);
        string[] traits = { "Fortitude I", "Combat Sense", "Resilience I", "Iron Will" };
        foreach (var trait in traits)
        {
            var tt = TL(rm, trait, $"• {trait}", 10, C_VALUE, false, ref y, 13f);
            string cap = trait;
            var trig = tt.gameObject.AddComponent<EventTrigger>();
            var en = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            en.callback.AddListener(_ => {
                if (tooltipTxt != null)
                    tooltipTxt.text = $"<b>{cap}</b>\n\nTrait description coming once class trait system is implemented.";
            });
            trig.triggers.Add(en);
            var ex = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            ex.callback.AddListener(_ => {
                if (tooltipTxt != null)
                    tooltipTxt.text = "Hover an item\nor trait for details.";
            });
            trig.triggers.Add(ex);
        }

        var rb = MakeCell("RightBot", screenRoot.transform,
            RIGHT_X0, -(TITLE_H + PAD + COL_H * 2f), RIGHT_X1, -(H - PAD));
        y = 8f;
        HeaderL(rb, "BAGS", ref y);
        BuildBagGrid(rb.transform, y);
    }

    // ── Equip Area ────────────────────────────────────────────────────────────

    void BuildEquipArea(Transform p, float w, float h)
    {
        float slotW = 58f, slotH = 58f, gap = 6f;

        string[] leftNames = { "Head", "Main Hand", "Body", "Legs" };
        string[] rightNames = { "Shoulders", "Off Hand", "Gloves", "Feet" };
        ArmorSlot?[] leftSlots = { ArmorSlot.Head, null, ArmorSlot.Body, ArmorSlot.Legs };
        ArmorSlot?[] rightSlots = { ArmorSlot.Shoulders, null, ArmorSlot.Gloves, ArmorSlot.Feet };

        for (int i = 0; i < 4; i++)
            EquipSlot(p, leftNames[i], gap, -(gap + i * (slotH + gap)), slotW, slotH, leftSlots[i], i == 1, false);
        for (int i = 0; i < 4; i++)
            EquipSlot(p, rightNames[i], w - slotW - gap, -(gap + i * (slotH + gap)), slotW, slotH, rightSlots[i], false, i == 1);

        float tipX0 = gap + slotW + gap;
        float tipX1 = w - slotW - gap * 2f;
        var tip = MakeRect("Tooltip", p, C_TIP,
            new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(tipX0, -(h - gap)), new Vector2(tipX1, -gap));
        AddBorder(tip);

        tooltipTxt = MakeTxt("TT", tip.transform,
            "Hover an item\nor trait for details.",
            10, C_LABEL, TextAlignmentOptions.TopLeft, false,
            new Vector2(0, 0), new Vector2(1, 1),
            new Vector2(6, 6), new Vector2(-6, -6));
        tooltipTxt.textWrappingMode = TextWrappingModes.Normal;
    }

    void EquipSlot(Transform parent, string label, float x, float y, float w, float h,
        ArmorSlot? armorSlot, bool isMainHand, bool isOffHand)
    {
        var slot = MakeRect($"Slot_{label}", parent, C_SLOT,
            new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(x, y - h), new Vector2(x + w, y));
        AddBorder(slot);

        var lbl = MakeTxt("Lbl", slot.transform, label, 8, C_LABEL,
            TextAlignmentOptions.Center, false,
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(2, 2), new Vector2(-2, -2));
        lbl.textWrappingMode = TextWrappingModes.Normal;

        // Icon child
        var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGO.transform.SetParent(slot.transform, false);
        var iconRect = iconGO.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.1f, 0.2f);
        iconRect.anchorMax = new Vector2(0.9f, 0.9f);
        iconRect.offsetMin = iconRect.offsetMax = Vector2.zero;
        var iconImage = iconGO.GetComponent<Image>();
        iconImage.preserveAspect = true;
        iconImage.color = Color.clear;

        if (isMainHand) slotMainHand = iconImage;
        else if (isOffHand) slotOffHand = iconImage;
        else if (armorSlot.HasValue)
        {
            int idx = (int)armorSlot.Value;
            if (idx < slotArmor.Length) slotArmor[idx] = iconImage;
        }

        var trig = slot.AddComponent<EventTrigger>();
        var en = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        en.callback.AddListener(_ =>
        {
            if (tooltipTxt == null) return;

            if (isMainHand && equipBlock != null && equipBlock.MainHand != null)
            {
                var wp = equipBlock.MainHand;
                string classList = (wp.allowedClasses != null && wp.allowedClasses.Length > 0)
                     ? string.Join(", ", System.Array.ConvertAll(wp.allowedClasses, c => ClassShortName(c)))
                      : "All";
                tooltipTxt.text =
                    $"<b>{wp.itemName}</b>  <size=80%>{wp.weaponClass}</size>\n" +
                    $"Item Level {wp.itemLevel}   Classes: {classList}\n\n" +
                    $"Damage     {wp.baseDamage:F0}–{wp.MaxDamage:F0}  (Glance: {wp.GlanceDamage:F0})\n" +
                    $"Speed      {wp.attackSpeed}s\n" +
                    $"Range      {wp.attackRange}m\n" +
                    (wp.hasElementalDamage ? $"Elemental  +{wp.elementalDamage:F0} {wp.elementalDamageType}\n" : "") +
                    (wp.bonusStrength != 0 ? $"STR +{wp.bonusStrength}\n" : "") +
                    (wp.bonusAgility != 0 ? $"AGI +{wp.bonusAgility}\n" : "") +
                    (wp.bonusIntellect != 0 ? $"INT +{wp.bonusIntellect}\n" : "") +
                    (string.IsNullOrEmpty(wp.description) ? "" : $"\n<i>{wp.description}</i>");
                return;
            }

            if (isOffHand && equipBlock != null && equipBlock.OffHand != null)
            {
                var wp = equipBlock.OffHand;
                tooltipTxt.text = $"<b>{wp.itemName}</b>\n\nOff hand weapon.";
                return;
            }

            if (armorSlot.HasValue && equipBlock != null)
            {
                var armor = equipBlock.GetArmorInSlot(armorSlot.Value);
                if (armor != null)
                {
                    tooltipTxt.text =
                        $"<b>{armor.itemName}</b>  <size=80%>{armor.armorType}</size>\n" +
                        $"Item Level {armor.itemLevel}\n\n" +
                        $"EAF        {armor.GetEAF():F1}\n" +
                        $"Phys Mit   {armor.physMit * 100f:F0}%\n" +
                        $"MDEF       {armor.GetBaseMDEF() * 100f:F0}\n" +
                        (armor.bonusStamina != 0 ? $"STA +{armor.bonusStamina}\n" : "") +
                        (armor.bonusStrength != 0 ? $"STR +{armor.bonusStrength}\n" : "") +
                        (armor.bonusAgility != 0 ? $"AGI +{armor.bonusAgility}\n" : "") +
                        (armor.bonusIntellect != 0 ? $"INT +{armor.bonusIntellect}\n" : "") +
                        (armor.bonusEmpathy != 0 ? $"EMP +{armor.bonusEmpathy}\n" : "") +
                        (armor.bonusCharisma != 0 ? $"CHA +{armor.bonusCharisma}\n" : "") +
                        (armor.bonusHP != 0 ? $"HP  +{armor.bonusHP}\n" : "") +
                        (armor.bonusSP != 0 ? $"HP  +{armor.bonusSP}\n" : "") +
                        (string.IsNullOrEmpty(armor.description) ? "" : $"\n<i>{armor.description}</i>");
                    return;
                }
            }

            tooltipTxt.text = $"<b>{label}</b>\n\nNo item equipped.";
        });
        trig.triggers.Add(en);

        var ex = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        ex.callback.AddListener(_ => {
            if (tooltipTxt != null)
                tooltipTxt.text = "Hover an item\nor trait for details.";
        });
        trig.triggers.Add(ex);
        var rc = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        rc.callback.AddListener(e => {
            var pe = (PointerEventData)e;
            if (pe.button != PointerEventData.InputButton.Right) return;

            if (playerInventory == null) FindPlayer();
            if (equipBlock == null || playerInventory == null) return;

            if (isMainHand)
                equipBlock.UnequipMainHand(playerInventory);
            else if (isOffHand)
                equipBlock.UnequipOffHand(playerInventory);
            else if (armorSlot.HasValue)
                equipBlock.UnequipArmorSlot(armorSlot.Value, playerInventory);

            FindAnyObjectByType<CharacterVisuals>()?.RefreshAll();
            Refresh();
            foreach (var bw in FindObjectsByType<BagWindowUI>())
                bw.Refresh();
        });
        trig.triggers.Add(rc);
    }
    private string ClassShortName(ClassName c)
    {
        switch (c)
        {
            case ClassName.Paladin: return "PAL";
            case ClassName.Mercenary: return "MER";
            case ClassName.Apothecary: return "APO";
            case ClassName.Pathfinder: return "PTH";
            case ClassName.Druid: return "DRU";
            case ClassName.Beastmaster: return "BST";
            case ClassName.Warlord: return "WAR";
            case ClassName.Oracle: return "ORA";
            case ClassName.Merchant: return "MCH";
            default: return c.ToString();
        }
    }

    // ── Accessory Row ─────────────────────────────────────────────────────────

    void BuildAccessoryRow(Transform p, float w, float h)
    {
        MakeTxt("AccLbl", p, "ACCESSORIES", 9, C_GOLD, TextAlignmentOptions.Left, true,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(6, -2), new Vector2(-6, -16));

        var scrollGO = new GameObject("AccScroll", typeof(RectTransform));
        scrollGO.transform.SetParent(p, false);
        var sr = scrollGO.AddComponent<ScrollRect>();
        sr.horizontal = true; sr.vertical = false;
        var srRect = scrollGO.GetComponent<RectTransform>();
        srRect.anchorMin = new Vector2(0, 0); srRect.anchorMax = new Vector2(1, 1);
        srRect.offsetMin = new Vector2(4, 14); srRect.offsetMax = new Vector2(-4, -18);

        var vpGO = new GameObject("Viewport", typeof(RectTransform), typeof(Image));
        vpGO.transform.SetParent(scrollGO.transform, false);
        vpGO.GetComponent<Image>().color = Color.clear;
        vpGO.AddComponent<Mask>().showMaskGraphic = false;
        var vpRect = vpGO.GetComponent<RectTransform>();
        vpRect.anchorMin = Vector2.zero; vpRect.anchorMax = Vector2.one;
        vpRect.offsetMin = Vector2.zero; vpRect.offsetMax = Vector2.zero;
        sr.viewport = vpRect;

        var contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(vpGO.transform, false);
        var contentRect = contentGO.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 0); contentRect.anchorMax = new Vector2(0, 1);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = new Vector2(16 * 52f, 0);
        sr.content = contentRect;

        var hlg = contentGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 4; hlg.padding = new RectOffset(2, 2, 2, 2);
        hlg.childForceExpandHeight = true; hlg.childForceExpandWidth = false;

        for (int i = 0; i < 16; i++)
        {
            bool on = i < 6;
            var slot = new GameObject($"Acc{i}", typeof(RectTransform), typeof(Image));
            slot.transform.SetParent(contentGO.transform, false);
            slot.GetComponent<Image>().color = on ? C_SLOT : C_DISABLED;
            var le = slot.AddComponent<LayoutElement>();
            le.preferredWidth = 46; le.minWidth = 46;
            var ol = slot.AddComponent<Outline>();
            ol.effectColor = on ? C_BORDER : new Color(0.2f, 0.2f, 0.2f, 0.4f);
            ol.effectDistance = new Vector2(1, 1);

            if (!on)
            {
                var dtGO = new GameObject("Dis", typeof(RectTransform));
                dtGO.transform.SetParent(slot.transform, false);
                var dtRect = dtGO.GetComponent<RectTransform>();
                dtRect.anchorMin = Vector2.zero; dtRect.anchorMax = Vector2.one;
                dtRect.offsetMin = Vector2.zero; dtRect.offsetMax = Vector2.zero;
                var dt = dtGO.AddComponent<TextMeshProUGUI>();
                dt.text = "—";
                dt.fontSize = 9 * fontScale;
                dt.color = new Color(0.3f, 0.3f, 0.3f, 0.6f);
                dt.alignment = TextAlignmentOptions.Center;
            }
        }

        var sbGO = new GameObject("SB", typeof(RectTransform), typeof(Image));
        sbGO.transform.SetParent(p, false);
        sbGO.GetComponent<Image>().color = new Color(0.12f, 0.12f, 0.12f, 1f);
        var sbRect = sbGO.GetComponent<RectTransform>();
        sbRect.anchorMin = new Vector2(0, 0); sbRect.anchorMax = new Vector2(1, 0);
        sbRect.offsetMin = new Vector2(4, 2); sbRect.offsetMax = new Vector2(-4, 14);

        var sb = sbGO.AddComponent<Scrollbar>();
        sb.direction = Scrollbar.Direction.LeftToRight;

        var handleGO = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleGO.transform.SetParent(sbGO.transform, false);
        handleGO.GetComponent<Image>().color = new Color(0.45f, 0.38f, 0.18f, 1f);
        var handleRect = handleGO.GetComponent<RectTransform>();
        handleRect.anchorMin = Vector2.zero;
        handleRect.anchorMax = Vector2.one;
        handleRect.offsetMin = Vector2.zero;
        handleRect.offsetMax = Vector2.zero;
        sb.handleRect = handleRect;

        sr.horizontalScrollbar = sb;
        sr.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        sr.scrollSensitivity = 30f;
    }

    // ── Bag Grid ──────────────────────────────────────────────────────────────

    void BuildBagGrid(Transform p, float headerY)
    {
        var grid = new GameObject("BagGrid", typeof(RectTransform));
        grid.transform.SetParent(p, false);
        var gr = grid.GetComponent<RectTransform>();
        gr.anchorMin = new Vector2(0, 0); gr.anchorMax = new Vector2(1, 1);
        gr.offsetMin = new Vector2(6, 6);
        gr.offsetMax = new Vector2(-6, -(headerY + 2f));

        var glg = grid.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(52, 52);
        glg.spacing = new Vector2(4, 4);
        glg.padding = new RectOffset(4, 4, 4, 4);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 3;
        glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
        glg.startAxis = GridLayoutGroup.Axis.Horizontal;
        glg.childAlignment = TextAnchor.UpperLeft;

        for (int i = 0; i < 6; i++)
        {
            var bagSlot = new GameObject($"Bag{i}", typeof(RectTransform), typeof(Image));
            bagSlot.transform.SetParent(grid.transform, false);
            bagSlot.GetComponent<Image>().color = C_SLOT;
            var ol = bagSlot.AddComponent<Outline>();
            ol.effectColor = C_BORDER; ol.effectDistance = new Vector2(1, 1);

            // Icon child
            var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(bagSlot.transform, false);
            var ir = iconGO.GetComponent<RectTransform>();
            ir.anchorMin = new Vector2(0.1f, 0.1f); ir.anchorMax = new Vector2(0.9f, 0.9f);
            ir.offsetMin = ir.offsetMax = Vector2.zero;
            var iconImg = iconGO.GetComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.color = Color.clear;

            // Label
            var lblGO = new GameObject("Lbl", typeof(RectTransform));
            lblGO.transform.SetParent(bagSlot.transform, false);
            var lr = lblGO.GetComponent<RectTransform>();
            lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
            lr.offsetMin = new Vector2(2, 2); lr.offsetMax = new Vector2(-2, -2);
            var lt2 = lblGO.AddComponent<TextMeshProUGUI>();
            lt2.text = $"Bag {i + 1}";
            lt2.fontSize = 9 * fontScale;
            lt2.color = C_LABEL;
            lt2.alignment = TextAlignmentOptions.Center;
            bagIcons[i] = iconImg;
            bagLabels[i] = lt2;

            var btn = bagSlot.AddComponent<Button>();
            int idx = i;
            Image capturedIcon = iconImg;
            TextMeshProUGUI capturedLbl = lt2;
            btn.onClick.AddListener(() => OnBagSlotClicked(idx, capturedIcon, capturedLbl));
        }

        // Weight display
        var weightGO = new GameObject("Weight", typeof(RectTransform));
        weightGO.transform.SetParent(p, false);
        var wr = weightGO.GetComponent<RectTransform>();
        wr.anchorMin = new Vector2(0, 0); wr.anchorMax = new Vector2(1, 0);
        wr.offsetMin = new Vector2(6, 6); wr.offsetMax = new Vector2(-6, 20);
        var wt = weightGO.AddComponent<TextMeshProUGUI>();
        wt.text = "Weight  0 / 50";
        wt.fontSize = 10 * fontScale;
        wt.color = C_LABEL;
        wt.alignment = TextAlignmentOptions.Center;
        tWeight = wt;
    }

    void OnBagSlotClicked(int slotIndex, Image iconImg, TextMeshProUGUI lbl)
    {
        if (playerInventory == null) FindPlayer();
        if (playerInventory == null) return;

        var equippedBag = playerInventory.GetEquippedBag(slotIndex);

        if (equippedBag != null)
        {
            iconImg.sprite = equippedBag.icon;
            iconImg.color = equippedBag.icon != null ? Color.white : Color.clear;
            lbl.text = "";
        }
        else
        {
            iconImg.color = Color.clear;
            lbl.text = $"Bag {slotIndex + 1}";
            return;
        }

        // Toggle bag window open/close
        if (openBagWindows[slotIndex] != null)
        {
            Destroy(openBagWindows[slotIndex].gameObject);
            openBagWindows[slotIndex] = null;
            return;
        }

        if (bagWindowPrefab == null)
        {
            Debug.LogError("BagWindow prefab not assigned on StatScreenUI!");
            return;
        }

        var windowGO = Instantiate(bagWindowPrefab, parentCanvas.transform, false);
        windowGO.name = $"BagWindow_{slotIndex}";
        var rt = windowGO.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(-300f + slotIndex * 60f, -150f + slotIndex * 30f);

        var bw = windowGO.GetComponent<BagWindowUI>();
        bw.Initialize(playerInventory, slotIndex, equippedBag, parentCanvas);
        openBagWindows[slotIndex] = bw;
    }
    // ── Primitives ────────────────────────────────────────────────────────────

    GameObject MakeRect(string name, Transform parent, Color col,
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

    GameObject MakeCell(string name, Transform parent, float x0, float y0, float x1, float y1)
    {
        var go = MakeRect(name, parent, C_CELL,
            new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(x0, y1), new Vector2(x1, y0));
        AddBorder(go);
        return go;
    }

    TextMeshProUGUI MakeTxt(string name, Transform parent, string text, int size, Color col,
        TextAlignmentOptions align, bool bold,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = anchorMin; r.anchorMax = anchorMax;
        r.offsetMin = offsetMin; r.offsetMax = offsetMax;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size * fontScale;
        t.color = col;
        t.alignment = align;
        t.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        t.textWrappingMode = TextWrappingModes.Normal;
        return t;
    }

    TextMeshProUGUI TL(GameObject cell, string name, string text, int size, Color col,
        bool bold, ref float y, float lineH)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(cell.transform, false);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0, 1); r.anchorMax = new Vector2(1, 1);
        r.offsetMin = new Vector2(8, -y - lineH);
        r.offsetMax = new Vector2(-8, -y);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size * fontScale;
        tmp.color = col;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        y += lineH + 2f;
        return tmp;
    }

    void HeaderL(GameObject cell, string text, ref float y)
    {
        TL(cell, "Hdr", text, 10, C_GOLD, true, ref y, 14f);
        var sep = new GameObject("Sep", typeof(RectTransform));
        sep.transform.SetParent(cell.transform, false);
        var r = sep.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0, 1); r.anchorMax = new Vector2(1, 1);
        r.offsetMin = new Vector2(8, -y - 4f);
        r.offsetMax = new Vector2(-8, -y);
        sep.AddComponent<Image>().color = C_BORDER;
        y += 6f;
    }

    void SepL(GameObject cell, ref float y)
    {
        var sep = new GameObject("Sep", typeof(RectTransform));
        sep.transform.SetParent(cell.transform, false);
        var r = sep.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0, 1); r.anchorMax = new Vector2(1, 1);
        r.offsetMin = new Vector2(8, -y - 3f);
        r.offsetMax = new Vector2(-8, -y);
        sep.AddComponent<Image>().color = C_BORDER;
        y += 5f;
    }

    void AddBorder(GameObject go)
    {
        var ol = go.AddComponent<Outline>();
        ol.effectColor = C_BORDER;
        ol.effectDistance = new Vector2(1, 1);
    }
    public void RefreshEquipSlots()
    {
        Refresh();
    }
}