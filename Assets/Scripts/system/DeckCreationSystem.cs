using System;
using System.Net;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

using Random = Unity.Mathematics.Random;

namespace DefaultNamespace
{
    
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CardCreationSystem))]
    public class DeckCreationSystem :JobComponentSystem
    {
        public NativeList<Entity> Deck;
        private EntityQuery query;
        private bool retrievedDeck;

        private EndSimulationEntityCommandBufferSystem barrier;


        protected override void OnCreate()
        {
            Deck =new NativeList<Entity>(GameplayConstants.DeckSize,Allocator.Persistent);
            query = GetEntityQuery(ComponentType.ReadOnly<CardData>());
            barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }


        protected override void OnDestroy()
        {
            Deck.Dispose();
        }
        
        [BurstCompile]
        private struct  AddCardToDeckJob:IJob
        {

            public NativeList<Entity> Deck;
            [DeallocateOnJobCompletion] public NativeArray<Entity> Entities;
            
            
            public void Execute()
            {
                for (var i = 0; i < Entities.Length; ++i)
                {
                    Deck.Add(Entities[i]);
                }
            }
        }
        
        [BurstCompile]
        private struct SordDeckJob: IJob
        {
            public NativeList<Entity> Deck;
            public uint Seed;
            
            
            public void Execute()
            {
                var rng = new Random(Seed);
                for (var i = 0; i <= Deck.Length - 2; ++i)
                {
                    var j = rng.NextInt(i, Deck.Length);
                    var tmp = Deck[i];
                    Deck[i] = Deck[j];
                    Deck[j] = tmp;
                }
            }
        }
        
        
        private struct StartGameJob : IJob
        {
            public EntityCommandBuffer CommandBuffer;

            void IJob.Execute()
            {
                var e = CommandBuffer.CreateEntity();
                CommandBuffer.AddComponent(e,new DealCardsFromDeckData{Amount=4});
                
            }
        }
        
        
        

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (retrievedDeck)
            {
                Enabled = false;
                return inputDeps;
            }

            retrievedDeck = true;

            var cardEntities = query.ToEntityArray(Allocator.TempJob);
            var jobA =new AddCardToDeckJob()
            {
                Deck=Deck,
                Entities = cardEntities
                 
            };
            
            var jobB=new SordDeckJob()
            {
                Deck=Deck,
                Seed =(uint)Environment.TickCount
            };
            
            var jobC=new StartGameJob()
            {
                CommandBuffer = barrier.CreateCommandBuffer()
            };

            inputDeps = jobA.Schedule(inputDeps);
            inputDeps = jobB.Schedule(inputDeps);
            inputDeps = jobC.Schedule(inputDeps);
            return inputDeps;


        }
    }
}