using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
[AlwaysUpdateSystem]
public class SlotManagementSystem : ComponentSystem
{
   
   
   
        private EntityQuery query;

        private int numEmptySlots;

        private float accTime;
        private bool isQueuingCardDealing;

        protected override void OnCreate()
        {
            query = GetEntityQuery(
                ComponentType.ReadOnly<SlotEmptiedData>());
        }

        protected override void OnUpdate()
        {
            Entities.With(query).ForEach(entity =>
            {
                PostUpdateCommands.DestroyEntity(entity);

                ++numEmptySlots;
                if (numEmptySlots == 3)
                {
                    numEmptySlots = 0;
                    isQueuingCardDealing = true;
                }
            });

            if (isQueuingCardDealing)
            {
                accTime += Time.deltaTime;
                if (accTime >= 1.0f)
                {
                    accTime = 0.0f;
                    isQueuingCardDealing = false;
                    DealCardsAfterDelay();
                }
            }
        }

        private void DealCardsAfterDelay()
        {
            var puc = PostUpdateCommands;
            var e = puc.CreateEntity();
            puc.AddComponent(e, new DealCardsFromDeckData
            {
                Amount = 3
            });

            foreach (CardView card in Object.FindObjectsOfType<CardView>())
            {
                if (card.isDisabled)
                    card.Kill();
            }
        }
    }

