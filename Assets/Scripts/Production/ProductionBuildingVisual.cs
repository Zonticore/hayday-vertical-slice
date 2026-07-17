using UnityEngine;

public sealed class ProductionBuildingVisual : MonoBehaviour
{
    private ProductionBuildingState _state;
    private SpriteRenderer _baseRenderer;
    private Sprite _idleSprite;
    private Sprite _workingSprite;
    private Sprite _readySprite;
    private SpriteRenderer _readyOutputRenderer;

    public void Initialize(
        ProductionBuildingState state,
        SpriteRenderer baseRenderer,
        Sprite idleSprite,
        Sprite workingSprite,
        Sprite readySprite,
        Sprite outputSprite,
        Vector3 outputOffset,
        float outputScale)
    {
        _state = state;
        _baseRenderer = baseRenderer;
        _idleSprite = idleSprite;
        _workingSprite = workingSprite;
        _readySprite = readySprite;

        var outputObject = new GameObject("ProductionReadyOutput");
        outputObject.transform.SetParent(transform, false);
        outputObject.transform.localPosition = outputOffset;
        outputObject.transform.localScale = Vector3.one * Mathf.Max(0.01f, outputScale);
        _readyOutputRenderer = outputObject.AddComponent<SpriteRenderer>();
        _readyOutputRenderer.sprite = outputSprite;

        int baseOrder = _baseRenderer != null ? _baseRenderer.sortingOrder : 0;
        _readyOutputRenderer.sortingOrder = baseOrder + 2;

        _state.StateChanged += HandleStateChanged;
        Refresh();
    }

    private void HandleStateChanged(ProductionBuildingState state)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (_state == null)
        {
            return;
        }

        if (_baseRenderer != null)
        {
            Sprite nextSprite = _idleSprite;
            if (_state.Phase == ProductionPhase.Producing && _workingSprite != null)
            {
                nextSprite = _workingSprite;
            }
            else if (_state.Phase == ProductionPhase.Ready && _readySprite != null)
            {
                nextSprite = _readySprite;
            }

            _baseRenderer.sprite = nextSprite;
        }

        if (_readyOutputRenderer != null)
        {
            _readyOutputRenderer.gameObject.SetActive(
                _state.Phase == ProductionPhase.Ready &&
                !_state.IsClaimInProgress &&
                _readyOutputRenderer.sprite != null);
        }
    }

    private void OnDestroy()
    {
        if (_state != null)
        {
            _state.StateChanged -= HandleStateChanged;
        }
    }
}
