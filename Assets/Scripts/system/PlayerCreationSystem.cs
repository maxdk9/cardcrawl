using Unity.Entities;

namespace DefaultNamespace
{
    public class PlayerCreationSystem:ComponentSystem
    {

        protected override void OnUpdate()
        {
            
        }

        protected override void OnCreate()
        {
            var player = EntityManager.CreateEntity(typeof(PlayerData));
             EntityManager.SetComponentData(player,new PlayerData(){Hp=GameplayConstants.MaxPlayerHp});    
            Enabled = false;
        }

        
    }

    public class GameplayConstants
    {
        public static int MaxPlayerHp = 20;
        public const int DeckSize = 54;

        public const float ResetCardDuration = 0.3f;

        public const float DisabledCardFadeOutValue = 0.7f;
        public const float DisabledCardFadeOutDuration = 0.3f;

        public const float KilledCardFadeOutDuration = 0.4f;

    }
}