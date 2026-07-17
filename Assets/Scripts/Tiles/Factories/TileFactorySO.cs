using UnityEngine;

public abstract class TileFactorySO : ScriptableObject
{
    public TileInstance Create(
        TileDefinitionSO definition,
        TileBuildContext context)
    {
        TileInstance instance = CreateBaseTile(definition, context);

        try
        {
            Configure(instance, definition, context);
            return instance;
        }
        catch
        {
            if (instance != null)
            {
                Destroy(instance.gameObject);
            }

            throw;
        }
    }

    protected virtual TileInstance CreateBaseTile(
        TileDefinitionSO definition,
        TileBuildContext context)
    {
        var tileObject = new GameObject();
        if (context.TileRoot != null)
        {
            tileObject.transform.SetParent(context.TileRoot, false);
        }

        tileObject.transform.position = context.Grid.GetFootprintWorldCenter(
            context.Request.GridPosition,
            definition.Footprint,
            context.Request.Orientation);

        SpriteRenderer spriteRenderer = tileObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = definition.Sprite;

        PolygonCollider2D collider = tileObject.AddComponent<PolygonCollider2D>();
        collider.pathCount = 1;
        collider.SetPath(
            0,
            context.Grid.GetFootprintLocalCorners(
                definition.Footprint,
                context.Request.Orientation));

        PolygonCollider2D visualCollider = tileObject.AddComponent<PolygonCollider2D>();
        visualCollider.isTrigger = true;
        TileVisualHitArea visualHitArea = tileObject.AddComponent<TileVisualHitArea>();
        visualHitArea.Initialize(spriteRenderer, visualCollider);

        GridOccupant occupant = tileObject.AddComponent<GridOccupant>();
        occupant.Initialize(
            context.Grid,
            context.Request.GridPosition,
            definition.Footprint,
            context.Request.Orientation);

        TileInstance instance = tileObject.AddComponent<TileInstance>();
        instance.Initialize(
            definition,
            context.Request.GridPosition,
            context.Request.Orientation,
            spriteRenderer,
            occupant);

        return instance;
    }

    protected abstract void Configure(
        TileInstance instance,
        TileDefinitionSO definition,
        TileBuildContext context);
}
