using UnityEngine;

[CreateAssetMenu(
    menuName = "Farm/Tiles/Factories/Chicken Coop",
    fileName = "Factory_ChickenCoop")]
public sealed class ChickenCoopTileFactorySO : TileFactorySO
{
    [Header("Items")]
    [SerializeField] private ItemDefinitionSO chickenItem;
    [SerializeField] private ItemDefinitionSO feedItem;
    [SerializeField] private ItemDefinitionSO eggItem;

    [Header("Production")]
    [SerializeField, Min(1)] private int maxChickens = 6;
    [SerializeField, Min(0f)] private float productionSeconds = 15f;

    [Header("Visuals")]
    [SerializeField] private Sprite producingChickenSprite;
    [SerializeField] private Sprite emptyTroughSprite;
    [SerializeField] private Sprite filledTroughSprite;
    [SerializeField] private Vector3 troughOffset = new Vector3(2.6f, -0.65f, 0f);
    [SerializeField] private Vector3 troughScale = new Vector3(0.55f, 0.55f, 1f);
    [SerializeField] private Vector3 eggCollectionOffset = new Vector3(0f, 0.35f, 0f);
    [SerializeField] private Vector3[] chickenOffsets;

    [Header("Context Actions")]
    [SerializeField] private ContextActionDefinitionSO feedAction;
    [SerializeField] private ContextActionDefinitionSO claimAction;

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
            chickenItem,
            feedItem,
            eggItem,
            maxChickens,
            productionSeconds,
            eggCollectionOffset);

        ChickenCoopVisual visual = target.AddComponent<ChickenCoopVisual>();
        visual.Initialize(
            state,
            instance.SpriteRenderer,
            definition.Sprite,
            chickenItem != null ? chickenItem.Sprite : null,
            producingChickenSprite,
            eggItem != null ? eggItem.Sprite : null,
            emptyTroughSprite,
            filledTroughSprite,
            troughOffset,
            troughScale,
            chickenOffsets);

        ChickenCoopActionProvider actions =
            target.AddComponent<ChickenCoopActionProvider>();
        actions.Initialize(state, feedAction, claimAction);
    }

    private void OnValidate()
    {
        maxChickens = Mathf.Max(1, maxChickens);
        productionSeconds = Mathf.Max(0f, productionSeconds);
    }
}
