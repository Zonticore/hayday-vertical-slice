using UnityEngine;

[CreateAssetMenu(
    menuName = "Farm/Tiles/Factories/Chicken Coop",
    fileName = "Factory_ChickenCoop")]
public sealed class ChickenCoopTileFactorySO : TileFactorySO
{
    [Header("Items")]
    [SerializeField] private string chickenItemId = "chicken";
    [SerializeField] private string feedItemId = "chicken_feed";
    [SerializeField] private string eggItemId = "egg";

    [Header("Production")]
    [SerializeField, Min(1)] private int maxChickens = 6;
    [SerializeField, Min(0f)] private float productionSeconds = 15f;

    [Header("Visuals")]
    [SerializeField] private Sprite fedCoopSprite;
    [SerializeField] private Sprite chickenSprite;
    [SerializeField] private Sprite eggSprite;
    [SerializeField] private Vector3 eggCollectionOffset = new Vector3(0f, 0.35f, 0f);
    [SerializeField] private Vector3[] chickenOffsets;

    [Header("Context Icons")]
    [SerializeField] private Sprite feedActionIcon;
    [SerializeField] private Sprite claimActionIcon;

    public Sprite ChickenSprite => chickenSprite;

    protected override void Configure(
        TileInstance instance,
        TileDefinitionSO definition,
        TileBuildContext context)
    {
        GameObject target = instance.gameObject;
        target.AddComponent<PointerInteractable>();
        target.AddComponent<ContextActionSource>();

        ChickenCoopState state = target.AddComponent<ChickenCoopState>();
        state.Initialize(
            chickenItemId,
            feedItemId,
            eggItemId,
            maxChickens,
            productionSeconds,
            eggSprite,
            eggCollectionOffset);

        ChickenCoopVisual visual = target.AddComponent<ChickenCoopVisual>();
        visual.Initialize(
            state,
            instance.SpriteRenderer,
            definition.Sprite,
            fedCoopSprite,
            chickenSprite,
            eggSprite,
            chickenOffsets);

        ChickenCoopActionProvider actions =
            target.AddComponent<ChickenCoopActionProvider>();
        actions.Initialize(state, feedActionIcon, claimActionIcon);
    }

    private void OnValidate()
    {
        chickenItemId = chickenItemId == null ? string.Empty : chickenItemId.Trim();
        feedItemId = feedItemId == null ? string.Empty : feedItemId.Trim();
        eggItemId = eggItemId == null ? string.Empty : eggItemId.Trim();
        maxChickens = Mathf.Max(1, maxChickens);
        productionSeconds = Mathf.Max(0f, productionSeconds);
    }
}
