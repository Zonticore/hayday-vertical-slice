using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class StartingInventoryEntry
{
    [SerializeField] private ItemDefinitionSO item;
    [SerializeField, Min(0)] private int amount;

    public ItemDefinitionSO Item => item;
    public int Amount => Mathf.Max(0, amount);
}

[CreateAssetMenu(
    menuName = "Farm/User/Starting Config",
    fileName = "UserStartingConfig_")]
public sealed class UserStartingConfigSO : ScriptableObject
{
    [SerializeField] private string farmName = "My Farm";
    [SerializeField, Min(1)] private int barnCapacity = 50;
    [SerializeField, Min(1)] private int grainCapacity = 50;
    [SerializeField, Min(0)] private int coins = 100;
    [SerializeField, Min(1)] private int level = 1;
    [SerializeField, Min(0)] private int experience;
    [SerializeField, Min(1)] private int baseExperienceToLevel = 10;
    [SerializeField] private List<StartingInventoryEntry> inventory =
        new List<StartingInventoryEntry>();

    public string FarmName => string.IsNullOrWhiteSpace(farmName)
        ? "My Farm"
        : farmName.Trim();
    public int BarnCapacity => Mathf.Max(1, barnCapacity);
    public int GrainCapacity => Mathf.Max(1, grainCapacity);
    public int Coins => Mathf.Max(0, coins);
    public int Level => Mathf.Max(1, level);
    public int Experience => Mathf.Max(0, experience);
    public int BaseExperienceToLevel => Mathf.Max(1, baseExperienceToLevel);
    public IReadOnlyList<StartingInventoryEntry> Inventory => inventory;

    private void OnValidate()
    {
        farmName = string.IsNullOrWhiteSpace(farmName) ? "My Farm" : farmName.Trim();
        barnCapacity = Mathf.Max(1, barnCapacity);
        grainCapacity = Mathf.Max(1, grainCapacity);
        coins = Mathf.Max(0, coins);
        level = Mathf.Max(1, level);
        experience = Mathf.Max(0, experience);
        baseExperienceToLevel = Mathf.Max(1, baseExperienceToLevel);
    }
}
