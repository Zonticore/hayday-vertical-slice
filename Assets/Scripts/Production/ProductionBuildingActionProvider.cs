using UnityEngine;

public sealed class ProductionBuildingActionProvider : MonoBehaviour, IContextActionProvider
{
    private ProductionBuildingState _state;
    private ContextActionDefinitionSO _startAction;
    private ContextActionDefinitionSO _collectAction;
    private ContextActionDefinitionSO _speedUpAction;

    public void Initialize(
        ProductionBuildingState state,
        ContextActionDefinitionSO startAction,
        ContextActionDefinitionSO collectAction,
        ContextActionDefinitionSO speedUpAction)
    {
        _state = state;
        _startAction = startAction;
        _collectAction = collectAction;
        _speedUpAction = speedUpAction;
    }

    public void CollectActions(ContextActionCollection collection)
    {
        if (_state == null)
        {
            return;
        }

        if (_state.Phase == ProductionPhase.Idle && _startAction != null)
        {
            collection.Add(_startAction.CreateRuntimeActionWithResult(
                _state.CanStart,
                () => _state.TryStart()));
        }

        if (_state.CanSpeedUp && _speedUpAction != null)
        {
            collection.Add(_speedUpAction.CreateRuntimeActionWithResult(
                true,
                () => _state.TryCompleteNow()));
        }

        if (_state.CanClaim && _collectAction != null)
        {
            collection.Add(_collectAction.CreateRuntimeActionWithResult(
                true,
                () => _state.TryClaim()));
        }
    }
}
