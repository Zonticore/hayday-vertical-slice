using UnityEngine;

public sealed class FarmPatchHarvestRewardSpawner : MonoBehaviour
{
    private FarmPatchState _patch;
    private SpriteRenderer _patchRenderer;
    private Sprite _rewardSprite;
    private int _rewardAmount;
    private float _rewardLifetimeSeconds;
    private float _rewardHopHeight;
    private Vector3 _rewardOffset;
    private int _experienceReward;

    public void Initialize(
        FarmPatchState patch,
        SpriteRenderer patchRenderer,
        Sprite rewardSprite,
        int rewardAmount,
        float rewardLifetimeSeconds,
        float rewardHopHeight,
        Vector3 rewardOffset,
        int experienceReward)
    {
        Unsubscribe();
        _patch = patch;
        _patchRenderer = patchRenderer;
        _rewardSprite = rewardSprite;
        _rewardAmount = Mathf.Max(1, rewardAmount);
        _rewardLifetimeSeconds = Mathf.Max(0.05f, rewardLifetimeSeconds);
        _rewardHopHeight = Mathf.Max(0f, rewardHopHeight);
        _rewardOffset = rewardOffset;
        _experienceReward = Mathf.Max(0, experienceReward);
        Subscribe();
    }

    private void Subscribe()
    {
        if (_patch != null)
        {
            _patch.Harvested += HandleHarvested;
        }
    }

    private void Unsubscribe()
    {
        if (_patch != null)
        {
            _patch.Harvested -= HandleHarvested;
        }
    }

    private void HandleHarvested(FarmPatchState patch, string cropId)
    {
        if (string.IsNullOrWhiteSpace(cropId))
        {
            return;
        }

        var rewardObject = new GameObject($"HarvestReward_{cropId}");
        rewardObject.transform.SetParent(transform.parent, true);
        rewardObject.transform.position = transform.position + _rewardOffset;

        SpriteRenderer rewardRenderer = rewardObject.AddComponent<SpriteRenderer>();
        rewardRenderer.sprite = _rewardSprite;
        if (_patchRenderer != null)
        {
            rewardRenderer.sortingLayerID = _patchRenderer.sortingLayerID;
            rewardRenderer.sortingOrder = _patchRenderer.sortingOrder + 10;
        }

        HarvestRewardItem reward = rewardObject.AddComponent<HarvestRewardItem>();
        reward.Initialize(
            cropId,
            _rewardAmount,
            StorageType.Grain,
            _rewardLifetimeSeconds,
            _rewardHopHeight);

        XpDisplay xpDisplay = XpDisplay.instance;
        if (xpDisplay != null && _experienceReward > 0)
        {
            xpDisplay.GainExperience(_experienceReward, rewardObject.transform.position);
        }
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }
}
