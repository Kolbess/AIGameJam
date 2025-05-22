using FlaxEngine;
using System.Collections.Generic;

namespace Game;

public class ItemManager : Script
{
    public enum ItemType { WaterFlask, MemoryShard, Mirage }

    [System.Serializable]
    public class ItemPrefab
    {
        public Prefab prefab;
        public ItemType type;
        [Range(0f, 1f)] public float spawnWeight;
    }

    [Serialize] public List<ItemPrefab> itemPrefabs;
    [Serialize] public GameSettings gameSettings;

    private SanityManager sanityManager;
    private VisualEffectsManager visualEffectsManager;
    [Serialize] public PlayerController playerController;

    private Dictionary<Actor, ItemType> activeItems = new Dictionary<Actor, ItemType>();
    private RandomStream _rng = new RandomStream((int)Time.UnscaledGameTime * 1000);

    public override void OnStart()
    {
        sanityManager = Actor?.GetScript<SanityManager>();
        if (sanityManager == null)
            Debug.LogError("ItemManager: SanityManager not found!");

        visualEffectsManager = Actor?.GetScript<VisualEffectsManager>();
        if (visualEffectsManager == null)
            Debug.LogError("ItemManager: VisualEffectsManager not found!");

        if (gameSettings == null)
            Debug.LogError("ItemManager: GameSettings not assigned!");
    }

    public void SpawnRandomItem(Vector3 position)
    {
        if (itemPrefabs == null || itemPrefabs.Count == 0)
        {
            Debug.LogWarning("ItemManager: No item prefabs assigned!");
            return;
        }

        float totalWeight = 0f;
        foreach (var item in itemPrefabs)
        {
            totalWeight += item.spawnWeight;
        }

        if (totalWeight <= 0)
        {
            Debug.LogWarning("ItemManager: Total spawn weight is zero or negative!");
            return;
        }

        float randomValue = _rng.RandRange(0f, totalWeight);
        ItemPrefab chosenItem = null;
        float cumulativeWeight = 0f;

        foreach (var item in itemPrefabs)
        {
            cumulativeWeight += item.spawnWeight;
            if (randomValue <= cumulativeWeight)
            {
                chosenItem = item;
                break;
            }
        }

        if (chosenItem != null && chosenItem.prefab != null)
        {
            Vector3 spawnPosition = position + Vector3.Up * playerController.GroundY;
            Actor spawnedActor = PrefabManager.SpawnPrefab(chosenItem.prefab, spawnPosition, Quaternion.Identity);

            Collider collider = spawnedActor.GetChild<Collider>();
            spawnedActor.Scale = new Float3(0.4f);
            if (collider == null)
            {
                var sphereCollider = spawnedActor.AddChild<SphereCollider>();
                sphereCollider.Radius = 0.5f;
                sphereCollider.IsTrigger = true;
            }
            else
            {
                collider.IsTrigger = true;
            }

            var itemTrigger = spawnedActor.GetScript<ItemTrigger>();
            if (itemTrigger == null)
            {
                itemTrigger = spawnedActor.AddScript<ItemTrigger>();
            }
            itemTrigger.Initialize(this, chosenItem.type);

            activeItems.Add(spawnedActor, chosenItem.type);

            Debug.Log($"[ItemManager] Spawned item: {chosenItem.type}");
        }
        else
        {
            Debug.LogWarning("ItemManager: Chosen item prefab is null!");
        }
    }

    public void OnItemCollected(ItemType type, Actor itemActor)
    {
        if (sanityManager == null || gameSettings == null)
        {
            Debug.LogError("ItemManager: SanityManager or GameSettings is null, cannot apply item effect.");
            return;
        }

        switch (type)
        {
            case ItemType.WaterFlask:
                Debug.Log("Collected Water Flask!");
                break;
            case ItemType.MemoryShard:
                sanityManager.AddSanity(gameSettings.MemoryShardSanityRestore);
                Debug.Log($"Collected Memory Shard! Sanity restored by {gameSettings.MemoryShardSanityRestore}.");
                break;
            case ItemType.Mirage:
                sanityManager.ReduceSanity(gameSettings.MirageSanityPenalty);
                Debug.Log($"Encountered Mirage! Sanity reduced by {gameSettings.MirageSanityPenalty}.");
                OnMirageEncountered();
                break;
            default:
                Debug.LogWarning($"ItemManager: Unhandled item type collected: {type}");
                break;
        }

        if (activeItems.ContainsKey(itemActor))
        {
            activeItems.Remove(itemActor);
        }
        Destroy(itemActor);
    }

    public void OnMirageEncountered()
    {
        visualEffectsManager?.TriggerMirageEffect();
    }
}
