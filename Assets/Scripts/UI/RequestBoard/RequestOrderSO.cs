using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class RequestOrderItem
{
    [SerializeField] private ItemDefinitionSO item;
    [SerializeField, Min(1)] private int amount = 1;

    public ItemDefinitionSO Item => item;
    public int Amount => Mathf.Max(1, amount);
}

[CreateAssetMenu(
    menuName = "Farm/Request Board/Order",
    fileName = "RequestOrder_")]
public sealed class RequestOrderSO : ScriptableObject
{
    [SerializeField] private string orderId;
    [SerializeField] private List<RequestOrderItem> requestedItems =
        new List<RequestOrderItem>();
    [SerializeField] private Vector2Int goldRewardRange = new Vector2Int(5, 10);
    [SerializeField] private Vector2Int experienceRewardRange = new Vector2Int(1, 5);

    public string OrderId => orderId;
    public IReadOnlyList<RequestOrderItem> RequestedItems => requestedItems;

    public int RollGoldReward()
    {
        return RollRange(goldRewardRange);
    }

    public int RollExperienceReward()
    {
        return RollRange(experienceRewardRange);
    }

    private static int RollRange(Vector2Int range)
    {
        int minimum = Mathf.Max(0, Mathf.Min(range.x, range.y));
        int maximum = Mathf.Max(minimum, Mathf.Max(range.x, range.y));
        return UnityEngine.Random.Range(minimum, maximum + 1);
    }

    private void OnValidate()
    {
        orderId = orderId == null ? string.Empty : orderId.Trim();
        goldRewardRange.x = Mathf.Max(0, goldRewardRange.x);
        goldRewardRange.y = Mathf.Max(0, goldRewardRange.y);
        experienceRewardRange.x = Mathf.Max(0, experienceRewardRange.x);
        experienceRewardRange.y = Mathf.Max(0, experienceRewardRange.y);
    }
}
