using System.Collections.Generic;
using UnityEngine;

public sealed class TileVisualHitArea : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private PolygonCollider2D _collider;
    private Sprite _currentSprite;
    private readonly List<Vector2> _points = new List<Vector2>();

    public void Initialize(
        SpriteRenderer spriteRenderer,
        PolygonCollider2D visualCollider)
    {
        _spriteRenderer = spriteRenderer;
        _collider = visualCollider;
        RefreshShape();
    }

    private void LateUpdate()
    {
        if (_spriteRenderer != null && _spriteRenderer.sprite != _currentSprite)
        {
            RefreshShape();
        }
    }

    private void RefreshShape()
    {
        _currentSprite = _spriteRenderer != null ? _spriteRenderer.sprite : null;
        if (_collider == null)
        {
            return;
        }

        if (_currentSprite == null)
        {
            _collider.enabled = false;
            return;
        }

        _collider.enabled = true;

        int shapeCount = _currentSprite.GetPhysicsShapeCount();
        if (shapeCount <= 0)
        {
            SetBoundsShape(_currentSprite.bounds);
            return;
        }

        _collider.pathCount = shapeCount;
        for (int pathIndex = 0; pathIndex < shapeCount; pathIndex++)
        {
            _points.Clear();
            _currentSprite.GetPhysicsShape(pathIndex, _points);
            if (_points.Count >= 3)
            {
                _collider.SetPath(pathIndex, _points);
            }
        }
    }

    private void SetBoundsShape(Bounds bounds)
    {
        Vector2 min = bounds.min;
        Vector2 max = bounds.max;
        _collider.pathCount = 1;
        _collider.SetPath(0, new[]
        {
            new Vector2(min.x, min.y),
            new Vector2(max.x, min.y),
            new Vector2(max.x, max.y),
            new Vector2(min.x, max.y)
        });
    }
}
