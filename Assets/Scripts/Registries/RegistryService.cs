using UnityEngine;

[DefaultExecutionOrder(-1000)]
public sealed class RegistryService : MonoBehaviourSingleton<RegistryService>
{
    private const bool verbose = false;

    [SerializeField] private TileRegistrySO tileRegistry;
    [SerializeField] private UserStartingConfigSO userStartingConfig;

    public TileRegistrySO TileRegistry => tileRegistry;
    public UserStartingConfigSO UserStartingConfig => userStartingConfig;

    protected override void Awake()
    {
        base.Awake();
        if (instance != this) return;

        if (tileRegistry == null && verbose)
        {
            Log.error("[RegistryService] A TileRegistrySO has not been assigned.");
        }

        if (userStartingConfig == null && verbose)
        {
            Log.error("[RegistryService] A UserStartingConfigSO has not been assigned.");
        }
    }
}
