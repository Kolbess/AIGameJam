namespace Game;

using FlaxEngine;
using System.Collections.Generic;

public class TileManager : Script
{
    public Prefab TilePrefab;
    public int TilesAhead = 10;
    public int TilesBehind = 5;
    public GameSettings GameSettings;

    private Queue<Actor> _activeTiles = new();
    private Dictionary<Vector3, TileData> _tileDataMap = new();
    private Vector3 _lastTilePosition = Vector3.Zero;
    private Quaternion _lastTileRotation = Quaternion.Identity;

    private PlayerController _playerController;
    private VisualEffectsManager _visualEffectsManager;
    private ItemManager _itemManager;

    public class TileData
    {
        public bool IsUnstable = false;
        public Actor TileActor;
        public bool HasItem = false;
    }

    public override void OnAwake()
    {
        _playerController = Scene.FindScript<PlayerController>();
        _visualEffectsManager = Scene.FindScript<VisualEffectsManager>();
        _itemManager = Scene.FindScript<ItemManager>();

        if (GameSettings == null)
            Debug.LogError("TileManager: GameSettings not assigned!");
    }

    public override void OnEnable()
    {
        if (_playerController != null)
        {
            _playerController.OnStepTaken += HandlePlayerStep;
        }
    }

    public override void OnDisable()
    {
        if (_playerController != null)
        {
            _playerController.OnStepTaken -= HandlePlayerStep;
        }
    }

    public override void OnStart()
    {
        for (int i = 0; i < TilesAhead + TilesBehind; i++)
        {
            GenerateNextTile(_lastTilePosition, PlayerController.Direction.Forward);
        }
    }

    private void HandlePlayerStep()
    {
        DestroyOldestTile();
    }

    public void GenerateNextTile(Vector3 playerPosition, PlayerController.Direction direction)
    {
        if (TilePrefab == null)
        {
            Debug.LogError("TileManager: TilePrefab not assigned!");
            return;
        }

        Vector3 nextPosition = _lastTilePosition + Vector3.Transform(Vector3.Forward, _lastTileRotation) * GameSettings.TileSize;
        Quaternion nextRotation = _lastTileRotation;

        // (Optional logic for forks goes here)

        if (_tileDataMap.ContainsKey(nextPosition))
        {
            Debug.LogWarning("TileManager: Tile already exists at " + nextPosition);
            return;
        }

        Actor newTile = PrefabManager.SpawnPrefab(TilePrefab, nextPosition, nextRotation);
        var data = new TileData { TileActor = newTile };

        _activeTiles.Enqueue(newTile);
        _tileDataMap[nextPosition] = data;

        _lastTilePosition = nextPosition;
        _lastTileRotation = nextRotation;

        if (_itemManager != null && new RandomStream().RandRange(0, 1) < GameSettings.ItemSpawnChance)
        {
            _itemManager.SpawnRandomItem(nextPosition);
            data.HasItem = true;
        }

        if (new RandomStream().RandRange(0, 1) < GameSettings.UnstableTileChance)
        {
            MarkTileUnstable(nextPosition);
        }
    }

    public void DestroyOldestTile()
    {
        if (_activeTiles.Count <= TilesBehind)
            return;

        Actor tile = _activeTiles.Dequeue();
        if (tile != null)
        {
            _tileDataMap.Remove(tile.Position);
            Destroy(tile);
        }
    }

    public TileData GetTileData(Vector3 position)
    {
        Vector3 snapped = SnapToGrid(position, GameSettings.TileSize);
        return _tileDataMap.TryGetValue(snapped, out var data) ? data : null;
    }

    private Vector3 SnapToGrid(Vector3 pos, float size)
    {
        return new Vector3(
            Mathf.Round(pos.X / size) * size,
            Mathf.Round(pos.Y / size) * size,
            Mathf.Round(pos.Z / size) * size
        );
    }

    public void MarkTileUnstable(Vector3 position)
    {
        var tileData = GetTileData(position);
        if (tileData != null && !tileData.IsUnstable)
        {
            tileData.IsUnstable = true;
            _visualEffectsManager?.TriggerTileInstabilityEffect(position);
            _ = ScheduleTileDestruction(position, GameSettings.TileLifespan);
        }
    }
    
    private async System.Threading.Tasks.Task ScheduleTileDestruction(Vector3 position, float delay)
    {
        await System.Threading.Tasks.Task.Delay((int)(delay * 1000f)); // milliseconds

        // Ensure this runs on main thread
        Scripting.InvokeOnUpdate(() =>
        {
            DestroyUnstableTile(position);
        });
    }


    private void DestroyUnstableTile(Vector3 position)
    {
        var tileData = GetTileData(position);
        if (tileData != null)
        {
            if (tileData.TileActor)
            {
                Destroy(tileData.TileActor);
            }

            _tileDataMap.Remove(position);
        }
    }
}
