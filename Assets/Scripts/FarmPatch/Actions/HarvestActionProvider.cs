public sealed class HarvestActionProvider : FarmPatchActionProviderBase
{
    public override void CollectActions(ContextActionCollection collection)
    {
        if (Patch != null && Patch.CanHarvest)
        {
            AddToolAction(collection);
        }
    }
}
