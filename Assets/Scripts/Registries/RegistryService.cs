using UnityEngine;

public sealed class RegistryService : MonoBehaviourSingleton<RegistryService>
{
    private const bool verbose = false;

    [SerializeField] private TileRegistrySO tileRegistry;

    public TileRegistrySO TileRegistry => tileRegistry;

    protected override void Awake()
    {
        base.Awake();
        if (instance != this) return;

        if (tileRegistry == null && verbose)
        {
            Log.error("[RegistryService] A TileRegistrySO has not been assigned.");
        }
    }
}
