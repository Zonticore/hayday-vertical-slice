using UnityEngine;

public sealed class ChickenCoopActionProvider : MonoBehaviour, IContextActionProvider
{
    private ChickenCoopState _state;
    private ContextActionDefinitionSO _feedAction;
    private ContextActionDefinitionSO _claimAction;

    public void Initialize(
        ChickenCoopState state,
        ContextActionDefinitionSO feedAction,
        ContextActionDefinitionSO claimAction)
    {
        _state = state;
        _feedAction = feedAction;
        _claimAction = claimAction;
    }

    public void CollectActions(ContextActionCollection collection)
    {
        if (_state == null)
        {
            return;
        }

        if (_state.CanAttemptFeed && _feedAction != null)
        {
            collection.Add(_feedAction.CreateRuntimeActionWithResult(
                true,
                () => _state.TryFeed()));
        }

        if (_state.CanClaim && _claimAction != null)
        {
            collection.Add(_claimAction.CreateRuntimeActionWithResult(
                true,
                () => _state.TryClaim()));
        }
    }
}
