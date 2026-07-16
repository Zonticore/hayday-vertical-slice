using UnityEngine;

public sealed class TileInstance : MonoBehaviour
{
    private TileDefinitionSO _definition;
    private Vector2Int _gridPosition;
    private TileOrientation _orientation;
    private SpriteRenderer _spriteRenderer;
    private GridOccupant _gridOccupant;

    public TileDefinitionSO Definition => _definition;
    public Vector2Int GridPosition => _gridPosition;
    public TileOrientation Orientation => _orientation;
    public SpriteRenderer SpriteRenderer => _spriteRenderer;
    public GridOccupant GridOccupant => _gridOccupant;

    public void Initialize(
        TileDefinitionSO definition,
        Vector2Int gridPosition,
        TileOrientation orientation,
        SpriteRenderer spriteRenderer,
        GridOccupant gridOccupant)
    {
        _definition = definition;
        _gridPosition = gridPosition;
        _orientation = orientation;
        _spriteRenderer = spriteRenderer;
        _gridOccupant = gridOccupant;

        gameObject.name = $"Tile_{definition.TileId}_{gridPosition.x}_{gridPosition.y}";
        RefreshSortingOrder();
    }

    public void RefreshSortingOrder()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.sortingOrder = -Mathf.RoundToInt(transform.position.y * 100f);
        }
    }

    public PlacedTileSaveData CreateSaveData(string runtimeStateJson = "")
    {
        return new PlacedTileSaveData
        {
            tileId = _definition.TileId,
            gridX = _gridPosition.x,
            gridY = _gridPosition.y,
            orientation = _orientation,
            runtimeStateJson = runtimeStateJson
        };
    }
}
