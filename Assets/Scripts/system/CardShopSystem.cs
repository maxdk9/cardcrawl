using System.Collections.Generic;
using CCG;
using Unity.Entities;
using UnityEngine;

namespace system
{
    public class CardShopSystem :ComponentSystem
    {

        private EntityQuery sellCardQuery;
        private EntityQuery playerQuery;
        private EntityArchetype lootCoinsEntityArchetype;
        private EntityArchetype postResolveCardArchetype;


        protected override void OnCreate()
        {
            sellCardQuery = GetEntityQuery(ComponentType.ReadOnly<Transform>(),
                ComponentType.ReadOnly<SellCardData>());

            playerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerData>());
            lootCoinsEntityArchetype = EntityManager.CreateArchetype(typeof(LootCoinsData));
            postResolveCardArchetype = EntityManager.CreateArchetype(typeof(PostResolveCardData));
            


        }

        protected override void OnUpdate()
        {
            List<CardView> cardsToKill=new List<CardView>();
            
            Entities.With(sellCardQuery).ForEach((Entity ent, CardView card, ref SellCardData sellCardData) =>
            {
                PostUpdateCommands.RemoveComponent<SellCardData>(ent);
                int price = sellCardData.Price;
                
                Entities.With(playerQuery).ForEach((Entity playerEntity, ref PlayerData playerData) =>
                    {
                        playerData.Coins += price;
                    });
                cardsToKill.Add(card);
                
            });

            foreach (var VARIABLE in cardsToKill)
            {
                VARIABLE.Kill();
            }

            if (cardsToKill.Count > 0)
            {
                PostUpdateCommands.CreateEntity(postResolveCardArchetype);
            }
            
        }
    }
}