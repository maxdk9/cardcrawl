using CCG;
using DefaultNamespace;
using Unity.Entities;
using UnityEngine;

namespace system
{
    [UpdateAfter(typeof(DamagePlayerSystem))]
    public class SyncPotionSystem : ComponentSystem
    {
        private EntityQuery entityQuery;

        protected override void OnCreate()
        {
            entityQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Transform>(),
                ComponentType.ReadOnly<PotionData>(),
                ComponentType.ReadOnly<DirtyData>());
        }

        protected override void OnUpdate()
        { 
                Entities.With(entityQuery).ForEach((Entity entity, CardView card) =>
                {
                    PostUpdateCommands.RemoveComponent<DirtyData>(entity);
                    card.Disable();
                });
        }
    }
}