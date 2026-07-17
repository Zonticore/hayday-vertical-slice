using UnityEngine;

public sealed class ChickenCoopActionProvider : MonoBehaviour, IContextActionProvider
{
    private ChickenCoopState _state;
    private Sprite _feedIcon;
    private Sprite _claimIcon;

    public void Initialize(ChickenCoopState state, Sprite feedIcon, Sprite claimIcon)
    {
        _state = state;
        _feedIcon = feedIcon;
        _claimIcon = claimIcon;
    }

    public void CollectActions(ContextActionCollection collection)
    {
        if (_state == null)
        {
            return;
        }

        if (_state.Phase == ChickenCoopPhase.Idle && _state.ChickenCount > 0)
        {
            collection.Add(new ContextAction(
                "feed_chickens",
                "Feed",
                _feedIcon,
                0,
                _state.CanFeed,
                true,
                null,
                () => _state.TryFeed()));
        }

        if (_state.Phase == ChickenCoopPhase.Ready)
        {
            collection.Add(new ContextAction(
                "collect_eggs",
                "Collect Eggs",
                _claimIcon,
                0,
                _state.CanClaim,
                true,
                null,
                () => _state.TryClaim()));
        }
    }
}
