using CCG;
using DefaultNamespace;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Assertions;

namespace system
{
    public class PostResolveCardSystem : ComponentSystem
    {
        private EntityQuery resolveQuery;
        private EntityQuery playerQuery;
        private DeckCreationSystem deckCreationSystem;
        private EntityArchetype gameFinishedArchetype;
        private GameBoard gameBoard;


        protected override void OnCreate()
        {
            resolveQuery = GetEntityQuery(ComponentType.ReadOnly<PostResolveCardData>());
            playerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerData>());
            gameFinishedArchetype = EntityManager.CreateArchetype(typeof(GameFinishedData));
        }

        protected override void OnStartRunning()
        {
            deckCreationSystem = World.GetExistingSystem<DeckCreationSystem>();

            var boardGo = GameObject.Find("GameBoard");
            Assert.IsNotNull(boardGo);
            gameBoard = boardGo.GetComponent<GameBoard>();
            Assert.IsNotNull(gameBoard);
        }

        protected override void OnUpdate()
        {
            var resolveEntities = resolveQuery.ToEntityArray(Allocator.TempJob);
            var playerData = playerQuery.ToComponentDataArray<PlayerData>(Allocator.TempJob);

            var deck = deckCreationSystem.Deck;
            for (var i = 0; i < resolveEntities.Length; ++i)
            {
                if (deck.Length == 0 && playerData[0].Hp > 0)
                {
                    var numOccupiedSlots = 0;
                    var j = 0;
                    while (j < 4)
                    {
                        var slotEntity = gameBoard.TopRowSlots[j++].GetComponent<GameObjectEntity>().Entity;
                        var slotData = EntityManager.GetComponentData<SlotData>(slotEntity);
                        if (slotData.Occupied == 1)
                            numOccupiedSlots += 1;
                    }

                    if (numOccupiedSlots == 0)
                    {
                        var e = PostUpdateCommands.CreateEntity(gameFinishedArchetype);
                        PostUpdateCommands.SetComponent(e, new GameFinishedData {PlayerWon = true});
                    }
                }

                PostUpdateCommands.DestroyEntity(resolveEntities[i]);
            }

            playerData.Dispose();
            resolveEntities.Dispose();
        }
    }
}