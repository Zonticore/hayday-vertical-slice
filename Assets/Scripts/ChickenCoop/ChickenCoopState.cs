using System;
using UnityEngine;

public sealed class ChickenCoopState : MonoBehaviour, IStoreItemDropTarget
{
    private const bool verbose = false;

    private string _chickenItemId = "chicken";
    private string _feedItemId = "chicken_feed";
    private string _eggItemId = "egg";
    private int _maxChickens = 6;
    private TimeSpan _productionDuration = TimeSpan.FromSeconds(15d);
    private Sprite _eggSprite;
    private Vector3 _eggCollectionOffset = new Vector3(0f, 0.35f, 0f);

    private int _chickenCount;
    private int _pendingEggs;
    private ChickenCoopPhase _phase;
    private DateTime _readyAtUtc;
    private bool _claimInProgress;

    public event Action<ChickenCoopState> StateChanged;
    public event Action<ChickenCoopState> ChickenCountChanged;

    public int ChickenCount => _chickenCount;
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

    public bool CanFeed
    {
        get
        {
            if (_phase != ChickenCoopPhase.Idle || _chickenCount <= 0)
            {
                return false;
            }

            UserModel user = UserModel.GetOrCreate();
            return user.BarnStorage.GetQuantity(_feedItemId) >= _chickenCount;
        }
    }

    public void Initialize(
        string chickenItemId,
        string feedItemId,
        string eggItemId,
        int maxChickens,
        float productionSeconds,
        Sprite eggSprite,
        Vector3 eggCollectionOffset)
    {
        _chickenItemId = string.IsNullOrWhiteSpace(chickenItemId)
            ? "chicken"
            : chickenItemId.Trim();
        _feedItemId = string.IsNullOrWhiteSpace(feedItemId)
            ? "chicken_feed"
            : feedItemId.Trim();
        _eggItemId = string.IsNullOrWhiteSpace(eggItemId)
            ? "egg"
            : eggItemId.Trim();
        _maxChickens = Mathf.Max(1, maxChickens);
        _productionDuration = TimeSpan.FromSeconds(Mathf.Max(0f, productionSeconds));
        _eggSprite = eggSprite;
        _eggCollectionOffset = eggCollectionOffset;
    }

    private void Update()
    {
        if (_phase == ChickenCoopPhase.Producing && DateTime.UtcNow >= _readyAtUtc)
        {
            _phase = ChickenCoopPhase.Ready;
            _pendingEggs = _chickenCount;
            StateChanged?.Invoke(this);

            if (verbose)
            {
                Log.info($"[ChickenCoopState] {_pendingEggs} eggs are ready.");
            }
        }
    }

    public bool CanAccept(StoreItemData item)
    {
        if (item == null || item.ItemId != _chickenItemId || !CanAddChicken)
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
        if (!user.TryRemoveItem(StorageType.Barn, _feedItemId, _chickenCount))
        {
            return false;
        }

        _phase = _productionDuration <= TimeSpan.Zero
            ? ChickenCoopPhase.Ready
            : ChickenCoopPhase.Producing;
        _readyAtUtc = DateTime.UtcNow.Add(_productionDuration);
        _pendingEggs = _phase == ChickenCoopPhase.Ready ? _chickenCount : 0;
        StateChanged?.Invoke(this);
        return true;
    }

    public bool TryClaim()
    {
        if (!CanClaim)
        {
            return false;
        }

        UserModel user = UserModel.GetOrCreate();
        int claimAmount = Mathf.Min(_pendingEggs, user.BarnStorage.RemainingCapacity);
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
                _eggSprite,
                transform.position + _eggCollectionOffset,
                StorageType.Barn,
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
        int accepted = user.AddItem(StorageType.Barn, _eggItemId, requestedAmount);
        _pendingEggs -= accepted;
        _claimInProgress = false;

        if (_pendingEggs <= 0)
        {
            _pendingEggs = 0;
            _phase = ChickenCoopPhase.Idle;
            _readyAtUtc = default(DateTime);
        }

        StateChanged?.Invoke(this);
    }
}
