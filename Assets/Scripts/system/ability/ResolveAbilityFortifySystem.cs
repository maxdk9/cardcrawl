
using CCG;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace system
{
    public class ResolveAbilityFortifySystem : JobComponentSystem
    {


        private EntityQuery query;
        private EndSimulationEntityCommandBufferSystem barrier;
        private EntityArchetype postResolveCardArchetype;

        protected override void OnCreate()
        {
            query = GetEntityQuery(ComponentType.ReadOnly<ResolveAbilityFortifyData>());
            barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            postResolveCardArchetype = EntityManager.CreateArchetype(typeof(PostResolveCardData));
        }
        
        private struct ResolveAbilityJob: IJobForEachWithEntity<ResolveAbilityFortifyData>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            public int InitialStatValue;
            public int StatIncrease;
            public EntityArchetype PostResolveCardArchetype;
            
            
            
            public void Execute(Entity entity, int index,[ReadOnly]  ref ResolveAbilityFortifyData resolveAbilityDat)
            {
                EntityCommandBuffer.RemoveComponent<ResolveAbilityFortifyData>(index,entity);
                Entity targetEntity = resolveAbilityDat.TargetEntity;
                EntityCommandBuffer.SetComponent(index,targetEntity,new StatData
                {
                    Value = InitialStatValue+StatIncrease
                });
                
                EntityCommandBuffer.AddComponent(index,targetEntity,new DirtyStatData());
                EntityCommandBuffer.CreateEntity(index, PostResolveCardArchetype);
            }
        }
        

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var data = query.ToComponentDataArray<ResolveAbilityFortifyData>(Allocator.TempJob);
            if (data.Length == 0)
            {
                data.Dispose();
                return inputDeps;
            }

            for (var i = 0; i < data.Length; i++)
            {
                var targetEntity = data[i].TargetEntity;
                if (EntityManager.HasComponent<StatData>(targetEntity))
                {
                    var job = new ResolveAbilityJob()
                    {
                        EntityCommandBuffer =barrier.CreateCommandBuffer().ToConcurrent(),
                        InitialStatValue = 5,
                        PostResolveCardArchetype = postResolveCardArchetype
                     };
                    inputDeps = job.Schedule(this, inputDeps);
                    barrier.AddJobHandleForProducer(inputDeps);
                }
        }
            data.Dispose();
            
            return inputDeps;
        }
    }

    
}