using UnityEngine;

[CreateAssetMenu(
    menuName = "Farm/Tiles/Factories/Request Board",
    fileName = "Factory_RequestBoard")]
public sealed class RequestBoardTileFactorySO : TileFactorySO
{
    [SerializeField] private string screenId = RequestBoardScreen.ScreenId;

    protected override void Configure(
        TileInstance instance,
        TileDefinitionSO definition,
        TileBuildContext context)
    {
        GameObject target = instance.gameObject;
        target.AddComponent<PointerInteractable>();

        RequestBoardTileInteraction interaction =
            target.AddComponent<RequestBoardTileInteraction>();
        interaction.Initialize(screenId);
    }

    private void OnValidate()
    {
        screenId = string.IsNullOrWhiteSpace(screenId)
            ? RequestBoardScreen.ScreenId
            : screenId.Trim();
    }
}
