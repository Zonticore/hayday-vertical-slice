using UnityEngine;

[CreateAssetMenu(menuName = "Farm/Tiles/Factories/Default Tile", fileName = "Factory_DefaultTile")]
public sealed class DefaultTileFactorySO : TileFactorySO
{
    protected override void Configure(
        TileInstance instance,
        TileDefinitionSO definition,
        TileBuildContext context)
    {
    }
}
