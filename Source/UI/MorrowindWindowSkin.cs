using UnityEngine;
using Verse;

namespace RoadboundWorld.UI;

public static class MorrowindWindowSkin
{
    public static void DrawWindow(Rect rect)
    {
        DrawTiledBackground(rect);
        DrawOuterFrame(rect);
    }

    public static void DrawPanel(Rect rect, float inset = 6f, bool darkFill = true)
    {
        DrawTiledBackground(rect);
        if (darkFill)
        {
            GUI.color = MorrowindUiResources.PanelShade;
            GUI.DrawTexture(rect.ContractedBy(2f), BaseContent.WhiteTex);
            GUI.color = Color.white;
        }

        DrawSimpleFrame(rect);
        if (inset > 0f)
        {
            GUI.color = MorrowindUiResources.PanelShadeSoft;
            GUI.DrawTexture(rect.ContractedBy(inset), BaseContent.WhiteTex);
            GUI.color = Color.white;
        }
    }

    public static void DrawSlot(Rect rect, bool selected)
    {
        GUI.color = Color.white;
        GUI.DrawTexture(rect, MorrowindUiResources.SlotFrame, ScaleMode.StretchToFill, true);
        if (selected)
        {
            GUI.color = MorrowindUiResources.SelectedOverlay;
            GUI.DrawTexture(rect.ContractedBy(2f), BaseContent.WhiteTex);
            GUI.color = Color.white;
        }
    }

    public static void DrawInsetFill(Rect rect)
    {
        GUI.color = MorrowindUiResources.PanelShade;
        GUI.DrawTexture(rect, BaseContent.WhiteTex);
        GUI.color = Color.white;
    }

    private static void DrawTiledBackground(Rect rect)
    {
        GUI.color = MorrowindUiResources.BackgroundTint;
        GUI.DrawTextureWithTexCoords(rect, MorrowindUiResources.Background, new Rect(0f, 0f, rect.width / 128f, rect.height / 128f));
        GUI.color = Color.white;
    }

    private static void DrawOuterFrame(Rect rect)
    {
        DrawSimpleFrame(rect);

        float cornerSize = 14f;
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, cornerSize, cornerSize), MorrowindUiResources.FrameCornerTl, ScaleMode.StretchToFill, true);
        GUI.DrawTexture(new Rect(rect.xMax - cornerSize, rect.yMin, cornerSize, cornerSize), MorrowindUiResources.FrameCornerTr, ScaleMode.StretchToFill, true);
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMax - cornerSize, cornerSize, cornerSize), MorrowindUiResources.FrameCornerBl, ScaleMode.StretchToFill, true);
        GUI.DrawTexture(new Rect(rect.xMax - cornerSize, rect.yMax - cornerSize, cornerSize, cornerSize), MorrowindUiResources.FrameCornerBr, ScaleMode.StretchToFill, true);
    }

    private static void DrawSimpleFrame(Rect rect)
    {
        Color old = GUI.color;

        GUI.color = MorrowindUiResources.GoldDark;
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 1f), BaseContent.WhiteTex);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), BaseContent.WhiteTex);
        GUI.DrawTexture(new Rect(rect.x, rect.y, 1f, rect.height), BaseContent.WhiteTex);
        GUI.DrawTexture(new Rect(rect.xMax - 1f, rect.y, 1f, rect.height), BaseContent.WhiteTex);

        GUI.color = MorrowindUiResources.GoldSoft;
        GUI.DrawTexture(new Rect(rect.x + 1f, rect.y + 1f, rect.width - 2f, 1f), BaseContent.WhiteTex);
        GUI.DrawTexture(new Rect(rect.x + 1f, rect.yMax - 2f, rect.width - 2f, 1f), BaseContent.WhiteTex);
        GUI.DrawTexture(new Rect(rect.x + 1f, rect.y + 1f, 1f, rect.height - 2f), BaseContent.WhiteTex);
        GUI.DrawTexture(new Rect(rect.xMax - 2f, rect.y + 1f, 1f, rect.height - 2f), BaseContent.WhiteTex);

        GUI.color = MorrowindUiResources.Gold;
        GUI.DrawTexture(new Rect(rect.x + 3f, rect.y + 3f, rect.width - 6f, 1f), BaseContent.WhiteTex);
        GUI.DrawTexture(new Rect(rect.x + 3f, rect.yMax - 4f, rect.width - 6f, 1f), BaseContent.WhiteTex);
        GUI.DrawTexture(new Rect(rect.x + 3f, rect.y + 3f, 1f, rect.height - 6f), BaseContent.WhiteTex);
        GUI.DrawTexture(new Rect(rect.xMax - 4f, rect.y + 3f, 1f, rect.height - 6f), BaseContent.WhiteTex);
        GUI.color = old;
    }
}
