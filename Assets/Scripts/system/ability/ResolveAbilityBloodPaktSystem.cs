
using CCG;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace system
{
    public class ResolveAbilityBloodPaktSystem : JobComponentSystem
    {


        private EntityQuery resolveQuery;
        private EntityQuery playerQuery;
        
        private EndSimulationEntityCommandBufferSystem barrier;
        private EntityArchetype postResolveCardArchetype;

        protected override void OnCreate()
        {
            resolveQuery = GetEntityQuery(ComponentType.ReadOnly<ResolveAbilityBloodPaktData>());
            playerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerData>());
            
            
            barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            postResolveCardArchetype = EntityManager.CreateArchetype(typeof(PostResolveCardData));
        }
        
        private struct ResolveAbilityJob: IJobForEachWithEntity<ResolveAbilityBloodPaktData>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            public int PlayerHp;
            public int PlayerCoins;
            public int MonsterHp;
            public Entity PlayerEntity;
            public EntityArchetype PostresolveCardArhetype;
            
            
            
            public void Execute(Entity entity, int index,  [ReadOnly]ref ResolveAbilityBloodPaktData resolveAbilityDat)
            {
                EntityCommandBuffer.RemoveComponent<ResolveAbilityBloodPaktData>(index,entity);

                var targetMonster = resolveAbilityDat.TargetMonster;
                EntityCommandBuffer.SetComponent(index, targetMonster, new StatData
                {
                    Value = PlayerHp
                });

                EntityCommandBuffer.AddComponent(index,targetMonster,new DirtyStatData());
                EntityCommandBuffer.SetComponent(index,PlayerEntity,new PlayerData
                {
                    Hp=MonsterHp,
                    Coins = PlayerCoins
                        
                });
                var e = EntityCommandBuffer.CreateEntity(index);
                EntityCommandBuffer.AddComponent(index,e,new SetHpData
                {
                    Amount = MonsterHp
                });
                EntityCommandBuffer.CreateEntity(index, PostresolveCardArhetype);

            }
        }
        

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var resolveData = resolveQuery.ToComponentDataArray<ResolveAbilityBloodPaktData>(Allocator.TempJob);
            if (resolveData.Length == 0)
            {
                resolveData.Dispose();
                return inputDeps;
            }

            var playerData = playerQuery.ToComponentDataArray<PlayerData>(Allocator.TempJob);
            var playerEntities = playerQuery.ToEntityArray(Allocator.TempJob);
            for (var i = 0; i < playerData.Length; i++)
            {
                var targetEntity = resolveData[i].TargetMonster;
                if (EntityManager.HasComponent<MonsterData>(targetEntity) &&
                    EntityManager.HasComponent<StatData>(targetEntity))
                {
                    var playerHp = playerData[0].Hp;
                    var playerCoins = playerData[0].Coins;
                    var monsterHp = EntityManager.GetComponentData<StatData>(targetEntity).Value;
                    var job = new ResolveAbilityJob
                    {
                        EntityCommandBuffer = barrier.CreateCommandBuffer().ToConcurrent(),
                        PlayerHp = playerHp,
                        PlayerCoins = playerCoins,
                        MonsterHp = monsterHp,
                        PlayerEntity = playerEntities[0],
                        PostresolveCardArhetype = postResolveCardArchetype
                    };
                    inputDeps = job.Schedule(this, inputDeps);
                    barrier.AddJobHandleForProducer(inputDeps);
                }


            }
            
            playerEntities.Dispose();
            playerData.Dispose();
            resolveData.Dispose();
            
            return inputDeps;
        }
    }

   
}