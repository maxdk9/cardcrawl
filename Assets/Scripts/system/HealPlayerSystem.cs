using CCG;
using DefaultNamespace;
using Unity.Collections;
using Unity.Entities;

namespace system
{
    public class HealPlayerSystem : ComponentSystem
    {

        private EntityQuery healQuery;
        private EntityQuery playerQuery;

        protected override void OnCreate()
        {
            healQuery=GetEntityQuery(ComponentType.ReadOnly<HealPlayerData>());

            playerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerData>());
        }


        protected override void OnUpdate()
        {
            var healData = healQuery.ToComponentDataArray<HealPlayerData>(Allocator.TempJob);
            if (healData.Length == 0)
            {
                healData.Dispose();
                return;
            }

            int healed = 0;


            var playerData = playerQuery.ToComponentDataArray<PlayerData>(Allocator.TempJob);
            PlayerData player = playerData[0];

            for (var i = 0; i < healData.Length; i++)
            {
                healed += healData[i].Amount;
            }

            int newHp = player.Hp + healed;
            if (newHp > GameplayConstants.MaxPlayerHp)
            {
                newHp = GameplayConstants.MaxPlayerHp;
            }

            var playerEntities = playerQuery.ToEntityArray(Allocator.TempJob);
            
            PostUpdateCommands.SetComponent(playerEntities[0],new PlayerData
            {
                Hp=newHp,
                Coins=player.Coins
            });
                
            playerData.Dispose();
            healData.Dispose();

        }
    }
}