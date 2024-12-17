using PokaiLand.Gameplay.Map;

namespace PokaiLand
{
    public interface IPostProcessMapObjectData<in T> where T : MapObjectData
    {
        void HandlePostProcessMapObjectData(T mapObjectData);
    }
}