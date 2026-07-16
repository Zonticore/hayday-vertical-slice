using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Farm/Tools/Plant Tool", fileName = "Tool_Plant_")]
public sealed class PlantToolSO : TileToolSO
{
    [Header("Crop")]
    [SerializeField] private string cropId;
    [SerializeField, Min(0f)] private float growthDurationSeconds = 60f;

    public string CropId => cropId;
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
               plantable.TryPlant(cropId, GrowthDuration);
    }

    private bool HasValidConfiguration()
    {
        return !string.IsNullOrWhiteSpace(cropId) &&
               growthDurationSeconds >= 0f &&
               !float.IsNaN(growthDurationSeconds) &&
               !float.IsInfinity(growthDurationSeconds);
    }

    private void OnValidate()
    {
        cropId = cropId == null ? string.Empty : cropId.Trim();
        growthDurationSeconds = Mathf.Max(0f, growthDurationSeconds);
    }
}
