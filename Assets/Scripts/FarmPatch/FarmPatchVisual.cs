using System;
using System.Collections;
using UnityEngine;

public sealed class FarmPatchVisual : MonoBehaviour
{
    private FarmPatchState _patch;
    private SpriteRenderer _renderer;
    private Sprite _emptySprite;
    private Sprite _plantedSprite;
    private Sprite _earlySprite;
    private Sprite _middleSprite;
    private Sprite _matureSprite;
    private Sprite _harvestedSprite;
    private float _harvestedDisplaySeconds;
    private Coroutine _harvestedRoutine;

    public void Initialize(
        FarmPatchState patch,
        SpriteRenderer spriteRenderer,
        Sprite emptySprite,
        Sprite plantedSprite,
        Sprite earlySprite,
        Sprite middleSprite,
        Sprite matureSprite,
        Sprite harvestedSprite,
        float harvestedDisplaySeconds)
    {
        Unsubscribe();
        _patch = patch;
        _renderer = spriteRenderer;
        _emptySprite = emptySprite;
        _plantedSprite = plantedSprite;
        _earlySprite = earlySprite;
        _middleSprite = middleSprite;
        _matureSprite = matureSprite;
        _harvestedSprite = harvestedSprite;
        _harvestedDisplaySeconds = Mathf.Max(0f, harvestedDisplaySeconds);
        Subscribe();
        RefreshVisual();
    }

    private void Update()
    {
        if (_patch != null && _patch.Stage == FarmPatchStage.Growing)
        {
            RefreshGrowingVisual();
        }
    }

    private void Subscribe()
    {
        if (_patch == null)
        {
            return;
        }

        _patch.Planted += HandlePlanted;
        _patch.Matured += HandleMatured;
        _patch.Harvested += HandleHarvested;
    }

    private void Unsubscribe()
    {
        if (_patch == null)
        {
            return;
        }

        _patch.Planted -= HandlePlanted;
        _patch.Matured -= HandleMatured;
        _patch.Harvested -= HandleHarvested;
    }

    private void HandlePlanted(FarmPatchState patch)
    {
        StopHarvestedRoutine();
        RefreshVisual();
    }

    private void HandleMatured(FarmPatchState patch)
    {
        StopHarvestedRoutine();
        SetSprite(_matureSprite);
    }

    private void HandleHarvested(FarmPatchState patch, string cropId)
    {
        StopHarvestedRoutine();

        if (_harvestedSprite == null || _harvestedDisplaySeconds <= 0f)
        {
            SetSprite(_emptySprite);
            return;
        }

        SetSprite(_harvestedSprite);
        _harvestedRoutine = StartCoroutine(ShowHarvestedThenEmpty());
    }

    private IEnumerator ShowHarvestedThenEmpty()
    {
        yield return new WaitForSeconds(_harvestedDisplaySeconds);
        _harvestedRoutine = null;
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (_patch == null)
        {
            return;
        }

        switch (_patch.Stage)
        {
            case FarmPatchStage.Growing:
                RefreshGrowingVisual();
                break;
            case FarmPatchStage.Mature:
                SetSprite(_matureSprite);
                break;
            default:
                SetSprite(_emptySprite);
                break;
        }
    }

    private void RefreshGrowingVisual()
    {
        DateTime planted = _patch.PlantedAtUtc;
        DateTime completion = _patch.CompletionTimeUtc;
        double totalSeconds = (completion - planted).TotalSeconds;
        double elapsedSeconds = (DateTime.UtcNow - planted).TotalSeconds;
        float progress = totalSeconds <= 0d
            ? 1f
            : Mathf.Clamp01((float)(elapsedSeconds / totalSeconds));

        if (progress < 0.2f)
        {
            SetSprite(_plantedSprite);
        }
        else if (progress < 0.6f)
        {
            SetSprite(_earlySprite);
        }
        else
        {
            SetSprite(_middleSprite);
        }
    }

    private void SetSprite(Sprite sprite)
    {
        if (_renderer != null && sprite != null && _renderer.sprite != sprite)
        {
            _renderer.sprite = sprite;
        }
    }

    private void StopHarvestedRoutine()
    {
        if (_harvestedRoutine == null)
        {
            return;
        }

        StopCoroutine(_harvestedRoutine);
        _harvestedRoutine = null;
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }
}
