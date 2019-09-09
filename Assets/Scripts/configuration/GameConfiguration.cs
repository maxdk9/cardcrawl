using UnityEngine;

namespace configuration
{
    [CreateAssetMenu(menuName = "CCG/Game configuration", fileName = "GameConfiguration", order = 0)]
    public class GameConfiguration : ScriptableObject
    {
        public GameObject MonsterCardPrefab;
        public GameObject SwordCardPrefab;
        public GameObject ShieldCardPrefab;
        public GameObject PotionCardPrefab;
        public GameObject CoinsCardPrefab;
        public GameObject AbilityCardPrefab;
        public GameObject GameFinishedPopupPrefab;
    }
}