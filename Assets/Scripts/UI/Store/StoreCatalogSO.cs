using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class StoreAnimalEntry
{
    [SerializeField] private ItemDefinitionSO item;
    [SerializeField, Min(0)] private int cost;

    public ItemDefinitionSO Item => item;
    public int Cost => Mathf.Max(0, cost);

    public StoreItemData CreateRuntimeData()
    {
        return item != null
            ? StoreItemData.CreateAnimal(item, Cost)
            : null;
    }
}

[CreateAssetMenu(menuName = "Farm/Store/Catalog", fileName = "StoreCatalog_")]
public sealed class StoreCatalogSO : ScriptableObject
{
    [SerializeField] private List<StoreAnimalEntry> animals =
        new List<StoreAnimalEntry>();
    [SerializeField] private List<TileDefinitionSO> buildings =
        new List<TileDefinitionSO>();

    public IReadOnlyList<StoreAnimalEntry> Animals => animals;
    public IReadOnlyList<TileDefinitionSO> Buildings => buildings;
}
