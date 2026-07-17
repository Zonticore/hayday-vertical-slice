using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Farm/Tools/Plant Tool", fileName = "Tool_Plant_")]
public sealed class PlantToolSO : TileToolSO
{
    [Header("Crop")]
    [SerializeField] private ItemDefinitionSO crop;
    [SerializeField, Min(0f)] private float growthDurationSeconds = 60f;

    public ItemDefinitionSO Crop => crop;
    public TimeSpan GrowthDuration => TimeSpan.FromSeconds(growthDurationSeconds);

    public override bool CanApply(ToolUseContext context)
    {
        return HasValidConfiguration() &&
               ComponentInterfaceUtility.TryGetInterface(
                   context.Target,
                   out IPlantable plantable) &&
               plantable.CanPlant;
    }

    public override bool Apply(ToolUseContext context)
    {
        return HasValidConfiguration() &&
               ComponentInterfaceUtility.TryGetInterface(
                   context.Target,
                   out IPlantable plantable) &&
               plantable.TryPlant(crop.ItemId, GrowthDuration);
    }

    private bool HasValidConfiguration()
    {
        return crop != null && !string.IsNullOrWhiteSpace(crop.ItemId) &&
               growthDurationSeconds >= 0f &&
               !float.IsNaN(growthDurationSeconds) &&
               !float.IsInfinity(growthDurationSeconds);
    }

    private void OnValidate()
    {
        growthDurationSeconds = Mathf.Max(0f, growthDurationSeconds);
    }
}
