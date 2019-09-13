
using System.ComponentModel;
using CCG;
using DefaultNamespace;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace system
{
    public class ResolveAbilityTradeSystem : JobComponentSystem
    {


        private EntityQuery query;
        
        private EndSimulationEntityCommandBufferSystem barrier;
        private EntityArchetype postResolveCardArchetype;

        protected override void OnCreate()
        {
            query = GetEntityQuery(ComponentType.ReadOnly<ResolveAbilityTradeData>());
            barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            postResolveCardArchetype = EntityManager.CreateArchetype(typeof(PostResolveCardData));
        }
        
        private struct ResolveAbilityJob: IJobForEachWithEntity<ResolveAbilityTradeData>
        {

            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            public EntityArchetype PostResolveCardArchetype;
            
            
            public void Execute(Entity entity, int index, [Unity.Collections.ReadOnly] ref ResolveAbilityTradeData c0)
            {
                EntityCommandBuffer.RemoveComponent<ResolveAbilityTradeData>(index,entity);
                var e = EntityCommandBuffer.CreateEntity(index);
                EntityCommandBuffer.AddComponent(index,e,new LootCoinsData
                {
                    Amount = 10,
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

            for (var i = 0; i < entities.Length; i++)
            {
                var job = new ResolveAbilityJob
                {
                    EntityCommandBuffer = barrier.CreateCommandBuffer().ToConcurrent(),
                    PostResolveCardArchetype = postResolveCardArchetype


                };
                inputDeps = job.Schedule(this, inputDeps);
                barrier.AddJobHandleForProducer(inputDeps);
            }
            
            entities.Dispose();
            return inputDeps;
            
            
            return inputDeps;
            
            
            
            
            
        }
    }

    
}