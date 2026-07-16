using System;
using System.Collections.Generic;

public sealed class ItemStorageModel
{
    private readonly Dictionary<string, int> _items =
        new Dictionary<string, int>(StringComparer.Ordinal);

    public event Action<string, int, int> ItemQuantityChanged;
    public event Action CapacityChanged;

    public int Capacity { get; private set; }
    public int UsedCapacity { get; private set; }
    public int RemainingCapacity => Math.Max(0, Capacity - UsedCapacity);
    public bool IsFull => UsedCapacity >= Capacity;
    public IReadOnlyDictionary<string, int> Items => _items;

    public ItemStorageModel(int capacity)
    {
        Capacity = Math.Max(0, capacity);
    }

    public int GetQuantity(string itemId)
    {
        return !string.IsNullOrWhiteSpace(itemId) &&
               _items.TryGetValue(itemId, out int quantity)
            ? quantity
            : 0;
    }

    public int Add(string itemId, int requestedAmount)
    {
        if (string.IsNullOrWhiteSpace(itemId) || requestedAmount <= 0)
        {
            return 0;
        }

        int acceptedAmount = Math.Min(requestedAmount, RemainingCapacity);
        if (acceptedAmount <= 0)
        {
            return 0;
        }

        int previousQuantity = GetQuantity(itemId);
        int newQuantity = previousQuantity + acceptedAmount;
        _items[itemId] = newQuantity;
        UsedCapacity += acceptedAmount;
        ItemQuantityChanged?.Invoke(itemId, previousQuantity, newQuantity);
        return acceptedAmount;
    }

    public bool TryRemove(string itemId, int amount)
    {
        if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
        {
            return false;
        }

        int previousQuantity = GetQuantity(itemId);
        if (previousQuantity < amount)
        {
            return false;
        }

        int newQuantity = previousQuantity - amount;
        if (newQuantity == 0)
        {
            _items.Remove(itemId);
        }
        else
        {
            _items[itemId] = newQuantity;
        }

        UsedCapacity -= amount;
        ItemQuantityChanged?.Invoke(itemId, previousQuantity, newQuantity);
        return true;
    }

    public void IncreaseCapacity(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Capacity += amount;
        CapacityChanged?.Invoke();
    }
}
