using System;
using UnityEngine;

public sealed class TutorialSetup : MonoBehaviour
{
    private const bool verbose = false;

    [SerializeField] private TutorialSetupConfigSO config;

    private bool _isSetup;

    private void Start()
    {
        UserModel.GetOrCreate();

        if (config != null && config.BuildOnStart)
        {
            SetupTutorialFarm();
        }
    }

    [ContextMenu("Setup Tutorial Farm")]
    public void SetupTutorialFarm()
    {
        if (_isSetup)
        {
            return;
        }

        if (config == null)
        {
            if (verbose) Log.error("[TutorialSetup] Setup config is not assigned.");
            return;
        }

        TileBuildService buildService = TileBuildService.instance;
        GridService grid = GridService.instance;
        if (buildService == null || grid == null)
        {
            if (verbose) Log.error("[TutorialSetup] Tile build or grid service is unavailable.");
            return;
        }

        bool setupSucceeded = true;
        int readyPatchCount = 0;
        int expectedPatchCount = 0;

        for (int areaIndex = 0; areaIndex < config.PatchAreas.Count; areaIndex++)
        {
            TutorialPatchArea area = config.PatchAreas[areaIndex];
            if (area == null || area.PatchTile == null)
            {
                setupSucceeded = false;
                continue;
            }

            Vector2Int areaSize = area.Size;
            expectedPatchCount += areaSize.x * areaSize.y;
            for (int row = 0; row < areaSize.y; row++)
            {
                for (int column = 0; column < areaSize.x; column++)
                {
                    Vector2Int position = area.FirstPosition +
                                          new Vector2Int(column, row);
                    FarmPatchState patch = FindOrBuildPatch(
                        buildService,
                        grid,
                        area.PatchTile,
                        position);
                    if (patch == null)
                    {
                        setupSucceeded = false;
                        continue;
                    }

                    PrepareCrop(
                        patch,
                        area.StartingCrop,
                        area.StartMature,
                        area.GrowthDuration);
                    readyPatchCount++;
                }
            }
        }

        for (int i = 0; i < config.TilePlacements.Count; i++)
        {
            TutorialTilePlacement placement = config.TilePlacements[i];
            if (placement == null || placement.Tile == null ||
                !FindOrBuildTile(
                    buildService,
                    grid,
                    placement.Tile,
                    placement.Position))
            {
                setupSucceeded = false;
            }
        }

        _isSetup = setupSucceeded && readyPatchCount == expectedPatchCount;

        if (verbose)
        {
            Log.info($"[TutorialSetup] Prepared {readyPatchCount}/{expectedPatchCount} configured patches.");
        }
    }

    private static bool FindOrBuildTile(
        TileBuildService buildService,
        GridService grid,
        TileDefinitionSO definition,
        Vector2Int position)
    {
        if (grid.TryGetOccupant(position, out GridOccupant existing))
        {
            TileInstance instance = existing != null
                ? existing.GetComponent<TileInstance>()
                : null;
            return instance != null && instance.Definition == definition;
        }

        TileBuildResult result = buildService.TryBuild(
            new TileBuildRequest(definition.TileId, position));
        if (!result.Succeeded && verbose)
        {
            Log.warning($"[TutorialSetup] Could not build '{definition.TileId}' at {position}: {result.Message}");
        }

        return result.Succeeded;
    }

    private static FarmPatchState FindOrBuildPatch(
        TileBuildService buildService,
        GridService grid,
        TileDefinitionSO definition,
        Vector2Int position)
    {
        if (grid.TryGetOccupant(position, out GridOccupant existing))
        {
            return existing != null
                ? existing.GetComponent<FarmPatchState>()
                : null;
        }

        TileBuildResult result = buildService.TryBuild(
            new TileBuildRequest(definition.TileId, position));
        if (!result.Succeeded)
        {
            if (verbose) Log.warning($"[TutorialSetup] Could not build patch at {position}: {result.Message}");
            return null;
        }

        return result.Tile.GetComponent<FarmPatchState>();
    }

    private static void PrepareCrop(
        FarmPatchState patch,
        ItemDefinitionSO crop,
        bool startMature,
        TimeSpan growthDuration)
    {
        if (crop == null)
        {
            return;
        }

        if (patch.CanPlant)
        {
            patch.TryPlant(
                crop.ItemId,
                startMature ? TimeSpan.Zero : growthDuration);
        }
        else if (startMature && patch.CanSpeedUp)
        {
            patch.TryCompleteNow();
        }
    }
}
