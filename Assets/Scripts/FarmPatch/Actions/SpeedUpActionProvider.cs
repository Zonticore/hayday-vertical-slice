public sealed class SpeedUpActionProvider : FarmPatchActionProviderBase
{
    public override void CollectActions(ContextActionCollection collection)
    {
        if (Patch != null && Patch.CanSpeedUp)
        {
            AddToolAction(collection);
        }
    }
}
