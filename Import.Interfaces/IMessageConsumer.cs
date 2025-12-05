namespace Import.Interfaces;

public interface IListener<T>
{
    Task On(T message);
}
