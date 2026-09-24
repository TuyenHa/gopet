namespace Gopet.Data.Collections;

public interface IIndexedSnapshot<out T>
{
    IReadOnlyList<T> Snapshot { get; }
}
