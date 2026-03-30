using System;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RoadboundWorld.Utility;

public static class MapEdgeUtility
{
    public static bool IsOnEdge(IntVec3 cell, Map map, int edgeThickness = 1)
    {
        return cell.x < edgeThickness ||
               cell.z < edgeThickness ||
               cell.x >= map.Size.x - edgeThickness ||
               cell.z >= map.Size.z - edgeThickness;
    }

    public static Direction8Way DirectionFromCenter(Map map, IntVec3 cell)
    {
        IntVec3 offset = cell - map.Center;
        if (offset.x == 0 && offset.z == 0)
        {
            return Direction8Way.North;
        }

        float angle = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
        angle = (90f - angle + 360f) % 360f;
        angle = (angle + 22.5f) % 360f;
        int sector = (int)(angle / 45f);
        return (Direction8Way)sector;
    }

    public static int GetNeighborTile(int fromTile, Direction8Way direction)
    {
        for (int i = 0; i < Find.WorldGrid.GetTileNeighborCount(fromTile); i++)
        {
            int neighbor = Find.WorldGrid.GetTileNeighbor(fromTile, i);
            if (Find.WorldGrid.GetDirection8WayFromTo(fromTile, neighbor) == direction)
            {
                return neighbor;
            }
        }

        return -1;
    }

    public static bool IsWalkableTile(int tile)
    {
        return tile >= 0;
    }

    public static Predicate<IntVec3> GetEntryValidator(Map targetMap, IntVec3 previousCell, IntVec3 previousSize, out IntVec3 cameraCell)
    {
        float nx = (float)previousCell.x / Math.Max(1, previousSize.x - 1);
        float nz = (float)previousCell.z / Math.Max(1, previousSize.z - 1);
        IntVec3 mirrored = new IntVec3(
            (int)((targetMap.Size.x - 1) * (1f - nx)),
            0,
            (int)((targetMap.Size.z - 1) * (1f - nz)));

        mirrored = mirrored.ClampInsideMap(targetMap);
        cameraCell = mirrored;

        return c => c.InBounds(targetMap)
                    && c.Standable(targetMap)
                    && (c - mirrored).LengthHorizontalSquared <= 144
                    && IsOnEdge(c, targetMap, 4);
    }

    public static string DescribeTile(int tile)
    {
        string text = $"Tile {tile}";

        var obj = Find.WorldObjects.AllWorldObjects.FirstOrDefault(w => w.Tile == tile);
        if (obj != null)
        {
            text += $"\n{obj.LabelCap}";
        }

        return text;
    }
}
