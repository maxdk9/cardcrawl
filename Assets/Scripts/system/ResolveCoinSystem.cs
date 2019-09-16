using System.ComponentModel;
using CCG;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace system
{
    public class ResolveCoinSystem : JobComponentSystem
    {
        private EntityQuery query;
        private EndSimulationEntityCommandBufferSystem barrier;
        private EntityArchetype lootCoinsArchetype;
        private EntityArchetype postResolveCardArchetype;
        
        
        
        
        protected override void OnCreate()
        {
            query = GetEntityQuery(ComponentType.ReadOnly<CoinsData>(),
                ComponentType.ReadOnly<StatData>(),
                ComponentType.ReadOnly<ResolveCardInteractionData>());

            barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            lootCoinsArchetype = EntityManager.CreateArchetype(typeof(LootCoinsData));
            postResolveCardArchetype = EntityManager.CreateArchetype(typeof(PostResolveCardData));
        }

        [RequireComponentTag(typeof(ResolveCardInteractionData))]
        private struct  ResolveCoinsJob : IJobForEachWithEntity<CoinsData,StatData>
        {

            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            public EntityArchetype LootCoinsArchetype;
            public EntityArchetype PostResolveCardArchetype;
            
            
            public void Execute(Entity entity, int index, ref CoinsData coinsData,[Unity.Collections.ReadOnly] ref StatData statData)
                {
                EntityCommandBuffer.RemoveComponent<ResolveCardInteractionData>(index,entity);
                    EntityCommandBuffer.AddComponent(index,entity,new DirtyData());

                    var e = EntityCommandBuffer.CreateEntity(index, LootCoinsArchetype);
                    
                    EntityCommandBuffer.SetComponent(index,e, new LootCoinsData
                    {
                        Amount = statData.Value
                    });
                    EntityCommandBuffer.CreateEntity(index, PostResolveCardArchetype);

                }
        }
        
        
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var entities = query.ToEntityArray(Allocator.TempJob);
            if (entities.Length == 0)
            {
                entities.Dispose();
                return inputDeps;
            }

            var job = new ResolveCoinsJob
            {
                EntityCommandBuffer = barrier.CreateCommandBuffer().ToConcurrent(),

                LootCoinsArchetype = lootCoinsArchetype,
                PostResolveCardArchetype = postResolveCardArchetype
            };

            inputDeps = job.Schedule(this, inputDeps);
            barrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }
    }
}