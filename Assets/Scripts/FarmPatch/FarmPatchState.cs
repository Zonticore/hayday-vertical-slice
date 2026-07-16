using System;
using UnityEngine;

public sealed class FarmPatchState : MonoBehaviour, IPlantable, IHarvestable, ISpeedUpTarget
{
    private FarmPatchStage _stage = FarmPatchStage.Empty;
    private string _cropId = string.Empty;
    private DateTime _plantedAtUtc;
    private DateTime _readyAtUtc;

    public event Action<FarmPatchState> Planted;
    public event Action<FarmPatchState> Matured;
    public event Action<FarmPatchState, string> Harvested;

    public FarmPatchStage Stage => _stage;
    public string CropId => _cropId;
    public DateTime PlantedAtUtc => _plantedAtUtc;
    public DateTime CompletionTimeUtc => _readyAtUtc;
    public bool CanPlant => _stage == FarmPatchStage.Empty;
    public bool CanHarvest => _stage == FarmPatchStage.Mature;
    public bool CanSpeedUp => _stage == FarmPatchStage.Growing;

    private void Update()
    {
        if (_stage == FarmPatchStage.Growing && DateTime.UtcNow >= _readyAtUtc)
        {
            SetMature();
        }
    }

    public bool TryPlant(string cropId, TimeSpan growthDuration)
    {
        if (!CanPlant || string.IsNullOrWhiteSpace(cropId) || growthDuration < TimeSpan.Zero)
        {
            return false;
        }

        _cropId = cropId;
        _plantedAtUtc = DateTime.UtcNow;
        _readyAtUtc = _plantedAtUtc.Add(growthDuration);
        _stage = growthDuration == TimeSpan.Zero
            ? FarmPatchStage.Mature
            : FarmPatchStage.Growing;

        Planted?.Invoke(this);

        if (_stage == FarmPatchStage.Mature)
        {
            Matured?.Invoke(this);
        }

        return true;
    }

    public bool TryHarvest()
    {
        if (!CanHarvest)
        {
            return false;
        }

        string harvestedCropId = _cropId;
        ResetToEmpty();
        Harvested?.Invoke(this, harvestedCropId);
        return true;
    }

    public bool TryCompleteNow()
    {
        if (!CanSpeedUp)
        {
            return false;
        }

        _readyAtUtc = DateTime.UtcNow;
        SetMature();
        return true;
    }

    public FarmPatchSaveData CreateSaveData()
    {
        return new FarmPatchSaveData
        {
            stage = _stage,
            cropId = _cropId,
            plantedAtUtcTicks = _plantedAtUtc.Ticks,
            readyAtUtcTicks = _readyAtUtc.Ticks
        };
    }

    public void Restore(FarmPatchSaveData data)
    {
        if (data == null)
        {
            ResetToEmpty();
            return;
        }

        _stage = data.stage;
        _cropId = data.cropId ?? string.Empty;
        _plantedAtUtc = FromTicksOrDefault(data.plantedAtUtcTicks);
        _readyAtUtc = FromTicksOrDefault(data.readyAtUtcTicks);

        if (_stage == FarmPatchStage.Growing && DateTime.UtcNow >= _readyAtUtc)
        {
            _stage = FarmPatchStage.Mature;
        }
    }

    private void SetMature()
    {
        if (_stage != FarmPatchStage.Growing)
        {
            return;
        }

        _stage = FarmPatchStage.Mature;
        Matured?.Invoke(this);
    }

    private void ResetToEmpty()
    {
        _stage = FarmPatchStage.Empty;
        _cropId = string.Empty;
        _plantedAtUtc = default(DateTime);
        _readyAtUtc = default(DateTime);
    }

    private static DateTime FromTicksOrDefault(long ticks)
    {
        return ticks > 0
            ? new DateTime(ticks, DateTimeKind.Utc)
            : default(DateTime);
    }
}
