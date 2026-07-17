using System.Collections.Generic;
using UnityEngine;

public sealed class ChickenCoopVisual : MonoBehaviour
{
    private ChickenCoopState _state;
    private SpriteRenderer _baseRenderer;
    private Sprite _idleCoopSprite;
    private Sprite _idleChickenSprite;
    private Sprite _producingChickenSprite;
    private Sprite _eggSprite;
    private Sprite _emptyTroughSprite;
    private Sprite _filledTroughSprite;
    private Vector3 _troughOffset;
    private Vector3 _troughScale;
    private Vector3[] _chickenOffsets;

    private readonly List<SpriteRenderer> _chickens = new List<SpriteRenderer>();
    private SpriteRenderer _troughRenderer;
    private SpriteRenderer _eggRenderer;

    public void Initialize(
        ChickenCoopState state,
        SpriteRenderer baseRenderer,
        Sprite idleCoopSprite,
        Sprite idleChickenSprite,
        Sprite producingChickenSprite,
        Sprite eggSprite,
        Sprite emptyTroughSprite,
        Sprite filledTroughSprite,
        Vector3 troughOffset,
        Vector3 troughScale,
        Vector3[] chickenOffsets)
    {
        _state = state;
        _baseRenderer = baseRenderer;
        _idleCoopSprite = idleCoopSprite;
        _idleChickenSprite = idleChickenSprite;
        _producingChickenSprite = producingChickenSprite;
        _eggSprite = eggSprite;
        _emptyTroughSprite = emptyTroughSprite;
        _filledTroughSprite = filledTroughSprite;
        _troughOffset = troughOffset;
        _troughScale = troughScale;
        _chickenOffsets = chickenOffsets != null && chickenOffsets.Length > 0
            ? chickenOffsets
            : CreateDefaultOffsets();

        _state.StateChanged += HandleStateChanged;
        _state.ChickenCountChanged += HandleChickenCountChanged;
        Refresh();
    }

    private void HandleStateChanged(ChickenCoopState state)
    {
        Refresh();
    }

    private void HandleChickenCountChanged(ChickenCoopState state)
    {
        RefreshChickens();
    }

    private void Refresh()
    {
        if (_state == null)
        {
            return;
        }

        if (_baseRenderer != null)
        {
            _baseRenderer.sprite = _idleCoopSprite;
        }

        RefreshTrough();
        RefreshChickens();
        RefreshEggs();
    }

    private void RefreshTrough()
    {
        if (_troughRenderer == null)
        {
            var troughObject = new GameObject("ChickenTrough");
            troughObject.transform.SetParent(transform, false);
            troughObject.transform.localPosition = _troughOffset;
            troughObject.transform.localScale = _troughScale;
            _troughRenderer = troughObject.AddComponent<SpriteRenderer>();
            ConfigureChildRenderer(_troughRenderer, _troughOffset, 2);
        }

        bool hasFeed = _state.Phase == ChickenCoopPhase.Producing;
        Sprite troughSprite = hasFeed ? _filledTroughSprite : _emptyTroughSprite;
        _troughRenderer.sprite = troughSprite;
        _troughRenderer.gameObject.SetActive(troughSprite != null);
    }

    private void RefreshChickens()
    {
        if (_state == null)
        {
            return;
        }

        while (_chickens.Count < _state.MaxChickens)
        {
            int index = _chickens.Count;
            var chickenObject = new GameObject($"Chicken_{index + 1}");
            chickenObject.transform.SetParent(transform, false);
            chickenObject.transform.localPosition = _chickenOffsets[
                index % _chickenOffsets.Length];

            SpriteRenderer renderer = chickenObject.AddComponent<SpriteRenderer>();
            ConfigureChildRenderer(
                renderer,
                chickenObject.transform.localPosition,
                3);
            _chickens.Add(renderer);
        }

        for (int i = 0; i < _chickens.Count; i++)
        {
            bool isProducing =
                _state.Phase == ChickenCoopPhase.Producing &&
                i < _state.FedChickenCount;
            Sprite chickenSprite = isProducing && _producingChickenSprite != null
                ? _producingChickenSprite
                : _idleChickenSprite;
            _chickens[i].sprite = chickenSprite;
            _chickens[i].gameObject.SetActive(
                i < _state.ChickenCount && chickenSprite != null);
        }
    }

    private void RefreshEggs()
    {
        if (_eggRenderer == null)
        {
            var eggObject = new GameObject("EggsReady");
            eggObject.transform.SetParent(transform, false);
            eggObject.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            _eggRenderer = eggObject.AddComponent<SpriteRenderer>();
            _eggRenderer.sprite = _eggSprite;
            ConfigureChildRenderer(
                _eggRenderer,
                eggObject.transform.localPosition,
                50);
        }

        _eggRenderer.gameObject.SetActive(
            _state.Phase == ChickenCoopPhase.Ready &&
            !_state.IsClaimInProgress &&
            _eggSprite != null);
    }

    private void ConfigureChildRenderer(
        SpriteRenderer renderer,
        Vector3 localPosition,
        int bias)
    {
        if (_baseRenderer != null)
        {
            renderer.sortingLayerID = _baseRenderer.sortingLayerID;
        }

        renderer.sortingOrder = GetChildSortingOrder(localPosition, bias);
    }

    private int GetChildSortingOrder(Vector3 localPosition, int bias)
    {
        int baseOrder = _baseRenderer != null ? _baseRenderer.sortingOrder : 0;
        return baseOrder + bias - Mathf.RoundToInt(localPosition.y * 10f);
    }

    private static Vector3[] CreateDefaultOffsets()
    {
        return new[]
        {
            new Vector3(-0.55f, 0.05f, 0f),
            new Vector3(0.05f, 0.18f, 0f),
            new Vector3(0.55f, 0.02f, 0f),
            new Vector3(-0.35f, -0.25f, 0f),
            new Vector3(0.3f, -0.3f, 0f),
            new Vector3(0f, 0.48f, 0f)
        };
    }

    private void OnDestroy()
    {
        if (_state != null)
        {
            _state.StateChanged -= HandleStateChanged;
            _state.ChickenCountChanged -= HandleChickenCountChanged;
        }
    }
}
