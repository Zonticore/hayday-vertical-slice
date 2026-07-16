using System.Collections.Generic;
using UnityEngine;

public sealed class GridOccupant : MonoBehaviour
{
    private GridService _grid;
    private Vector2Int _origin;
    private Vector2Int _baseFootprint;
    private TileOrientation _orientation;
    private List<Vector2Int> _occupiedCells = new List<Vector2Int>();

    public Vector2Int Origin => _origin;
    public Vector2Int BaseFootprint => _baseFootprint;
    public TileOrientation Orientation => _orientation;
    public IReadOnlyList<Vector2Int> OccupiedCells => _occupiedCells;

    public void Initialize(
        GridService grid,
        Vector2Int origin,
        Vector2Int footprint,
        TileOrientation orientation)
    {
        _grid = grid;
        _origin = origin;
        _baseFootprint = footprint;
        _orientation = orientation;
        _occupiedCells = grid.GetFootprintCells(origin, footprint, orientation);
    }

    private void OnDestroy()
    {
        if (_grid != null)
        {
            _grid.Unregister(this);
        }
    }
}
