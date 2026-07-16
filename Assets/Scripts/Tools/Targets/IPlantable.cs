using System;

public interface IPlantable
{
    bool CanPlant { get; }
    bool TryPlant(string cropId, TimeSpan growthDuration);
}
