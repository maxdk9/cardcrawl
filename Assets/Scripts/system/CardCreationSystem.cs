using System;
using Unity.Entities;

namespace DefaultNamespace
{
    public class CardCreationSystem : ComponentSystem
    {
        protected override void OnCreate()
        {
            CreateMonsterCards();
            CreateSwordCards();
            CreateShieldCards();
            CreatePotionCards();
            CreateCoinCards();
            CreateAbilityCards();

            Enabled = false;
        }

        protected override void OnUpdate()
        {
        }

        private void CreateMonsterCards()
        {
            var defaultPowers = new []
            {
                10, 10, 10,
                9, 9,
                8, 8,
                7, 7,
                6, 6,
                5, 5,
                4, 4,
                3, 3,
                2, 2
            };

            foreach (var power in defaultPowers)
            {
                var card = EntityManager.CreateEntity(CardArchetypes.MonsterCardArchetype);
                EntityManager.SetComponentData(card, new StatData { Value = power });
            }
        }

        private void CreateSwordCards()
        {
            var defaultDmg = new []
            {
                7, 6, 5, 4, 3, 2
            };

            foreach (var dmg in defaultDmg)
            {
                var card = EntityManager.CreateEntity(CardArchetypes.SwordCardArchetype);
                EntityManager.SetComponentData(card, new StatData { Value = dmg });
            }
        }
        
        private void CreateShieldCards()
        {
            var defaultResistances = new []
            {
                7, 6, 5, 4, 3, 2
            };

            foreach (var resistance in defaultResistances)
            {
                var card = EntityManager.CreateEntity(CardArchetypes.ShieldCardArchetype);
                EntityManager.SetComponentData(card, new StatData { Value = resistance });
            }
        }

        private void CreatePotionCards()
        {
            var defaultHp = new []
            {
                10, 9, 8, 7, 6, 5, 4, 3, 2
            };

            foreach (var hp in defaultHp)
            {
                var card = EntityManager.CreateEntity(CardArchetypes.PotionCardArchetype);
                EntityManager.SetComponentData(card, new StatData { Value = hp });
            }
        }
        
        private void CreateCoinCards()
        {
            var defaultAmounts = new []
            {
                10, 9, 8, 7, 6, 5, 4, 3, 2
            };

            foreach (var amount in defaultAmounts)
            {
                var card = EntityManager.CreateEntity(CardArchetypes.CoinsCardArchetype);
                EntityManager.SetComponentData(card, new StatData { Value = amount });
            }
        }

        private void CreateAbilityCards()
        {
            foreach (var ability in Enum.GetValues(typeof(AbilityType)))
            {
                var card = EntityManager.CreateEntity(CardArchetypes.AbilityCardArchetype);
                EntityManager.SetComponentData(
                    card,
                    new AbilityData
                    {
                        Type = (AbilityType)ability
                    });
            }
        }
    }
}