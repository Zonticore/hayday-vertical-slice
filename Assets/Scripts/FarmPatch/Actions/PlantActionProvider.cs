public sealed class PlantActionProvider : FarmPatchActionProviderBase
{
    public override void CollectActions(ContextActionCollection collection)
    {
        if (Patch != null && Patch.CanPlant)
        {
            AddToolAction(collection);
        }
    }
}
