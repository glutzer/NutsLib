using System.Collections.Generic;

namespace NutsLib;

/// <summary>
/// Similar to a dictionary, stores things by an id.
/// </summary>
public class QueuedArray<T>
{
    private readonly Queue<int> freeIndices = new();
    private int nextIndex;
    public T[] array;
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

        if (nextIndex == array.Length)
        {
            Array.Resize(ref array, array.Length * 2);
        }

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