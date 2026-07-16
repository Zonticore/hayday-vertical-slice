using UnityEngine;

[CreateAssetMenu(menuName = "Farm/Tiles/Factories/Farm Patch", fileName = "Factory_FarmPatch")]
public sealed class FarmPatchTileFactorySO : TileFactorySO
{
    [Header("Context Actions")]
    [SerializeField] private ContextActionDefinitionSO plantAction;
    [SerializeField] private ContextActionDefinitionSO harvestAction;
    [SerializeField] private ContextActionDefinitionSO speedUpAction;

    [Header("Patch Visuals")]
    [SerializeField] private Sprite emptyPatchSprite;
    [SerializeField] private Sprite plantedWheatSprite;
    [SerializeField] private Sprite earlyWheatSprite;
    [SerializeField] private Sprite middleWheatSprite;
    [SerializeField] private Sprite matureWheatSprite;
    [SerializeField] private Sprite harvestedPatchSprite;
    [SerializeField, Min(0f)] private float harvestedDisplaySeconds = 0.35f;

    [Header("Harvest Reward")]
    [SerializeField] private Sprite harvestRewardSprite;
    [SerializeField, Min(1)] private int harvestRewardAmount = 2;
    [SerializeField, Min(0.05f)] private float rewardLifetimeSeconds = 0.8f;
    [SerializeField, Min(0f)] private float rewardHopHeight = 0.35f;
    [SerializeField] private Vector3 rewardOffset = new Vector3(0f, 0.35f, 0f);

    protected override void Configure(
        TileInstance instance,
        TileDefinitionSO definition,
        TileBuildContext context)
    {
        GameObject target = instance.gameObject;

        target.AddComponent<PointerInteractable>();
        target.AddComponent<ContextActionSource>();

        FarmPatchState patch = target.AddComponent<FarmPatchState>();

        FarmPatchVisual visual = target.AddComponent<FarmPatchVisual>();
        visual.Initialize(
            patch,
            instance.SpriteRenderer,
            emptyPatchSprite != null ? emptyPatchSprite : definition.Sprite,
            plantedWheatSprite,
            earlyWheatSprite,
            middleWheatSprite,
            matureWheatSprite,
            harvestedPatchSprite,
            harvestedDisplaySeconds);

        FarmPatchHarvestRewardSpawner rewardSpawner =
            target.AddComponent<FarmPatchHarvestRewardSpawner>();
        rewardSpawner.Initialize(
            patch,
            instance.SpriteRenderer,
            harvestRewardSprite,
            harvestRewardAmount,
            rewardLifetimeSeconds,
            rewardHopHeight,
            rewardOffset);

        PlantActionProvider planting = target.AddComponent<PlantActionProvider>();
        planting.Initialize(patch, plantAction);

        HarvestActionProvider harvesting = target.AddComponent<HarvestActionProvider>();
        harvesting.Initialize(patch, harvestAction);

        SpeedUpActionProvider speedUp = target.AddComponent<SpeedUpActionProvider>();
        speedUp.Initialize(patch, speedUpAction);
    }

    private void OnValidate()
    {
        harvestedDisplaySeconds = Mathf.Max(0f, harvestedDisplaySeconds);
        harvestRewardAmount = Mathf.Max(1, harvestRewardAmount);
        rewardLifetimeSeconds = Mathf.Max(0.05f, rewardLifetimeSeconds);
        rewardHopHeight = Mathf.Max(0f, rewardHopHeight);
    }
}
