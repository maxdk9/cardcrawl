
using System.ComponentModel;
using CCG;
using DefaultNamespace;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace system
{
    public class ResolveAbilityLifeSystem : JobComponentSystem
    {


        private EntityQuery resolveQuery;
        private EntityQuery playerQuery;
        private EndSimulationEntityCommandBufferSystem barrier;
        private EntityArchetype postResolveCardArchetype;

        protected override void OnCreate()
        {
            resolveQuery = GetEntityQuery(ComponentType.ReadOnly<ResolveAbilityLIfeData>());
            playerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerData>());
            barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            postResolveCardArchetype = EntityManager.CreateArchetype(typeof(EntityArchetype));
            
        }
        
        private struct ResolveAbilityJob: IJobForEachWithEntity<ResolveAbilityLIfeData>
        {

            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            public int PlayerHP;
            public int PlayerCoins;
            public Entity PlayerEntity;
            public EntityArchetype PostResolveCardArchetype;
            
            
            public void Execute(Entity entity, int index, [Unity.Collections.ReadOnly] ref ResolveAbilityLIfeData c0)
            {
                EntityCommandBuffer.RemoveComponent<ResolveAbilityLIfeData>(index,entity);
                EntityCommandBuffer.SetComponent(index,PlayerEntity,new PlayerData
                {
                    Hp=PlayerHP,
                    Coins = PlayerCoins
                });
                var e = EntityCommandBuffer.CreateEntity(index);
                EntityCommandBuffer.AddComponent(index,e,new SetHpData
                {
                    Amount=PlayerHP
                });
                EntityCommandBuffer.CreateEntity(index, PostResolveCardArchetype);
            }
        }
        

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var resolveEntities = resolveQuery.ToEntityArray(Allocator.TempJob);
            if (resolveEntities.Length == 0)
            {
                resolveEntities.Dispose();
                return inputDeps;
            }

            var playerData = playerQuery.ToComponentDataArray<PlayerData>(Allocator.TempJob);
            var playerEntities = playerQuery.ToEntityArray(Allocator.TempJob);
            for (var i = 0; i < playerData.Length; i++)
            {
                int playerHp = playerData[0].Hp;
                int playerCoins = playerData[0].Coins;
                playerHp += 5;
                if (playerHp >= GameplayConstants.MaxPlayerHp)
                {
                    GameplayConstants.MaxPlayerHp = playerHp;
                }

                var job = new ResolveAbilityJob
                {
                    EntityCommandBuffer = barrier.CreateCommandBuffer().ToConcurrent(),
                    PlayerHP = playerHp,
                    PlayerCoins = playerCoins,
                    PostResolveCardArchetype = postResolveCardArchetype,
                    PlayerEntity = playerEntities[0]
                };

                inputDeps = job.Schedule(this, inputDeps);
                barrier.AddJobHandleForProducer(inputDeps);

            }
            
            playerEntities.Dispose();
            playerData.Dispose();
            resolveEntities.Dispose();
            return inputDeps;
            
            
            
            
            
        }
    }

    
}