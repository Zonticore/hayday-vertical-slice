using System;
using UnityEngine;

public class StorageHudDisplay : MonoBehaviourSingleton<StorageHudDisplay>
{
    [SerializeField] public RectTransform siloSpriteDisplay;
    [SerializeField] public RectTransform barnSpriteDisplay;

    private int _siloFlights;
    private int _barnFlights;

    private void Start()
    {
        SetTargetVisible(StorageType.Grain, false);
        SetTargetVisible(StorageType.Barn, false);
    }

    public void FlyItem(Sprite itemSprite, Vector3 worldPosition, StorageType storageType, Action onArrived)
    {
        RectTransform target = GetTarget(storageType);
        ChangeFlightCount(storageType, 1);
        UiRewardFlyer.Fly(itemSprite, worldPosition, target, () =>
        {
            ChangeFlightCount(storageType, -1);
            onArrived?.Invoke();
        });
    }

    private RectTransform GetTarget(StorageType storageType) =>
        storageType == StorageType.Grain ? siloSpriteDisplay : barnSpriteDisplay;

    private void ChangeFlightCount(StorageType storageType, int amount)
    {
        if (storageType == StorageType.Grain)
            _siloFlights = Mathf.Max(0, _siloFlights + amount);
        else
            _barnFlights = Mathf.Max(0, _barnFlights + amount);
        SetTargetVisible(storageType,
            storageType == StorageType.Grain ? _siloFlights > 0 : _barnFlights > 0);
    }

    private void SetTargetVisible(StorageType storageType, bool visible)
    {
        RectTransform target = GetTarget(storageType);
        if (target != null) target.gameObject.SetActive(visible);
    }
}
