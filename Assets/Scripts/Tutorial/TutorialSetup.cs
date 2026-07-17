using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class TutorialSetup : MonoBehaviour
{
    private const bool verbose = false;

    [Header("Tutorial Farm")]
    private string farmPatchTileId = "crop_empty";
    private string startingCropId = "wheat";
    private Vector2Int firstPatchPosition = Vector2Int.zero;
    private Vector2Int patchGridSize = new Vector2Int(3, 2);

    [Header("Feature Tiles")]
    private bool buildFeatureTiles = true;
    private string chickenCoopTileId = "coop";
    private Vector2Int chickenCoopPosition = new Vector2Int(8, 2);
    private string requestBoardTileId = "request_board";
    private Vector2Int requestBoardPosition = new Vector2Int(-5, 4);
    
    private string siloId = "silo";
    private Vector2Int siloPosition = new Vector2Int(5, -5);
    private string barnId = "barn";
    private Vector2Int barnPosition = new Vector2Int(10, -9);

    [Header("Setup")]
    private bool buildOnStart = true;
    private bool allowKeyboardTrigger = true;

    private bool _isSetup;

    private void Start()
    {
        UserModel.GetOrCreate();

        if (buildOnStart)
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

        TileBuildService buildService = TileBuildService.instance;
        GridService grid = GridService.instance;
        if (buildService == null || grid == null)
        {
            if (verbose) Log.error("[TutorialSetup] Tile build or grid service is unavailable.");
            return;
        }

        int columns = Mathf.Max(1, patchGridSize.x);
        int rows = Mathf.Max(1, patchGridSize.y);
        int readyPatchCount = 0;

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                Vector2Int position = firstPatchPosition + new Vector2Int(column, row);
                FarmPatchState patch = FindOrBuildPatch(buildService, grid, position);
                if (patch == null)
                {
                    continue;
                }

                MakeCropMature(patch);
                readyPatchCount++;
            }
        }

        bool featureTilesReady = true;
        if (buildFeatureTiles)
        {
            bool coopReady = FindOrBuildTile(
                buildService,
                grid,
                chickenCoopTileId,
                chickenCoopPosition);
            bool requestBoardReady = FindOrBuildTile(
                buildService,
                grid,
                requestBoardTileId,
                requestBoardPosition);
            featureTilesReady = coopReady && requestBoardReady;
        }
        
        FindOrBuildTile(buildService, grid, barnId, barnPosition);
        FindOrBuildTile(buildService, grid, siloId, siloPosition);

        _isSetup = readyPatchCount == columns * rows && featureTilesReady;

        if (verbose)
        {
            Log.info($"[TutorialSetup] Prepared {readyPatchCount}/{columns * rows} mature wheat patches.");
        }
    }

    private bool FindOrBuildTile(
        TileBuildService buildService,
        GridService grid,
        string tileId,
        Vector2Int position)
    {
        if (grid.TryGetOccupant(position, out GridOccupant existing))
        {
            return existing != null &&
                   existing.GetComponent<TileInstance>() != null &&
                   existing.GetComponent<TileInstance>().Definition.TileId == tileId;
        }

        TileBuildResult result = buildService.TryBuild(new TileBuildRequest(tileId, position));
        if (!result.Succeeded && verbose)
        {
            Log.warning($"[TutorialSetup] Could not build '{tileId}' at {position}: {result.Message}");
        }

        return result.Succeeded;
    }

    private FarmPatchState FindOrBuildPatch(
        TileBuildService buildService,
        GridService grid,
        Vector2Int position)
    {
        if (grid.TryGetOccupant(position, out GridOccupant existing))
        {
            return existing != null
                ? existing.GetComponent<FarmPatchState>()
                : null;
        }

        var request = new TileBuildRequest(farmPatchTileId, position);
        TileBuildResult result = buildService.TryBuild(request);
        if (!result.Succeeded)
        {
            if (verbose) Log.warning($"[TutorialSetup] Could not build patch at {position}: {result.Message}");
            return null;
        }

        return result.Tile.GetComponent<FarmPatchState>();
    }

    private void MakeCropMature(FarmPatchState patch)
    {
        if (patch.CanPlant)
        {
            patch.TryPlant(startingCropId, TimeSpan.Zero);
        }
        else if (patch.CanSpeedUp)
        {
            patch.TryCompleteNow();
        }
    }

    private void OnValidate()
    {
        farmPatchTileId = farmPatchTileId == null ? string.Empty : farmPatchTileId.Trim();
        startingCropId = startingCropId == null ? string.Empty : startingCropId.Trim();
        chickenCoopTileId = chickenCoopTileId == null ? string.Empty : chickenCoopTileId.Trim();
        requestBoardTileId = requestBoardTileId == null ? string.Empty : requestBoardTileId.Trim();
        patchGridSize.x = Mathf.Max(1, patchGridSize.x);
        patchGridSize.y = Mathf.Max(1, patchGridSize.y);
    }
}
