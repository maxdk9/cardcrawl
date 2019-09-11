using CCG;
using DefaultNamespace;
using TMPro;
using Unity.Entities;
using UnityEngine;

namespace system
{
    public class UpdatePlayerHPUISystem: ComponentSystem
    {
        private EntityQuery damageQuery;
        private EntityQuery healQuery;
        private EntityQuery setHpQuery;

        private int currentHp;
        private TextMeshPro playerHpText;
        
        
        
        protected override void OnCreate()
        {
            damageQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<DamagePlayerData>());
            healQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<HealPlayerData>());
            setHpQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<SetHpData>());
            currentHp = GameplayConstants.MaxPlayerHp;
            
        }


        protected override void OnStartRunning()
        {
            playerHpText = GameObject.Find("HpText").GetComponent<TextMeshPro>();
        }

        protected override void OnUpdate()
        {
            Entities.With(damageQuery).ForEach((Entity ent, ref DamagePlayerData damagePlayerData) =>
            {
                PostUpdateCommands.DestroyEntity(ent);
                currentHp -= damagePlayerData.Amount;
            });
            
            
            Entities.With(healQuery).ForEach((Entity ent, ref HealPlayerData healPlayerData) =>
            {
                PostUpdateCommands.DestroyEntity(ent);
                currentHp += healPlayerData.Amount;
            });
            
            Entities.With(setHpQuery).ForEach((Entity ent, ref SetHpData setHpData) =>
            {
                PostUpdateCommands.DestroyEntity(ent);
                currentHp = setHpData.Amount;
            });

            if (currentHp < 0)
            {
                currentHp = 0;
            }

            if (currentHp > GameplayConstants.MaxPlayerHp)
            {
                currentHp = GameplayConstants.MaxPlayerHp;
            }
            playerHpText.SetText($"{currentHp}/{GameplayConstants.MaxPlayerHp}");
            
            
        }
    }
}