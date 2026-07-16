using UnityEngine;

[CreateAssetMenu(menuName = "Farm/Tiles/Tile Definition", fileName = "TileDefinition_")]
public sealed class TileDefinitionSO : ScriptableObject
{
    [SerializeField] private string tileId;
    [SerializeField] private Sprite sprite;
    [SerializeField] private TileCategory category;
    [SerializeField] private Vector2Int footprint = Vector2Int.one;
    [SerializeField] private TileFactorySO factory;

    public string TileId => tileId;
    public Sprite Sprite => sprite;
    public TileCategory Category => category;
    public Vector2Int Footprint => footprint;
    public TileFactorySO Factory => factory;

    private void OnValidate()
    {
        tileId = tileId == null ? string.Empty : tileId.Trim();
        footprint.x = Mathf.Max(1, footprint.x);
        footprint.y = Mathf.Max(1, footprint.y);
    }
}
