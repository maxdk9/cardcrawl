using System;
using CCG;
using DG.Tweening;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;

namespace DefaultNamespace
{
    public class PlayerInputSystem:ComponentSystem
    {
        private Camera mainCamera;
        private LayerMask slotLayer;
        private LayerMask cardLayer;
        private LayerMask shopLayer;

        private bool isDraggingCard;
        private GameObject selectedCard;

        protected override void OnStartRunning()
        {
            mainCamera=Camera.main;
            slotLayer = 1 << LayerMask.NameToLayer("Slot");
            shopLayer = 1 << LayerMask.NameToLayer("Shop");
            cardLayer = 1 << LayerMask.NameToLayer("Card");
        }


        protected override void OnUpdate()
        {
            var mousPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

            if (isDraggingCard)
            {
                var newCardPos = mousPos;
                newCardPos.z = 0;
                selectedCard.transform.position = newCardPos;
            }

            if (Input.GetMouseButtonDown(0))
            {
                var hits = Raycast(mousPos, Vector3.forward, slotLayer, shopLayer);
                var slotHit = hits.Item1;
                var cardHit = hits.Item2;
                if(slotHit.collider!=null&&cardHit.collider!=null&&IsCardSelectable(cardHit.collider.gameObject))
                {
                    isDraggingCard = true;
                    selectedCard = cardHit.collider.gameObject;
                    selectedCard.GetComponent<SortingGroup>().sortingOrder += 1;
                }
            }
            else if (Input.GetMouseButtonUp(0) && isDraggingCard)
            {
                var hits = Raycast(mousPos, Vector3.forward, slotLayer, cardLayer);
                var slotHit = hits.Item1;
                var shopHit= hits.Item2;
                if (slotHit.collider != null)
                {
                    ProcessCard(selectedCard, slotHit.collider.gameObject);
                }
                else if(shopHit.collider!=null&& IsCardSellable(selectedCard))
                {
                    SellCard(selectedCard);
                }
                else
                {
                    ResetSelectedCard();
                }

                isDraggingCard = false;
                selectedCard = null;
            }   
        }

        private bool IsCardSelectable(GameObject cardGo)
        {
            var card = cardGo.GetComponent<GameObjectEntity>().Entity;
            CardView cardView = cardGo.GetComponent<CardView>();
            var slotGo = cardView.Slot;
            var slot = slotGo.GetComponent<GameObjectEntity>().Entity;
            if (EntityManager.HasComponent<ShieldData>(card) &&
                EntityManager.GetComponentData<SlotData>(slot).Type != SlotType.Deck &&
                EntityManager.GetComponentData<SlotData>(slot).Type != SlotType.Bag)
            {
                return false;
            }

            if (cardView.isDisabled)
            {
                return false;
            }

            return true;
        }

        private bool IsCardSellable(GameObject cardGo)
        {
            var card = cardGo.GetComponent<GameObjectEntity>().Entity;
            if (!EntityManager.HasComponent<MonsterData>(card))
            {
                return true;
            }

            return false;
        }

        private void ProcessCard(GameObject entityGo, GameObject slotGo)
        {
            var entity = entityGo.GetComponent<GameObjectEntity>().Entity;
            if (EntityManager.HasComponent<MonsterData>(entity))
            {
                ProcessMonsterCard(entityGo, slotGo);
            }
            
            if (EntityManager.HasComponent<SwordData>(entity))
            {
                ProcessSwordCard(entityGo, slotGo);
            }
            if (EntityManager.HasComponent<ShieldData>(entity))
            {
                ProcessShieldCard( slotGo);
            }
            
            if (EntityManager.HasComponent<PotionData>(entity))
            {
                ProcessPotionCard(entityGo, slotGo);
            }
            
            if (EntityManager.HasComponent<CoinsData>(entity))
            {
                ProcessCoinCard(entityGo, slotGo);
            }
            
            
            
        }

        private void ProcessSwordCard(GameObject entityGo, GameObject slotGo)
        {
            throw new NotImplementedException();
        }

        private void ProcessShieldCard( GameObject slotGo)
        {
            var slotEntity = slotGo.GetComponent<GameObjectEntity>().Entity;
            SlotData slotData = EntityManager.GetComponentData<SlotData>(slotEntity);
            if (slotData.Type != SlotType.Deck &&
                slotData.Type != SlotType.Player &&
                slotData.Occupied == 0)
            {
                EnterSlot(selectedCard,slotGo);
            }
            else
            {
                ResetSelectedCard();
            }
        }

        private void ProcessPotionCard(GameObject entityGo, GameObject slotGo)
        {
            throw new NotImplementedException();
        }

        private void ProcessCoinCard(GameObject entityGo, GameObject slotGo)
        {
            throw new NotImplementedException();
        }

        private void ProcessMonsterCard(GameObject monsterGo, GameObject slotGo)
        {
            var monsterEntity = monsterGo.GetComponent<GameObjectEntity>().Entity;
            MonsterData monsterData = EntityManager.GetComponentData<MonsterData>(monsterEntity);
            var slotEntity = slotGo.GetComponent<GameObjectEntity>().Entity;
            SlotData slotData = EntityManager.GetComponentData<SlotData>(slotEntity);
            if (slotData.Type != SlotType.Deck &&
                slotData.Type != SlotType.Bag &&
                slotData.Occupied == 1 &&
                EntityManager.HasComponent<ShieldData>(slotEntity))
            {
                EntityManager.AddComponentData(monsterEntity,new ResolveCardInteractionData());
                EntityManager.AddComponentData(slotData.Entity,new ResolveCardInteractionData());
            }
            else if(slotData.Type==SlotType.Player)
            {
                EntityManager.AddComponentData(monsterEntity,new ResolvePlayerInteractionData());
            }
            else
            {
                ResetSelectedCard();
            }
               
        }
        
        
        


        private ValueTuple<RaycastHit, RaycastHit> Raycast(Vector3 origin, Vector3 direction, LayerMask layer1, LayerMask layer2)
        {
            var results = new NativeArray<RaycastHit>(2, Allocator.TempJob);
            var commands = new NativeArray<RaycastCommand>(2, Allocator.TempJob)
            {
                [0] = new RaycastCommand(origin, direction, Mathf.Infinity, layer1),
                [1] = new RaycastCommand(origin, direction, Mathf.Infinity, layer2)
            };

            var handle = RaycastCommand.ScheduleBatch(commands, results, 1);
            handle.Complete();

            var hits = new ValueTuple<RaycastHit, RaycastHit>(results[0], results[1]);
    
            results.Dispose();
            commands.Dispose();

            return hits;
        }



        private void EnterSlot(GameObject card, GameObject slot)
        {
            ExitSlot(card);
            MoveCard(card, slot.transform.position);
            card.GetComponent<CardView>().Slot = slot;
            var cardEntity = card.GetComponent<GameObjectEntity>().Entity;
            var slotEntity = slot.GetComponent<GameObjectEntity>().Entity;
            SlotData slotData = EntityManager.GetComponentData<SlotData>(slotEntity);
            EntityManager.SetComponentData(slotEntity,new SlotData
            {
                Type=slotData.Type,
                Occupied =  1,
                Entity = cardEntity
                 
            });
            EntityManager.SetComponentData(cardEntity, new CardSlotData
            {
                Type=slotData.Type
            });
        }

        private void ExitSlot(GameObject card)
        {
            CardView cardView = card.GetComponent<CardView>();
            var slot = cardView.Slot;
            cardView.Slot = null;

            var slotEntity = slot.GetComponent<GameObjectEntity>().Entity;
            SlotData slotData = EntityManager.GetComponentData<SlotData>(slotEntity);
            
            EntityManager.SetComponentData(slotEntity,new SlotData
            {
                Type=slotData.Type,
                Occupied = 0,
                Entity = Entity.Null
            });
            if (slotData.Type == SlotType.Deck)
            {
                var evt = EntityManager.CreateEntity();
                EntityManager.AddComponentData(evt, new SlotEmptiedData());
            }
        }
        
        


        private void ResetSelectedCard()
        {
            MoveCard(selectedCard,selectedCard.GetComponent<CardView>().Slot.transform.position);
            
        }
        
        private void MoveCard(GameObject card, Vector3 pos)
        {
            var seq = DOTween.Sequence();
            seq.Append(card.transform.DOMove(pos, GameplayConstants.ResetCardDuration));
            seq.AppendCallback(() => card.GetComponent<SortingGroup>().sortingOrder -= 1);
        }

        private void SellCard(GameObject card)
        {
            var cardEntity = card.GetComponent<GameObjectEntity>().Entity;
            EntityManager.AddComponentData(cardEntity,new SellCardData
            {
                Price = GetValueCoins(cardEntity)
            });
        }

        private int GetValueCoins(Entity cardEntity)
        {
            if (EntityManager.HasComponent<StatData>(cardEntity))
            {
                return EntityManager.GetComponentData<StatData>(cardEntity).Value;
            }

            return 0;
        }

        private void ProcessFortifyCard(SlotData slot, Entity abilityEntity, GameObject abilityGo)
        {
            if (slot.Type != SlotType.Player && slot.Occupied == 1)
            {
                EntityManager.AddComponentData(abilityEntity,new ResolveAbilityFortifyData
                {
                    TargetEntity = slot.Entity
                });
            }
            else
            {
                ResetSelectedCard();
            }
        }
        
        
        
    }
}