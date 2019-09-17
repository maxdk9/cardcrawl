using CCG;
using TMPro;
using Unity.Entities;
using UnityEngine;

namespace system
{
    [UpdateAfter(typeof(LootCoinsSystem))]
    public class UpdatePlayerCoinsUISystem : ComponentSystem
    {
        private EntityQuery query;
        private int currentCoin;
        private TextMeshPro playerCoinsText;
        
        
        protected override void OnCreate()
        {
            query = GetEntityQuery(ComponentType.ReadOnly<LootCoinsData>());
        }

        protected override void OnStartRunning()
        { 
            playerCoinsText = GameObject.Find("CoinText").GetComponent<TextMeshPro>();
        }


        protected override void OnUpdate()
        {
            int amount = 0;
            Entities.With(query).ForEach((Entity ent, ref LootCoinsData lootCoinsData) =>
            {
                PostUpdateCommands.DestroyEntity(ent);
                amount += lootCoinsData.Amount;
            });

            currentCoin = amount;
            playerCoinsText.SetText($"{currentCoin}");
        }
    }
}