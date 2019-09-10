using Unity.Entities;

namespace CCG
{
    public struct GameFinishedData : IComponentData
    {
        public bool PlayerWon;
    }
}