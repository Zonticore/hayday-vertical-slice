using System.Collections.Generic;
using UnityEngine;

public sealed class ChickenCoopVisual : MonoBehaviour
{
    private ChickenCoopState _state;
    private SpriteRenderer _baseRenderer;
    private Sprite _idleCoopSprite;
    private Sprite _fedCoopSprite;
    private Sprite _chickenSprite;
    private Sprite _eggSprite;
    private Vector3[] _chickenOffsets;

    private readonly List<SpriteRenderer> _chickens = new List<SpriteRenderer>();
    private SpriteRenderer _eggRenderer;

    public void Initialize(
        ChickenCoopState state,
        SpriteRenderer baseRenderer,
        Sprite idleCoopSprite,
        Sprite fedCoopSprite,
        Sprite chickenSprite,
        Sprite eggSprite,
        Vector3[] chickenOffsets)
    {
        _state = state;
        _baseRenderer = baseRenderer;
        _idleCoopSprite = idleCoopSprite;
        _fedCoopSprite = fedCoopSprite;
        _chickenSprite = chickenSprite;
        _eggSprite = eggSprite;
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
            bool fed = _state.Phase != ChickenCoopPhase.Idle;
            _baseRenderer.sprite = fed && _fedCoopSprite != null
                ? _fedCoopSprite
                : _idleCoopSprite;
        }

        RefreshChickens();
        RefreshEggs();
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
            renderer.sprite = _chickenSprite;
            renderer.sortingOrder = GetChildSortingOrder(
                chickenObject.transform.localPosition,
                2);
            _chickens.Add(renderer);
        }

        for (int i = 0; i < _chickens.Count; i++)
        {
            _chickens[i].gameObject.SetActive(
                i < _state.ChickenCount && _chickenSprite != null);
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
            _eggRenderer.sortingOrder = GetChildSortingOrder(
                eggObject.transform.localPosition,
                3);
        }

        _eggRenderer.gameObject.SetActive(
            _state.Phase == ChickenCoopPhase.Ready && _eggSprite != null);
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
