using System;
using System.Collections.Generic;

/// <summary>
/// Shamelessly stolen from https://neerc.ifmo.ru/wiki/index.php?title=Двоичная_куча
/// </summary>
/// <typeparam name="V"></typeparam>
/// <typeparam name="K"></typeparam>
public class PriorityQueue<V, K> where K : IComparable
{
    private readonly List<(K key, V value)> heap = new();

    public int Count => heap.Count;

    public PriorityQueue() { }

    public void Enqueue(K priority, V value)
    {
        heap.Add((priority, value));
        SiftUp(heap.Count - 1);
    }

    public (K priority, V value) Dequeue()
    {
        var min = heap[0];
        heap[0] = heap[^1];
        heap.RemoveAt(heap.Count - 1);
        SiftDown(0);
        return min;
    }

    void SiftDown(int i)
    {
        while (2 * i + 1 < heap.Count)
        {
            int left = 2 * i + 1;
            int right = 2 * i + 2;
            int j = left;

            if (right < heap.Count && Lt(right, left))
                j = right;

            if (Le(i, j))
                break;

            Swap(i, j);
            i = j;
        }
    }

    void SiftUp(int i)
    {
        while(Lt(i, (i - 1) / 2))
        {
            Swap(i, (i - 1) /  2);
            i = (i - 1) / 2;
        }
    }

    /// <summary>
    /// heap[i].key is less than heap[j].key
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <returns></returns>
    bool Lt(int i, int j) => heap[i].key.CompareTo(heap[j].key) < 0;

    /// <summary>
    /// heap[i].key is less than or equal to heap[j].key
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <returns></returns>
    bool Le(int i, int j) => heap[i].key.CompareTo(heap[j].key) <= 0;

    /// <summary>
    /// Swaps heap elements at indices i and j
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    void Swap(int i, int j)
    {
        (heap[i], heap[j]) = (heap[j], heap[i]);
    }
}
