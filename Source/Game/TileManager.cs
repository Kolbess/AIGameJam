using System;

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
    private Vector3 _currentDirection = Vector3.Forward;
    private RandomStream _random = new RandomStream();
    private int _consecutiveTurnCount = 0;
    private const int MaxConsecutiveTurns = 2;
    private static readonly Vector3[] Directions = new[]
    {
        Vector3.Forward,
        Vector3.Left,
        Vector3.Backward,
        Vector3.Right
    };
    private int _currentDirIndex = 0; // Start jako forward



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
        if (_playerController == null)
            return;
        _currentDirIndex = 0;
        _currentDirection = Directions[_currentDirIndex];
        Vector3 playerPos = _playerController.Actor.Position;
        Vector3 forward = _playerController.Actor.Transform.Forward;
        _lastTileRotation = _playerController.Actor.Orientation;

        Vector3 centerTile = SnapToGrid(playerPos, GameSettings.TileSize);
        _lastTilePosition = centerTile;

        for (int i = -TilesBehind; i <= TilesAhead; i++)
        {
            Vector3 tilePos = centerTile + forward * (i * GameSettings.TileSize);

            if (!_tileDataMap.ContainsKey(tilePos))
            {
                Actor newTile = PrefabManager.SpawnPrefab(TilePrefab, tilePos, _lastTileRotation);
                var data = new TileManager.TileData { TileActor = newTile };
                _activeTiles.Enqueue(newTile);
                _tileDataMap[tilePos] = data;

                // Apply item spawn chance
                if (_itemManager != null && _random.RandRange(0f, 1f) < GameSettings.ItemSpawnChance)
                {
                    _itemManager.SpawnRandomItem(tilePos);
                    data.HasItem = true;
                }

                // Apply instability chance
                if (i < 0)
                {
                    MarkTileUnstable(tilePos); // Will handle scheduling
                }
            }

            if (i == TilesAhead)
            {
                _lastTilePosition = tilePos;
            }
        }
    }



    private void HandlePlayerStep()
    {
        DestroyOldestTile();
    }


    public void GenerateNextTile()
{
    if (TilePrefab == null)
    {
        Debug.Log("TileManager: TilePrefab not assigned!");
        return;
    }

    // Decide direction
    Vector3 leftDir = Vector3.Transform(_currentDirection, Quaternion.Euler(0, -90, 0));
    Vector3 rightDir = Vector3.Transform(_currentDirection, Quaternion.Euler(0, 90, 0));
    Vector3 forwardDir = _currentDirection;

    Vector3 forwardPos = SnapToGrid(_lastTilePosition + forwardDir * GameSettings.TileSize, GameSettings.TileSize);
    Vector3 leftPos = SnapToGrid(_lastTilePosition + leftDir * GameSettings.TileSize, GameSettings.TileSize);
    Vector3 rightPos = SnapToGrid(_lastTilePosition + rightDir * GameSettings.TileSize, GameSettings.TileSize);

    // Path mostly goes forward (e.g. 80% of time)
    float forwardChance = 0.8f;
    List<(Vector3 dir, Vector3 pos, int turnType)> options = new();

    // Always prefer forward if it's free
    if (!_tileDataMap.ContainsKey(forwardPos))
        options.Add((forwardDir, forwardPos, 0));

    // Occasionally allow left/right if not occupied
    if (_random.RandRange(0f, 1f) > forwardChance)
    {
        if (!_tileDataMap.ContainsKey(leftPos))
            options.Add((leftDir, leftPos, 1));
        if (!_tileDataMap.ContainsKey(rightPos))
            options.Add((rightDir, rightPos, 2));
    }

    if (options.Count == 0)
    {
        Debug.LogWarning("TileManager: No available direction, forcing forward even if overlapping.");
        options.Add((forwardDir, forwardPos, 0));
    }

    var selected = options[_random.RandRange(0, options.Count - 1)];
    _currentDirection = selected.dir;
    Vector3 nextPosition = selected.pos;
    Quaternion nextRotation = Quaternion.LookRotation(_currentDirection);

    if (_tileDataMap.ContainsKey(nextPosition))
    {
        Debug.Log("TileManager: Tile already exists at " + nextPosition);
        return;
    }

    // Spawn tile
    Actor newTile = PrefabManager.SpawnPrefab(TilePrefab, nextPosition, nextRotation);
    var data = new TileData { TileActor = newTile };

    _activeTiles.Enqueue(newTile);
    _tileDataMap[nextPosition] = data;

    _lastTilePosition = nextPosition;
    _lastTileRotation = nextRotation;

    // Spawn item
    if (_itemManager != null && _random.RandRange(0f, 1f) < GameSettings.ItemSpawnChance)
    {
        _itemManager.SpawnRandomItem(nextPosition);
        data.HasItem = true;
    }

    // Mark unstable
    if (_random.RandRange(0f, 1f) < GameSettings.UnstableTileChance)
    {
        Debug.Log("Unstable Tile to mark at " + nextPosition);
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
            Debug.Log("Scheduling destruction");
            ScheduleTileDestruction(position, GameSettings.TileLifespan);
        }
    }
    
    private async System.Threading.Tasks.Task ScheduleTileDestruction(Vector3 position, float delay, bool forceEvenIfStable = false)
    {
        await System.Threading.Tasks.Task.Delay((int)(delay * 1000f)); // milliseconds

        Scripting.InvokeOnUpdate(() =>
        {
            var tileData = GetTileData(position);
            if (tileData != null)
            {
                if (tileData.IsUnstable || forceEvenIfStable)
                {
                    if (tileData.TileActor)
                        Destroy(tileData.TileActor);

                    _tileDataMap.Remove(position);
                }
            }
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
