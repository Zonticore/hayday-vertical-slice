public interface IHarvestable
{
    bool CanHarvest { get; }
    bool TryHarvest();
}
