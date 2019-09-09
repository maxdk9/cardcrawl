using configuration;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Assertions;

namespace DefaultNamespace
{
    public class CardDealingSystem : ComponentSystem
    {
        private EntityQuery dealCardsQuery;
        private GameBoard gameBoard;
        private TextMeshProUGUI deckSizeText;
        private GameConfiguration gameConfig;
        private DeckCreationSystem deckCreationSystem;

        private bool applyFaith;

        protected override void OnCreate()
        {
            dealCardsQuery = GetEntityQuery(ComponentType.ReadOnly<DealCardsFromDeckData>());
        }


        protected override void OnStartRunning()
        {
            deckCreationSystem = World.GetExistingSystem<DeckCreationSystem>();
            
            Assert.IsNotNull(deckCreationSystem);
            gameConfig = Resources.Load<GameConfiguration>("GameConfiguration");

            var boardGo = GameObject.Find("GameBoard");
            Assert.IsNotNull(boardGo);

            gameBoard = boardGo.GetComponent<GameBoard>();
            Assert.IsNotNull(gameBoard);
            
            var deckSizeTextGo = GameObject.Find("DeckSizeText");
            Assert.IsNotNull(deckSizeTextGo);
            deckSizeText = deckSizeTextGo.GetComponent<TextMeshProUGUI>();
            Assert.IsNotNull(deckSizeText);

            deckSizeText.text = deckCreationSystem.Deck.Length.ToString();
        }

        protected override void OnUpdate()
        {
            var amount = 0;
            Entities.With(dealCardsQuery).ForEach((Entity entity, ref DealCardsFromDeckData data) =>
            {
                PostUpdateCommands.DestroyEntity(entity);
                amount += data.Amount;
            });
            DealCards(amount);



        }

        private void DealCards(int amount)
        {
            var deck = deckCreationSystem.Deck;
            var j = 0;
            for (var i = 0; i < amount; ++i)
            {
                if (deck.Length == 0)
                    break;
        
                var card = deck[0];
                var cardView = CreateCardView(card);
        
                var cardEntity = cardView.GetComponent<GameObjectEntity>().Entity;

                var selectedSlotEntity = Entity.Null;
                var selectedSlotData = default(SlotData);
                var foundFreeSlot = false;
                while (!foundFreeSlot)
                {
                    var slotEntity = gameBoard.TopRowSlots[j].GetComponent<GameObjectEntity>().Entity;
                    var slotData = EntityManager.GetComponentData<SlotData>(slotEntity);
                    if (slotData.Occupied == 0)
                    {
                        selectedSlotEntity = slotEntity;
                        selectedSlotData = slotData;
                        foundFreeSlot = true;
                    }
                    else
                    {
                        ++j;
                    }
                }

                cardView.transform.position = gameBoard.TopRowSlots[j].transform.position;
                cardView.Slot = gameBoard.TopRowSlots[j];
                EntityManager.SetComponentData(selectedSlotEntity, new SlotData
                {
                    Type = selectedSlotData.Type,
                    Occupied = 1,
                    Entity = cardEntity
                });

                EntityManager.AddComponentData(cardEntity, new CardSlotData
                {
                    Type = SlotType.Deck
                });
        
                deck.RemoveAtSwapBack(0);
                EntityManager.DestroyEntity(card);
            }

            deckSizeText.text = deckCreationSystem.Deck.Length.ToString();
        }

        private CardView CreateCardView(Entity card)
        {
            throw new System.NotImplementedException();
        }
    }
}