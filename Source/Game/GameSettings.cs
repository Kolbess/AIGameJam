using FlaxEngine;

namespace Game;

public class GameSettings : Script
{
    [Header("Sanity Settings")]
    [Tooltip("Rate at which sanity drains per second.")]
    public float SanityDrainRate = 1.0f;

    [Tooltip("Max Sanity Amount.")]
    public float maxSanity;

    [Tooltip("Amount of sanity restored by collecting a Water Flask.")]
    public float WaterFlaskSanityRestore = 20.0f;

    [Tooltip("Amount of sanity restored by collecting a Memory Shard.")]
    public float MemoryShardSanityRestore = 5.0f;

    [Tooltip("Amount of sanity reduced by encountering a Mirage.")]
    public float MirageSanityPenalty = 15.0f;

    [Tooltip("Minimum sanity level before visual/audio distortions begin.")]
    [Limit(0, 100)]
    public float SanityDistortionThreshold = 30.0f;

    [Tooltip("Start intensity for chromatic aberration before mirage distortion.")]
    [Limit(0, 1)]
    public float StartChromaticAberration = 0.0f;

    [Tooltip("Max chromatic aberration intensity caused by mirage.")]
    [Limit(0, 1)]
    public float MirageChromaticAberrationIntensity = 0.5f;

    [Tooltip("Maximum chromatic aberration intensity when sanity is at 0.")]
    [Limit(0, 1)]
    public float MaxChromaticAberrationIntensity = 0.8f;

    [Tooltip("Start lens distortion before mirage distortion.")]
    [Limit(-1, 1)]
    public float StartLensDistortion = 0.0f;

    [Tooltip("Max lens distortion intensity caused by mirage.")]
    [Limit(-1, 1)]
    public float MirageLensDistortionIntensity = -0.4f;

    [Tooltip("Maximum lens distortion intensity when sanity is at 0.")]
    [Limit(-1, 1)]
    public float MaxLensDistortionIntensity = -0.6f;

    [Header("Tile Settings")]
    [Tooltip("Number of tiles the player can see behind them before they are destroyed.")]
    public int TileLifespan = 10;

    [Tooltip("Probability (0-1) of a tile being unstable.")]
    [Limit(0, 1)]
    public float UnstableTileChance = 0.1f;

    [Tooltip("Time in seconds before an unstable tile collapses after being stepped on.")]
    public float UnstableTileCollapseDelay = 1.0f;

    [Tooltip("Distance between the center of adjacent tiles.")]
    public float TileSize = 1.0f;

    [Tooltip("Number of tiles ahead to generate.")]
    public int TilesAheadToGenerate = 5;
    
    [Tooltip("Duration of camera shake on tile instability.")]
    public float tileInstabilityShakeDuration = 0.3f;

    [Tooltip("Magnitude of camera shake on tile instability.")]
    public float tileInstabilityShakeMagnitude = 0.2f;


    [Header("Item Settings")]
    [Tooltip("Overall probability (0-1) of spawning an item or mirage on a new tile.")]
    [Limit(0, 1)]
    public float ItemSpawnChance = 0.05f;

    [Tooltip("Probability (0-1) that a spawned item is a Water Flask (relative to other items).")]
    [Limit(0, 1)]
    public float WaterFlaskProbability = 0.3f;

    [Tooltip("Probability (0-1) that a spawned item is a Memory Shard (relative to other items).")]
    [Limit(0, 1)]
    public float MemoryShardProbability = 0.5f;

    [Tooltip("Probability (0-1) that a spawned item is a Mirage (relative to other items).")]
    [Limit(0, 1)]
    public float MirageProbability = 0.2f;

    [Header("Player Settings")]
    [Tooltip("Speed at which the player moves between tiles.")]
    public float PlayerMoveSpeed = 5.0f;

    [Tooltip("Time in seconds the player has to make a directional choice at a fork.")]
    public float DirectionalChoiceTimeLimit = 3.0f;

    [Header("Camera Settings")]
    [Tooltip("Smoothness of camera follow.")]
    public float CameraFollowSmoothness = 5.0f;

    [Tooltip("Offset of the camera from the player.")]
    public Vector3 CameraOffset = new Vector3(0, 3, -5);

    [Tooltip("Base camera shake magnitude for events like falling.")]
    public float BaseCameraShakeMagnitude = 0.5f;

    [Tooltip("Base camera shake duration for events like falling.")]
    public float BaseCameraShakeDuration = 0.2f;

    [Header("Audio Settings")]
    [Tooltip("Volume multiplier for ambient sounds.")]
    [Limit(0, 1)]
    public float AmbientVolume = 0.5f;

    [Tooltip("Volume multiplier for music.")]
    [Limit(0, 1)]
    public float MusicVolume = 0.3f;

    [Tooltip("Volume multiplier for sound effects.")]
    [Limit(0, 1)]
    public float SFXVolume = 0.7f;
}
