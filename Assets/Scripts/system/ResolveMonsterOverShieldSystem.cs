using System.Collections.Generic;
using CCG;
using Unity.Collections;
using Unity.Entities;

namespace DefaultNamespace
{
    public class ResolveMonsterOverShieldSystem : ComponentSystem
    {
        private EntityQuery monsterQuery;
        private EntityQuery shieldQuery;

        private EntityArchetype damagePlayerArchetype;
        private EntityArchetype postResolveCardArchetype;

        protected override void OnCreate()
        {
            monsterQuery = GetEntityQuery(
                ComponentType.ReadOnly<MonsterData>(),
                ComponentType.ReadOnly<StatData>(),
                ComponentType.ReadOnly<ResolveCardInteractionData>(),
                ComponentType.ReadOnly<CardView>());

            shieldQuery = GetEntityQuery(
                ComponentType.ReadOnly<ShieldData>(),
                ComponentType.ReadOnly<StatData>(),
                ComponentType.ReadOnly<ResolveCardInteractionData>(),
                ComponentType.ReadOnly<CardView>());


            damagePlayerArchetype = EntityManager.CreateArchetype(typeof(DamagePlayerData));
            postResolveCardArchetype = EntityManager.CreateArchetype(typeof(PostResolveCardData));
        }

        protected override void OnUpdate()
        {
            var monsterEntities = monsterQuery.ToEntityArray(Allocator.TempJob);
            var shieldEntities = shieldQuery.ToEntityArray(Allocator.TempJob);
            if (monsterEntities.Length != shieldEntities.Length)
            {
                shieldEntities.Dispose();
                monsterEntities.Dispose();
            }


            var cardsToKill = new List<CardView>();
            Entities.With(monsterQuery).ForEach(
                (Entity monsterEntity, CardView monsterCard, ref StatData monsterStat) =>
                {
                    PostUpdateCommands.RemoveComponent<ResolveCardInteractionData>(monsterEntity);
                    var monsterValue = monsterStat.Value;

                    Entities.With(shieldQuery).ForEach(
                        (Entity shieldEntity, CardView shieldCard, ref StatData shieldStat) =>
                        {
                            PostUpdateCommands.RemoveComponent<ResolveCardInteractionData>(shieldEntity);
                            var shieldValue = shieldStat.Value;
                            if (monsterValue >= shieldValue)
                            {
                                cardsToKill.Add(monsterCard);
                                cardsToKill.Add(shieldCard);
                                var e = PostUpdateCommands.CreateEntity(damagePlayerArchetype);
                                PostUpdateCommands.SetComponent(e, new DamagePlayerData
                                {
                                    Amount = monsterValue - shieldValue
                                });
                            }
                            else
                            {
                                cardsToKill.Add(monsterCard);
                                int newShieldStatValue = shieldValue - monsterValue;
                                shieldCard.SetStat(newShieldStatValue);
                                shieldStat.Value = newShieldStatValue;
                            }
                        }
                    );
                }
            );
            foreach (CardView cw in cardsToKill)
            {
                cw.Kill();
            }

            PostUpdateCommands.CreateEntity((postResolveCardArchetype));
            shieldEntities.Dispose();
            monsterEntities.Dispose();
        }
    }
}