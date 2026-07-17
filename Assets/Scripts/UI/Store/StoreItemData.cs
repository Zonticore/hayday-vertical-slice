using UnityEngine;

public enum StoreItemKind
{
    Animal = 0,
    Building = 1
}

public sealed class StoreItemData
{
    public string ItemId { get; }
    public string DisplayName { get; }
    public Sprite Sprite { get; }
    public int Cost { get; }
    public StoreItemKind Kind { get; }
    public string BuildingTileId { get; }
    public bool IsBuilding => Kind == StoreItemKind.Building;

    public StoreItemData(string itemId, string displayName, Sprite sprite, int cost)
        : this(
            itemId,
            displayName,
            sprite,
            cost,
            StoreItemKind.Animal,
            string.Empty)
    {
    }

    private StoreItemData(
        string itemId,
        string displayName,
        Sprite sprite,
        int cost,
        StoreItemKind kind,
        string buildingTileId)
    {
        ItemId = itemId ?? string.Empty;
        DisplayName = displayName ?? string.Empty;
        Sprite = sprite;
        Cost = Mathf.Max(0, cost);
        Kind = kind;
        BuildingTileId = buildingTileId ?? string.Empty;
    }

    public static StoreItemData CreateBuilding(TileDefinitionSO definition)
    {
        if (definition == null)
        {
            return null;
        }

        return new StoreItemData(
            definition.TileId,
            definition.DisplayName,
            definition.Sprite,
            definition.PurchaseCost,
            StoreItemKind.Building,
            definition.TileId);
    }

    public static StoreItemData CreateAnimal(ItemDefinitionSO definition, int cost)
    {
        if (definition == null)
        {
            return null;
        }

        return new StoreItemData(
            definition.ItemId,
            definition.DisplayName,
            definition.Sprite,
            cost);
    }
}
