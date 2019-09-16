using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace CCG
{
    public class ResolveMonsterOverPlayerSystem : ComponentSystem
    {

        private EntityQuery monsterQuery;
        private EntityQuery playerQuery;
        private EntityArchetype damagePlayerArchetype;
        private EntityArchetype postResolveCardArchetype;

        protected override void OnCreate()
        {
            monsterQuery = GetEntityQuery(ComponentType.ReadOnly<Transform>(),
                ComponentType.ReadOnly<MonsterData>(),
                ComponentType.ReadOnly<StatData>(),
                ComponentType.ReadOnly<ResolvePlayerInteractionData>()
            );
            
            
            

            playerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerData>());
            damagePlayerArchetype=EntityManager.CreateArchetype(typeof(DamagePlayerData));
             postResolveCardArchetype=EntityManager.CreateArchetype(typeof(PostResolveCardData));
        }

        protected override void OnUpdate()
        {
            var monsterEntities = monsterQuery.ToEntityArray(Allocator.TempJob);
            var playerData = playerQuery.ToEntityArray(Allocator.TempJob);

            if (monsterEntities.Length != playerData.Length)
            {
                monsterEntities.Dispose();
                playerData.Dispose();
                return;
            }
            List<CardView> cardsToKill=new List<CardView>();
            int playerDamage = 0;
            Entities.With(monsterQuery).ForEach(
                (Entity monsterEntity, CardView monsterCard, ref StatData monsterStat) =>
                {
                    PostUpdateCommands.RemoveComponent<ResolvePlayerInteractionData>(monsterEntity);
                    playerDamage += monsterStat.Value;
                    cardsToKill.Add(monsterCard);
                });


            foreach (CardView VARIABLE in cardsToKill)
            {
                VARIABLE.Kill();
            }
            
            if (playerDamage > 0)
            {
                var e = PostUpdateCommands.CreateEntity(damagePlayerArchetype);
                PostUpdateCommands.SetComponent(e,new DamagePlayerData
                {
                    Amount = playerDamage
                });
                
            }
            
            
            
            monsterEntities.Dispose();
            playerData.Dispose();

        }
    }
}