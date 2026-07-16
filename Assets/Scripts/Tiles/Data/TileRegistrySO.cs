using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Farm/Tiles/Tile Registry", fileName = "TileRegistry")]
public sealed class TileRegistrySO : ScriptableObject
{
    private const bool verbose = false;

    [SerializeField] private List<TileDefinitionSO> tiles = new List<TileDefinitionSO>();

    private readonly Dictionary<string, TileDefinitionSO> _lookup =
        new Dictionary<string, TileDefinitionSO>(StringComparer.Ordinal);

    public IReadOnlyList<TileDefinitionSO> Tiles => tiles;

    public bool TryGet(string tileId, out TileDefinitionSO definition)
    {
        if (_lookup.Count != tiles.Count)
        {
            RebuildLookup();
        }

        if (string.IsNullOrWhiteSpace(tileId))
        {
            definition = null;
            return false;
        }

        return _lookup.TryGetValue(tileId, out definition);
    }

    public List<TileDefinitionSO> GetByCategory(TileCategory category)
    {
        var results = new List<TileDefinitionSO>();

        for (int i = 0; i < tiles.Count; i++)
        {
            TileDefinitionSO definition = tiles[i];
            if (definition != null && definition.Category == category)
            {
                results.Add(definition);
            }
        }

        return results;
    }

    private void OnEnable()
    {
        RebuildLookup();
    }

    private void OnValidate()
    {
        RebuildLookup();
    }

    private void RebuildLookup()
    {
        _lookup.Clear();

        for (int i = 0; i < tiles.Count; i++)
        {
            TileDefinitionSO definition = tiles[i];
            if (definition == null || string.IsNullOrWhiteSpace(definition.TileId))
            {
                if (verbose)
                {
                    Log.warning($"[TileRegistrySO] Entry {i} is null or has no tile ID.");
                }

                continue;
            }

            if (_lookup.ContainsKey(definition.TileId))
            {
                if (verbose)
                {
                    Log.error($"[TileRegistrySO] Duplicate tile ID: {definition.TileId}");
                }

                continue;
            }

            _lookup.Add(definition.TileId, definition);
        }
    }
}
