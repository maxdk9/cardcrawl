using CCG;
using Unity.Entities;

namespace DefaultNamespace
{
    public class DamagePlayerSystem : ComponentSystem
    {
        private EntityQuery damageQuery;
        private EntityQuery playerQuery;
        private EntityArchetype gameFinishedArchetype;

        protected override void OnCreate()
        {
            damageQuery = GetEntityQuery(ComponentType.ReadOnly<DamagePlayerData>());
            playerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerData>());
        }


        protected override void OnStartRunning()
        {
            gameFinishedArchetype = EntityManager.CreateArchetype(typeof(GameFinishedData));
        }


        protected override void OnUpdate()
        {
            var finalHp = -1;
            Entities.With(playerQuery).ForEach((Entity damageEntity, ref DamagePlayerData damageData) =>
                {
                    var damage = damageData.Amount;
                    Entities.With(playerQuery).ForEach((Entity playerEntity, ref PlayerData playerData) =>
                    {
                        var newHp = playerData.Hp - damage;
                        if (newHp < 0)
                        {
                            newHp = 0;
                        }

                        playerData.Hp = newHp;
                        finalHp = newHp;
                    });
                }
                );

            if (finalHp == 0)
            {
                var e = PostUpdateCommands.CreateEntity(gameFinishedArchetype);
                PostUpdateCommands.SetComponent(e,new GameFinishedData
                {
                    PlayerWon=false
                });
            }
            
            
        }
    }
}