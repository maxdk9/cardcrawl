using CCG;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace system
{
    public class LootCoinsSystem : JobComponentSystem
    {

        private EntityQuery lootCoinsQuery;
        private EntityQuery playerQuery;
        private EndSimulationEntityCommandBufferSystem barrier;

        protected override void OnCreate()
        {
            lootCoinsQuery = GetEntityQuery(ComponentType.ReadOnly<LootCoinsData>());
            playerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerData>());
            barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }

        public struct LootCoinsJob : IJobForEachWithEntity<PlayerData>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            public int Hp;
            public int Coins;
        
        
            public void Execute(Entity entity, int index, ref PlayerData playerData)
            {
                EntityCommandBuffer.SetComponent(index,entity,new PlayerData
                {
                    Hp=Hp,
                    Coins = Coins
                });            
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var lootCoinsData = lootCoinsQuery.ToComponentDataArray<LootCoinsData>(Allocator.TempJob);
            if (lootCoinsData.Length == 0)
            {
                lootCoinsData.Dispose();
                return inputDeps;
            }


            int coins = 0;
            for (var i = 0; i < lootCoinsData.Length; i++)
            {
                coins += lootCoinsData[i].Amount;
            }
                
            var playerData = playerQuery.ToComponentDataArray<PlayerData>(Allocator.TempJob);

            var job = new LootCoinsJob
            {
                EntityCommandBuffer = barrier.CreateCommandBuffer().ToConcurrent(),
                Hp = playerData[0].Hp,
                Coins = coins
            };
            inputDeps = job.Schedule(this, inputDeps);
            barrier.AddJobHandleForProducer(inputDeps);
            playerData.Dispose();
            lootCoinsData.Dispose();
            return inputDeps;

        }
    }
}