using System.Linq;
using FlaxEngine;

namespace Game;

public class ItemTrigger : Script
{
    private ItemManager itemManager;
    private ItemManager.ItemType itemType;
    private bool collected = false;

    public void Initialize(ItemManager manager, ItemManager.ItemType type)
    {
        itemManager = manager;
        itemType = type;
    }

    public override void OnEnable()
    {
        // Register for event
        Actor.GetChild<Collider>().TriggerEnter += OnTriggerEnter;
    }

    public override void OnDisable()
    {
        // Unregister for event
        Actor.GetChild<Collider>().TriggerEnter -= OnTriggerEnter;
    }

    void OnTriggerEnter(PhysicsColliderActor collider)
    {
        if (collected)
            return;

        if (collider)
        {
            if (collider.As<Actor>().HasTag("Player"))
            {
                collected = true;
                itemManager?.OnItemCollected(itemType, Actor);
            }
            else if (collider.GetChild<Actor>().HasTag("Player"))
            {
                collected = true;
                itemManager?.OnItemCollected(itemType, Actor);
            }
        }
    }
}
    