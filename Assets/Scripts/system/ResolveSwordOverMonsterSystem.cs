using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace CCG
{
    public class ResolveSwordOverMonsterSystem : ComponentSystem
    {

        private EntityQuery swordQuery;
        private EntityQuery monsterQuery;
        private EntityArchetype postResolveCardArchetype;


        protected override void OnCreate()
        {
            swordQuery = GetEntityQuery(ComponentType.ReadOnly<Transform>(),
            ComponentType.ReadOnly<SwordData>(),
            ComponentType.ReadOnly<StatData>(),
                ComponentType.ReadOnly<ResolveCardInteractionData>()
                );
            
            monsterQuery = GetEntityQuery(ComponentType.ReadOnly<Transform>(),
                ComponentType.ReadOnly<MonsterData>(),
                ComponentType.ReadOnly<StatData>(),
                ComponentType.ReadOnly<ResolveCardInteractionData>()
            );
        }

        protected override void OnUpdate()
        {
            var swordEntities = swordQuery.ToEntityArray(Allocator.TempJob);
            var monsterEntities = monsterQuery.ToEntityArray(Allocator.TempJob);

            if (swordEntities.Length != monsterEntities.Length)
            {
                swordEntities.Dispose();
                monsterEntities.Dispose();
                return;
            }
            
           List< CardView> cardsToKill=new List<CardView>();
            Entities.With(swordQuery).ForEach((Entity swordEntity, CardView swordCard, ref StatData swordStat) =>
            {
                PostUpdateCommands.RemoveComponent<ResolveCardInteractionData>(swordEntity);
                var swordValue = swordStat.Value;
                Entities.With(monsterQuery).ForEach(
                    (Entity monsterEntity, CardView monsterCard, ref StatData monsterStat) =>
                    {
                        PostUpdateCommands.RemoveComponent<ResolveCardInteractionData>(monsterEntity);
                        int MonsterValue = monsterStat.Value;
                        if (swordValue > MonsterValue)
                        {
                            cardsToKill.Add(monsterCard);
                            cardsToKill.Add(swordCard);
                            
                        }
                        else
                        {
                            cardsToKill.Add(swordCard);
                            int newMonsterStatValue = MonsterValue - swordValue;

                            monsterStat.Value = newMonsterStatValue;
                            monsterCard.SetStat(newMonsterStatValue);


                        }
                    });



            });
            foreach (CardView cardview in cardsToKill)
            {
                cardview.Kill();
            }

            PostUpdateCommands.CreateEntity(postResolveCardArchetype);
            monsterEntities.Dispose();
            swordEntities.Dispose();

        }
    }
}