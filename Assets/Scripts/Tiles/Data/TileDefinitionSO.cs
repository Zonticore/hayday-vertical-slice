using UnityEngine;

[CreateAssetMenu(menuName = "Farm/Tiles/Tile Definition", fileName = "TileDefinition_")]
public sealed class TileDefinitionSO : ScriptableObject
{
    [SerializeField] private string tileId;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite sprite;
    [SerializeField] private TileCategory category;
    [SerializeField] private Vector2Int footprint = Vector2Int.one;
    [SerializeField] private TileFactorySO factory;

    [Header("Store")]
    [SerializeField, Min(0)] private int purchaseCost = 10;

    public string TileId => tileId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName)
        ? GetFallbackDisplayName()
        : displayName;
    public Sprite Sprite => sprite;
    public TileCategory Category => category;
    public Vector2Int Footprint => footprint;
    public TileFactorySO Factory => factory;
    public int PurchaseCost => purchaseCost;

    private void OnValidate()
    {
        tileId = tileId == null ? string.Empty : tileId.Trim();
        displayName = displayName == null ? string.Empty : displayName.Trim();
        footprint.x = Mathf.Max(1, footprint.x);
        footprint.y = Mathf.Max(1, footprint.y);
        purchaseCost = Mathf.Max(0, purchaseCost);
    }

    private string GetFallbackDisplayName()
    {
        if (string.IsNullOrWhiteSpace(tileId))
        {
            return "Building";
        }

        string value = tileId.Replace('_', ' ').Trim();
        return value.Length == 0
            ? "Building"
            : char.ToUpperInvariant(value[0]) + value.Substring(1);
    }
}
