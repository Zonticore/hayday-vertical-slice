using System;
using UnityEngine;

public sealed class UserModel : MonoBehaviourSingleton<UserModel>
{
    private const bool verbose = false;

    [Header("Identity")]
    [SerializeField] private string farmName = "My Farm";

    [Header("Starting Storage")]
    [SerializeField, Min(1)] private int barnCapacity = 50;
    [SerializeField, Min(1)] private int grainCapacity = 50;

    [Header("Starting Progress")]
    [SerializeField, Min(0)] private int coins = 100;
    [SerializeField, Min(1)] private int level = 1;
    [SerializeField, Min(0)] private int experience;
    [SerializeField, Min(1)] private int baseExperienceToLevel = 10;

    [Header("Starting Inventory")]
    [SerializeField] private string startingFeedItemId = "chicken_feed";
    [SerializeField, Min(0)] private int startingFeedAmount = 12;

    public event Action<string> FarmNameChanged;
    public event Action<StorageType, string, int, int> ItemQuantityChanged;
    public event Action<int> CoinsChanged;
    public event Action<int, int, int> ExperienceChanged;

    public string FarmName => farmName;
    public ItemStorageModel BarnStorage { get; private set; }
    public ItemStorageModel GrainStorage { get; private set; }
    public int Coins => coins;
    public int Level => level;
    public int Experience => experience;
    public int ExperienceToNextLevel => baseExperienceToLevel * level;

    protected override void Awake()
    {
        base.Awake();
        if (instance != this)
        {
            return;
        }

        BarnStorage = new ItemStorageModel(barnCapacity);
        GrainStorage = new ItemStorageModel(grainCapacity);
        BarnStorage.ItemQuantityChanged += HandleBarnQuantityChanged;
        GrainStorage.ItemQuantityChanged += HandleGrainQuantityChanged;

        if (startingFeedAmount > 0 && !string.IsNullOrWhiteSpace(startingFeedItemId))
        {
            BarnStorage.Add(startingFeedItemId, startingFeedAmount);
        }

        DontDestroyOnLoad(gameObject);
    }

    public static UserModel GetOrCreate()
    {
        UserModel current = instance;
        if (current != null)
        {
            return current;
        }

        var userObject = new GameObject(nameof(UserModel));
        return userObject.AddComponent<UserModel>();
    }

    public ItemStorageModel GetStorage(StorageType storageType)
    {
        return storageType == StorageType.Grain
            ? GrainStorage
            : BarnStorage;
    }

    public int AddItem(StorageType storageType, string itemId, int amount)
    {
        ItemStorageModel storage = GetStorage(storageType);
        int accepted = storage != null ? storage.Add(itemId, amount) : 0;

        if (verbose)
        {
            Log.info($"[UserModel] Added {accepted}/{amount} '{itemId}' to {storageType} storage.");
        }

        return accepted;
    }

    public bool TryRemoveItem(StorageType storageType, string itemId, int amount)
    {
        ItemStorageModel storage = GetStorage(storageType);
        return storage != null && storage.TryRemove(itemId, amount);
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        coins += amount;
        CoinsChanged?.Invoke(coins);
    }

    public bool TrySpendCoins(int amount)
    {
        if (amount < 0 || coins < amount)
        {
            return false;
        }

        if (amount == 0)
        {
            return true;
        }

        coins -= amount;
        CoinsChanged?.Invoke(coins);
        return true;
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0) return;
        experience += amount;
        while (experience >= ExperienceToNextLevel)
        {
            experience -= ExperienceToNextLevel;
            level++;
        }
        ExperienceChanged?.Invoke(level, experience, ExperienceToNextLevel);
    }

    public void SetFarmName(string newFarmName)
    {
        string normalized = string.IsNullOrWhiteSpace(newFarmName)
            ? "My Farm"
            : newFarmName.Trim();

        if (farmName == normalized)
        {
            return;
        }

        farmName = normalized;
        FarmNameChanged?.Invoke(farmName);
    }

    private void HandleBarnQuantityChanged(string itemId, int previous, int current)
    {
        ItemQuantityChanged?.Invoke(StorageType.Barn, itemId, previous, current);
    }

    private void HandleGrainQuantityChanged(string itemId, int previous, int current)
    {
        ItemQuantityChanged?.Invoke(StorageType.Grain, itemId, previous, current);
    }

    private void OnDestroy()
    {
        if (BarnStorage != null)
        {
            BarnStorage.ItemQuantityChanged -= HandleBarnQuantityChanged;
        }

        if (GrainStorage != null)
        {
            GrainStorage.ItemQuantityChanged -= HandleGrainQuantityChanged;
        }
    }

    private void OnValidate()
    {
        farmName = string.IsNullOrWhiteSpace(farmName) ? "My Farm" : farmName.Trim();
        barnCapacity = Mathf.Max(1, barnCapacity);
        grainCapacity = Mathf.Max(1, grainCapacity);
        coins = Mathf.Max(0, coins);
        level = Mathf.Max(1, level);
        experience = Mathf.Max(0, experience);
        baseExperienceToLevel = Mathf.Max(1, baseExperienceToLevel);
        startingFeedItemId = startingFeedItemId == null
            ? string.Empty
            : startingFeedItemId.Trim();
        startingFeedAmount = Mathf.Max(0, startingFeedAmount);
    }
}
