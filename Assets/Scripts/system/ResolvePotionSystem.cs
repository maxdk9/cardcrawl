using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CCG
{
    public class ResolvePotionSystem : JobComponentSystem
    {
        private EndSimulationEntityCommandBufferSystem barrier;
        private EntityArchetype healPlayerArchetype;
        private EntityArchetype postResolveCardArchetype;
        
        
        protected override void OnCreate()
        {
            barrier=World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            healPlayerArchetype = EntityManager.CreateArchetype(typeof(HealPlayerData));
            postResolveCardArchetype = EntityManager.CreateArchetype(typeof(PostResolveCardData));

        }


        [RequireComponentTag(typeof(ResolveCardInteractionData))]
        private struct ResolvePotionJob:IJobForEachWithEntity<PotionData,StatData>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            public EntityArchetype HealPlayerArchetype;
            public EntityArchetype PostResolveCardArchetype;
            
            
            
            public void Execute(Entity entity, int index, ref PotionData potionData, ref StatData statData)
            {
                EntityCommandBuffer.RemoveComponent<ResolveCardInteractionData>(index,entity);
                EntityCommandBuffer.AddComponent(index,entity,new DirtyData());
                var e = EntityCommandBuffer.CreateEntity(index, HealPlayerArchetype);
                EntityCommandBuffer.SetComponent(index,e,new HealPlayerData
                {
                    Amount = statData.Value
                });

                EntityCommandBuffer.CreateEntity(index, PostResolveCardArchetype);
            }
        }
        
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new ResolvePotionJob
            {
                EntityCommandBuffer = barrier.CreateCommandBuffer().ToConcurrent(),
                HealPlayerArchetype = healPlayerArchetype,
                PostResolveCardArchetype = postResolveCardArchetype
                
            }.Schedule(this, inputDeps);
            barrier.AddJobHandleForProducer(job);
            return job;
        }
    }
}