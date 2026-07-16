using UnityEngine;

[CreateAssetMenu(menuName = "Farm/Tiles/Factories/Farm Patch", fileName = "Factory_FarmPatch")]
public sealed class FarmPatchTileFactorySO : TileFactorySO
{
    [Header("Context Actions")]
    [SerializeField] private ContextActionDefinitionSO plantAction;
    [SerializeField] private ContextActionDefinitionSO harvestAction;
    [SerializeField] private ContextActionDefinitionSO speedUpAction;

    protected override void Configure(
        TileInstance instance,
        TileDefinitionSO definition,
        TileBuildContext context)
    {
        GameObject target = instance.gameObject;

        target.AddComponent<PointerInteractable>();
        target.AddComponent<ContextActionSource>();

        FarmPatchState patch = target.AddComponent<FarmPatchState>();

        PlantActionProvider planting = target.AddComponent<PlantActionProvider>();
        planting.Initialize(patch, plantAction);

        HarvestActionProvider harvesting = target.AddComponent<HarvestActionProvider>();
        harvesting.Initialize(patch, harvestAction);

        SpeedUpActionProvider speedUp = target.AddComponent<SpeedUpActionProvider>();
        speedUp.Initialize(patch, speedUpAction);
    }
}
