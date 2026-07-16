using System.Collections.Generic;
using UnityEngine;

public sealed class GridService : MonoBehaviourSingleton<GridService>
{
    [Header("2:1 Isometric Cell Size")]
    [SerializeField] private Vector2 cellSize = new Vector2(2.56f, 1.28f);
    [SerializeField] private Vector3 worldOrigin;

    private readonly Dictionary<Vector2Int, GridOccupant> _occupancy =
        new Dictionary<Vector2Int, GridOccupant>();

    public Vector2 CellSize => cellSize;
    public Vector3 WorldOrigin => worldOrigin;

    public Vector3 CellToWorld(Vector2Int cell)
    {
        float x = (cell.x - cell.y) * cellSize.x * 0.5f;
        float y = (cell.x + cell.y) * cellSize.y * 0.5f;
        return worldOrigin + new Vector3(x, y, 0f);
    }

    public Vector2Int WorldToCell(Vector3 worldPosition)
    {
        Vector3 local = worldPosition - worldOrigin;
        float axisX = local.x / (cellSize.x * 0.5f);
        float axisY = local.y / (cellSize.y * 0.5f);

        float gridX = (axisX + axisY) * 0.5f;
        float gridY = (axisY - axisX) * 0.5f;

        return new Vector2Int(Mathf.RoundToInt(gridX), Mathf.RoundToInt(gridY));
    }

    public Vector3 GetFootprintWorldCenter(
        Vector2Int origin,
        Vector2Int footprint,
        TileOrientation orientation)
    {
        Vector2Int oriented = TileOrientationUtility.GetOrientedFootprint(
            footprint,
            orientation);

        Vector2Int lastCell = origin + new Vector2Int(oriented.x - 1, oriented.y - 1);
        return (CellToWorld(origin) + CellToWorld(lastCell)) * 0.5f;
    }

    public Vector2[] GetFootprintLocalCorners(
        Vector2Int footprint,
        TileOrientation orientation)
    {
        Vector2Int oriented = TileOrientationUtility.GetOrientedFootprint(
            footprint,
            orientation);

        Vector2 axisX = new Vector2(cellSize.x * 0.5f, cellSize.y * 0.5f);
        Vector2 axisY = new Vector2(-cellSize.x * 0.5f, cellSize.y * 0.5f);
        Vector2 fullX = axisX * oriented.x;
        Vector2 fullY = axisY * oriented.y;

        return new[]
        {
            -(fullX + fullY) * 0.5f,
            (fullX - fullY) * 0.5f,
            (fullX + fullY) * 0.5f,
            (-fullX + fullY) * 0.5f
        };
    }

    public bool CanPlace(
        Vector2Int origin,
        Vector2Int footprint,
        TileOrientation orientation)
    {
        List<Vector2Int> cells = GetFootprintCells(origin, footprint, orientation);
        for (int i = 0; i < cells.Count; i++)
        {
            if (_occupancy.ContainsKey(cells[i]))
            {
                return false;
            }
        }

        return true;
    }

    public bool TryRegister(GridOccupant occupant)
    {
        if (occupant == null) return false;

        IReadOnlyList<Vector2Int> cells = occupant.OccupiedCells;
        for (int i = 0; i < cells.Count; i++)
        {
            if (_occupancy.ContainsKey(cells[i]))
            {
                return false;
            }
        }

        for (int i = 0; i < cells.Count; i++)
        {
            _occupancy.Add(cells[i], occupant);
        }

        return true;
    }

    public void Unregister(GridOccupant occupant)
    {
        if (occupant == null) return;

        IReadOnlyList<Vector2Int> cells = occupant.OccupiedCells;
        for (int i = 0; i < cells.Count; i++)
        {
            if (_occupancy.TryGetValue(cells[i], out GridOccupant current) &&
                current == occupant)
            {
                _occupancy.Remove(cells[i]);
            }
        }
    }

    public bool TryGetOccupant(Vector2Int cell, out GridOccupant occupant)
    {
        return _occupancy.TryGetValue(cell, out occupant);
    }

    public List<Vector2Int> GetFootprintCells(
        Vector2Int origin,
        Vector2Int footprint,
        TileOrientation orientation)
    {
        Vector2Int oriented = TileOrientationUtility.GetOrientedFootprint(
            footprint,
            orientation);

        var cells = new List<Vector2Int>(oriented.x * oriented.y);
        for (int x = 0; x < oriented.x; x++)
        {
            for (int y = 0; y < oriented.y; y++)
            {
                cells.Add(origin + new Vector2Int(x, y));
            }
        }

        return cells;
    }
}
