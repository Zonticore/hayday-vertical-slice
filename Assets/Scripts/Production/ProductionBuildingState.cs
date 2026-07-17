using System;
using UnityEngine;

public sealed class ProductionBuildingState : MonoBehaviour, ISpeedUpTarget
{
    private ProductionRecipeSO _recipe;
    private Vector3 _collectionOffset = new Vector3(0f, 0.5f, 0f);
    private ProductionPhase _phase;
    private DateTime _completionTimeUtc;
    private int _pendingOutputAmount;
    private bool _claimInProgress;

    public event Action<ProductionBuildingState> StateChanged;

    public ProductionRecipeSO Recipe => _recipe;
    public ProductionPhase Phase => _phase;
    public DateTime CompletionTimeUtc => _completionTimeUtc;
    public int PendingOutputAmount => _pendingOutputAmount;
    public bool IsClaimInProgress => _claimInProgress;
    public bool CanSpeedUp => _phase == ProductionPhase.Producing;

    public bool CanStart
    {
        get
        {
            if (_phase != ProductionPhase.Idle || _recipe == null)
            {
                return false;
            }

            UserModel user = UserModel.GetOrCreate();
            ItemStorageModel input = user.GetStorage(_recipe.InputStorage);
            ItemStorageModel output = user.GetStorage(_recipe.OutputStorage);
            return input != null &&
                   output != null &&
                   input.GetQuantity(_recipe.InputItemId) >= _recipe.InputAmount &&
                   output.RemainingCapacity >= _recipe.OutputAmount;
        }
    }

    public bool CanClaim
    {
        get
        {
            if (_phase != ProductionPhase.Ready ||
                _recipe == null ||
                _pendingOutputAmount <= 0 ||
                _claimInProgress)
            {
                return false;
            }

            ItemStorageModel output = UserModel.GetOrCreate().GetStorage(
                _recipe.OutputStorage);
            return output != null && output.RemainingCapacity > 0;
        }
    }

    public void Initialize(
        ProductionRecipeSO recipe,
        Vector3 collectionOffset)
    {
        _recipe = recipe;
        _collectionOffset = collectionOffset;
    }

    private void Update()
    {
        if (_phase == ProductionPhase.Producing &&
            DateTime.UtcNow >= _completionTimeUtc)
        {
            CompleteProduction();
        }
    }

    public bool TryStart()
    {
        if (!CanStart)
        {
            return false;
        }

        UserModel user = UserModel.GetOrCreate();
        if (!user.TryRemoveItem(
                _recipe.InputStorage,
                _recipe.InputItemId,
                _recipe.InputAmount))
        {
            return false;
        }

        _pendingOutputAmount = 0;
        _completionTimeUtc = DateTime.UtcNow.AddSeconds(
            _recipe.ProductionSeconds);
        _phase = _recipe.ProductionSeconds <= 0f
            ? ProductionPhase.Ready
            : ProductionPhase.Producing;

        if (_phase == ProductionPhase.Ready)
        {
            _pendingOutputAmount = _recipe.OutputAmount;
        }

        StateChanged?.Invoke(this);
        return true;
    }

    public bool TryCompleteNow()
    {
        if (!CanSpeedUp)
        {
            return false;
        }

        CompleteProduction();
        return true;
    }

    public bool TryClaim()
    {
        if (!CanClaim)
        {
            return false;
        }

        ItemStorageModel output = UserModel.GetOrCreate().GetStorage(
            _recipe.OutputStorage);
        int claimAmount = Mathf.Min(
            _pendingOutputAmount,
            output.RemainingCapacity);
        if (claimAmount <= 0)
        {
            return false;
        }

        _claimInProgress = true;
        StateChanged?.Invoke(this);

        StorageHudDisplay display = StorageHudDisplay.instance;
        if (display != null)
        {
            display.FlyItem(
                _recipe.OutputSprite,
                transform.position + _collectionOffset,
                _recipe.OutputStorage,
                () => CompleteClaim(claimAmount));
        }
        else
        {
            CompleteClaim(claimAmount);
        }

        return true;
    }

    private void CompleteProduction()
    {
        if (_phase != ProductionPhase.Producing || _recipe == null)
        {
            return;
        }

        _phase = ProductionPhase.Ready;
        _completionTimeUtc = DateTime.UtcNow;
        _pendingOutputAmount = _recipe.OutputAmount;
        StateChanged?.Invoke(this);
    }

    private void CompleteClaim(int requestedAmount)
    {
        UserModel user = UserModel.GetOrCreate();
        int accepted = user.AddItem(
            _recipe.OutputStorage,
            _recipe.OutputItemId,
            requestedAmount);
        _pendingOutputAmount -= accepted;
        _claimInProgress = false;

        if (_pendingOutputAmount <= 0)
        {
            _pendingOutputAmount = 0;
            _phase = ProductionPhase.Idle;
            _completionTimeUtc = default(DateTime);
        }

        StateChanged?.Invoke(this);
    }
}
