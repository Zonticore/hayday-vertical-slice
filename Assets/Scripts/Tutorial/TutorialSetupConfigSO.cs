using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class TutorialPatchArea
{
    [SerializeField] private TileDefinitionSO patchTile;
    [SerializeField] private ItemDefinitionSO startingCrop;
    [SerializeField] private Vector2Int firstPosition;
    [SerializeField] private Vector2Int size = Vector2Int.one;
    [SerializeField] private bool startMature = true;
    [SerializeField, Min(0f)] private float growthDurationSeconds = 60f;

    public TileDefinitionSO PatchTile => patchTile;
    public ItemDefinitionSO StartingCrop => startingCrop;
    public Vector2Int FirstPosition => firstPosition;
    public Vector2Int Size => new Vector2Int(
        Mathf.Max(1, size.x),
        Mathf.Max(1, size.y));
    public bool StartMature => startMature;
    public TimeSpan GrowthDuration => TimeSpan.FromSeconds(
        Mathf.Max(0f, growthDurationSeconds));
}

[Serializable]
public sealed class TutorialTilePlacement
{
    [SerializeField] private TileDefinitionSO tile;
    [SerializeField] private Vector2Int position;

    public TileDefinitionSO Tile => tile;
    public Vector2Int Position => position;
}

[CreateAssetMenu(
    menuName = "Farm/Tutorial/Setup Config",
    fileName = "TutorialSetup_")]
public sealed class TutorialSetupConfigSO : ScriptableObject
{
    [SerializeField] private bool buildOnStart = true;
    [SerializeField] private List<TutorialPatchArea> patchAreas =
        new List<TutorialPatchArea>();
    [SerializeField] private List<TutorialTilePlacement> tilePlacements =
        new List<TutorialTilePlacement>();

    public bool BuildOnStart => buildOnStart;
    public IReadOnlyList<TutorialPatchArea> PatchAreas => patchAreas;
    public IReadOnlyList<TutorialTilePlacement> TilePlacements => tilePlacements;
}
