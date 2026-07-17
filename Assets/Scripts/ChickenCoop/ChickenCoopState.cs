using System;
using UnityEngine;

public sealed class ChickenCoopState : MonoBehaviour, IStoreItemDropTarget
{
    private const bool verbose = false;

    private ItemDefinitionSO _chickenItem;
    private ItemDefinitionSO _feedItem;
    private ItemDefinitionSO _eggItem;
    private int _maxChickens = 6;
    private TimeSpan _productionDuration = TimeSpan.FromSeconds(15d);
    private Vector3 _eggCollectionOffset = new Vector3(0f, 0.35f, 0f);

    private int _chickenCount;
    private int _fedChickenCount;
    private int _pendingEggs;
    private ChickenCoopPhase _phase;
    private DateTime _readyAtUtc;
    private bool _claimInProgress;

    public event Action<ChickenCoopState> StateChanged;
    public event Action<ChickenCoopState> ChickenCountChanged;

    public int ChickenCount => _chickenCount;
    public int FedChickenCount => _fedChickenCount;
    public int MaxChickens => _maxChickens;
    public int PendingEggs => _pendingEggs;
    public ChickenCoopPhase Phase => _phase;
    public DateTime ReadyAtUtc => _readyAtUtc;
    public bool CanAddChicken => _chickenCount < _maxChickens;
    public bool CanClaim =>
        _phase == ChickenCoopPhase.Ready &&
        _pendingEggs > 0 &&
        !_claimInProgress;
    public bool IsClaimInProgress => _claimInProgress;

    public bool CanAttemptFeed =>
        (_phase == ChickenCoopPhase.Idle ||
         _phase == ChickenCoopPhase.Producing) &&
        _chickenCount > 0 &&
        _fedChickenCount < _chickenCount;

    public bool CanFeed
    {
        get
        {
            if (!CanAttemptFeed)
            {
                return false;
            }

            if (_feedItem == null)
            {
                return false;
            }

            UserModel user = UserModel.GetOrCreate();
            return user.GetStorage(_feedItem.Storage)
                       .GetQuantity(_feedItem.ItemId) >= 1;
        }
    }

    public void Initialize(
        ItemDefinitionSO chickenItem,
        ItemDefinitionSO feedItem,
        ItemDefinitionSO eggItem,
        int maxChickens,
        float productionSeconds,
        Vector3 eggCollectionOffset)
    {
        _chickenItem = chickenItem;
        _feedItem = feedItem;
        _eggItem = eggItem;
        _maxChickens = Mathf.Max(1, maxChickens);
        _productionDuration = TimeSpan.FromSeconds(Mathf.Max(0f, productionSeconds));
        _eggCollectionOffset = eggCollectionOffset;
    }

    private void Update()
    {
        if (_phase == ChickenCoopPhase.Producing && DateTime.UtcNow >= _readyAtUtc)
        {
            _phase = ChickenCoopPhase.Ready;
            _pendingEggs = _fedChickenCount;
            StateChanged?.Invoke(this);

            if (verbose)
            {
                Log.info($"[ChickenCoopState] {_pendingEggs} eggs are ready.");
            }
        }
    }

    public bool CanAccept(StoreItemData item)
    {
        if (item == null || _chickenItem == null ||
            item.ItemId != _chickenItem.ItemId || !CanAddChicken)
        {
            return false;
        }

        UserModel user = UserModel.GetOrCreate();
        return user.Coins >= item.Cost;
    }

    public bool TryAccept(StoreItemData item)
    {
        if (!CanAccept(item))
        {
            return false;
        }

        UserModel user = UserModel.GetOrCreate();
        if (!user.TrySpendCoins(item.Cost))
        {
            return false;
        }

        _chickenCount++;
        ChickenCountChanged?.Invoke(this);
        StateChanged?.Invoke(this);

        if (verbose)
        {
            Log.info($"[ChickenCoopState] Purchased chicken {_chickenCount}/{_maxChickens}.");
        }

        return true;
    }

    public bool TryFeed()
    {
        if (!CanFeed)
        {
            return false;
        }

        UserModel user = UserModel.GetOrCreate();
        if (_feedItem == null ||
            !user.TryRemoveItem(
                _feedItem.Storage,
                _feedItem.ItemId,
                1))
        {
            return false;
        }

        _fedChickenCount++;

        if (_phase == ChickenCoopPhase.Idle)
        {
            _phase = _productionDuration <= TimeSpan.Zero
                ? ChickenCoopPhase.Ready
                : ChickenCoopPhase.Producing;
            _readyAtUtc = DateTime.UtcNow.Add(_productionDuration);
        }

        _pendingEggs = _phase == ChickenCoopPhase.Ready
            ? _fedChickenCount
            : 0;
        StateChanged?.Invoke(this);

        if (verbose)
        {
            Log.info($"[ChickenCoopState] Fed {_fedChickenCount}/{_chickenCount} chickens.");
        }

        return true;
    }

    public bool TryClaim()
    {
        if (!CanClaim)
        {
            return false;
        }

        UserModel user = UserModel.GetOrCreate();
        if (_eggItem == null)
        {
            return false;
        }

        ItemStorageModel eggStorage = user.GetStorage(_eggItem.Storage);
        int claimAmount = Mathf.Min(_pendingEggs, eggStorage.RemainingCapacity);
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
                _eggItem.Sprite,
                transform.position + _eggCollectionOffset,
                _eggItem.Storage,
                () => CompleteClaim(claimAmount));
        }
        else
        {
            CompleteClaim(claimAmount);
        }

        return true;
    }

    private void CompleteClaim(int requestedAmount)
    {
        UserModel user = UserModel.GetOrCreate();
        if (_eggItem == null)
        {
            _claimInProgress = false;
            StateChanged?.Invoke(this);
            return;
        }

        int accepted = user.AddItem(
            _eggItem.Storage,
            _eggItem.ItemId,
            requestedAmount);
        _pendingEggs -= accepted;
        _claimInProgress = false;

        if (_pendingEggs <= 0)
        {
            _pendingEggs = 0;
            _fedChickenCount = 0;
            _phase = ChickenCoopPhase.Idle;
            _readyAtUtc = default(DateTime);
        }

        StateChanged?.Invoke(this);
    }
}
