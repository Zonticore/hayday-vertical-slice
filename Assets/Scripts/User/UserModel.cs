using System;
using UnityEngine;

public sealed class UserModel : MonoBehaviourSingleton<UserModel>
{
    private const bool verbose = false;

    private string farmName;
    private int coins;
    private int level;
    private int experience;
    private int baseExperienceToLevel;

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

        RegistryService registries = RegistryService.instance;
        UserStartingConfigSO config = registries != null
            ? registries.UserStartingConfig
            : null;

        farmName = config != null ? config.FarmName : "My Farm";
        coins = config != null ? config.Coins : 0;
        level = config != null ? config.Level : 1;
        experience = config != null ? config.Experience : 0;
        baseExperienceToLevel = config != null
            ? config.BaseExperienceToLevel
            : 10;

        BarnStorage = new ItemStorageModel(config != null ? config.BarnCapacity : 50);
        GrainStorage = new ItemStorageModel(config != null ? config.GrainCapacity : 50);
        BarnStorage.ItemQuantityChanged += HandleBarnQuantityChanged;
        GrainStorage.ItemQuantityChanged += HandleGrainQuantityChanged;

        if (config != null)
        {
            for (int i = 0; i < config.Inventory.Count; i++)
            {
                StartingInventoryEntry entry = config.Inventory[i];
                ItemDefinitionSO item = entry?.Item;
                if (item != null && entry.Amount > 0)
                {
                    GetStorage(item.Storage).Add(item.ItemId, entry.Amount);
                }
            }
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

}
