using System;

[Serializable]
public sealed class FarmPatchSaveData
{
    public FarmPatchStage stage;
    public string cropId;
    public long plantedAtUtcTicks;
    public long readyAtUtcTicks;
}
