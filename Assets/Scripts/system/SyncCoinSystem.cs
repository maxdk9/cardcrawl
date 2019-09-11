using CCG;
using Unity.Entities;
using UnityEngine;

namespace system
{
    
    [UpdateAfter(typeof(ResolveCoinSystem))]
    public class SyncCoinSystem : ComponentSystem
    {
        private EntityQuery query;
        protected override void OnCreate()
        {
            query = GetEntityQuery(ComponentType.ReadOnly<Transform>(),
                ComponentType.ReadOnly<CoinsData>(),
                ComponentType.ReadOnly<DirtyData>());
        }

        protected override void OnUpdate()
        {
              Entities.With(query).ForEach((Entity ent, CardView card) =>
              {
                  PostUpdateCommands.RemoveComponent<DirtyData>(ent);
                  card.Disable();
              });
        }
    }
}