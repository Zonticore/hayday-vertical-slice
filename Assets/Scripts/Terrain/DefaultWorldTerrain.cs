using UnityEngine;
using UnityEngine.Tilemaps;

public sealed class DefaultWorldTerrain : MonoBehaviour
{
    private const bool verbose = false;

    [Header("Terrain Area")]
    [SerializeField, Min(1)] private int width = 90;
    [SerializeField, Min(1)] private int height = 90;

    [Header("Terrain Tiles")]
    [SerializeField] private string grassTileId = "grass";
    [SerializeField] private string dirtTileId = "dirt";
    [SerializeField, Range(0f, 1f)] private float dirtCoverage = 0.18f;
    [SerializeField, Min(0.001f)] private float dirtPatchScale = 0.09f;
    [SerializeField] private int terrainSeed = 1729;

    [Header("Rendering")]
    [SerializeField] private int sortingOrder = -30000;
    [SerializeField] private bool buildOnStart = true;

    private GameObject _terrainRoot;
    private Tile _grassTile;
    private Tile _dirtTile;

    private void Start()
    {
        if (buildOnStart) BuildTerrain();
    }

    [ContextMenu("Build Default Terrain")]
    public void BuildTerrain()
    {
        GridService gameplayGrid = GridService.instance;
        RegistryService registries = RegistryService.instance;
        if (gameplayGrid == null || registries == null || registries.TileRegistry == null)
        {
            if (verbose) Log.error("[DefaultWorldTerrain] Grid or tile registry is unavailable.");
            return;
        }

        if (!registries.TileRegistry.TryGet(grassTileId, out TileDefinitionSO grassDefinition) ||
            !registries.TileRegistry.TryGet(dirtTileId, out TileDefinitionSO dirtDefinition) ||
            grassDefinition.Sprite == null || dirtDefinition.Sprite == null)
        {
            if (verbose) Log.error("[DefaultWorldTerrain] Grass or dirt tile definition is unavailable.");
            return;
        }

        ClearTerrain();
        CreateRuntimeTiles(grassDefinition.Sprite, dirtDefinition.Sprite);

        _terrainRoot = new GameObject("DefaultWorldTerrainTilemap");
        _terrainRoot.transform.SetParent(transform, false);
        _terrainRoot.transform.position = gameplayGrid.WorldOrigin;

        Grid visualGrid = _terrainRoot.AddComponent<Grid>();
        visualGrid.cellLayout = GridLayout.CellLayout.Isometric;
        visualGrid.cellSize = new Vector3(gameplayGrid.CellSize.x, gameplayGrid.CellSize.y, 1f);

        var tilemapObject = new GameObject("Ground", typeof(Tilemap), typeof(TilemapRenderer));
        tilemapObject.transform.SetParent(_terrainRoot.transform, false);
        Tilemap tilemap = tilemapObject.GetComponent<Tilemap>();
        TilemapRenderer renderer = tilemapObject.GetComponent<TilemapRenderer>();
        renderer.sortingOrder = sortingOrder;

        int startX = -width / 2;
        int startY = -height / 2;
        var bounds = new BoundsInt(startX, startY, 0, width, height, 1);
        var tiles = new TileBase[width * height];

        int index = 0;
        for (int y = startY; y < startY + height; y++)
        {
            for (int x = startX; x < startX + width; x++)
            {
                tiles[index++] = IsDirt(x, y) ? _dirtTile : _grassTile;
            }
        }

        tilemap.SetTilesBlock(bounds, tiles);
        tilemap.CompressBounds();

        if (verbose) Log.info($"[DefaultWorldTerrain] Built {width}x{height} visual terrain.");
    }

    private bool IsDirt(int x, int y)
    {
        float offsetX = terrainSeed * 0.173f;
        float offsetY = terrainSeed * 0.317f;
        float noise = Mathf.PerlinNoise((x + offsetX) * dirtPatchScale, (y + offsetY) * dirtPatchScale);
        return noise > 1f - dirtCoverage;
    }

    private void CreateRuntimeTiles(Sprite grassSprite, Sprite dirtSprite)
    {
        _grassTile = ScriptableObject.CreateInstance<Tile>();
        _grassTile.hideFlags = HideFlags.HideAndDontSave;
        _grassTile.sprite = grassSprite;

        _dirtTile = ScriptableObject.CreateInstance<Tile>();
        _dirtTile.hideFlags = HideFlags.HideAndDontSave;
        _dirtTile.sprite = dirtSprite;
    }

    private void ClearTerrain()
    {
        if (_terrainRoot != null) Destroy(_terrainRoot);
        if (_grassTile != null) Destroy(_grassTile);
        if (_dirtTile != null) Destroy(_dirtTile);
        _terrainRoot = null;
        _grassTile = null;
        _dirtTile = null;
    }

    private void OnDestroy() => ClearTerrain();

    private void OnValidate()
    {
        width = Mathf.Max(1, width);
        height = Mathf.Max(1, height);
        dirtPatchScale = Mathf.Max(0.001f, dirtPatchScale);
        grassTileId = grassTileId == null ? string.Empty : grassTileId.Trim();
        dirtTileId = dirtTileId == null ? string.Empty : dirtTileId.Trim();
    }
}
