namespace PokaiLand
{
    public interface ISystem<out T>
    {
        T Init(params object[] args);
    }

    public interface ISystemBase
    {
        void Init(params object[] args);
    }
}