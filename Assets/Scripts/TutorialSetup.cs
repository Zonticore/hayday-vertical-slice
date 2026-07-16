using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class TutorialSetup : MonoBehaviour
{
    private const bool verbose = false;

    [Header("Tutorial Farm")]
    [SerializeField] private string farmPatchTileId = "crop_empty";
    [SerializeField] private string startingCropId = "wheat";
    [SerializeField] private Vector2Int firstPatchPosition = Vector2Int.zero;
    [SerializeField] private Vector2Int patchGridSize = new Vector2Int(3, 2);
    [SerializeField] private bool buildOnStart = true;
    [SerializeField] private bool allowKeyboardTrigger = true;

    private bool _isSetup;

    private void Start()
    {
        UserModel.GetOrCreate();

        if (buildOnStart)
        {
            SetupTutorialFarm();
        }
    }

    private void Update()
    {
        if (allowKeyboardTrigger &&
            Keyboard.current != null &&
            Keyboard.current.tKey.wasPressedThisFrame)
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

        _isSetup = readyPatchCount == columns * rows;

        if (verbose)
        {
            Log.info($"[TutorialSetup] Prepared {readyPatchCount}/{columns * rows} mature wheat patches.");
        }
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
        patchGridSize.x = Mathf.Max(1, patchGridSize.x);
        patchGridSize.y = Mathf.Max(1, patchGridSize.y);
    }
}
