using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class BuildingPlacementController : MonoBehaviourSingleton<BuildingPlacementController>
{
    private const bool verbose = false;

    [Header("Preview")]
    [SerializeField] private Color validColor = new Color(0.65f, 1f, 0.65f, 0.85f);
    [SerializeField] private Color invalidColor = new Color(1f, 0.45f, 0.45f, 0.75f);
    [SerializeField] private int sortingOrderOffset = 1;

    private StoreItemData _item;
    private TileDefinitionSO _definition;
    private GridService _grid;
    private TileBuildService _buildService;
    private Camera _worldCamera;
    private GameObject _previewObject;
    private SpriteRenderer _previewRenderer;
    private Vector2Int _gridPosition;
    private bool _isValidPlacement;

    public event Action<TileInstance, StoreItemData> BuildingPlaced;

    public bool IsPlacing => _item != null && _previewObject != null;

    public static BuildingPlacementController GetOrCreate()
    {
        BuildingPlacementController current = instance;
        if (current != null)
        {
            return current;
        }

        var placementObject = new GameObject(nameof(BuildingPlacementController));
        return placementObject.AddComponent<BuildingPlacementController>();
    }

    public bool BeginPlacement(StoreItemData item, Vector2 screenPosition)
    {
        if (item == null || !item.IsBuilding)
        {
            return false;
        }

        ClearPlacement();

        RegistryService registries = RegistryService.instance;
        _grid = GridService.instance;
        _buildService = TileBuildService.instance;
        _worldCamera = Camera.main;

        if (registries == null ||
            registries.TileRegistry == null ||
            !registries.TileRegistry.TryGet(item.BuildingTileId, out TileDefinitionSO definition) ||
            definition == null ||
            definition.Factory == null ||
            _grid == null ||
            _buildService == null ||
            _worldCamera == null)
        {
            ClearPlacement();
            if (verbose) Log.warning($"[BuildingPlacementController] Could not start placement for '{item.ItemId}'.");
            return false;
        }

        UserModel user = UserModel.GetOrCreate();
        if (user.Coins < item.Cost)
        {
            ClearPlacement();
            if (verbose) Log.info($"[BuildingPlacementController] Not enough coins for '{item.ItemId}'.");
            return false;
        }

        _item = item;
        _definition = definition;
        CreatePreview();
        RefreshPreview(screenPosition);
        return IsPlacing;
    }

    public void CancelPlacement()
    {
        ClearPlacement();
    }

    private void Update()
    {
        if (!IsPlacing)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CancelPlacement();
            return;
        }

        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            CancelPlacement();
            return;
        }

        Pointer pointer = Pointer.current;
        if (pointer == null)
        {
            CancelPlacement();
            return;
        }

        Vector2 screenPosition = pointer.position.ReadValue();
        RefreshPreview(screenPosition);

        if (pointer.press.wasReleasedThisFrame)
        {
            TryCommitPlacement();
        }
    }

    private void CreatePreview()
    {
        _previewObject = new GameObject($"BuildingPreview_{_definition.TileId}");
        _previewRenderer = _previewObject.AddComponent<SpriteRenderer>();
        _previewRenderer.sprite = _definition.Sprite;
    }

    private void RefreshPreview(Vector2 screenPosition)
    {
        if (_previewObject == null || _definition == null || _grid == null)
        {
            _isValidPlacement = false;
            return;
        }

        Vector3 worldPosition = ScreenToWorld(screenPosition);
        _gridPosition = _grid.WorldToCell(worldPosition);
        _previewObject.transform.position = _grid.GetFootprintWorldCenter(
            _gridPosition,
            _definition.Footprint,
            TileOrientation.North);

        UserModel user = UserModel.GetOrCreate();
        UISystem ui = UISystem.instance;
        bool pointerOverUi = ui != null && ui.IsPointerOverUi(screenPosition);
        _isValidPlacement = !pointerOverUi &&
                            user.Coins >= _item.Cost &&
                            _grid.CanPlace(
                                _gridPosition,
                                _definition.Footprint,
                                TileOrientation.North);

        if (_previewRenderer != null)
        {
            _previewRenderer.color = _isValidPlacement ? validColor : invalidColor;
            _previewRenderer.sortingOrder =
                -Mathf.RoundToInt(_previewObject.transform.position.y * 100f) +
                sortingOrderOffset;
        }
    }

    private void TryCommitPlacement()
    {
        if (!_isValidPlacement || _item == null || _buildService == null)
        {
            ClearPlacement();
            return;
        }

        StoreItemData purchasedItem = _item;
        UserModel user = UserModel.GetOrCreate();
        if (!user.TrySpendCoins(purchasedItem.Cost))
        {
            ClearPlacement();
            return;
        }

        TileBuildResult result = _buildService.TryBuild(new TileBuildRequest(
            purchasedItem.BuildingTileId,
            _gridPosition,
            TileOrientation.North));

        if (!result.Succeeded)
        {
            user.AddCoins(purchasedItem.Cost);
            if (verbose)
            {
                Log.warning($"[BuildingPlacementController] Placement failed: {result.Message}");
            }

            ClearPlacement();
            return;
        }

        TileInstance tile = result.Tile;
        ClearPlacement();
        BuildingPlaced?.Invoke(tile, purchasedItem);
    }

    private Vector3 ScreenToWorld(Vector2 screenPosition)
    {
        float distance = Mathf.Abs(_worldCamera.transform.position.z);
        Vector3 world = _worldCamera.ScreenToWorldPoint(new Vector3(
            screenPosition.x,
            screenPosition.y,
            distance));
        world.z = 0f;
        return world;
    }

    private void ClearPlacement()
    {
        if (_previewObject != null)
        {
            Destroy(_previewObject);
        }

        _item = null;
        _definition = null;
        _grid = null;
        _buildService = null;
        _worldCamera = null;
        _previewObject = null;
        _previewRenderer = null;
        _isValidPlacement = false;
    }

    private void OnDisable()
    {
        ClearPlacement();
    }

    private void OnValidate()
    {
        sortingOrderOffset = Mathf.Max(0, sortingOrderOffset);
    }
}
