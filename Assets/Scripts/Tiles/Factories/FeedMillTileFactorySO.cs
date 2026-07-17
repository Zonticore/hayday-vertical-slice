using UnityEngine;

[CreateAssetMenu(
    menuName = "Farm/Tiles/Factories/Feed Mill",
    fileName = "Factory_FeedMill")]
public sealed class FeedMillTileFactorySO : TileFactorySO
{
    [Header("Recipe")]
    [SerializeField] private ProductionRecipeSO chickenFeedRecipe;

    [Header("Context Actions")]
    [SerializeField] private ContextActionDefinitionSO makeFeedAction;
    [SerializeField] private ContextActionDefinitionSO collectFeedAction;
    [SerializeField] private ContextActionDefinitionSO speedUpAction;

    [Header("Visuals")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite workingSprite;
    [SerializeField] private Sprite readySprite;
    [SerializeField] private Vector3 outputOffset = new Vector3(0f, 0.55f, 0f);
    [SerializeField, Min(0.01f)] private float outputScale = 0.2f;

    protected override void Configure(
        TileInstance instance,
        TileDefinitionSO definition,
        TileBuildContext context)
    {
        GameObject target = instance.gameObject;
        target.AddComponent<PointerInteractable>();
        target.AddComponent<ContextActionSource>();

        ProductionBuildingState state =
            target.AddComponent<ProductionBuildingState>();
        state.Initialize(chickenFeedRecipe, outputOffset);

        ProductionBuildingVisual visual =
            target.AddComponent<ProductionBuildingVisual>();
        visual.Initialize(
            state,
            instance.SpriteRenderer,
            idleSprite != null ? idleSprite : definition.Sprite,
            workingSprite,
            readySprite,
            chickenFeedRecipe != null ? chickenFeedRecipe.OutputSprite : null,
            outputOffset,
            outputScale);

        ProductionBuildingActionProvider actions =
            target.AddComponent<ProductionBuildingActionProvider>();
        actions.Initialize(
            state,
            makeFeedAction,
            collectFeedAction,
            speedUpAction);
    }

    private void OnValidate()
    {
        outputScale = Mathf.Max(0.01f, outputScale);
    }
}
