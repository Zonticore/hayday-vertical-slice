using UnityEngine;

[CreateAssetMenu(menuName = "Farm/Tools/Harvest Tool", fileName = "Tool_Harvest")]
public sealed class HarvestToolSO : TileToolSO
{
    public override bool CanApply(ToolUseContext context)
    {
        return ComponentInterfaceUtility.TryGetInterface(
                   context.Target,
                   out IHarvestable harvestable) &&
               harvestable.CanHarvest;
    }

    public override bool Apply(ToolUseContext context)
    {
        return ComponentInterfaceUtility.TryGetInterface(
                   context.Target,
                   out IHarvestable harvestable) &&
               harvestable.TryHarvest();
    }
}
