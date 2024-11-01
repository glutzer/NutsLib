using System.Collections.Generic;

namespace MareLib;

public class QueuedArray<T>
{
    private readonly Queue<int> freeIndices = new();
    private int nextIndex;
    public readonly T[] array;
    public int Count { get; private set; }

    public QueuedArray(int size)
    {
        array = new T[size];
    }

    public T this[int index] => array[index];

    public int Add(T item)
    {
        if (freeIndices.Count > 0)
        {
            int index = freeIndices.Dequeue();
            array[index] = item;
            return index;
        }

        Count++;

        array[nextIndex] = item;
        return nextIndex++;
    }

    public void Remove(int index)
    {
        Count--;

        array[index] = default!; // May store a null value.
        freeIndices.Enqueue(index);
    }
}