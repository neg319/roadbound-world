using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RoadboundWorld.UI;

public static class MorrowindGearTabRenderer
{
    private const float TitleHeight = 30f;
    private const float ModeTabsHeight = 28f;
    private const float CategoryTabsHeight = 26f;
    private const float FooterHeight = 42f;
    private const float LeftPaneWidth = 228f;
    private const float InventoryCellSize = 46f;
    private const float InventoryCellPadding = 4f;
    private const float PaperdollSlotSize = 34f;

    public static void Draw(Rect rect, Pawn pawn)
    {
        if (pawn == null)
        {
            return;
        }

        MorrowindInventoryState state = MorrowindInventoryStateStore.For(pawn);
        Text.Font = GameFont.Small;
        GUI.color = Color.white;
        MorrowindWindowSkin.DrawWindow(rect);

        Rect titleRect = new(rect.x + 10f, rect.y + 8f, rect.width - 20f, TitleHeight);
        Rect topBarRect = new(rect.x + 10f, titleRect.yMax + 5f, rect.width - 20f, ModeTabsHeight);
        Rect contentRect = new(rect.x + 10f, topBarRect.yMax + 8f, rect.width - 20f, rect.height - TitleHeight - ModeTabsHeight - FooterHeight - 34f);
        Rect footerRect = new(rect.x + 10f, contentRect.yMax + 8f, rect.width - 20f, FooterHeight);

        DrawTitleBar(titleRect, pawn);
        DrawTopBar(topBarRect, pawn, state);

        Rect leftRect = new(contentRect.x, contentRect.y, LeftPaneWidth, contentRect.height);
        Rect rightRect = new(leftRect.xMax + 10f, contentRect.y, contentRect.width - LeftPaneWidth - 10f, contentRect.height);

        DrawLeftPane(leftRect, pawn, state);

        switch (state.activeTab)
        {
            case MorrowindInventoryTab.Inventory:
                DrawInventoryPane(rightRect, pawn, state);
                break;
            case MorrowindInventoryTab.Equipment:
                DrawEquipmentPane(rightRect, pawn, state);
                break;
            case MorrowindInventoryTab.Stats:
                DrawStatsPane(rightRect, pawn, state);
                break;
        }

        DrawFooter(footerRect, pawn, state);
    }

    private static void DrawTitleBar(Rect rect, Pawn pawn)
    {
        MorrowindWindowSkin.DrawPanel(rect, inset: 3f, darkFill: false);
        DrawLabelCentered(rect, pawn.Name?.ToStringShort ?? pawn.LabelCap, MorrowindUiResources.TextPrimary);
    }

    private static void DrawTopBar(Rect rect, Pawn pawn, MorrowindInventoryState state)
    {
        Rect weightRect = new(rect.x, rect.y, 118f, rect.height);
        DrawWeightBox(weightRect, pawn);

        Rect tabsRect = new(weightRect.xMax + 10f, rect.y, rect.width - weightRect.width - 10f, rect.height);
        DrawModeTabs(tabsRect, state);
    }

    private static void DrawWeightBox(Rect rect, Pawn pawn)
    {
        MorrowindWindowSkin.DrawPanel(rect, inset: 3f);
        GUI.color = MorrowindUiResources.CarryWeightFill;
        GUI.DrawTexture(rect.ContractedBy(4f), BaseContent.WhiteTex);
        GUI.color = Color.white;
        float carriedMass = MassUtility.GearAndInventoryMass(pawn);
        float capacity = MassUtility.Capacity(pawn);
        DrawLabelCentered(rect, $"{Mathf.RoundToInt(carriedMass)}/{Mathf.RoundToInt(capacity)}", MorrowindUiResources.TextPrimary);
    }

    private static void DrawModeTabs(Rect rect, MorrowindInventoryState state)
    {
        string[] labels = { "Inventory", "Equipment", "Stats" };
        MorrowindInventoryTab[] tabs =
        {
            MorrowindInventoryTab.Inventory,
            MorrowindInventoryTab.Equipment,
            MorrowindInventoryTab.Stats,
        };

        float tabWidth = 118f;
        for (int i = 0; i < tabs.Length; i++)
        {
            Rect tabRect = new(rect.x + i * (tabWidth + 6f), rect.y, tabWidth, rect.height);
            bool active = state.activeTab == tabs[i];
            DrawTab(tabRect, labels[i], active);
            if (Widgets.ButtonInvisible(tabRect))
            {
                state.activeTab = tabs[i];
            }
        }
    }

    private static void DrawTab(Rect rect, string label, bool active)
    {
        GUI.color = Color.white;
        GUI.DrawTexture(rect, active ? MorrowindUiResources.TabActive : MorrowindUiResources.TabInactive, ScaleMode.StretchToFill, true);
        DrawLabelCentered(rect, label, active ? MorrowindUiResources.ActiveTabText : MorrowindUiResources.InactiveTabText);
    }

    private static void DrawLeftPane(Rect rect, Pawn pawn, MorrowindInventoryState state)
    {
        MorrowindWindowSkin.DrawPanel(rect);

        Rect paperRect = new(rect.x + 10f, rect.y + 10f, rect.width - 20f, rect.height - 44f);
        DrawPaperdoll(paperRect, pawn, state);

        Rect armorRect = new(rect.x + 10f, rect.yMax - 30f, rect.width - 20f, 22f);
        DrawArmorLine(armorRect, pawn);
    }

    private static void DrawArmorLine(Rect rect, Pawn pawn)
    {
        int armor = Mathf.RoundToInt((pawn.GetStatValue(StatDefOf.ArmorRating_Sharp) + pawn.GetStatValue(StatDefOf.ArmorRating_Blunt)) * 50f);
        DrawLabelLeft(rect, $"Armor: {armor}", MorrowindUiResources.TextPrimary);
    }

    private static void DrawPaperdoll(Rect rect, Pawn pawn, MorrowindInventoryState state)
    {
        MorrowindWindowSkin.DrawPanel(rect);
        Rect inner = rect.ContractedBy(8f);
        MorrowindWindowSkin.DrawInsetFill(inner);

        Rect portraitRect = new(inner.x + 14f, inner.y + 18f, inner.width - 28f, inner.height - 50f);
        Texture portrait = PortraitsCache.Get(
            pawn,
            portraitRect.size,
            Rot4.South,
            Vector3.zero,
            1f,
            true,
            true,
            true,
            true,
            null,
            null,
            false);

        GUI.color = Color.white;
        GUI.DrawTexture(portraitRect, portrait, ScaleMode.ScaleToFit, true);

        Dictionary<string, Thing> slots = BuildSlotMap(pawn);
        foreach ((string key, Rect slotRect) in PaperdollSlots(inner))
        {
            Thing thing = slots.TryGetValue(key, out Thing value) ? value : null;
            MorrowindSelectionSource source = thing is Apparel ? MorrowindSelectionSource.Apparel : MorrowindSelectionSource.Equipment;
            bool selected = thing != null && state.selectedThingId == thing.thingIDNumber && state.selectionSource == source;

            MorrowindWindowSkin.DrawSlot(slotRect, selected);
            if (thing != null)
            {
                DrawThingIcon(slotRect.ContractedBy(4f), thing);
                TooltipHandler.TipRegion(slotRect, $"{key}: {thing.LabelCap}");
                if (Widgets.ButtonInvisible(slotRect))
                {
                    state.Select(thing, source);
                }
            }
            else
            {
                TooltipHandler.TipRegion(slotRect, key);
            }
        }
    }

    private static IEnumerable<(string, Rect)> PaperdollSlots(Rect rect)
    {
        float centerX = rect.x + rect.width * 0.5f;
        float topY = rect.y + 10f;
        float shoulderY = rect.y + rect.height * 0.32f;
        float waistY = rect.y + rect.height * 0.56f;
        float lowerY = rect.y + rect.height * 0.79f;

        yield return ("Head", new Rect(centerX - PaperdollSlotSize * 0.5f, topY, PaperdollSlotSize, PaperdollSlotSize));
        yield return ("Neck", new Rect(centerX - PaperdollSlotSize * 0.5f, topY + 38f, PaperdollSlotSize, PaperdollSlotSize));
        yield return ("Main hand", new Rect(rect.x + 8f, shoulderY, PaperdollSlotSize, PaperdollSlotSize));
        yield return ("Off hand", new Rect(rect.xMax - PaperdollSlotSize - 8f, shoulderY, PaperdollSlotSize, PaperdollSlotSize));
        yield return ("Chest", new Rect(centerX - PaperdollSlotSize * 0.5f, shoulderY + 14f, PaperdollSlotSize, PaperdollSlotSize));
        yield return ("Hands", new Rect(rect.x + 8f, waistY, PaperdollSlotSize, PaperdollSlotSize));
        yield return ("Belt", new Rect(centerX - PaperdollSlotSize * 0.5f, waistY, PaperdollSlotSize, PaperdollSlotSize));
        yield return ("Utility", new Rect(rect.xMax - PaperdollSlotSize - 8f, waistY, PaperdollSlotSize, PaperdollSlotSize));
        yield return ("Legs", new Rect(centerX - PaperdollSlotSize * 0.5f, lowerY, PaperdollSlotSize, PaperdollSlotSize));
        yield return ("Feet", new Rect(centerX - PaperdollSlotSize * 0.5f, lowerY + 36f, PaperdollSlotSize, PaperdollSlotSize));
    }

    private static Dictionary<string, Thing> BuildSlotMap(Pawn pawn)
    {
        Dictionary<string, Thing> map = new();
        HashSet<Thing> used = new();

        ThingWithComps primary = pawn.equipment?.Primary;
        if (primary != null)
        {
            map["Main hand"] = primary;
            used.Add(primary);
        }

        List<Apparel> apparel = pawn.apparel?.WornApparel ?? new List<Apparel>();
        TryAssign(map, used, "Head", apparel, a => Covers(a, "Head"));
        TryAssign(map, used, "Neck", apparel, a => Covers(a, "Neck"));
        TryAssign(map, used, "Chest", apparel, a => Covers(a, "Torso") || Covers(a, "Chest") || Covers(a, "Shoulder"));
        TryAssign(map, used, "Hands", apparel, a => Covers(a, "Hand") || Covers(a, "Arm"));
        TryAssign(map, used, "Belt", apparel, a => Covers(a, "Waist") || CoversLayer(a, "Belt"));
        TryAssign(map, used, "Legs", apparel, a => Covers(a, "Leg"));
        TryAssign(map, used, "Feet", apparel, a => Covers(a, "Foot"));
        TryAssign(map, used, "Utility", apparel, a => CoversLayer(a, "Overhead") || CoversLayer(a, "Middle") || CoversLayer(a, "Belt"));
        TryAssign(map, used, "Off hand", apparel, a => Covers(a, "Hand") || Covers(a, "Arm"));

        return map;
    }

    private static void TryAssign(Dictionary<string, Thing> map, HashSet<Thing> used, string key, IEnumerable<Apparel> apparels, Predicate<Apparel> predicate)
    {
        Apparel apparel = apparels.FirstOrDefault(a => !used.Contains(a) && predicate(a));
        if (apparel != null)
        {
            map[key] = apparel;
            used.Add(apparel);
        }
    }

    private static bool Covers(Apparel apparel, string token)
    {
        return apparel.def.apparel?.bodyPartGroups?.Any(group => group.defName.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0) == true;
    }

    private static bool CoversLayer(Apparel apparel, string token)
    {
        return apparel.def.apparel?.layers?.Any(layer => layer.defName.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0) == true;
    }

    private static void DrawInventoryPane(Rect rect, Pawn pawn, MorrowindInventoryState state)
    {
        MorrowindWindowSkin.DrawPanel(rect);
        Rect inner = rect.ContractedBy(10f);
        Rect categoryRect = new(inner.x, inner.y, inner.width, CategoryTabsHeight);
        DrawCategoryTabs(categoryRect, state);

        List<MorrowindInventoryEntry> entries = GatherInventoryEntries(pawn, state.activeCategory);
        Rect gridRect = new(inner.x, categoryRect.yMax + 6f, inner.width, inner.height - CategoryTabsHeight - 20f);
        DrawInventoryGrid(gridRect, entries, state, pawn);

        Rect ornamentRect = new(inner.x + 4f, inner.yMax - 8f, inner.width - 8f, 4f);
        DrawBottomRail(ornamentRect);
    }

    private static void DrawCategoryTabs(Rect rect, MorrowindInventoryState state)
    {
        (MorrowindItemCategory category, string label)[] tabs =
        {
            (MorrowindItemCategory.All, "All"),
            (MorrowindItemCategory.Weapons, "Weapons"),
            (MorrowindItemCategory.Clothing, "Clothing"),
            (MorrowindItemCategory.Medicine, "Magic"),
            (MorrowindItemCategory.Misc, "Misc"),
        };

        float x = rect.x;
        foreach ((MorrowindItemCategory category, string label) in tabs)
        {
            float width = label.Length > 7 ? 90f : 72f;
            Rect tabRect = new(x, rect.y, width, rect.height);
            bool active = state.activeCategory == category;
            DrawTab(tabRect, label, active);
            if (Widgets.ButtonInvisible(tabRect))
            {
                state.activeCategory = category;
            }

            x += width + 4f;
        }
    }

    private static List<MorrowindInventoryEntry> GatherInventoryEntries(Pawn pawn, MorrowindItemCategory category)
    {
        List<MorrowindInventoryEntry> entries = new();
        foreach (Thing thing in GatherEquippedThings(pawn).Where(t => MatchesCategory(t, category)))
        {
            MorrowindSelectionSource source = thing is Apparel ? MorrowindSelectionSource.Apparel : MorrowindSelectionSource.Equipment;
            entries.Add(new MorrowindInventoryEntry(thing, source, true));
        }

        IEnumerable<Thing> inventoryThings = pawn.inventory?.innerContainer?.ToList() ?? Enumerable.Empty<Thing>();
        foreach (Thing thing in inventoryThings.Where(t => MatchesCategory(t, category)))
        {
            entries.Add(new MorrowindInventoryEntry(thing, MorrowindSelectionSource.Inventory, false));
        }

        return entries
            .OrderByDescending(entry => entry.equipped)
            .ThenBy(entry => CategorySortIndex(entry.thing))
            .ThenBy(entry => entry.thing.LabelCap.ToString())
            .ThenByDescending(entry => entry.thing.stackCount)
            .ToList();
    }

    private static bool MatchesCategory(Thing thing, MorrowindItemCategory category)
    {
        return category switch
        {
            MorrowindItemCategory.All => true,
            MorrowindItemCategory.Weapons => thing.def.IsWeapon,
            MorrowindItemCategory.Clothing => thing is Apparel,
            MorrowindItemCategory.Medicine => thing.def.IsMedicine || thing.def.ingestible != null,
            MorrowindItemCategory.Misc => !thing.def.IsWeapon && thing is not Apparel && !thing.def.IsMedicine && thing.def.ingestible == null,
            _ => true,
        };
    }

    private static int CategorySortIndex(Thing thing)
    {
        if (thing.def.IsWeapon)
        {
            return 0;
        }

        if (thing is Apparel)
        {
            return 1;
        }

        if (thing.def.IsMedicine || thing.def.ingestible != null)
        {
            return 2;
        }

        return 3;
    }

    private static void DrawInventoryGrid(Rect rect, List<MorrowindInventoryEntry> entries, MorrowindInventoryState state, Pawn pawn)
    {
        int columns = Mathf.Max(1, Mathf.FloorToInt((rect.width - 4f) / (InventoryCellSize + InventoryCellPadding)));
        int rows = Mathf.Max(1, Mathf.CeilToInt(entries.Count / (float)columns));
        float viewHeight = Mathf.Max(rect.height, rows * (InventoryCellSize + InventoryCellPadding) + 4f);
        Rect view = new(0f, 0f, rect.width - 16f, viewHeight);

        Widgets.BeginScrollView(rect, ref state.inventoryScroll, view);

        for (int index = 0; index < entries.Count; index++)
        {
            MorrowindInventoryEntry entry = entries[index];
            Thing thing = entry.thing;
            int row = index / columns;
            int col = index % columns;
            Rect cell = new(col * (InventoryCellSize + InventoryCellPadding), row * (InventoryCellSize + InventoryCellPadding), InventoryCellSize, InventoryCellSize);
            bool selected = state.selectedThingId == thing.thingIDNumber && state.selectionSource == entry.source;
            MorrowindWindowSkin.DrawSlot(cell, selected);
            if (entry.equipped)
            {
                MorrowindWindowSkin.DrawEquippedOutline(cell);
            }

            DrawThingIcon(cell.ContractedBy(5f), thing);

            if (thing.stackCount > 1)
            {
                Text.Anchor = TextAnchor.LowerRight;
                GUI.color = MorrowindUiResources.TextPrimary;
                Widgets.Label(cell.ContractedBy(3f), thing.stackCount.ToString());
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
            }

            TooltipHandler.TipRegion(cell, entry.equipped ? $"[Equipped] {thing.LabelCap}" : thing.LabelCap.ToString());
            if (Widgets.ButtonInvisible(cell))
            {
                state.Select(thing, entry.source);
                if (Event.current != null && Event.current.clickCount > 1)
                {
                    PerformPrimaryAction(pawn, state);
                }
            }
        }

        Widgets.EndScrollView();
    }

    private static void DrawBottomRail(Rect rect)
    {
        GUI.color = MorrowindUiResources.GoldDark;
        GUI.DrawTexture(rect, BaseContent.WhiteTex);
        GUI.color = MorrowindUiResources.Gold;
        GUI.DrawTexture(new Rect(rect.x + 1f, rect.y + 1f, rect.width - 2f, 1f), BaseContent.WhiteTex);
        GUI.color = Color.white;
    }

    private static void DrawEquipmentPane(Rect rect, Pawn pawn, MorrowindInventoryState state)
    {
        MorrowindWindowSkin.DrawPanel(rect);
        Rect inner = rect.ContractedBy(10f);
        Rect headerRect = new(inner.x, inner.y, inner.width, 24f);
        DrawLabelLeft(headerRect, "Equipped items", MorrowindUiResources.TextPrimary);

        List<(string slot, Thing thing, MorrowindSelectionSource source)> equipped = GatherEquippedSlots(pawn);
        Rect listRect = new(inner.x, inner.y + 28f, inner.width, inner.height - 28f);
        float viewHeight = Mathf.Max(listRect.height, equipped.Count * 44f + 4f);
        Rect view = new(0f, 0f, listRect.width - 16f, viewHeight);

        Widgets.BeginScrollView(listRect, ref state.equipmentScroll, view);
        float y = 0f;
        foreach ((string slot, Thing thing, MorrowindSelectionSource source) in equipped)
        {
            DrawEquipmentRow(new Rect(0f, y, view.width, 40f), slot, thing, state, source);
            y += 44f;
        }

        Widgets.EndScrollView();
    }

    private static List<(string slot, Thing thing, MorrowindSelectionSource source)> GatherEquippedSlots(Pawn pawn)
    {
        List<(string, Thing, MorrowindSelectionSource)> list = new();

        if (pawn.equipment?.Primary != null)
        {
            list.Add(("Main hand", pawn.equipment.Primary, MorrowindSelectionSource.Equipment));
        }

        foreach (Thing thing in GatherEquippedThings(pawn).Where(t => t is Apparel))
        {
            list.Add((InferSlotLabel(thing), thing, MorrowindSelectionSource.Apparel));
        }

        return list;
    }

    private static string InferSlotLabel(Thing thing)
    {
        if (thing is not Apparel apparel)
        {
            return "Gear";
        }

        if (Covers(apparel, "Head")) return "Head";
        if (Covers(apparel, "Neck")) return "Neck";
        if (Covers(apparel, "Torso") || Covers(apparel, "Chest")) return "Chest";
        if (Covers(apparel, "Waist") || CoversLayer(apparel, "Belt")) return "Belt";
        if (Covers(apparel, "Leg")) return "Legs";
        if (Covers(apparel, "Foot")) return "Feet";
        if (Covers(apparel, "Hand") || Covers(apparel, "Arm")) return "Hands";
        return "Gear";
    }

    private static void DrawEquipmentRow(Rect rect, string slot, Thing thing, MorrowindInventoryState state, MorrowindSelectionSource source)
    {
        bool selected = state.selectedThingId == thing.thingIDNumber && state.selectionSource == source;
        if (selected)
        {
            GUI.color = MorrowindUiResources.SelectedOverlay;
            GUI.DrawTexture(rect, BaseContent.WhiteTex);
            GUI.color = Color.white;
        }

        Rect slotLabelRect = new(rect.x, rect.y, 90f, rect.height);
        DrawLabelLeft(slotLabelRect, slot, MorrowindUiResources.TextMuted);

        Rect iconRect = new(rect.x + 94f, rect.y + 2f, 36f, 36f);
        MorrowindWindowSkin.DrawSlot(iconRect, selected);
        DrawThingIcon(iconRect.ContractedBy(4f), thing);

        Rect labelRect = new(rect.x + 138f, rect.y, rect.width - 138f, rect.height);
        DrawLabelLeft(labelRect, thing.LabelCap, MorrowindUiResources.TextPrimary);
        TooltipHandler.TipRegion(rect, thing.LabelCap);
        if (Widgets.ButtonInvisible(rect))
        {
            state.Select(thing, source);
        }
    }

    private static void DrawStatsPane(Rect rect, Pawn pawn, MorrowindInventoryState state)
    {
        MorrowindWindowSkin.DrawPanel(rect);
        Rect inner = rect.ContractedBy(10f);
        Rect listRect = new(inner.x, inner.y + 4f, inner.width, inner.height - 4f);
        float viewHeight = Mathf.Max(listRect.height, 360f + (pawn.skills?.skills.Count ?? 0) * 24f);
        Rect view = new(0f, 0f, listRect.width - 16f, viewHeight);

        Widgets.BeginScrollView(listRect, ref state.statsScroll, view);
        float y = 0f;
        y = DrawStatLine(view, y, "Move speed", pawn.GetStatValue(StatDefOf.MoveSpeed).ToString("F2"));
        y = DrawStatLine(view, y, "Shooting accuracy", pawn.GetStatValue(StatDefOf.ShootingAccuracyPawn).ToStringPercent());
        y = DrawStatLine(view, y, "Melee hit chance", pawn.GetStatValue(StatDefOf.MeleeHitChance).ToStringPercent());
        y = DrawStatLine(view, y, "Armor sharp", pawn.GetStatValue(StatDefOf.ArmorRating_Sharp).ToStringPercent());
        y = DrawStatLine(view, y, "Armor blunt", pawn.GetStatValue(StatDefOf.ArmorRating_Blunt).ToStringPercent());
        y = DrawStatLine(view, y, "Armor heat", pawn.GetStatValue(StatDefOf.ArmorRating_Heat).ToStringPercent());
        y = DrawStatLine(view, y, "Pain", pawn.health.hediffSet.PainTotal.ToStringPercent());
        y = DrawStatLine(view, y, "Health", pawn.health.summaryHealth.SummaryHealthPercent.ToStringPercent());
        y += 10f;
        y = DrawSectionLabel(view, y, "Skills");

        if (pawn.skills != null)
        {
            foreach (SkillRecord skill in pawn.skills.skills.OrderByDescending(s => s.Level))
            {
                y = DrawStatLine(view, y, skill.def.skillLabel.CapitalizeFirst(), skill.Level.ToString());
            }
        }

        Widgets.EndScrollView();
    }

    private static float DrawSectionLabel(Rect view, float y, string label)
    {
        Rect row = new(0f, y, view.width, 24f);
        DrawLabelLeft(row, label, MorrowindUiResources.TextPrimary);
        return y + 26f;
    }

    private static float DrawStatLine(Rect view, float y, string left, string right)
    {
        Rect row = new(0f, y, view.width, 22f);
        DrawLabelLeft(new Rect(row.x, row.y, row.width * 0.62f, row.height), left, MorrowindUiResources.TextMuted);
        DrawLabelRight(new Rect(row.x + row.width * 0.62f, row.y, row.width * 0.35f, row.height), right, MorrowindUiResources.TextPrimary);
        GUI.color = MorrowindUiResources.GoldSoft;
        GUI.DrawTexture(new Rect(row.x, row.yMax, row.width, 1f), BaseContent.WhiteTex);
        GUI.color = Color.white;
        return y + 24f;
    }

    private static void DrawFooter(Rect rect, Pawn pawn, MorrowindInventoryState state)
    {
        MorrowindWindowSkin.DrawPanel(rect, inset: 3f);
        Thing selectedThing = ResolveSelection(pawn, state);

        Rect labelRect = new(rect.x + 10f, rect.y + 7f, rect.width * 0.52f, 28f);
        DrawLabelLeft(labelRect, selectedThing?.LabelCap ?? "Nothing selected", MorrowindUiResources.TextPrimary);

        float buttonWidth = 88f;
        float gap = 6f;
        Rect clearRect = new(rect.xMax - buttonWidth, rect.y + 7f, buttonWidth, 28f);
        Rect dropRect = new(clearRect.x - gap - buttonWidth, clearRect.y, buttonWidth, clearRect.height);
        Rect primaryRect = new(dropRect.x - gap - 110f, clearRect.y, 110f, clearRect.height);

        if (DrawActionButton(clearRect, "Clear"))
        {
            state.ClearSelection();
        }

        GUI.enabled = selectedThing != null;
        if (DrawActionButton(dropRect, "Drop"))
        {
            DropSelectedThing(pawn, state);
        }

        if (DrawActionButton(primaryRect, PrimaryActionLabel(selectedThing, state.selectionSource)))
        {
            PerformPrimaryAction(pawn, state);
        }
        GUI.enabled = true;
    }

    private static bool DrawActionButton(Rect rect, string label)
    {
        MorrowindWindowSkin.DrawPanel(rect, inset: 2f);
        DrawLabelCentered(rect, label, GUI.enabled ? MorrowindUiResources.TextPrimary : MorrowindUiResources.TextMuted);
        return Widgets.ButtonInvisible(rect);
    }

    private static string PrimaryActionLabel(Thing selectedThing, MorrowindSelectionSource source)
    {
        if (selectedThing == null)
        {
            return "Use";
        }

        return source switch
        {
            MorrowindSelectionSource.Inventory when selectedThing is Apparel => "Wear",
            MorrowindSelectionSource.Inventory when selectedThing is ThingWithComps => "Equip",
            MorrowindSelectionSource.Inventory => "Use",
            MorrowindSelectionSource.Equipment => "Unequip",
            MorrowindSelectionSource.Apparel => "Unequip",
            _ => "Use",
        };
    }

    private static Thing ResolveSelection(Pawn pawn, MorrowindInventoryState state)
    {
        if (pawn == null || state.selectedThingId < 0)
        {
            return null;
        }

        return state.selectionSource switch
        {
            MorrowindSelectionSource.Inventory => pawn.inventory?.innerContainer?.FirstOrDefault(t => t.thingIDNumber == state.selectedThingId),
            MorrowindSelectionSource.Equipment => pawn.equipment?.AllEquipmentListForReading?.FirstOrDefault(t => t.thingIDNumber == state.selectedThingId),
            MorrowindSelectionSource.Apparel => pawn.apparel?.WornApparel?.FirstOrDefault(t => t.thingIDNumber == state.selectedThingId),
            _ => null,
        };
    }

    private static void PerformPrimaryAction(Pawn pawn, MorrowindInventoryState state)
    {
        Thing selectedThing = ResolveSelection(pawn, state);
        if (pawn == null || selectedThing == null)
        {
            return;
        }

        switch (state.selectionSource)
        {
            case MorrowindSelectionSource.Inventory:
                if (selectedThing is Apparel apparel)
                {
                    if (pawn.inventory?.innerContainer?.Remove(apparel) == true)
                    {
                        pawn.apparel?.Wear(apparel, false, false);
                    }
                }
                else if (selectedThing is ThingWithComps equippable)
                {
                    ThingWithComps primary = pawn.equipment?.Primary;
                    if (primary != null && pawn.inventory?.innerContainer != null)
                    {
                        pawn.equipment.TryTransferEquipmentToContainer(primary, pawn.inventory.innerContainer);
                    }

                    if (pawn.inventory?.innerContainer?.Remove(equippable) == true)
                    {
                        pawn.equipment?.AddEquipment(equippable);
                    }
                }
                break;
            case MorrowindSelectionSource.Equipment:
                if (selectedThing is ThingWithComps gear && pawn.inventory?.innerContainer != null)
                {
                    pawn.equipment?.TryTransferEquipmentToContainer(gear, pawn.inventory.innerContainer);
                }
                break;
            case MorrowindSelectionSource.Apparel:
                if (selectedThing is Apparel worn && pawn.inventory?.innerContainer != null)
                {
                    pawn.apparel?.Remove(worn);
                    pawn.inventory.innerContainer.TryAdd(worn);
                }
                break;
        }

        state.ClearSelection();
    }

    private static void DropSelectedThing(Pawn pawn, MorrowindInventoryState state)
    {
        Thing selectedThing = ResolveSelection(pawn, state);
        if (pawn == null || selectedThing == null)
        {
            return;
        }

        switch (state.selectionSource)
        {
            case MorrowindSelectionSource.Inventory:
                pawn.inventory?.innerContainer?.TryDrop(selectedThing, pawn.PositionHeld, pawn.MapHeld, ThingPlaceMode.Near, out _);
                break;
            case MorrowindSelectionSource.Equipment:
                if (selectedThing is ThingWithComps gear)
                {
                    pawn.equipment?.TryDropEquipment(gear, out _, pawn.PositionHeld, forbid: false);
                }
                break;
            case MorrowindSelectionSource.Apparel:
                if (selectedThing is Apparel apparel)
                {
                    pawn.apparel?.TryDrop(apparel, out _, pawn.PositionHeld, forbid: false);
                }
                break;
        }

        state.ClearSelection();
    }

    private static List<Thing> GatherEquippedThings(Pawn pawn)
    {
        List<Thing> list = new();
        if (pawn.equipment?.AllEquipmentListForReading != null)
        {
            list.AddRange(pawn.equipment.AllEquipmentListForReading);
        }

        if (pawn.apparel?.WornApparel != null)
        {
            list.AddRange(pawn.apparel.WornApparel);
        }

        return list;
    }

    private static void DrawThingIcon(Rect rect, Thing thing)
    {
        Texture icon = thing.def?.uiIcon ?? BaseContent.BadTex;
        Color oldColor = GUI.color;
        GUI.color = thing.DrawColor;
        GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit, true);
        GUI.color = oldColor;
    }

    private static void DrawLabelCentered(Rect rect, string text, Color color)
    {
        Text.Anchor = TextAnchor.MiddleCenter;
        GUI.color = color;
        Widgets.Label(rect, text);
        GUI.color = Color.white;
        Text.Anchor = TextAnchor.UpperLeft;
    }

    private static void DrawLabelLeft(Rect rect, string text, Color color)
    {
        Text.Anchor = TextAnchor.MiddleLeft;
        GUI.color = color;
        Widgets.Label(rect, text);
        GUI.color = Color.white;
        Text.Anchor = TextAnchor.UpperLeft;
    }

    private static void DrawLabelRight(Rect rect, string text, Color color)
    {
        Text.Anchor = TextAnchor.MiddleRight;
        GUI.color = color;
        Widgets.Label(rect, text);
        GUI.color = Color.white;
        Text.Anchor = TextAnchor.UpperLeft;
    }
}
