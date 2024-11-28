namespace PokaiLand
{
    public interface ISystem<out T>
    {
        T Init(params object[] args);
    }
}