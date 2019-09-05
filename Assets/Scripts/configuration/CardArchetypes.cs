using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public static class CardArchetypes
{
    public static EntityArchetype MonsterCardArchetype;
    public static EntityArchetype SwordCardArchetype;
    public static EntityArchetype ShieldCardArchetype;
    public static EntityArchetype PotionCardArchetype;
    public static EntityArchetype CoinsCardArchetype;
    public static EntityArchetype AbilityCardArchetype;

    static CardArchetypes()
    {
        var entityManager = World.Active.EntityManager;
        MonsterCardArchetype = entityManager.CreateArchetype(
            typeof(CardData), typeof(MonsterData), typeof(StatData));
        SwordCardArchetype = entityManager.CreateArchetype(
            typeof(CardData), typeof(SwordData), typeof(StatData));
        ShieldCardArchetype = entityManager.CreateArchetype(
            typeof(CardData), typeof(ShieldData), typeof(StatData));
        PotionCardArchetype = entityManager.CreateArchetype(
            typeof(CardData), typeof(PotionData), typeof(StatData));
        CoinsCardArchetype = entityManager.CreateArchetype(
            typeof(CardData), typeof(CoinsData), typeof(StatData));
        AbilityCardArchetype = entityManager.CreateArchetype(
            typeof(CardData), typeof(AbilityData));
    }
}
